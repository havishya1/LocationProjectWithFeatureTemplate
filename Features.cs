using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LocationProjectWithFeatureTemplate
{
    class Features
    {
        public string T2 { get; set; }
        public string T1 { get; set; }
        public string T { get; set; }
        public List<string> Sentence { get; set; }
        public int Pos { get; set; }
        private readonly List<FeatureEnums> _featureList;

        enum FeatureEnums
        {
           TRIGRAMTags,
           BiWordTag,
           CurWordPrevTag,
           BiGram,
           Suff1Tag,
           Suff2Tag,
           Suff3Tag,
           AlphaNumTag,
           AllCapsTag,
           StartWithCapTag,
           SingleCharTag,
           StartsWithNum,
           AllNumTag,
           ContainsSymbolsTag,
           FirstStringTag,
           PrevWordCurrentTag,
           NextWordCurrentTag,
        }

        public Features(string t2, string t1, string t, List<string> sentence, int pos)
        {
            T2 = t2;
            T1 = t1;
            T = t;
            Sentence = sentence;
            Pos = pos;
            _featureList = new List<FeatureEnums>
            {
                //FeatureEnums.TRIGRAMTags,
                //FeatureEnums.BiGram,
                //FeatureEnums.CurWordPrevTag,
                FeatureEnums.BiWordTag,
                FeatureEnums.Suff2Tag,
                FeatureEnums.Suff3Tag,
                FeatureEnums.SingleCharTag,
                FeatureEnums.AllCapsTag,
                FeatureEnums.AlphaNumTag,
                FeatureEnums.StartWithCapTag,
                FeatureEnums.StartsWithNum,
                FeatureEnums.AllNumTag,
                FeatureEnums.ContainsSymbolsTag,
                FeatureEnums.FirstStringTag,
                FeatureEnums.PrevWordCurrentTag,
                FeatureEnums.NextWordCurrentTag
            };
        }

        public IEnumerable<string>  GetFeatures()
        {
            foreach (var feature in _featureList)
            {
                switch (feature)
                {   
                    //case FeatureEnums.TRIGRAMTags:
                    //    yield return GetTriGramFeature();
                    //    break;
                    case FeatureEnums.BiWordTag:
                        if (Pos < Sentence.Count)
                        {
                            yield return GetTagFeature();
                        }
                        break;
                    case FeatureEnums.CurWordPrevTag:
                    {
                        if (Pos < Sentence.Count)
                        {
                            yield return GetCurrentWordPrevTag();
                        }
                        break;                        
                    }

                    case FeatureEnums.BiGram:
                    {
                        yield return GetBiGram();
                        break;
                    }

                    case FeatureEnums.Suff2Tag:
                    {
                        string tag = GetSuffixTag(2);
                        if (!string.IsNullOrEmpty(tag))
                        {
                            yield return tag;
                        }
                        break;
                    }

                    case FeatureEnums.Suff1Tag:
                    {
                        string tag = GetSuffixTag(1);
                        if (!string.IsNullOrEmpty(tag))
                        {
                            yield return tag;
                        }
                        break;
                    }

                    case FeatureEnums.Suff3Tag:
                    {
                        string tag = GetSuffixTag(3);
                        if (!string.IsNullOrEmpty(tag))
                        {
                            yield return tag;
                        }
                        break;
                    }
                    case FeatureEnums.SingleCharTag:
                    {
                        string tag = GetOneLength();
                        if (!string.IsNullOrEmpty(tag))
                        {
                            yield return tag;
                        }
                        break;
                    }
                    case FeatureEnums.AllCapsTag:
                    {
                        string tag = GetAllCapsTag();
                        if (!string.IsNullOrEmpty(tag))
                        {
                            yield return tag;
                        }
                        break;
                    }
                    case FeatureEnums.AllNumTag:
                    {
                        string tag = GetNumTag();
                        if (!string.IsNullOrEmpty(tag))
                        {
                            yield return tag;
                        }
                        break;
                    }

                    case FeatureEnums.AlphaNumTag:
                    {
                        string tag = GetAlphaNumTag();
                        if (!string.IsNullOrEmpty(tag))
                        {
                            yield return tag;
                        }
                        break;
                    }

                    case FeatureEnums.StartWithCapTag:
                    {
                        string tag = GetStartWithCap();
                        if (!string.IsNullOrEmpty(tag))
                        {
                            yield return tag;
                        }
                        break;
                    }

                    case FeatureEnums.StartsWithNum:
                    {
                        string tag = GetStartWithNum();
                        if (!string.IsNullOrEmpty(tag))
                        {
                            yield return tag;
                        }
                        break;
                    }

                    case FeatureEnums.PrevWordCurrentTag:
                    {
                        string tag = GetPrevWordCurrentTag();
                        if (!string.IsNullOrEmpty(tag))
                        {
                            yield return tag;
                        }
                        break;
                    }
                    case FeatureEnums.NextWordCurrentTag:
                    {
                        string tag = GetNextWordCurrentTag();
                        if (!string.IsNullOrEmpty(tag))
                        {
                            yield return tag;
                        }
                        break;
                    }
                }
            }
        }

        //private string GetTriGramFeature()
        //{
        //    return "TRIGRAMTAG:" + T2 +":"+ T1 +":"+ T;
        //}

        private string GetTagFeature()
        {
            return "TAG:" + Sentence[Pos] + ":" + T;
        }

        private string GetCurrentWordPrevTag()
        {
            return "CurWPrevTAG:" + Sentence[Pos] + ":" + T1;
        }

        private string GetBiGram()
        {
            return "BIGRAMTAG:"+ T1 +":" + T;
        }

        private string GetSuffixTag(int suffix)
        {
            if (Pos >= Sentence.Count) return null;
            var length = Sentence[Pos].Length;
            if (length - suffix >= 0)
            {
                return "SUFF" + suffix.ToString(CultureInfo.InvariantCulture) +
                       "TAG:" + Sentence[Pos].Substring(length - suffix) + ":" + T;
            }
            return null;
        }

        public string GetAllCapsTag()
        {
            if (Pos >= Sentence.Count)
            {
                return null;
            }
            if (IsAllUpper(Sentence[Pos]))
            {
                return "ALLUPPERTAG:1:" +T;
            }
            else
            {
                return "ALLUPPERTAG:0:" + T;
            }
        }

        public string GetAlphaNumTag()
        {
            if (Pos >= Sentence.Count)
            {
                return null;
            }
            var alpha = false;
            var num = false;
            foreach (var ch in Sentence[Pos])
            {
                if (!alpha)
                {
                    alpha = char.IsLetter(ch);
                }
                if (!num)
                {
                    num = char.IsNumber(ch);
                }
                if (alpha && num)
                {
                    return "ALPHANUMTAG:1:" + T;
                }
            }
            return "ALPHANUMTAG:O:" + T;
        }

        public string GetStartWithCap()
        {
            if (Pos >= Sentence.Count || Pos == 0)
            {
                return null;
            }
            if (char.IsUpper(Sentence[Pos][0]) && char.IsLetter(Sentence[Pos][0]))
            {
                return "FIRSTCHARUPPERTAG:1:" + T;
            }
            return "FIRSTCHARUPPERTAG:0:" + T;
        }

        public string GetStartWithNum()
        {
            if (Pos >= Sentence.Count)
            {
                return null;
            }
            if (char.IsNumber(Sentence[Pos][0]))
            {
                return "FIRSTCHARNUMTAG:1:" + T;
            }
            return "FIRSTCHARNUMTAG:0:" + T;
        }

        public string GetNumTag()
        {
            if (Pos >= Sentence.Count)
            {
                return null;
            }
            var num = Sentence[Pos].All(char.IsNumber);
            if (num)
            {
                return "NUMTAG:1:" + T;
            }
            return "NUMTAG:0:" + T;
        }

        public string GetOneLength()
        {
            if (Pos >= Sentence.Count)
            {
                return null;
            }
            if (Sentence[Pos].Length == 1)
            {
                return "ONELENGTHTAG:1:" + T;
            }
            return "ONELENGTHTAG:0:" + T;
        }

        public string GetSymbolTag()
        {
            if (Pos > Sentence[Pos].Count())
            {
                return null;
            }
            if (IsAnySymbol(Sentence[Pos]))
            {
                return "ANYSYMBOLTAG:1:" + T;
            }
            return "ANYSYMBOLTAG:0:" + T;
        }

        private string GetPrevWordCurrentTag()
        {
            if (Pos > 1)
            {
                return "PREVWORDCURRTAG:" + Sentence[Pos - 1] + ":" + T;
            }
            return null;
        }

        public string GetNextWordCurrentTag()
        {
            if (Pos >= Sentence.Count - 1)
                return null;
            return "NEXTWORDCURRTAG:" + Sentence[Pos + 1] + ":" + T;
        }

        public string GetFirstStringTag()
        {
            if (Pos == 0)
            {
                return "FIRSTSTRTAG:1:" + T;
            }
            return "FIRSTSTRTAG:0:" + T;
        }

        bool IsAnySymbol(string input)
        {
            return input.Any(t => !char.IsLetterOrDigit(t));
        }

        bool IsAllUpper(string input)
        {
            return input.All(t => Char.IsLetter(t) && Char.IsUpper(t));
        }

        public static float GetWeight(string feature)
        {
            if (feature.Contains("TRIGRAM:"))
            {
                return (float) .0001;
                //return (float)1;
            }
            else if (feature.Contains("TAG:"))
            {
                return 1;
            }
            return 1;
        }

    }
}
