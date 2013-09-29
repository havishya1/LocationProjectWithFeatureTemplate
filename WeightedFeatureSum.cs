using System;
using System.Collections.Generic;
using System.Globalization;

namespace LocationProjectWithFeatureTemplate
{
    class WeightedFeatureSum
    {
        private readonly List<string> _sentence;
        private readonly bool _crf;
        public WeightVector WeightVector { get; set; }

        public WeightedFeatureSum(WeightVector weightVector, List<string> sentence, bool crf = false)
        {
            _sentence = sentence;
            _crf = crf;
            WeightVector = weightVector;
        }

        public double GetFeatureValue(string t2, string t1, string t, int pos)
        {
            string debugStr;
            return GetFeatureValue(t2, t1, t, pos, false, out debugStr);
        }

        public double GetFeatureValue(string t2, string t1, string t, int pos, bool debug, out string debugStr)
        {
            var features = new Features(t2, t1, t, _sentence, pos);
            double sum = 0;
            debugStr = "@@";
            foreach (var feature in features.GetFeatures())
            {
                var weight = WeightVector.Get(feature);
                sum += weight;
                if (debug)
                {
                    debugStr += feature + "#" + weight.ToString(CultureInfo.InvariantCulture) + "%%";
                }
            }
            debugStr += "@@";
            if (_crf)
            {
                return Math.Exp(sum);
            }
            return sum;
        }

        internal double GetFeatureValue(string temp, int k, bool debug, out string debugStr)
        {
            var tags = temp.Split(new char[] {':'});
            if (tags.Length != 3)
            {
                throw new Exception(temp +" doesn't contain 3 tags");
            }
            return GetFeatureValue(tags[0], tags[1], tags[2], k, debug, out debugStr);
        }

        public double GetAllFeatureK(List<string> tags, int k, List<string> line)
        {
            double sum = 0;
            for (var i = 0; i < tags.Count; i++)
            {
                var prevTag = "*";
                if (i > 0)
                {
                    prevTag = tags[i - 1];
                }
                sum += GetFeatureK(prevTag, tags[i], i, k, line);
            }
            return sum;
        }

        public double GetFeatureK(string prevTag, string tag, int pos, int k, List<string> line)
        {
            var features = new Features("*", prevTag, tag, line, pos);
            foreach (var feature in features.GetFeatures())
            {
                if (WeightVector.GetFeatureToK(feature) == k)
                {
                    return Math.Exp(WeightVector.Get(k));
                }
            }
            return 0;
        }
    }

}
