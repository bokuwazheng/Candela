using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Candela.Models
{
    public class RenderQueueItemModel : ViewModels.BaseViewModel
    {
        // Backing fields
        private bool _colorCorrection;
        private bool _premultiplyAlpha;
        private bool _luminanceToAlpha;

        // Properties
        public string FileName { get; set; }
        public string Origin { get; set; }
        public string RootFolder { get; set; }
        public bool ColorCorrection
        {
            get
            {
                return _colorCorrection;
            }
            set
            {
                if (value != _colorCorrection)
                {
                    _colorCorrection = value;
                    RaisePropertyChanged("ColorCorrection");
                }
            }
        }
        public bool PremultiplyAlpha
        {
            get
            {
                return _premultiplyAlpha;
            }
            set
            {
                if (value != _premultiplyAlpha)
                {
                    _premultiplyAlpha = value;
                    RaisePropertyChanged("PremultiplyAlpha");
                }
            }
        }
        public bool LuminanceToAlpha
        {
            get
            {
                return _luminanceToAlpha;
            }
            set
            {
                if (value != _luminanceToAlpha)
                {
                    _luminanceToAlpha = value;
                    RaisePropertyChanged("LuminanceToAlpha");
                }
            }
        }
    }
}
