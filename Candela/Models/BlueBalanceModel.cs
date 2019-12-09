using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Candela.Models
{
    public class BlueBalanceModel : BaseAdjustmentModel
    {
        protected override Matrix Calculate(Matrix matrix)
        {
            return matrix * new Matrix(new double[,]
            {
                { 1.0, .0, .0, .0 },
                { .0, 1.0, .0, .0 },
                { .0, .0, dv, .0 },
                { .0, .0, .0, 1.0 },
            });
        }

        protected override double FindDiff()
        {
            double num = (v + 100) / 100.0;
            double den = (prev_v + 100) / 100.0;
            double diff = num / den;

            return diff;
        }
    }
}
