using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

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

        public ForwardBackwordAlgo(List<string> inputSentence, WeightVector wc, List<string> tagList)
        {
            _inputSentence = inputSentence;
            _wc = wc;
            _tagList = tagList;
            _tags = new Tags(tagList);
            _alphaDictionary = new Dictionary<int, Dictionary<string, double>>();
            _betaDictionary = new Dictionary<int, Dictionary<string, double>>();
            _uDictionary = new Dictionary<int, Dictionary<string, double>>();
            UabDictionary = new Dictionary<int, Dictionary<string, double>>();
            Z = 0;
            _weightedFeaturesum = new WeightedFeatureSum(wc, inputSentence, true);
        }

        public double GetQ(int j, string a, string b)
        {
            if (UabDictionary.ContainsKey(j))
            {
                if (UabDictionary[j].ContainsKey(a+"#"+b))
                {
                    return UabDictionary[j][a + "#" + b];
                }
            }
            return 0;
        }

        public void Run()
        {
            InitAlpha();
            InitBeta();
            InitUab();
        }

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
            foreach (var tag in _tagList)
            {
                // initialize.
                _betaDictionary.Add(_inputSentence.Count - 1, new Dictionary<string, double>());
                _betaDictionary[_inputSentence.Count - 1].Add(tag, 1);
            }

            for (var i = _inputSentence.Count - 2 ; i >= 0; i--)
            {
                _betaDictionary.Add(i, new Dictionary<string, double>());
                foreach (var tag in _tagList)
                {
                    double sum = 0;
                    foreach (var itag in _tagList)
                    {
                        var temp = _weightedFeaturesum.GetFeatureValue("*", tag, itag, i+1);
                        sum += (temp * _betaDictionary[i + 1][itag]);
                    }
                    _betaDictionary[i][tag] = sum;
                }
            }
        }

        public void InitAlpha()
        {
            foreach (var tag in _tagList)
            {
                // initialize.
                var sum = _weightedFeaturesum.GetFeatureValue("*", "*", tag, 0);
                _alphaDictionary.Add(0, new Dictionary<string, double>());
                _alphaDictionary[0].Add(tag, sum);
            }

            for (var i = 1; i < _inputSentence.Count; i++)
            {
                _alphaDictionary.Add(i, new Dictionary<string, double>());
                foreach (var tag in _tagList)
                {
                    double sum = 0;
                    foreach (var itag in _tagList)
                    {
                        var temp = _weightedFeaturesum.GetFeatureValue("*", itag, tag, i);
                        sum += (temp * _alphaDictionary[i-1][itag]);
                    }
                    _alphaDictionary[i][tag] = sum;
                }
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
