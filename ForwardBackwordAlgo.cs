using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace LocationProjectWithFeatureTemplate
{
    class ForwardBackwordAlgo
    {
        private readonly List<string> _inputSentence;
        private readonly WeightVector _wc;
        private readonly List<string> _tagList;
        private Tags _tags;
        private Dictionary<int, Dictionary<string, double>> _alphaDictionary;
        private Dictionary<int, Dictionary<string, double>> _betaDictionary;
        public double Z;
        private Dictionary<int, Dictionary<string, double>> _uDictionary;
        public Dictionary<int, Dictionary<string, double>> UabDictionary;
        private readonly WeightedFeatureSum _weightedFeaturesum;
        private string[] _twoGramsList;
        private List<double> cList;
        private List<double> dList;

        public ForwardBackwordAlgo(List<string> inputSentence, WeightVector wc, List<string> tagList)
        {
            _inputSentence = inputSentence;
            _wc = wc;
            _tagList = tagList;
            _alphaDictionary = new Dictionary<int, Dictionary<string, double>>();
            _betaDictionary = new Dictionary<int, Dictionary<string, double>>();
            _uDictionary = new Dictionary<int, Dictionary<string, double>>();
            UabDictionary = new Dictionary<int, Dictionary<string, double>>();
            Z = 0;
            _weightedFeaturesum = new WeightedFeatureSum(wc, inputSentence, true);
            cList = new List<double>(_inputSentence.Count);
            dList = new List<double>(_inputSentence.Count);

            _twoGramsList = new string[4];
            var ngramTags = new Tags(_tagList);
            int index = 0;
            foreach (var ngram in ngramTags.GetNGramTags(2))
            {
                if (index >= _twoGramsList.Length)
                {
                    Array.Resize(ref _twoGramsList, index + 1);
                }
                string[] split = ngram.Split(new[] { ':' });
                _twoGramsList[index] = split[0] + "@#" + split[1];
                index++;
            }
        }

        public double GetQ(int j, string a, string b)
        {
            if (UabDictionary.ContainsKey(j))
            {
                if (UabDictionary[j].ContainsKey(a+"#"+b))
                {
                    return UabDictionary[j][a + "#" + b]/Z;
                }
            }
            return 0;
        }

        public void Run()
        {
            InitAlpha();
            InitBeta();
            //ValidateCListAndDlist();
            InitUab();
            Z = (Z != 0) ? Z : 1;
        }

        //private void ValidateCListAndDlist()
        //{
        //    int count = _inputSentence.Count-1;
            
        //    Console.WriteLine("c["+count+"]="+cList[count]+" == td["+0+"]="+dList[0]);
        //    Console.WriteLine("diff = " + (cList[count] - dList[0]));

        //    Console.ReadLine();

        //    for (int i = 0; i < count; i++)
        //    {
        //        //Console.WriteLine("c["+i+"]="+cList[i]+"\td["+(i+1)+"]="+dList[i+1]);
        //        Console.WriteLine("multiply: c["+i+"] * d["+(i+1)+"]="+ (cList[i] * dList[i+1]));
        //        Console.WriteLine("diff= " + ((cList[i] * dList[i + 1]) - cList[count]));
        //    }
        //    Console.ReadLine();
        //}

        private void InitUab()
        {
            for (int i = 0; i < _inputSentence.Count; i++)
            {
                UabDictionary.Add(i, new Dictionary<string, double>());
            }
            foreach (var tag in _tagList)
            {
                foreach (var itag in _tagList)
                {
                    for (int i = 0; i < _inputSentence.Count - 1; i++)
                    {
                        var key = tag + "#" + itag;
                        var w = _weightedFeaturesum.GetFeatureValue("*", tag, itag, i);
                        var value = _alphaDictionary[i][tag]*w*_betaDictionary[i][itag];
                        UabDictionary[i][key] = value;
                    }
                }
            }
        }

        private void InitBeta()
        {
            _betaDictionary.Add(_inputSentence.Count - 1, new Dictionary<string, double>());
            foreach (var tag in _tagList)
            {
                // initialize.
                _betaDictionary[_inputSentence.Count - 1].Add(tag, 1.0/ cList[_inputSentence.Count - 1]);
            }
                
            for (var i = _inputSentence.Count - 2 ; i >= 0; i--)
            {
                _betaDictionary.Add(i, new Dictionary<string, double>());
                foreach (var tag in _tagList)
                {
                    double betaParts = 0;
                    foreach (var itag in _tagList)
                    {
                        var temp = _weightedFeaturesum.GetFeatureValue("*", tag, itag, i+1);
                        betaParts += (temp * _betaDictionary[i + 1][itag]);
                    }
                    _betaDictionary[i][tag] = betaParts / cList[i];
                }
            }
        }

        public void InitAlpha()
        {
            _alphaDictionary.Add(0, new Dictionary<string, double>());
            double sum = 0;
            foreach (var tag in _tagList)
            {
                // initialize.
                var tagExpectation = _weightedFeaturesum.GetFeatureValue("*", "*", tag, 0);
                _alphaDictionary[0].Add(tag, tagExpectation);
                sum += tagExpectation;
            }
            cList.Add(sum);
            //Console.WriteLine(cList.Count-1 +"value:"+ cList[cList.Count-1]);
            foreach (var tag in _tagList)
            {
                // scale to 1.
                _alphaDictionary[0][tag] = _alphaDictionary[0][tag] / sum;
            }

            for (var i = 1; i < _inputSentence.Count; i++)
            {
                sum = 0;
                _alphaDictionary.Add(i, new Dictionary<string, double>());
                foreach (var tag in _tagList)
                {
                    double alphaParts = 0;
                    foreach (var itag in _tagList)
                    {
                        var tagExpectation = _weightedFeaturesum.GetFeatureValue("*", itag, tag, i);
                        alphaParts += (tagExpectation * _alphaDictionary[i - 1][itag]);
                    }
                    _alphaDictionary[i][tag] = alphaParts;
                    sum += alphaParts;
                }

                foreach (var tag in _tagList)
                {
                    _alphaDictionary[i][tag] /= sum;
                }
                cList.Add(sum);
                //Console.WriteLine(cList.Count - 1 + "value:" + cList[cList.Count - 1]);
            }

            foreach (var tag in _tagList)
            {
                Z += _alphaDictionary[_inputSentence.Count - 1][tag];
            }

        }

        private void InitU()
        {
            for (int i = 0; i < _inputSentence.Count; i++)
            {
                _uDictionary.Add(i, new Dictionary<string, double>());
            }

            foreach (var tag in _tagList)
            {
                for (int i = 0; i < _inputSentence.Count; i++)
                {
                    var value = _alphaDictionary[i][tag] * _betaDictionary[i][tag];
                    _uDictionary[i].Add(tag, value);
                }
            }
        }

    }
}
