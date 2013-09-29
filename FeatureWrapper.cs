using System.Collections.Generic;

namespace LocationProjectWithFeatureTemplate
{
    class FeatureWrapper
    {
        private readonly List<string> _tags;
        private readonly List<string> _sentence;
        private int _pos;

        public FeatureWrapper(List<string> tags, List<string> sentence)
        {
            _tags = tags;
            _sentence = sentence;
            _pos = 0;
        }

        public IEnumerable<KeyValuePair<string, string>> NextFeature()
        {
            string t2 = "*", t1 = "*";
            for (; _pos < _sentence.Count; _pos++)
            {
                var t = _tags[_pos];
                if (_pos > 1)
                {
                    t2 = _tags[_pos - 2];
                    t1 = _tags[_pos - 1];
                }
                else if (_pos == 1)
                {
                    t1 = _tags[_pos - 1];
                }

                var features = new Features(t2, t1, t, _sentence, _pos);
                foreach (var f in features.GetFeatures())
                {
                    var pair = new KeyValuePair<string, string>(t, f);
                    yield return pair;
                }
            }
        }
    }
}
