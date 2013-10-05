using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private Dictionary<int, Dictionary<string, ExpWrapper>> _alphaDictionary;
        private Dictionary<int, Dictionary<string, ExpWrapper>> _betaDictionary;
        public ExpWrapper Z;
        private Dictionary<int, Dictionary<string, ExpWrapper>> _uDictionary;
        public Dictionary<int, Dictionary<string, ExpWrapper>> UabDictionary;
        private readonly WeightedFeatureSum _weightedFeaturesum;

        public ForwardBackwordAlgo(List<string> inputSentence, WeightVector wc, List<string> tagList)
        {
            _inputSentence = inputSentence;
            _wc = wc;
            _tagList = tagList;
            _tags = new Tags(tagList);
            _alphaDictionary = new Dictionary<int, Dictionary<string, ExpWrapper>>();
            _betaDictionary = new Dictionary<int, Dictionary<string, ExpWrapper>>();
            _uDictionary = new Dictionary<int, Dictionary<string, ExpWrapper>>();
            UabDictionary = new Dictionary<int, Dictionary<string, ExpWrapper>>();
            Z = new ExpWrapper();
            _weightedFeaturesum = new WeightedFeatureSum(wc, inputSentence, true);
        }

        public ExpWrapper GetQ(int j, string a, string b)
        {
            if (UabDictionary.ContainsKey(j))
            {
                if (UabDictionary[j].ContainsKey(a+"#"+b))
                {
                    UabDictionary[j][a + "#" + b].DivideByExp(Z);
                    return UabDictionary[j][a + "#" + b];
                }
            }
            return new ExpWrapper();
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
                UabDictionary.Add(i, new Dictionary<string, ExpWrapper>());
            }
            foreach (var tag in _tagList)
            {
                foreach (var itag in _tagList)
                {
                    for (int i = 0; i < _inputSentence.Count - 1; i++)
                    {
                        var key = tag + "#" + itag;
                        var w = new ExpWrapper(_weightedFeaturesum.GetFeatureValue("*", tag, itag, i));
                        w.MultiplyExp(_alphaDictionary[i][tag]);
                        w.MultiplyExp(_betaDictionary[i][itag]);
                        //var value = _alphaDictionary[i][tag] *w*_betaDictionary[i][itag];
                        UabDictionary[i][key] = w;
                    }
                }
            }
        }

        private void InitBeta()
        {
            _betaDictionary.Add(_inputSentence.Count - 1, new Dictionary<string, ExpWrapper>());
            foreach (var tag in _tagList)
            {
                // initialize.
                _betaDictionary[_inputSentence.Count - 1].Add(tag, new ExpWrapper(0));
            }

            for (var i = _inputSentence.Count - 2 ; i >= 0; i--)
            {
                _betaDictionary.Add(i, new Dictionary<string, ExpWrapper>());
                foreach (var tag in _tagList)
                {
                    var sum = new ExpWrapper();
                    foreach (var itag in _tagList)
                    {
                        var temp = new ExpWrapper(_weightedFeaturesum.GetFeatureValue("*", tag, itag, i+1));
                        temp.MultiplyExp(_betaDictionary[i + 1][itag]);
                        sum.AddExp(temp); 
                    }
                    _betaDictionary[i][tag] = sum;
                }
            }
        }

        public void InitAlpha()
        {
            _alphaDictionary.Add(0, new Dictionary<string, ExpWrapper>());
            foreach (var tag in _tagList)
            {
                // initialize.
                var sum = new ExpWrapper(_weightedFeaturesum.GetFeatureValue("*", "*", tag, 0));
                _alphaDictionary[0].Add(tag, sum);
            }

            for (var i = 1; i < _inputSentence.Count; i++)
            {
                _alphaDictionary.Add(i, new Dictionary<string, ExpWrapper>());
                foreach (var tag in _tagList)
                {
                    var sum = new ExpWrapper();
                    foreach (var itag in _tagList)
                    {
                        var temp = new ExpWrapper(_weightedFeaturesum.GetFeatureValue("*", itag, tag, i));
                        temp.MultiplyExp(_alphaDictionary[i-1][itag]);
                        sum.AddExp(temp);
                    }
                    _alphaDictionary[i].Add(tag, sum);
                }
            }

            foreach (var tag in _tagList)
            {
                Z.AddExp(_alphaDictionary[_inputSentence.Count - 1][tag]);
            }

        }

        //private void InitU()
        //{
        //    for (int i = 0; i < _inputSentence.Count; i++)
        //    {
        //        _uDictionary.Add(i, new Dictionary<string, double>());
        //    }

        //    foreach (var tag in _tagList)
        //    {
        //        for (int i = 0; i < _inputSentence.Count; i++)
        //        {
        //            var value = _alphaDictionary[i][tag] * _betaDictionary[i][tag];
        //            _uDictionary[i].Add(tag, value);
        //        }
        //    }
        //}

    }
}
