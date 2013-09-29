using System;
using System.Collections.Generic;
using System.IO;

namespace LocationProjectWithFeatureTemplate
{
    class WriteModel
    {
        private readonly StreamWriter _writer;

        public WriteModel() : this(null)
        {
        }

        public WriteModel(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            _writer = new StreamWriter(fileName);
        }

        public void WriteLine(string line)
        {
            _writer.WriteLine(line);
        }

        public void Flush()
        {
            _writer.Flush();
            _writer.Close();
        }

        public void WriteDataWithTag(List<string> line, List<string> outputTags)
        {
            for (int i = 0; i < line.Count; i++)
            {
                string dump = line[i] + " " + outputTags[i];
                WriteLine(dump);
            }
            WriteLine("");
        }

        public void WriteDataWithTagDebug(List<string> line, List<string> outputTags, List<string> debugList)
        {
            for (int i = 0; i < line.Count; i++)
            {
                string dump = line[i] + " " + outputTags[i] +" "+ debugList[i];
                WriteLine(dump);
            }
            WriteLine("");
        }
    }
}
