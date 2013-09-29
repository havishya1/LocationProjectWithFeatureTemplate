using System;
using System.Collections.Generic;

namespace LocationProjectWithFeatureTemplate
{
    class Tags
    {
        private readonly List<string> _tags;
        public Tags(List<string> tags)
        {
            _tags = tags;
        }

        public IEnumerable<string> GetNGramTags(int n, string temp =null)
        {   
            foreach (var tag in _tags)
            {
                string output = tag;
                if (!string.IsNullOrEmpty(temp))
                {
                    output = temp + ":" + output;
                }
                if (n == 1)
                {
                    yield return output;
                }
                else
                {
                    foreach (var newTag in GetNGramTags(n - 1, output))
                    {
                        yield return newTag;
                    }
                }
            }
        }

        internal void Dump(int n)
        {
            foreach (var nGramTagFeature in GetNGramTags(n))
            {
                Console.WriteLine(nGramTagFeature);
            }
            Console.ReadLine();
        }
    }
}
