using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LocationProjectWithFeatureTemplate
{
    public class WeightVector
    {
        public Dictionary<int, double> WDictionary;
        public Dictionary<string, int> FeatureKDictionary;
        public double[] WeightArray;

        public WeightVector()
        {
            WDictionary = new Dictionary<int, double>();
            FeatureKDictionary = new Dictionary<string, int>();
            WeightArray = new double[40000];
            Array.Clear(WeightArray, 0, WeightArray.Length);
            FeatureCount = 0;
            throw new Exception("did you forget ???");
        }

        public WeightVector(Dictionary<string, int> inputFeatureTemp, int count = 0)
        {
            WDictionary = new Dictionary<int, double>();
            FeatureKDictionary = inputFeatureTemp;
            WeightArray = new double[count+1000];
            Array.Clear(WeightArray, 0, WeightArray.Length);
            FeatureCount = count;
        }

        public int FeatureCount { get; set; }

        public WeightVector DeepCopy()
        {
            var copy = (WeightVector)MemberwiseClone();
            copy.WeightArray = new double[WeightArray.Length];
            Array.Copy(WeightArray, copy.WeightArray, WeightArray.Length);
            return copy;
        }

        public void Add(KeyValuePair<string, string> input)
        {
            if (FeatureKDictionary.ContainsKey(input.Key))
            {
                var k = FeatureKDictionary[input.Key];
                if (k >= FeatureCount)
                {
                    FeatureCount = k+1;
                }
                if (k > WeightArray.Length)
                {
                    Array.Resize(ref WeightArray, WeightArray.Length+1000);
                }
                WeightArray[k] = double.Parse(input.Value);

            }
        }

        public int GetFeatureToK(string feature)
        {
            try
            {
                return FeatureKDictionary[feature];
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public double Get(string tag)
        {
            try
            {
                return Get(FeatureKDictionary[tag]);
            }
            catch (Exception)
            {
                return 0;
            }
            
        }

        public double Get(int k)
        {
            if (k < FeatureCount)
                return WeightArray[k];
            else
            {
                return 0;
            }
            
        }

        public void AddToKey(string key, double value)
        {
            if (FeatureKDictionary.ContainsKey(key))
            {
                AddToKey(FeatureKDictionary[key], value);
            }
        }

        public void AddToKey(int key, double value)
        {
            if (key >= FeatureCount)
            {
                FeatureCount = key+1;
            }
            if (key > WeightArray.Length)
            {
                Array.Resize(ref WeightArray, WeightArray.Length + 1000);
            }
            WeightArray[key] += value;
            if (double.IsInfinity(WeightArray[key]))
            {
                WeightArray[key] = double.MaxValue;
            }
        }

        public void SetKey(int key, double value)
        {
            lock (this)
            {
                if (key >= FeatureCount)
                {
                    FeatureCount = key + 1;
                }
                if (key > WeightArray.Length)
                {
                    Array.Resize(ref WeightArray, WeightArray.Length + 1000);
                }

                if (double.IsNaN(value) || double.IsInfinity(value) || double.IsNegativeInfinity(value))
                {
                    Console.WriteLine("weightvector setting NAN k:" + key + " weight: " + Get(key));
                    value = double.MaxValue;
                }
                WeightArray[key] = value;
            }         
        }

        public void DividebyNum(int number)
        {
            for (int i = 0; i < WeightArray.Length; i++)
            {
                WeightArray[i] /= number;
            }
        }

        public void AddWeightVector(WeightVector weightVector)
        {
            for (int i = 0; i < weightVector.WeightArray.Length; i++)
            {
                WeightArray[i] += weightVector.WeightArray[i];
            }
        }

        public void ResetAllToZero()
        {
            Array.Clear(WeightArray, 0, WeightArray.Length);
        }
    }
}
