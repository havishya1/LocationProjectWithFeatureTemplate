using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
        private string[] _twoGramsList;
        private KeyValuePair<string,string>[] _twoGramPair;

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
            _twoGramsList = new string[4];
            _twoGramPair = new KeyValuePair<string, string>[4];
            var ngramTags = new Tags(_tagList);
            int index = 0;
            foreach (var ngram in ngramTags.GetNGramTags(2))
            {
                if (index >= _twoGramsList.Length)
                {
                    Array.Resize(ref _twoGramsList, index+1);
                    Array.Resize(ref _twoGramPair, index + 1);
                }
                string[] split = ngram.Split(new[] { ':' });
                _twoGramsList[index] = split[0] +"@#"+ split[1];
                _twoGramPair[index] = new KeyValuePair<string, string>(split[0], split[1]);
                index++;
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

        public WeightVector RunIterations(WeightVector weightVector, int iterationCount, int threadCount = 1)
        {
            _weightVector = weightVector;
            //var newWeightVector = new WeightVector(weightVector.FeatureKDictionary, _weightVector.FeatureCount);

            for (int iter = 0; iter < iterationCount; iter++)
            {
                Console.WriteLine(DateTime.Now + " running iteration " + iter);
                //var newWeightVector = new WeightVector(weightVector.FeatureKDictionary, _weightVector.FeatureCount);
                SetForwardBackwordAlgo(weightVector);
                if (threadCount > 1)
                {
                    var doneEvents = new ManualResetEvent[threadCount];
                    var partition = weightVector.FeatureCount/threadCount;

                    for (int threadIndex = 0; threadIndex < threadCount; threadIndex++)
                    {
                        var start = threadIndex*partition;
                        var end = start + partition;
                        end = end > weightVector.FeatureCount ? weightVector.FeatureCount : end;
                        doneEvents[threadIndex] = new ManualResetEvent(false);

                        var info = new ThreadInfoObject(this, start, end, weightVector,
                            doneEvents[threadIndex]);
                        ThreadPool.QueueUserWorkItem(info.StartGradientComputing, threadIndex);
                    }

                    WaitHandle.WaitAll(doneEvents);
                }
                else
                {
                    ComputeRange(0, _weightVector.FeatureCount, _weightVector);
                }
                //for (var k = _weightVector.FeatureKDictionary.Count-1; k >= 0; k--)
                //{
                //    var wk = Compute(k);
                //    newWeightVector.SetKey(k, wk);
                //}
                _weightVector = weightVector;
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
            string kstring = "@#" + k.ToString(CultureInfo.InvariantCulture);
            //var weightedFeaturesum = new WeightedFeatureSum(weightVector, null, true);

            if (_inputSentence.Count != _outputTagsList.Count)
            {
                throw new Exception("counts dont match "+ _inputSentence.Count + "with "+ _outputTagsList.Count);
            }

            // first term.
            //foreach (var sentence in _inputSentence)
            for (lineIndex = 0; lineIndex< _inputSentence.Count; lineIndex++)
            {
                var outputTags = _outputTagsList[lineIndex];

                var initOutput = GetAllFeatureKFromCache(outputTags, k, lineIndex);

                output += CalculateGradient(outputTags, k,
                    lineIndex, initOutput, kstring);

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
            int k, int lineIndex, double initOutput, string kString)
        {
            double output = initOutput;
            double secondTerm = 0;
            //output += weightedFeatureSum.GetAllFeatureK(outputTags, k, sentence);
            
            

            // second term.
            for (var pos = 0; pos < outputTags.Count; pos++)
            {
                //double sum = 0;
                secondTerm += GetSecondTerm(lineIndex, pos, k, kString);
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

        private double GetSecondTerm(int lineIndex, int pos, int k, string kString)
        {
            double sum = 0;
            //foreach (var ngramTag in ngramTags.GetNGramTags(2))
            for(var i = 0; i< _twoGramsList.Length; i++)
            {
                if (_cache.Contains(_twoGramsList[i], kString, pos, lineIndex))
                {
                    var value = forwardBackwordAlgos[lineIndex].GetQ(pos, _twoGramPair[i].Key,
                        _twoGramPair[i].Value);
                    sum += (value * _weightVector.Get(k));
                }
            }
            return sum;
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
            //return sum;
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

