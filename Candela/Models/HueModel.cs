using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Candela.Models
{
    public class HueModel : BaseAdjustmentModel
    {
        protected override Matrix Calculate(Matrix matrix)
        {
            double theta = dv * Math.PI / 180.0;
            double cos = Math.Cos(theta);
            double sin = Math.Sin(theta);

            return matrix * new Matrix(new double[,]
            {
                { 0.213 + 0.787 * cos - 0.213 * sin, 0.213 - 0.213 * cos + 0.143 * sin, 0.213 - 0.213 * cos - 0.787 * sin, .0 },
                { 0.715 - 0.715 * cos - 0.715 * sin, 0.715 + 0.285 * cos + 0.140 * sin, 0.715 - 0.715 * cos + 0.715 * sin, .0 },
                { 0.072 - 0.072 * cos + 0.928 * sin, 0.072 - 0.072 * cos - 0.283 * sin, 0.072 + 0.928 * cos + 0.072 * sin, .0 },
                { .0, .0, .0, 1.0 }
            });
        }

        protected override double FindDiff()
        {
            return Convert.ToDouble(v - prev_v);
        }
    }
}
