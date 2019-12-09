using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Candela
{
    public delegate void AdjustmentHandler(object sender, long ms);

    public interface IAdjustmentPanel
    {
        int Brightness { get; set; }
        double Gamma { get; set; }
        int RedBalance { get; set; }
        int GreenBalance { get; set; }
        int BlueBalance { get; set; }
        int Hue { get; set; }
        int Saturation { get; set; }
        bool LTA { get; set; }
        bool PMA { get; set; }

        event AdjustmentHandler AdjustmentsChanged;
    }
}
