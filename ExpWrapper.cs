using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace LocationProjectWithFeatureTemplate
{
    public class ExpWrapper
    {
        public List<double> PowerList;
        public List<double> MultiplyFactor;

        public ExpWrapper()
        {
            PowerList = new List<double>();
            MultiplyFactor = new List<double>();
        }

        public ExpWrapper(double power)
        {
            PowerList = new List<double> {power};
            MultiplyFactor = new List<double>{1};
        }

        public ExpWrapper(ExpWrapper exp)
        {
            PowerList = exp.PowerList.ToList();
            MultiplyFactor = exp.MultiplyFactor.ToList();
        }

        public void XMultiplyFactor(double value)
        {
            for (int i = 0; i < MultiplyFactor.Count; i++)
            {
                MultiplyFactor[i] *= value;
            }
        }

        public void AddPowerWithMultiplier(double p, double multiplier)
        {
            PowerList.Add(p);
            MultiplyFactor.Add(multiplier);
        }

        public void AddPower(double p)
        {
            PowerList.Add(p);
            MultiplyFactor.Add(1);
        }

        public void AddExp(ExpWrapper ex)
        {
            int count = ex.PowerList.Count;
            for (int i = 0; i < count; i++)
            {
                PowerList.Add(ex.PowerList[i]);
                MultiplyFactor.Add(ex.MultiplyFactor[i]);
            }
        }

        public void MultiplyExp(ExpWrapper exp)
        {
            if (exp.PowerList.Count == 0)
            {
                return;
            }
            var count = PowerList.Count;
            for (int i = 0; i < count; i++)
            {
                PowerList[i] *= exp.PowerList[0];
                MultiplyFactor[i] *= exp.MultiplyFactor[0];
                for (int j = 1; j < exp.PowerList.Count; j++)
                {
                    PowerList.Add(PowerList[i] * exp.PowerList[j]);
                    MultiplyFactor.Add(MultiplyFactor[i] * exp.MultiplyFactor[j]);
                }
            }
        }

        public void MultiplyPower(double p)
        {
            for (int i = 0; i < PowerList.Count; i++)
            {
                PowerList[i] += p;
            }
        }

        public void DividePower(int p)
        {
            for (int i = 0; i < PowerList.Count; i++)
            {
                PowerList[i] -= p;
            }
        }

        public void DivideByExp(ExpWrapper exp)
        {
            if (exp.PowerList.Count == 0)
            {
                return;
            }
            var minPower = PowerList[0];
            for (int i = 1; i < PowerList.Count; i++)
            {
                if (minPower > PowerList[i])
                {
                    minPower = PowerList[i];
                }
            }
            var expMinPower = exp.PowerList[0];
            for (int i = 1; i < exp.PowerList.Count; i++)
            {
                if (expMinPower > exp.PowerList[i])
                {
                    expMinPower = exp.PowerList[i];
                }
            }

            double sum = 0;
            for (int i = 0; i < PowerList.Count; i++)
            {
                sum += (Math.Exp(PowerList[i] - minPower) * MultiplyFactor[i]);
            }
            double expSum = 0;

            for (int i = 0; i < exp.PowerList.Count; i++)
            {
                expSum += (Math.Exp(exp.PowerList[i] - expMinPower) * exp.MultiplyFactor[i]);
            }

            minPower -= expMinPower;
            PowerList.Clear();
            MultiplyFactor.Clear();
            
            PowerList.Add(minPower);
            MultiplyFactor.Add(Math.Exp(minPower));
        }

        public double GetFinalOuputDouble()
        {
            if (PowerList.Count == 0)
            {
                return 0;
            }
            var minPower = PowerList[0];
            for (int i = 1; i < PowerList.Count; i++)
            {
                if (minPower > PowerList[i])
                {
                    minPower = PowerList[i];
                }
            }
            double sum = 0;
            for (int i = 0; i < PowerList.Count; i++)
            {
                sum += (Math.Exp(PowerList[i] - minPower) * MultiplyFactor[i]);
            }
            sum *= Math.Exp(minPower);
            return sum;
        }
    }
}
