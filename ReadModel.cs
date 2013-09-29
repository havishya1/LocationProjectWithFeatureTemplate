using System;
using System.Collections.Generic;
using System.IO;

namespace LocationProjectWithFeatureTemplate
{
    class ReadModel
    {
        readonly StreamReader _stream;
        public ReadModel(string file)
        {
            string fileName = file;

            try
            {
                _stream = new StreamReader(fileName);
            }
            catch (Exception)
            {
                Console.WriteLine(fileName + "is not valid");
                throw;
            }
        }

        public Dictionary<string, int> GetFeatureToKdDictionary()
        {
            var dict = new Dictionary<string, int>();
            string line;
            while ((line = _stream.ReadLine()) != null)
            {
                var splits = line.Split(new[] {'\t'});
                dict.Add(splits[0], int.Parse(splits[1]));
            }
            return dict;
        }

        public IEnumerable<string> GetNextLine()
        {
            string line;
            while ((line = _stream.ReadLine()) != null)
            {
                yield return line.Trim();
            }
        }

        public IEnumerable<KeyValuePair<string, string>> ModelIterator()
        {
            string line;
            while ((line = _stream.ReadLine()) != null)
            {
                line = line.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                string[] str = line.Split(new[]{' '});
                if (str.Length != 3 && str.Length != 2)
                {
                    Console.WriteLine(line + " doesn't have 2 words");
                    throw new Exception();
                }
                else if (str.Length == 3)
                {
                    yield return new KeyValuePair<string, string>(str[1], str[2]);
                }
                else
                {
                    yield return new KeyValuePair<string, string>(str[0], str[1]);
                }
            }
        }
    }
}
