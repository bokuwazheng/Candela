using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Candela.Models;

namespace Candela.ViewModels
{
    class AlphaAdjustmentViewModel : BaseAdjustmentViewModel
    {
        public AlphaAdjustmentViewModel(ImageWorker iw) : base(iw) { }

        protected override void Update(PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Brightness"))
            {
                _iw.UpdateAlphaMatrix(_bm);
            }

            if (e.PropertyName.Equals("Saturation"))
            {
                _iw.UpdateAlphaMatrix(_sm);
            }

            if (e.PropertyName.Equals("Hue"))
            {
                _iw.UpdateAlphaMatrix(_hm);
            }

            if (e.PropertyName.Equals("Gamma"))
            {
                _iw.UpdateAlphaGamma(_gamma);
            }

            if (e.PropertyName.Equals("RedBalance"))
            {
                _iw.UpdateAlphaMatrix(_rbm);
            }

            if (e.PropertyName.Equals("GreenBalance"))
            {
                _iw.UpdateAlphaMatrix(_gbm);
            }

            if (e.PropertyName.Equals("BlueBalance"))
            {
                _iw.UpdateAlphaMatrix(_bbm);
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
