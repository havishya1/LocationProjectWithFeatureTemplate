using System;
using System.Collections.Generic;
using System.Linq;

namespace LocationProjectWithFeatureTemplate
{
    class Perceptron
    {
        private readonly string _inputFile;
        private readonly string _outputFile;
        private readonly bool _useAvg;
        public WeightVector WeightVector;
        private readonly ViterbiForGlobalLinearModel _viterbiForGlobalLinearModel;
        public MapFeaturesToK MapFeatures;

        public List<List<string>> InputSentences;
        public List<List<string>> TagsList;

        public Perceptron(string inputFile, string outputFile, List<string> tagList, bool useAvg = false)
        {
            _inputFile = inputFile;
            _outputFile = outputFile;
            _useAvg = useAvg;
            var tags = new Tags(tagList);
            MapFeatures = new MapFeaturesToK(inputFile, string.Concat(outputFile, ".featuresToK"), tagList);
            MapFeatures.StartMapping();
            WeightVector = new WeightVector(MapFeatures.DictFeaturesToK, MapFeatures.FeatureCount);
            AvgWeightVector = new WeightVector(MapFeatures.DictFeaturesToK, MapFeatures.FeatureCount);
            _viterbiForGlobalLinearModel = new ViterbiForGlobalLinearModel(WeightVector, tags);
            InputSentences = new List<List<string>>();
            TagsList = new List<List<string>>();
            ReadInputs();
        }

        public WeightVector AvgWeightVector { get; set; }

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
            const int iterationCount = 10;
            for (var i = 0; i < iterationCount; i++)
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
                        var inputAdd = 1*Features.GetWeight(inputFeature.Current.Value);
                        var outputRemove = -1*Features.GetWeight(outputFeature.Current.Value);
                        WeightVector.AddToKey(inputFeature.Current.Value,inputAdd);
                        WeightVector.AddToKey(outputFeature.Current.Value, outputRemove);
                    }
                }

                AvgWeightVector.AddWeightVector(WeightVector);
                inputData.Reset();    
            }

            AvgWeightVector.DividebyNum(iterationCount);

            Console.WriteLine(DateTime.Now+" training is complete");
            
        }

        public void ReMapFeatureToK(bool normalize = true)
        {
            MapFeatures.ReMappingFromWeightVector(_useAvg ? AvgWeightVector : WeightVector, normalize);
            if (_useAvg)
            {
                WeightVector = AvgWeightVector;
            }
        }

        public void Dump()
        {
            var output = new WriteModel(string.Concat(_outputFile, ""));
            
            for (int index = 0; index < WeightVector.FeatureCount; index++)
            {
                output.WriteLine(string.Format("{0} {1} {2}", index,
                    MapFeatures.DictKToFeatures[index],
                    _useAvg ? AvgWeightVector.WeightArray[index] : WeightVector.WeightArray[index]));
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
