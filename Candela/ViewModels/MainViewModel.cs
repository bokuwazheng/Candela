using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;

namespace Candela.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        public string _selectedView = ViewOptions.RGBA;
        public ImageWorker _iw;
        private RgbAdjustmentViewModel _rgbAVM;
        private AlphaAdjustmentViewModel _alphaAVM;
        private List<IAdjustmentPanel> _adjustmentPanels = new List<IAdjustmentPanel>();
        private IAdjustmentPanel _currentAdjustmentPanel;

        public string BGColor { get; set; } = "#FFE6E6E6";
        public string BGImagePath { get; private set; }
        public string RenderTime { get; private set; }
        public Image DisplayedImage
        {
            get
            {
                if (_selectedView == ViewOptions.Source) return _iw?.GetSource();
                if (_selectedView == ViewOptions.RGB) return _iw?.GetRgb();
                if (_selectedView == ViewOptions.Alpha) return _iw?.GetAlpha();
                if (_selectedView == ViewOptions.RGBA) return _iw?.GetRendered();
                return null;
            }
        }
        public string DisplayedPanel
        {
            get
            {
                if (CurrentAdjustmentPanel == null) return "null";
                if (CurrentAdjustmentPanel.GetType() == typeof(RgbAdjustmentViewModel)) return "RgbAdjPanel";
                if (CurrentAdjustmentPanel.GetType() == typeof(AlphaAdjustmentViewModel)) return "AlphaAdjPanel";
                return null;
            }
        }
        public List<string> Views => ViewOptions.AsList();
        public string SelectedView
        {
            get => _selectedView;
            set
            {
                if (value != _selectedView)
                {
                    _selectedView = value;
                    RaisePropertyChanged("DisplayedImage");
                }
            }
        }
        public bool LumToAlpha
        {
            get => _adjustmentPanels.Count == 0 ? false : _adjustmentPanels[0].LTA;
            set
            {
                if (value != _adjustmentPanels[0].LTA)
                {
                    _adjustmentPanels[0].LTA = value;
                }
            }
        }
        public bool PremulAlpha
        {
            get => _adjustmentPanels.Count == 0 ? false : _adjustmentPanels[0].PMA;
            set
            {
                if (value != _adjustmentPanels[0].PMA)
                {
                    _adjustmentPanels[0].PMA = value;
                }
            }
        }
        public IAdjustmentPanel CurrentAdjustmentPanel
        {
            get => _currentAdjustmentPanel;
            set
            {
                if (_currentAdjustmentPanel != value)
                {
                    _currentAdjustmentPanel = value;
                    RaisePropertyChanged("CurrentAdjustmentPanel");
                }
            }
        }
        public BaseViewModel BatchPanel { get; private set; }

        private void CreateAdjustmentPanels(ImageWorker iw)
        {
            if (_adjustmentPanels.Count != 0)
            {
                foreach (IAdjustmentPanel ap in _adjustmentPanels)
                {
                    ap.AdjustmentsChanged -= _OnAdjustmentsChanged;
                }
                CurrentAdjustmentPanel = null;
                _adjustmentPanels.Clear();
            }

            _rgbAVM = new RgbAdjustmentViewModel(iw);
            _alphaAVM = new AlphaAdjustmentViewModel(iw);
            _rgbAVM.AdjustmentsChanged += _OnAdjustmentsChanged;
            _alphaAVM.AdjustmentsChanged += _OnAdjustmentsChanged;

            _adjustmentPanels.Add(_rgbAVM);
            _adjustmentPanels.Add(_alphaAVM);

            CurrentAdjustmentPanel = _adjustmentPanels[0];
            RaisePropertyChanged("CurrentAdjustmentPanel");
            RaisePropertyChanged("DisplayedPanel");
            RaisePropertyChanged("LumToAlpha");
            RaisePropertyChanged("PremulAlpha");
        }

        private void _OnAdjustmentsChanged(object sender, long ms)
        {
            RenderTime = ms.ToString() + " ms";
            RaisePropertyChanged("DisplayedImage");
            RaisePropertyChanged("RenderTime");
        }

        public MainViewModel()
        {
            _iw = new ImageWorker();
        }

        // Commands
        public DelegateCommand<string> ChangeAdjustmentPanelCommand => new DelegateCommand<string>(s =>
        {
            CurrentAdjustmentPanel = _adjustmentPanels[int.Parse(s)];
            RaisePropertyChanged("DisplayedPanel");
        });

        public DelegateCommand UploadCommand => new DelegateCommand(() =>
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.Multiselect = false;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    _iw = new ImageWorker(Image.FromFile(dialog.FileName));
                    RaisePropertyChanged("DisplayedImage");

                    CreateAdjustmentPanels(_iw);
                }
            }
        });

        public DelegateCommand OpenBatchPanelCommand => new DelegateCommand(() =>
        {
            if (BatchPanel == null)
            {
                BatchPanel = new BatchViewModel(_iw);
            }
            else
            {
                BatchPanel = null;
            }

            RaisePropertyChanged("BatchPanel");
        });

        public DelegateCommand UploadBGCommand => new DelegateCommand(() =>
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.Multiselect = false;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var image = dialog.FileName;
                    BGImagePath = image;
                    RaisePropertyChanged("BGImagePath");
                }
            }
        });

        public DelegateCommand<string> SetBGImageCommand => new DelegateCommand<string>((s) =>
        {
            BGImagePath = s;
            RaisePropertyChanged("BGImagePath");
        });

        public DelegateCommand PickBackgroundColorCommand => new DelegateCommand(() =>
        {
            using (var colorDialog = new ColorDialog())
            {
                colorDialog.FullOpen = true;

                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    int R = colorDialog.Color.R;
                    int G = colorDialog.Color.G;
                    int B = colorDialog.Color.B;

                    var RGB = new[] { R, G, B };
                    var hex = "#";

                    for (int i = 0; i < RGB.Length; i++)
                    {
                        hex += RGB[i].ToString("X2");
                    }

                    BGColor = hex;
                    RaisePropertyChanged("BGColor");
                    return;
                }
            }
        });

        public DelegateCommand<string> SetBGColorCommand => new DelegateCommand<string>((s) =>
        {
            string[] RGB = Regex.Split(s, @"\D+");

            if (RGB.Length == 3)
            {
                var HEX = "#";

                for (int i = 0; i < RGB.Length; i++)
                {
                    var temp = int.Parse(RGB[i]);
                    HEX += temp.ToString("X2");
                }

                BGColor = HEX;
                RaisePropertyChanged("BGColor");
                return;
            }

            BGColor = s;
            RaisePropertyChanged("BGColor");
        });

        public DelegateCommand SaveCommand => new DelegateCommand(() =>
        {
            if (DisplayedImage == null) return;

            using (var dialog = new CommonSaveFileDialog())
            {
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var fileName = dialog.FileName;
                    var folder = Path.GetDirectoryName(fileName);

                    DisplayedImage.Save(fileName + ".png", ImageFormat.Png);
                    System.Diagnostics.Process.Start(folder);
                }
            }
        });

        public class ViewOptions
        {
            public const string Source = "Source";
            public const string RGBA = "RGBA";
            public const string Alpha = "Alpha";
            public const string RGB = "RGB";

            public static List<string> AsList()
            {
                return new List<string>()
                {
                    Source, RGBA, Alpha, RGB
                };
            }
        }
    }
}
