using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Candela.Models
{
    public class SaturationModel : BaseAdjustmentModel
    {
        protected override Matrix Calculate(Matrix matrix)
        {
            var rwgt = .3086;
            var gwgt = .6094;
            var bwgt = .0820;

            var a = (1.0 - dv) * rwgt + dv;
            var b = (1.0 - dv) * rwgt;
            var c = (1.0 - dv) * rwgt;
            var d = (1.0 - dv) * gwgt;
            var e = (1.0 - dv) * gwgt + dv;
            var f = (1.0 - dv) * gwgt;
            var g = (1.0 - dv) * bwgt;
            var h = (1.0 - dv) * bwgt;
            var i = (1.0 - dv) * bwgt + dv;

            return matrix * new Matrix(new double[,]
            {
                { a, b, c, .0 },
                { d, e, f, .0 },
                { g, h, i, .0 },
                { .0, .0, .0, 1.0 }
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
