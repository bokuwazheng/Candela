using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Candela.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Candela.ViewModels
{
    class BatchViewModel : BaseViewModel
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private ParallelOptions _po = new ParallelOptions();

        private bool _colorCorrection = true;
        private bool _premultiplyAlpha = true;
        private bool _luminanceToAlpha = true;
        private ImageWorker _iw;

        public ObservableCollection<RenderQueueItemModel> RenderQueue { get; private set; }
        public bool IncludeSubfolders { get; set; } = true;
        public bool KeepFolderStructure { get; set; } = true;
        public string OutputFolder { get; private set; }
        public bool RenderInProgress { get; private set; }
        public int RenderProgress { get; set; } = 0;
        public bool ColorCorrection
        {
            get => _colorCorrection;
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
            get => _premultiplyAlpha;
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
            get => _luminanceToAlpha;
            set
            {
                if (value != _luminanceToAlpha)
                {
                    _luminanceToAlpha = value;
                    RaisePropertyChanged("LuminanceToAlpha");
                }
            }
        }

        public BatchViewModel(ImageWorker iw)
        {
            _iw = iw;

            _cts = new CancellationTokenSource();
            _po.CancellationToken = _cts.Token;
            _po.MaxDegreeOfParallelism = Environment.ProcessorCount;

            RenderQueue = new ObservableCollection<RenderQueueItemModel>();

            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "ColorCorrection")
                {
                    foreach (RenderQueueItemModel rqitem in RenderQueue)
                    {
                        rqitem.ColorCorrection = this.ColorCorrection;
                    }
                }

                if (e.PropertyName == "PremultiplyAlpha")
                {
                    foreach (RenderQueueItemModel rqitem in RenderQueue)
                    {
                        rqitem.PremultiplyAlpha = this.PremultiplyAlpha;
                    }
                }

                if (e.PropertyName == "LuminanceToAlpha")
                {
                    foreach (RenderQueueItemModel rqitem in RenderQueue)
                    {
                        rqitem.LuminanceToAlpha = this.LuminanceToAlpha;
                    }
                }
            };
        }

        //Commands
        public DelegateCommand LoadFilesCommand => new DelegateCommand(() =>
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var folder = dialog.FileName;
                    var files = new string[] { };
                    var logTxt = Path.Combine(folder, "candela_error_log.txt");

                    if (File.Exists(logTxt)) File.Delete(logTxt);

                    if (IncludeSubfolders == true)
                    {
                        files = Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories);
                    }
                    else
                    {
                        files = Directory.GetFiles(folder, "*.png");
                    }

                    for (int i = 0; i < files.Length; i++)
                    {
                        if (Image.FromFile(files[i]).PixelFormat != PixelFormat.Format32bppArgb)
                        {
                            using (StreamWriter writer = new StreamWriter(logTxt, true))
                            {
                                writer.WriteLine($"{ files[i] } is not a 32-bit image.");
                            }

                            continue;
                        }

                        RenderQueue.Add(new RenderQueueItemModel()
                        {
                            FileName = Path.GetFileName(files[i]),
                            Origin = Path.GetDirectoryName(files[i]),
                            RootFolder = folder,
                        });
                    }

                    System.Diagnostics.Process.Start(logTxt);
                    RaisePropertyChanged("RenderQueue");
                }
            }
        });

        public DelegateCommand OutputToCommand => new DelegateCommand(() =>
        {
            if (RenderQueue.Count == 0) return;

            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    OutputFolder = dialog.FileName;
                }
            }
        });

        public DelegateCommand ClearCommand => new DelegateCommand(() =>
        {
            RenderQueue.Clear();
            RaisePropertyChanged("RenderQueue");
        });

        public DelegateCommand RenderCommand => new DelegateCommand(async () =>
        {
            if (RenderQueue.Count() == 0)
            {
                System.Windows.Forms.MessageBox.Show("Render queue is empty.");
                return;
            }

            if (OutputFolder == null)
            {
                System.Windows.Forms.MessageBox.Show("Please specify the output folder.");
                return;
            }

            RenderInProgress = !RenderInProgress;
            RaisePropertyChanged("RenderInProgress");

            if (RenderInProgress == false)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = new CancellationTokenSource();

                _po.CancellationToken = _cts.Token;

                System.Diagnostics.Process.Start(OutputFolder);
                return;
            }

            var rqSize = RenderQueue.Count();
            var itemsRendered = 0;

            await Task.Run(() =>
            {
                foreach (RenderQueueItemModel item in RenderQueue)
                {
                    try
                    {
                        _cts.Token.ThrowIfCancellationRequested();

                        using (var image = _iw.Render(item, _po))
                        {
                            // Selected source folder
                            var rootName = item.RootFolder.Split(new[] { "\\" }, StringSplitOptions.None).Last();
                            // Subfolder path after selected folder
                            var branch = item.Origin.Split(new[] { rootName }, StringSplitOptions.None).Last();
                            // Output folder + subfolder info
                            var folder = OutputFolder + branch;

                            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                            var outputTo = "";

                            if (KeepFolderStructure == true)
                            {
                                outputTo = OutputFolder + branch + "\\" + item.FileName;
                            }
                            else
                            {
                                outputTo = OutputFolder + "\\" + item.FileName;
                            }

                            image.Save(outputTo, ImageFormat.Png);
                        }

                        // Report progress
                        itemsRendered++;
                        RenderProgress = (itemsRendered * 100) / rqSize;
                        RaisePropertyChanged("RenderProgress");
                    }
                    catch (OperationCanceledException)
                    {
                        System.Windows.Forms.MessageBox.Show("Rendering canceled.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                        //System.Windows.Forms.MessageBox.Show($"{ ex.Message }");
                        //break;
                    }
                }
            });

            // Reset
            if (itemsRendered == rqSize)
            {
                RenderProgress = 0;
                RaisePropertyChanged("RenderProgress");
                RenderInProgress = false;
                RaisePropertyChanged("RenderInProgress");

                System.Diagnostics.Process.Start(OutputFolder);
            }
        });

        public DelegateCommand CancelCommand => new DelegateCommand(() =>
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();

            _po.CancellationToken = _cts.Token;
        });

        public DelegateCommand<int?> RemoveCommand => new DelegateCommand<int?>((i) =>
        {
            if (i.HasValue)
            {
                RenderQueue.RemoveAt(i.Value);
                RaisePropertyChanged("RenderQueue");
            }
        });
    }
}
