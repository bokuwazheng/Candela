using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Candela.Models;
using System.ComponentModel;

namespace Candela.ViewModels
{
    public class BaseAdjustmentViewModel : BaseViewModel, IAdjustmentPanel
    {
        protected double _gamma = 1.0;
        protected bool _lta;
        protected bool _pma;

        protected ImageWorker _iw;
        protected BrightnessModel _bm;
        protected SaturationModel _sm;
        protected HueModel _hm;
        protected RedBalanceModel _rbm;
        protected GreenBalanceModel _gbm;
        protected BlueBalanceModel _bbm;

        protected CancellationTokenSource _cts;
        protected ParallelOptions _po;

        #region Properties
        public int Brightness
        {
            get
            {
                return _bm.Get();
            }
            set
            {
                if (value != _bm.Get())
                {
                    _bm.Set(value);
                    RaisePropertyChanged("Brightness");
                }
            }
        }

        public double Gamma
        {
            get
            {
                return _gamma;
            }
            set
            {
                if (value != _gamma)
                {
                    _gamma = value;
                    RaisePropertyChanged("Gamma");
                }
            }
        }

        public int RedBalance
        {
            get
            {
                return _rbm.Get();
            }
            set
            {
                if (value != _rbm.Get())
                {
                    _rbm.Set(value);
                    RaisePropertyChanged("RedBalance");
                }
            }
        }

        public int GreenBalance
        {
            get
            {
                return _gbm.Get();
            }
            set
            {
                if (value != _gbm.Get())
                {
                    _gbm.Set(value);
                    RaisePropertyChanged("GreenBalance");
                }
            }
        }

        public int BlueBalance
        {
            get
            {
                return _bbm.Get();
            }
            set
            {
                if (value != _bbm.Get())
                {
                    _bbm.Set(value);
                    RaisePropertyChanged("BlueBalance");
                }
            }
        }

        public int Hue
        {
            get
            {
                return _hm.Get();
            }
            set
            {
                if (value != _hm.Get())
                {
                    _hm.Set(value);
                    RaisePropertyChanged("Hue");
                }
            }
        }

        public int Saturation
        {
            get
            {
                return _sm.Get();
            }
            set
            {
                if (value != _sm.Get())
                {
                    _sm.Set(value);
                    RaisePropertyChanged("Saturation");
                }
            }
        }

        public bool LTA
        {
            get
            {
                return _lta;
            }
            set
            {
                if (value != _lta)
                {
                    _lta = value;
                    RaisePropertyChanged("LTA");
                }
            }
        }

        public bool PMA
        {
            get
            {
                return _pma;
            }
            set
            {
                if (value != _pma)
                {
                    _pma = value;
                    RaisePropertyChanged("PMA");
                }
            }
        }
        #endregion

        public event AdjustmentHandler AdjustmentsChanged;

        public virtual void OnAdjustmentsChanged(long ms)
        {
            AdjustmentHandler handler = AdjustmentsChanged;

            if (handler != null)
            {
                handler(this, ms);
            }
        }

        public BaseAdjustmentViewModel(ImageWorker iw)
        {
            _iw = iw;
            _bm = new BrightnessModel();
            _sm = new SaturationModel();
            _hm = new HueModel();
            _rbm = new RedBalanceModel();
            _gbm = new GreenBalanceModel();
            _bbm = new BlueBalanceModel();

            _po = new ParallelOptions();
            _cts = new CancellationTokenSource();
            _po.CancellationToken = _cts.Token;
            _po.MaxDegreeOfParallelism = Environment.ProcessorCount;

            this.PropertyChanged += async (s, e) =>
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = new CancellationTokenSource();
                _po.CancellationToken = _cts.Token;

                var watch = new System.Diagnostics.Stopwatch();

                await Task.Run(() =>
                {
                    // Stopwatch start
                    watch.Start();

                    Update(e);

                    // Render
                    try
                    {
                        _iw.ApplyAdjustment(_po);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    finally
                    {
                        // Stopwatch stop
                        watch.Stop();
                        OnAdjustmentsChanged(watch.ElapsedMilliseconds);
                    }
                });
            };
        }

        protected virtual void Update(PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
