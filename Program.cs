using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocationProjectWithFeatureTemplate;

namespace LocationProjectWithFeatureTemplate
{
    class Program
    {
        static void Main(string[] args)
        {
            var tags = new List<string> { "LOCATION", "OTHER" };
            //ReadNewsWireData();
            TrainingTest(tags);
            //Test1(tags, false, true);

            //const string modelFile = "../../data/tag.model";
            //const string input = "../../data/gene.test";
            //const string outputFile = "../../data/gene_test.p2.out";

        }

        private static string EvaluateModel(string keyFile, string devFile, string outputDump)
        {
            var evalModel = new EvalModel();
            return evalModel.Evalulate(keyFile, devFile, outputDump);
        }


        static void TrainingTest(List<string> tags)
        {
            //const string modelFile = "../../data/gene.key.model";
            //const string input = "../../data/gene.key";

            const string modelFile = "../../data/training/tag.model.trial1";
            const string input = "../../data/training/NYT_19980403_parsed.key";
            var perceptron = new Perceptron(input, modelFile, tags);
            perceptron.Train();
            perceptron.ReMapFeatureToK();
            perceptron.Dump();
            perceptron.MapFeatures.Dump();
            perceptron.ReadInputs();
            var featureCache = new FeatureCache(perceptron.InputSentences, tags,
                perceptron.MapFeatures.DictFeaturesToK);
            featureCache.CreateCache();
            var gradient = new ComputeGradient(perceptron.InputSentences, perceptron.TagsList,
                tags, .1, featureCache);
            gradient.RunIterations(perceptron.WeightVector, 10);
            gradient.Dump(modelFile, perceptron.MapFeatures.DictKToFeatures);
        }

        static void Test1(List<string> tags, bool debug, bool eval)
        {
            //const string input = "../../data/gene.dev";
            //const string outputFile = "../../data/gene_dev.output3";
            //const string modelFile = "../../data/gene.key.model";

            var inputFiles = new[]
                                 {
                                     "../../data/training/NYT_19980403_parsed",
                                     "../../data/training/APW_19980314_parsed",
                                     "../../data/training/APW_19980424_parsed",
                                     "../../data/training/APW_19980429_parsed",
                                     "../../data/training/NYT_19980315_parsed",
                                     "../../data/training/NYT_19980407_parsed"
                                 };

            foreach (var inputFile in inputFiles)
            {
                string input = inputFile + ".key.dev";
                string outputFile = inputFile + ".dev.output1";
                string keyFile = inputFile + ".key";
                string outputEval = inputFile + ".dev.evalDump";
                const string modelFile = "../../data/training/tag.model.10itertation";


                var testGLMViterbi = new TestGLMViterbi(modelFile, input, outputFile, tags);
                testGLMViterbi.Setup(debug);

                if (eval)
                {
                    var dump = EvaluateModel(keyFile, outputFile, outputEval);
                    Console.WriteLine(dump);
                    Console.ReadLine();
                }    
            }
            
        }

        private static void ReadNewsWireData()
        {
            string[] input =
            {
                "../../data/training/APW_19980314",
                "../../data/training/APW_19980424",
                "../../data/training/APW_19980429",
                "../../data/training/NYT_19980315",
                "../../data/training/NYT_19980403",
                "../../data/training/NYT_19980407"
            };
            string[] output =
            {
                "../../data/training/APW_19980314_parsed.key",
                "../../data/training/APW_19980424_parsed.key",
                "../../data/training/APW_19980429_parsed.key",
                "../../data/training/NYT_19980315_parsed.key",
                "../../data/training/NYT_19980403_parsed.key",
                "../../data/training/NYT_19980407_parsed.key"
            };
            //const string input = "../../data/training/APW_19980314";
            //const string output = "../../data/training/APW_19980314.parsed_key";
            for (int i = 0; i < input.Length; i++)
            {
                var parseNEWSWIRE = new ParseNEWSWIRETrainingData();
                parseNEWSWIRE.Parse(input[i], output[i]);
            }
        }

        /*
                static void Test()
                {
                    const string inputFile = "../../data/tag.model";
                    //const string outputFile = "../../test.output1";
                    var readModel = new ReadModel(inputFile);
                    //var writeModel = new WriteModel(outputFile);
                    var weightVector = new WeightVector();
                    var tags = new List<string> {"I-GENE", "O"};

                    PrintFeatureList(tags);

                    foreach (var pair in readModel.ModelIterator())
                    {
                        weightVector.Add(pair);
                
                    }
                    //writeModel.WriteLine(line);
                    //writeModel.Flush();
                }
        */

        static void PrintFeatureList(List<string> tags)
        {
            var featureTags = new Tags(tags);
            featureTags.Dump(3);
        }
    }
}
