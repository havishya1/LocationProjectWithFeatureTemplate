using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocationProjectWithFeatureTemplate
{
    class ComputeGradient
    {
        private readonly List<List<string>> _inputSentence;
        private readonly List<List<string>> _outputTagsList;
        private readonly List<string> _tagList;
        private readonly double _lambda;
        private readonly FeatureCache _cache;
        private List<ForwardBackwordAlgo> forwardBackwordAlgos;
        private WeightVector _weightVector;

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
        }

        public void Dump(string outputFile, Dictionary<int, string> dictKtoFeature)
        {
            Console.WriteLine(DateTime.Now+" training is complete");
            var output = new WriteModel(outputFile);
            var sortedDictionary = from pair in _weightVector.WDictionary
                orderby Math.Abs(pair.Value) descending
                select pair;
            foreach (var weight in sortedDictionary)
            {
                output.WriteLine(string.Format("{0} {1} {2}", weight.Key,
                    dictKtoFeature[weight.Key], weight.Value));
            }
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

        public WeightVector RunIterations(WeightVector weightVector, int iterationCount)
        {
            for (int iter = 0; iter < iterationCount; iter++)
            {
                Console.WriteLine(DateTime.Now + " running iteration " + iter);
                var newWeightVector = new WeightVector(weightVector.FeatureKDictionary);
                SetForwardBackwordAlgo(weightVector);
                //for (var k = 0; k < weightVector.FeatureKDictionary.Count; k++)
                for (var k = weightVector.FeatureKDictionary.Count-1; k >= 0; k--)
                {
                    if (k%100 == 0)
                    {
                        Console.WriteLine(DateTime.Now + " running iteration for k " + k);
                    }
                    var wk = Compute(k, weightVector);
                    wk = weightVector.Get(k) + _lambda*wk;
                    newWeightVector.SetKey(k, wk);
                }
                weightVector = newWeightVector;
            }
            _weightVector = weightVector;
            return weightVector;
        }

        private double Compute(int k, WeightVector weightVector)
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
            foreach (var sentence in _inputSentence)
            {
                var outputTags = _outputTagsList[lineIndex];

                if (sentence.Count != outputTags.Count)
                {
                    throw new Exception("compute counts dont match " + sentence.Count + "with " + outputTags.Count);
                }

                output += CalculateGradient(outputTags, k,
                    ngramTags, lineIndex);

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
                lineIndex++;
            }

            output = output - (_lambda*weightVector.Get(k));
            return output;
        }

        private double CalculateGradient(List<string> outputTags,
            int k, Tags ngramTags, int lineIndex)
        {
            double output = 0;
            double secondTerm = 0;
            //output += weightedFeatureSum.GetAllFeatureK(outputTags, k, sentence);
            output += GetAllFeatureKFromCache(outputTags, k, lineIndex);
            

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
            foreach (var ngramTag in ngramTags.GetNGramTags(2))
            {
                string[] split = ngramTag.Split(new[] { ':' });

                if (_cache.Contains(split[0], split[1], k, pos, lineIndex))
                {
                    sum += (forwardBackwordAlgos[lineIndex].GetQ(pos, split[0], split[1]) *
                    _weightVector.Get(k));    
                }
                //else
                //{
                //    sum += (forwardBackwordAlgos[lineIndex].GetQ(j, split[0], split[1]) *
                //    weightedFeatureSum.GetFeatureK(split[0], split[1], j, k, sentence));
                //}

            }
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

