using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocationProjectWithFeatureTemplate
{
    public class ComputeGradient
    {
        private readonly List<List<string>> _inputSentence;
        private readonly List<List<string>> _outputTagsList;
        private readonly List<string> _tagList;
        private readonly double _lambda;
        private readonly FeatureCache _cache;
        private List<ForwardBackwordAlgo> forwardBackwordAlgos;
        private WeightVector _weightVector;
        private List<Tuple<string, string>> _twoGramsList;

        public ComputeGradient(List<List<string>> inputSentence, List<List<string>> tagsList,
            List<string> tagList, double lambda, FeatureCache cache)
        {
            _inputSentence = inputSentence;
            _outputTagsList = tagsList;
            _tagList = tagList;
            _lambda = lambda;
            _cache = cache;
            forwardBackwordAlgos = new List<ForwardBackwordAlgo>();
            _weightVector = null;
            _twoGramsList = new List<Tuple<string, string>>();
            var ngramTags = new Tags(_tagList);
            foreach (var ngram in ngramTags.GetNGramTags(2))
            {
                string[] split = ngram.Split(new[] { ':' });
                _twoGramsList.Add(new Tuple<string, string>(split[0], split[1]));
            }
        }

        public void Dump(string outputFile, Dictionary<int, string> dictKtoFeature)
        {
            Console.WriteLine(DateTime.Now+" training is complete");
            var output = new WriteModel(outputFile);
            for (int index = 0; index <_weightVector.FeatureCount; index++)
            {
                output.WriteLine(string.Format("{0} {1} {2}", index,
                    dictKtoFeature[index], _weightVector.WeightArray[index]));
            }
            //var sortedDictionary = from pair in _weightVector.WDictionary
            //    orderby Math.Abs(pair.Value) descending
            //    select pair;
            //foreach (var weight in sortedDictionary)
            //{
            //    output.WriteLine(string.Format("{0} {1} {2}", weight.Key,
            //        dictKtoFeature[weight.Key], weight.Value));
            //}
            output.Flush();
        }

        private void SetForwardBackwordAlgo(WeightVector weightVector)
        {
            if (_inputSentence.Count != _outputTagsList.Count)
            {
                throw new Exception("counts dont match " + _inputSentence.Count + "with " + _outputTagsList.Count);
            }
            int counter = 0;
            forwardBackwordAlgos.Clear();
            foreach (var sentence in _inputSentence)
            {
                var outputTags = _outputTagsList[counter];
                if (sentence.Count != outputTags.Count)
                {
                    throw new Exception("counts dont match " + sentence.Count + "with " + outputTags.Count);
                }
                forwardBackwordAlgos.Add(new ForwardBackwordAlgo(sentence, weightVector, outputTags));
                counter++;
            }
        }

        public WeightVector RunIterations(WeightVector weightVector, int iterationCount, int threadCount)
        {
            _weightVector = weightVector;

            for (int iter = 0; iter < iterationCount; iter++)
            {
                Console.WriteLine(DateTime.Now + " running iteration " + iter);
                var newWeightVector = new WeightVector(weightVector.FeatureKDictionary, _weightVector.FeatureCount);
                SetForwardBackwordAlgo(weightVector);
                var doneEvents = new ManualResetEvent[threadCount];
                var partition = weightVector.FeatureCount/threadCount;

                for (int threadIndex = 0; threadIndex < threadCount; threadIndex++)
                {
                    var start = threadIndex*partition;
                    var end = start + partition;
                    end = end > weightVector.FeatureCount ? weightVector.FeatureCount : end;
                    doneEvents[threadIndex] = new ManualResetEvent(false);

                    var info = new ThreadInfoObject(this, start, end, newWeightVector,
                        doneEvents[threadIndex]);
                    ThreadPool.QueueUserWorkItem(info.StartGradientComputing, threadIndex);
                }

                WaitHandle.WaitAll(doneEvents);
                //for (var k = _weightVector.FeatureKDictionary.Count-1; k >= 0; k--)
                //{
                //    var wk = Compute(k);
                //    newWeightVector.SetKey(k, wk);
                //}
                _weightVector = weightVector = newWeightVector;
            }
            return _weightVector;
        }

        public void ComputeRange(int start, int end, WeightVector newWeightVector, int threadIndex = 0)
        {
            for (var k = start; k < end; k++)
            {
                if (k % 100 == 0)
                {
                    Console.WriteLine(DateTime.Now + "threadIndex: "+ threadIndex+
                        " running iteration for k " + k);
                }
                var wk = Compute(k);
                newWeightVector.SetKey(k, wk);
            }
        }

        private double Compute(int k)
        {
            double output = 0;
            //double secondTerm = 0;
            int lineIndex = 0;
            //var weightedFeaturesum = new WeightedFeatureSum(weightVector, null, true);

            if (_inputSentence.Count != _outputTagsList.Count)
            {
                throw new Exception("counts dont match "+ _inputSentence.Count + "with "+ _outputTagsList.Count);
            }
            var ngramTags = new Tags(_tagList);

            // first term.
            //foreach (var sentence in _inputSentence)
            for (lineIndex = 0; lineIndex< _inputSentence.Count; lineIndex++)
            {
                var outputTags = _outputTagsList[lineIndex];

                var initOutput = GetAllFeatureKFromCache(outputTags, k, lineIndex);

                output += CalculateGradient(outputTags, k,
                    ngramTags, lineIndex, initOutput);

                //output += weightedFeaturesum.GetAllFeatureK(outputTags, k, sentence);

                //// second term.
                //for (var j = 0; j < outputTags.Count; j++)
                //{
                //    double sum = 0;
                //    foreach (var ngramTag in ngramTags.GetNGramTags(2))
                //    {
                //        string[] split = ngramTag.Split(new[] {':'});
                //        sum += (forwardBackwordAlgos[i].GetQ(j, split[0], split[1]) *
                //            weightedFeaturesum.GetFeatureK(split[0], split[1], j, k, sentence));
                //    }
                //    secondTerm += sum;
                //}
            }

            output = output - (_lambda * _weightVector.Get(k));
            output = _weightVector.Get(k) + _lambda * output;
            return output;
        }

        private double CalculateGradient(List<string> outputTags,
            int k, Tags ngramTags, int lineIndex, double initOutput)
        {
            double output = initOutput;
            double secondTerm = 0;
            //output += weightedFeatureSum.GetAllFeatureK(outputTags, k, sentence);
            
            

            // second term.
            for (var pos = 0; pos < outputTags.Count; pos++)
            {
                //double sum = 0;
                secondTerm += GetSecondTerm(ngramTags, lineIndex, pos, k);
                //foreach (var ngramTag in ngramTags.GetNGramTags(2))
                //{
                //    string[] split = ngramTag.Split(new[] { ':' });
                //    sum += (forwardBackwordAlgos[i].GetQ(j, split[0], split[1]) *
                //        weightedFeatureSum.GetFeatureK(split[0], split[1], j, k, sentence));
                //}
                //secondTerm += sum;
            }
            return output - secondTerm;
        }

        private double GetSecondTerm(Tags ngramTags, 
            int lineIndex, int pos, int k)
        {
            double sum = 0;
            //foreach (var ngramTag in ngramTags.GetNGramTags(2))
            foreach (var tuple in _twoGramsList)
            {
                if (_cache.Contains(tuple.Item1, tuple.Item2, k, pos, lineIndex))
                {
                    var value = forwardBackwordAlgos[lineIndex].GetQ(pos, tuple.Item1, tuple.Item2);
                    sum += ( value * _weightVector.Get(k));
                }
            }
            //{
            //    string[] split = ngramTag.Split(new[] { ':' });

            //    if (_cache.Contains(split[0], split[1], k, pos, lineIndex))
            //    {
            //        sum += (forwardBackwordAlgos[lineIndex].GetQ(pos, split[0], split[1]) *
            //        _weightVector.Get(k));    
            //    }
            //    //else
            //    //{
            //    //    sum += (forwardBackwordAlgos[lineIndex].GetQ(j, split[0], split[1]) *
            //    //    weightedFeatureSum.GetFeatureK(split[0], split[1], j, k, sentence));
            //    //}

            //}
            return sum;
        }

        public double GetAllFeatureKFromCache(List<string> tags, int k, int lineIndex)
        {
            double sum = 0;
            for (var pos = 0; pos < tags.Count; pos++)
            {
                var prevTag = "*";
                if (pos > 0)
                {
                    prevTag = tags[pos - 1];
                }
                if (_cache.Contains(prevTag, tags[pos], k, pos, lineIndex))
                {
                    sum += Math.Exp(_weightVector.Get(k));
                }
            }
            return sum;
        }
    }
    
}

