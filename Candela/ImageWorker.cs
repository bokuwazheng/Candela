using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Candela.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;

namespace Candela
{
    public class ImageWorker
    {
        private Image _source;
        private Image _rendered;
        private Matrix _rgbMatrix;
        private Matrix _alphaMatrix;
        private Channel _lastAdjusted;
        private bool _used;

        double _rgbGamma = 1.0;
        double _alphaGamma = 1.0;
        private bool _lumToAlpha;
        private bool _premulAlpha;

        public ImageWorker()
        {
            _rgbMatrix = new Matrix();
            _alphaMatrix = new Matrix();
        }

        public ImageWorker(Image image)
        {
            if (image.PixelFormat != PixelFormat.Format32bppArgb)
            {
                MessageBox.Show("Only 32-bit images are supported.");
                return;
            }

            _source = image;
            _rendered = image;
            _rgbMatrix = new Matrix();
            _alphaMatrix = new Matrix();
        }

        public Image GetSource()
        {
            return _source ?? null;
        }

        public Image GetRendered()
        {
            return _used ? null : _rendered;
        }

        public unsafe Bitmap GetAlpha()
        {
            var source = new Bitmap(_rendered);

            Rectangle rect = new Rectangle(0, 0, source.Width, source.Height);
            BitmapData src_bData = source.LockBits(rect, ImageLockMode.ReadWrite, source.PixelFormat);

            byte* src_scan0 = (byte*)src_bData.Scan0.ToPointer();

            Parallel.For(0, src_bData.Height, i =>
            {
                for (int j = 0; j < src_bData.Width; ++j)
                {
                    byte* src_data = src_scan0 + i * src_bData.Stride + j * 32 / 8;

                    src_data[0] = (byte)0;
                    src_data[1] = (byte)0;
                    src_data[2] = (byte)0;
                }
            });

            source.UnlockBits(src_bData);
            return source;
        }

        public unsafe Bitmap GetRgb()
        {
            var source = new Bitmap(_rendered);

            Rectangle rect = new Rectangle(0, 0, source.Width, source.Height);
            BitmapData src_bData = source.LockBits(rect, ImageLockMode.ReadWrite, source.PixelFormat);

            byte* src_scan0 = (byte*)src_bData.Scan0.ToPointer();

            Parallel.For(0, src_bData.Height, i =>
            {
                for (int j = 0; j < src_bData.Width; ++j)
                {
                    byte* src_data = src_scan0 + i * src_bData.Stride + j * 32 / 8;

                    src_data[3] = (byte)255;
                }
            });

            source.UnlockBits(src_bData);
            return source;
        }

        private enum Channel
        {
            None,
            Color,
            Alpha
        }

        public void UpdateRgbMatrix(BaseAdjustmentModel adj)
        {
            _rgbMatrix = adj.Execute(_rgbMatrix);
            _lastAdjusted = Channel.Color;
        }

        public void UpdateAlphaMatrix(BaseAdjustmentModel adj)
        {
            _alphaMatrix = adj.Execute(_alphaMatrix);
            _lastAdjusted = Channel.Alpha;
        }

        public void UpdateRgbGamma(double gamma)
        {
            _rgbGamma = gamma;
            _lastAdjusted = Channel.Color;
        }

        public void UpdateAlphaGamma(double gamma)
        {
            _alphaGamma = gamma;
            _lastAdjusted = Channel.Alpha;
        }

        public void UpdateLTA(bool lta)
        {
            _lumToAlpha = lta;
            _lastAdjusted = Channel.Alpha;
        }

        public void UpdatePMA(bool pma)
        {
            _premulAlpha = pma;
            _lastAdjusted = Channel.Color;
        }

