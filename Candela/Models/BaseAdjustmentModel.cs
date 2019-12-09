using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Candela.Models
{
    public abstract class BaseAdjustmentModel
    {
        protected int v;
        protected int prev_v;
        protected double dv;
        protected bool canExecute;

        public int Get()
        {
            return v;
        }

        public void Set(int value)
        {
            prev_v = v;
            v = value > -100 ? value : prev_v;
            dv = FindDiff();
            canExecute = true;
        }

        public Matrix Execute(Matrix matrix)
        {
            if (!canExecute) return matrix;

            canExecute = false;

            return Calculate(matrix);
        }

        protected abstract Matrix Calculate(Matrix matrix);

        protected abstract double FindDiff();
    }
}
