using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LocationProjectWithFeatureTemplate
{
    class ParseNEWSWIRETrainingData
    {

        public void Parse(string input, string output)
        {
            var readModel= new ReadModel(input);
            var writeModel = new WriteModel(output);
            var writeDevModel = new WriteModel(output+".dev");
            //var tempWrite = new WriteModel(output + "tempWrite");
            var temp = new List<string>();

            foreach (var line in readModel.GetNextLine())
            {
                var newLine = RemoveTags(line);
                newLine = ReplaceTags(newLine);
                newLine = RemoveAllTags(newLine);
                if (string.IsNullOrEmpty(newLine)) continue;
                
                //tempWrite.WriteLine(newLine);
                var split = newLine.Split(new char[] {' '});
                temp.AddRange(split.ToList());
                //temp.Add("##NEWLINE##");
            }
            //tempWrite.Flush();
            bool location = false;
            var lastStr = string.Empty;

            foreach (var tempStr in temp)
            {
                var str = tempStr.Trim();
                if (string.IsNullOrEmpty(str))
                {
                    lastStr = "";
                    continue;
                }

                //if (str.Equals("##NEWLINE##"))
                {
                    if (!location && lastStr.EndsWith(".") && !IsSalutationAbbr(lastStr))
                    {
                        lastStr = string.Empty;
                        writeModel.WriteLine("");
                        writeDevModel.WriteLine("");
                        continue;
                    }
                }
                if (location)
                {
                    if (str.Equals("##ENDTAG##"))
                    {
                        location = false;
                        lastStr = "";
                        continue;
                    }
                    writeModel.WriteLine(str + " " + "LOCATION");
                    writeDevModel.WriteLine(str);
                    lastStr = str;
                    continue;
                }
                if (str.Equals("##LOCATIONSTARTTAG##"))
                {
                    lastStr = "";
                    location = true;
                    continue;
                }
                if (str.Equals("##ENDTAG##"))
                {
                    lastStr = "";
                    continue;
                }
                writeModel.WriteLine(str + " "+ "OTHER");
                writeDevModel.WriteLine(str);
                lastStr = str;
            }
            writeModel.Flush();
            writeDevModel.Flush();
        }

        private bool IsSalutationAbbr(string lastStr)
        {
            if (char.IsUpper(lastStr[0]) &&
                lastStr.Length <= 4)
            {
                if (lastStr.Length >= 3)
                {
                    return char.IsLower(lastStr[lastStr.Length - 2]);
                }
            }
            return false;
        }

        private static string ReplaceTags(string line)
        {
            line = line.Trim();
            line = Regex.Replace(line, @"\s+", " ");

            line = line.Replace("<e_enamex>", " ##ENDTAG## ");
            line = line.Replace("<b_enamex type=\"LOCATION\">", " ##LOCATIONSTARTTAG## ");
            line = line.Replace("<b_enamex type=\"ORGANIZATION\">", " ##LOCATIONSTARTTAG## ");
            if (line.Contains("<b_enamex type="))
            {
                line = Regex.Replace(line, @"<b_enamex type=.+?>", " ");
            }
            line = Regex.Replace(line, @"\s+", " ");
            return line;
        }

        private string RemoveAllTags(string str)
        {
            str = Regex.Replace(str, @"<.*?>", "");
            str = str.Replace("''", "");
            str = str.Replace("``", "");
            return str;
        }

        private string RemoveTags(string line)
        {
            if (line.Contains("<DOC>") ||
                line.Contains("<DOCNO>") ||
                line.Contains("<DOCTYPE>") ||
                line.Contains("</DOC>") ||
                line.Contains("DATE_TIME") ||
                line.Contains("<BODY>") ||
                line.Contains("<HEADLINE>") ||
                line.Contains("/<HEADLINE>") ||
                line.Contains("<TEXT>") ||
                line.Contains("</TEXT>")
                )
                return string.Empty;
            
                return line;
        }
    }
}