        public unsafe void ApplyAdjustment(ParallelOptions po)
        {
            _used = true;
            var source = new Bitmap(_source);
            var cache = new Bitmap(_rendered);

            Rectangle rect = new Rectangle(0, 0, source.Width, source.Height);
            BitmapData src_bData = source.LockBits(rect, ImageLockMode.ReadWrite, source.PixelFormat);
            BitmapData cch_bData = cache.LockBits(rect, ImageLockMode.ReadWrite, source.PixelFormat);

            byte* src_scan0 = (byte*)src_bData.Scan0.ToPointer();
            byte* cch_scan0 = (byte*)cch_bData.Scan0.ToPointer();

            Parallel.For(0, src_bData.Height, po, i =>
            {
                for (int j = 0; j < src_bData.Width; ++j)
                {
                    if (po.CancellationToken.IsCancellationRequested == true) return;

                    byte* src_data = src_scan0 + i * src_bData.Stride + j * 32 / 8;
                    byte* cch_data = cch_scan0 + i * cch_bData.Stride + j * 32 / 8;

                    double b;
                    double g;
                    double r;
                    double[] rgba;

                    if (_lastAdjusted == Channel.Color)
                    {
                        b = src_data[0];
                        g = src_data[1];
                        r = src_data[2];

                        rgba = new double[] { r, g, b, 1.0 } * _rgbMatrix;

                        src_data[0] = (byte)Clamp(rgba[2]);
                        src_data[1] = (byte)Clamp(rgba[1]);
                        src_data[2] = (byte)Clamp(rgba[0]);
                        src_data[3] = cch_data[3];
                    }

                    if (_rgbGamma != 1.0)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            src_data[k] = (byte)GammaCorrection(src_data[k], _rgbGamma);
                        }
                    }

                    if (_premulAlpha)
                    {
                        src_data[0] = (byte)Clamp(src_data[0] * (src_data[3] / 255.0));
                        src_data[1] = (byte)Clamp(src_data[1] * (src_data[3] / 255.0));
                        src_data[2] = (byte)Clamp(src_data[2] * (src_data[3] / 255.0));
                    }

                    if (_lastAdjusted == Channel.Alpha)
                    {
                        b = src_data[0];
                        g = src_data[1];
                        r = src_data[2];

                        if (_lumToAlpha)
                        {
                            rgba = new double[] { r, g, b, 1.0 } * _alphaMatrix;
                            if (_alphaGamma != 1.0)
                            {
                                for (int k = 0; k < 3; k++) rgba[k] = GammaCorrection(rgba[k], _alphaGamma);
                            }
                            src_data[3] = (byte)Clamp(rgba[0] * .2125f + rgba[1] * .7154f + rgba[2] * .0721f);
                        }

                        src_data[0] = cch_data[0];
                        src_data[1] = cch_data[1];
                        src_data[2] = cch_data[2];
                    }
                }
            });

            source.UnlockBits(src_bData);
            cache.UnlockBits(cch_bData);
            _rendered = source;
            _used = false;
        }

        public unsafe Image Render(RenderQueueItemModel rqItem, ParallelOptions po)
        {
            var source = new Bitmap(rqItem.Origin + "\\" + rqItem.FileName);
            var output = new Bitmap(rqItem.Origin + "\\" + rqItem.FileName);

            Rectangle rect = new Rectangle(0, 0, source.Width, source.Height);
            BitmapData src_bData = source.LockBits(rect, ImageLockMode.ReadWrite, source.PixelFormat);
            BitmapData out_bData = output.LockBits(rect, ImageLockMode.ReadWrite, output.PixelFormat);

            byte* src_scan0 = (byte*)src_bData.Scan0.ToPointer();
            byte* out_scan0 = (byte*)out_bData.Scan0.ToPointer();

            Parallel.For(0, src_bData.Height, po, i =>
            {
                for (int j = 0; j < src_bData.Width; ++j)
                {
                    if (po.CancellationToken.IsCancellationRequested == true) return;

                    byte* src_data = src_scan0 + i * src_bData.Stride + j * 32 / 8;
                    byte* out_data = out_scan0 + i * out_bData.Stride + j * 32 / 8;

                    double b = src_data[0];
                    double g = src_data[1];
                    double r = src_data[2];
                    double a = src_data[3];
                    double[] rgba;

                    if (rqItem.ColorCorrection)
                    {
                        rgba = new double[] { r, g, b, a } * _rgbMatrix;
                        out_data[0] = (byte)Clamp(rgba[2]);
                        out_data[1] = (byte)Clamp(rgba[1]);
                        out_data[2] = (byte)Clamp(rgba[0]);
                    }

                    out_data[0] = (byte)GammaCorrection(out_data[0], _rgbGamma);
                    out_data[1] = (byte)GammaCorrection(out_data[1], _rgbGamma);
                    out_data[2] = (byte)GammaCorrection(out_data[2], _rgbGamma);

                    if (rqItem.PremultiplyAlpha)
                    {
                        out_data[0] = (byte)Clamp(out_data[0] * (out_data[3] / 255.0));
                        out_data[1] = (byte)Clamp(out_data[1] * (out_data[3] / 255.0));
                        out_data[2] = (byte)Clamp(out_data[2] * (out_data[3] / 255.0));
                    }

                    if (rqItem.LuminanceToAlpha)
                    {
                        rgba = new double[] { r, g, b, a } * _alphaMatrix;
                        for (int k = 0; k < rgba.Length; k++) rgba[k] = GammaCorrection(rgba[k], _alphaGamma);
                        out_data[3] = (byte)Clamp(rgba[0] * .2125f + rgba[1] * .7154f + rgba[2] * .0721f);
                    }
                }
            });

            source.UnlockBits(src_bData);
            output.UnlockBits(out_bData);
            return output;
        }

        private double Clamp(double number)
        {
            return number > 255.0 ? 255.0 : number < .0 ? .0 : number;
        }

        private double GammaCorrection(double channel, double gamma)
        {
            // Validate 
            if (channel >= 255.0) return 255.0;
            if (channel <= .0) return .0;
            if (gamma != 1.0)
            {
                // To linear
                var linear = channel / 255.0;
                // Apply gamma
                var corrected = Math.Pow(linear, gamma);
                // Back to RGB space
                return Clamp(corrected * 255.0);
            }

            return Clamp(channel);
        }
    }
}
