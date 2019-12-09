using Prism.Commands;
using Candela.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

namespace Candela.ViewModels
{
    class RgbAdjustmentViewModel : BaseAdjustmentViewModel
    {
        public RgbAdjustmentViewModel(ImageWorker iw) : base(iw) { }

        protected override void Update(PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Brightness"))
            {
                _iw.UpdateRgbMatrix(_bm);
            }

            if (e.PropertyName.Equals("Saturation"))
            {
                _iw.UpdateRgbMatrix(_sm);
            }

            if (e.PropertyName.Equals("Hue"))
            {
                _iw.UpdateRgbMatrix(_hm);
            }

            if (e.PropertyName.Equals("Gamma"))
            {
                _iw.UpdateRgbGamma(_gamma);
            }

            if (e.PropertyName.Equals("RedBalance"))
            {
                _iw.UpdateRgbMatrix(_rbm);
            }

            if (e.PropertyName.Equals("GreenBalance"))
            {
                _iw.UpdateRgbMatrix(_gbm);
            }

            if (e.PropertyName.Equals("BlueBalance"))
            {
                _iw.UpdateRgbMatrix(_bbm);
            }

            if (e.PropertyName.Equals("LTA"))
            {
                _iw.UpdateLTA(LTA);
            }

            if (e.PropertyName.Equals("PMA"))
            {
                _iw.UpdatePMA(PMA);
            }
        }

    }
}
