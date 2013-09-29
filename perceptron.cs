using System;
using System.Collections.Generic;
using System.Linq;

namespace LocationProjectWithFeatureTemplate
{
    class Perceptron
    {
        private readonly string _inputFile;
        private readonly string _outputFile;
        public readonly WeightVector WeightVector;
        private readonly ViterbiForGlobalLinearModel _viterbiForGlobalLinearModel;
        public MapFeaturesToK MapFeatures;

        public List<List<string>> InputSentences;
        public List<List<string>> TagsList;

        public Perceptron(string inputFile, string outputFile, List<string> tagList)
        {
            _inputFile = inputFile;
            _outputFile = outputFile;
            var tags = new Tags(tagList);
            MapFeatures = new MapFeaturesToK(inputFile, string.Concat(outputFile, ".featuresToK"), tagList);
            MapFeatures.StartMapping();
            WeightVector = new WeightVector(MapFeatures.DictFeaturesToK, MapFeatures.FeatureCount);
            _viterbiForGlobalLinearModel = new ViterbiForGlobalLinearModel(WeightVector, tags);
            InputSentences = new List<List<string>>();
            TagsList = new List<List<string>>();
            ReadInputs();
        }

        public void ReadInputs()
        {
            var inputData = new ReadInputData(_inputFile);
            foreach (var line in inputData.GetSentence())
            {
                var inputTags = new List<string>(line.Count);
                var inputList = new List<string>(line.Count);
                for (var j = 0; j < line.Count; j++)
                {
                    var split = line[j].Split(new char[] { ' ' });
                    inputList.Add(split[0]);
                    inputTags.Add(split[1]);
                }
                InputSentences.Add(inputList);
                TagsList.Add(inputTags);
            }
            inputData.Reset();    
        }

        public void Train()
        {
            for (var i = 0; i < 1; i++)
            {
                Console.WriteLine(DateTime.Now+" training iteration: "+ i);
                var inputData = new ReadInputData(_inputFile);
                foreach (var line in inputData.GetSentence())
                {
                    var inputTags = new List<string>(line.Count);
                    for(var j = 0; j < line.Count;j++)
                    {
                        var split = line[j].Split(new char[] {' '});
                        line[j] = split[0];
                        inputTags.Add(split[1]);
                    }
                    List<string> temp;
                    var outputTags = _viterbiForGlobalLinearModel.Decode(line, false, out temp);
                    if (Match(inputTags, outputTags)) continue;
                    var inputFeature = (new FeatureWrapper(inputTags, line)).NextFeature().GetEnumerator();
                    var outputFeature= new FeatureWrapper(outputTags, line).NextFeature().GetEnumerator();
                    while (inputFeature.MoveNext() && outputFeature.MoveNext())
                    {
                        if (inputFeature.Current.Key.Equals(outputFeature.Current.Key))
                            continue;
                        WeightVector.AddToKey(inputFeature.Current.Value,
                            1 * Features.GetWeight(inputFeature.Current.Value));
                        WeightVector.AddToKey(outputFeature.Current.Value,
                            -1 * Features.GetWeight(inputFeature.Current.Value));
                    }
                }
                
                inputData.Reset();    
            }

            //  _weightVector.NormalizeAllWeights(100);

            Console.WriteLine(DateTime.Now+" training is complete");
            
        }

        public void ReMapFeatureToK()
        {
            MapFeatures.ReMappingFromWeightVector(WeightVector);
        }

        public void Dump()
        {
            var output = new WriteModel(string.Concat(_outputFile, ".temp"));
            var sortedDictionary = from pair in WeightVector.WDictionary
                                   orderby Math.Abs(pair.Value) descending
                                   select pair;
            foreach (var weight in sortedDictionary)
            {
                output.WriteLine(string.Format("{0} {1}", 
                    MapFeatures.DictKToFeatures[weight.Key], weight.Value));
                //output.WriteLine(string.Format("{0} {1} {2}", weight.Key,
                //    MapFeatures.DictKToFeatures[weight.Key], weight.Value));
            }
            output.Flush();
        }

        private static bool Match(IReadOnlyCollection<string> inputTags, IReadOnlyList<string> outputTags)
        {
            if (inputTags == null) return false;
            if (inputTags.Count != outputTags.Count)
            {
                throw new Exception(inputTags.Count + " don't match " + outputTags.Count);
            }

            return !inputTags.Where((t, i) => !t.Equals(outputTags[i])).Any();
        }
    }
}
