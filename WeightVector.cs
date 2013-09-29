using System;
using System.Collections.Generic;
using System.Linq;

namespace LocationProjectWithFeatureTemplate
{
    public class WeightVector
    {
        public Dictionary<int, double> WDictionary;
        public Dictionary<string, int> FeatureKDictionary;
        public double[] WeightArray;

        public WeightVector()
        {
            WDictionary = new Dictionary<int, double>();
            FeatureKDictionary = new Dictionary<string, int>();
            WeightArray = new double[40000];
            FeatureCount = 0;
            throw new Exception("did you forget ???");
        }

        public WeightVector(Dictionary<string, int> inputFeatureTemp, int count)
        {
            WDictionary = new Dictionary<int, double>();
            FeatureKDictionary = inputFeatureTemp;
            WeightArray = new double[count+1000];
            WeightArray.Initialize();
            FeatureCount = count;
        }

        public int FeatureCount { get; set; }

        public void Add(KeyValuePair<string, string> input)
        {
            if (FeatureKDictionary.ContainsKey(input.Key))
            {
                var k = FeatureKDictionary[input.Key];
                WDictionary.Add(k, double.Parse(input.Value));

            }
        }

        public int GetFeatureToK(string feature)
        {
            try
            {
                return FeatureKDictionary[feature];
            }
            catch (Exception)
            {
                return -1;
            }
            
            //return FeatureKDictionary.ContainsKey(feature) ? FeatureKDictionary[feature] : -1;
        }

        public double Get(string tag)
        {
            try
            {
                return Get(FeatureKDictionary[tag]);
            }
            catch (Exception)
            {
                return 0;
            }
            //return FeatureKDictionary.ContainsKey(tag) ? Get(FeatureKDictionary[tag]) : 0;
        }

        public double Get(int k)
        {
            try
            {
                return WDictionary[k];
            }
            catch (Exception)
            {
                return 0;
            }
            //return (WDictionary.ContainsKey(k) && !Double.IsNaN(WDictionary[k])) ? WDictionary[k] : 0;
            //return (WDictionary.ContainsKey(k)) ? WDictionary[k] : 0;
        }

        public void AddToKey(string key, double value)
        {
            if (FeatureKDictionary.ContainsKey(key))
            {
                AddToKey(FeatureKDictionary[key], value);
            }
        }

        public void AddToKey(int key, double value)
        {
            if (!WDictionary.ContainsKey(key))
            {
                WDictionary.Add(key, 0);
            }
            WDictionary[key] += value;
        }

        public void SetKey(int key, double value)
        {
            if (!WDictionary.ContainsKey(key))
            {
                WDictionary.Add(key, 0);
            }
            WDictionary[key] = value;            
        }

        //private float Abs(float value)
        //{
        //    if (value < 0)
        //    {
        //        value = -1 * value;
        //    }
        //    return value;
        //}
        //public void NormalizeAllWeights(int normalize = 10)
        //{
        //    var tagDictionary = new Dictionary<string, float>();
        //    foreach (var keyValuePair in WDictionary)
        //    {
        //        var tag = keyValuePair.Key.Substring(0, 
        //            keyValuePair.Key.IndexOf("TAG:", System.StringComparison.Ordinal));
        //        if (tagDictionary.ContainsKey(tag))
        //        {
        //            tagDictionary[tag] += Abs(keyValuePair.Value);
        //        }
        //        else
        //        {
        //            tagDictionary.Add(tag, Abs(keyValuePair.Value));
        //        }
        //    }


        //    List<string> keyList = WDictionary.Keys.ToList();
        //    foreach (var key in keyList)
        //    {
        //        var tag = key.Substring(0,
        //            key.IndexOf("TAG:", StringComparison.Ordinal));

        //        WDictionary[key] = (WDictionary[key] * normalize) /
        //            tagDictionary[tag];
        //    }
        //}
    }
}
