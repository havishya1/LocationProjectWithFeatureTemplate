using System.Globalization;

namespace LocationProjectWithFeatureTemplate
{
    class EvalModel
    {
        public string Evalulate(string keyFile, string devFile, string dumpFile)
        {
            var keyModel = new ReadModel(keyFile);
            var devModel = new ReadModel(devFile);

            var dumpOutputModel = new WriteModel(dumpFile);

            var keyIter = keyModel.ModelIterator().GetEnumerator();
            var devIter = devModel.ModelIterator().GetEnumerator();

            float expected = 0;
            float correct = 0;
            float found = 0;
            float line = 0;

            string dump;
            while (keyIter.MoveNext() && devIter.MoveNext())
            {
                var key = keyIter.Current;
                var dev = devIter.Current;
                line++;
                if (!key.Key.Equals(dev.Key))
                {
                    dump = "line: " + line + " " + key.Key + " doesn't match " + dev.Key+"\r\n";
                    dumpOutputModel.WriteLine(dump);
                    dumpOutputModel.Flush();
                    return dump;
                }

                if (key.Value.Contains("LOCATION"))
                {
                    expected++;
                    if (!dev.Value.Contains("LOCATION")) continue;
                    found++;
                    correct++;
                }
                else if (dev.Value.Contains("LOCATION"))
                {
                    found++;
                }
            }

            dump = "found: " + found + " expected: " + expected + " correct: " + correct +"\r\n";
            dumpOutputModel.WriteLine(dump);
            float precision = correct/found;
            float recall = correct/expected;
            float f1Score = (2*precision*recall)/(precision + recall);
            dump += "precision\t recall \t f1score\t\r\n";
            dumpOutputModel.WriteLine("precision\t recall \t f1score\t");
            dump += precision.ToString(CultureInfo.InvariantCulture) + "\t" +
                    recall.ToString(CultureInfo.InvariantCulture) + "\t" +
                    f1Score.ToString(CultureInfo.InvariantCulture) +"\r\n";
            dumpOutputModel.WriteLine(precision.ToString(CultureInfo.InvariantCulture)+"\t"+
                recall.ToString(CultureInfo.InvariantCulture)+ "\t" +
                f1Score.ToString(CultureInfo.InvariantCulture));
            dumpOutputModel.Flush();
            return dump;
        }
    }
}

