using System;
using System.Collections.Generic;
using System.Linq;

namespace LocationProjectWithFeatureTemplate
{
    class ViterbiForGlobalLinearModel
    {
        public WeightVector WeightVector { get; set; }
        public Tags Tags { get; set; }
        public List<Dictionary<string, double>> Pi { get; set; }
        public List<Dictionary<string, string>> Bp { get; set; }

        public ViterbiForGlobalLinearModel(WeightVector weightVector, Tags tags)
        {
            WeightVector = weightVector;
            Tags = tags;
            Pi = new List<Dictionary<string, double>>();
            Bp = new List<Dictionary<string, string>>();
        }

        public List<string> Decode(List<string> inputSentance, bool debug, out List<string> debugList)
        {
            var outputTags = new string[(inputSentance.Count)];
            var weightedFeatureSum  = new WeightedFeatureSum(WeightVector, inputSentance);
            var init = new Dictionary<string, double> {{"*:*", 0}};
            Pi.Add(init);
            var lastTwo = string.Empty;
            double lastTwoTagsValue = -0xFFFF;
            int k;

            debugList = new List<string>(inputSentance.Count);

            for (k = 0; k < inputSentance.Count; k++ )
            {
                double max = - 0xFFFF;
                Pi.Add(new Dictionary<string, double>());
                Bp.Add(new Dictionary<string, string>());
                foreach (var tagStr in Tags.GetNGramTags(k == 0 ? 1 : 2))
                {
                    // follow algo from notes;
                    var tagsKey = tagStr;
                    double current;
                    if (k > 1)
                    {
                        var split = tagStr.Split(new char[] {':'});
                        foreach (var t in Tags.GetNGramTags(1))
                        {
                            string debugStr;
                            var  newTemp = t+ ":"+ tagsKey;
                            Initialize(k - 1, t + ":" + split[0]);
                            double newWeight = weightedFeatureSum.GetFeatureValue(newTemp, k, debug, out debugStr);
                            current = Pi[k - 1][t + ":" + split[0]] + newWeight;
                            if (current > max)
                            {
                                max = current;
                                outputTags[k] = split[1];
                                if (debug)
                                {
                                    debugList.Insert(k, debugStr);
                                }
                            }   
                            Initialize(k, tagsKey);
                            if (!(current > Pi[k][tagsKey])) continue;
                            Pi[k][tagsKey] = current;
                            Bp[k][tagsKey] = t;
                        }
                    }
                    else
                    {
                        if (k == 0)
                        {
                            tagsKey = "*:" + tagsKey;
                        }
                        var split = tagsKey.Split(new char[]{':'});
                        var newTemp = "*:" + tagsKey;
                        string debugStr;
                        current = weightedFeatureSum.GetFeatureValue(newTemp, k, debug, out debugStr);
                        Initialize(k, tagsKey);
                        if (current > Pi[k][tagsKey])
                        {
                            Pi[k][tagsKey] = current;
                            Bp[k][tagsKey] = "*";
                        }
                        if (current > max)
                        {
                            max = current;
                            outputTags[k] = split[1];
                            if (debug)
                                debugList.Insert(k, debugStr);
                        }
                    }
                    if (k != inputSentance.Count - 1) continue;
                    //var temp = tagsKey + ":STOP";
                    //current = Pi[k][tagsKey] + weightedFeatureSum.GetFeatureValue(temp, k + 1);
                    current = Pi[k][tagsKey];
                    if (!(current >= lastTwoTagsValue)) continue;
                    lastTwo = tagsKey;
                    lastTwoTagsValue = current;
                }
            }
            var n = inputSentance.Count - 1;
            var lastTwoSplit = lastTwo.Split(new char[] {':'});
            if (lastTwoSplit.Count() != 2)
            {
                throw new Exception("count mismatch for lastTwo tags"+ lastTwo);
            }
            if (n-1 >= 0)
                outputTags[n-1] = lastTwoSplit[0];
            outputTags[n] = lastTwoSplit[1];

            //for (k = n - 2; k >= 0; k--)
            //{
            //    outputTags[k] = Bp[k + 2][outputTags[k + 1] + ":" + outputTags[k + 2]];
            //}
            return outputTags.ToList();
        }

        void Initialize(int k, string key)
        {
            if (!Pi[k].ContainsKey(key))
            {
                Pi[k].Add(key, -0xFFFF);
            }

            if (!Bp[k].ContainsKey(key))
            {
                Bp[k].Add(key, "");
            }
        }
    }
}
