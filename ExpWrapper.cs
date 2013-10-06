using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocationProjectWithFeatureTemplate
{
    class ExpWrapper
    {
        private List<int> powerList;

        public ExpWrapper()
        {
            powerList = new List<int>();
        }

        public void AddPower(int p)
        {
            powerList.Add(p);
        }

        public void MultiplyPower(int p)
        {
            for (int i = 0; i < powerList.Count; i++)
            {
                powerList[i] += p;
            }
        }

        public void DividePower(int p)
        {
            for (int i = 0; i < powerList.Count; i++)
            {
                powerList[i] -= p;
            }
        }

        public double GetFinalOuputDouble()
        {
            int minPower = powerList[0];
            for (int i = 1; i < powerList.Count; i++)
            {
                if (minPower > powerList[i])
                {
                    minPower = powerList[i];
                }
            }
            double sum = 0;
            for (int i = 0; i < powerList.Count; i++)
            {
                sum += Math.Exp(powerList[i] - minPower);
            }
            sum *= Math.Exp(minPower);
            return sum;
        }
    }
}
