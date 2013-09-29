using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocationProjectWithFeatureTemplate
{
    public class FeatureCache
    {
        private readonly List<string> _tags;
        List<List<string>> Sentences { get; set; }
        //Dictionary<int,Dictionary<int, string>> Cache { get; set; }
        public List<List<HashSet<string>>> Cache { get; set; }
        private readonly Dictionary<string, int> _featureTokDictionary;

        public FeatureCache(List<List<string>> inputList, List<string> tags,
            Dictionary<string, int> dict)
        {
            _tags = tags;
            Sentences = inputList;
            _featureTokDictionary = dict;
            Cache = new List<List<HashSet<string>>>();
            int lineIndex = 0;
            foreach (var sentence in Sentences)
            {
                Cache.Add(new List<HashSet<string>>());
                foreach (var word in sentence)
                {
                    Cache[lineIndex].Add(new HashSet<string>());
                }
                lineIndex++;
            }
        }

        public void CreateCache()
        {
            Console.WriteLine(DateTime.Now + "creating Cache begin");
            var ngramTags = new Tags(_tags);
            foreach (var ngramTag in ngramTags.GetNGramTags(2))
            {
                string[] split = ngramTag.Split(new[] { ':' });
                for (var lineIndex = 0; lineIndex < Sentences.Count; lineIndex++)
                {
                    for (var pos = 0; pos < Sentences[lineIndex].Count; pos++)
                    {
                        if (pos == 0)
                        {
                            StoreFeature("*", split[1], pos, lineIndex);
                        }
                        else
                        {
                            StoreFeature(split[0], split[1], pos, lineIndex);   
                        }
                    }
                }
            }
            Console.WriteLine(DateTime.Now + "creating Cache end");
        }

        public bool Contains(string prevTag, string tag, int k, int pos, int lineIndex)
        {
            var key = prevTag + "@#" + tag + "@#" + k.ToString(CultureInfo.InvariantCulture);
            return Cache[lineIndex][pos].Contains(key);
        }

        private void StoreFeature(string prevTag, string tag, int pos, int lineIndex)
        {
            var features = new Features("*", prevTag, tag, Sentences[lineIndex], pos);
            foreach (var feature in features.GetFeatures())
            {
                int k = _featureTokDictionary[feature];
                Cache[lineIndex][pos].Add(string.Format("{0}@#{1}@#{2}", prevTag, tag,
                    k.ToString(CultureInfo.InvariantCulture)));
            }
        }
    }
}
