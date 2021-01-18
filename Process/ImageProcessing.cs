using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace UGO.Process
{
    public class ImageProcessing
    {
        public static BitmapSource GetImage(int[] imageArray, int Width, int Height, double Min = 0, double Max = 0)
        {
            WriteableBitmap bitmap = new WriteableBitmap(Width, Height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, BitmapPalettes.Gray256);
            byte[] imageData = new byte[Width * Height * 4];
            if (Min == 0 && Max == 0)
            {
                foreach (var v in imageArray)
                {
                    if (v < Min) Min = v;
                    if (v > Max) Max = v;
                }
            }

            int length = Width * Height;
            double amplitude = 1.0 / (Max - Min);
            for (int n = 0; n < length; n++)
            {
                double value = (imageArray[n] - Min) * amplitude;
                if (value < 0) value = 0;
                else if (value > 1) value = 1;
                byte byteValue = (byte)(255.0 * value);
                int pos = n * 4;
                imageData[pos] = byteValue;
                imageData[pos + 1] = byteValue;
                imageData[pos + 2] = byteValue;
                imageData[pos + 3] = 255;
            }

            bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, Width, Height), imageData, Width * 4, 0);

            return bitmap;
        }

        public static BitmapSource GetImage(int[][] brgbImageArray, int Width, int Height, double[] Min, double[] Max)
        {
            WriteableBitmap bitmap = new WriteableBitmap(Width, Height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, BitmapPalettes.Gray256);
            byte[] imageData = new byte[Width * Height * 4];
            if (Min == null || Max == null)
            {
                Min = new double[4];
                Max = new double[4];
            }
            for (int n = 1; n < 4; n++)
            {
                if (Min[n] == Max[n])
                {
                    var imageArray = brgbImageArray[n];

                    Min[n] = double.MaxValue;
                    Max[n] = double.MinValue;
                    foreach (var v in imageArray)
                    {
                        if (v < Min[n]) Min[n] = v;
                        if (v > Max[n]) Max[n] = v;
                    }
                }
            }

            int length = Width * Height;
            for (int color = 1; color < 4; color++)
            {
                double amplitude = 1.0 / (Max[color] - Min[color]);
                for (int n = 0; n < length; n++)
                {
                    double value = (brgbImageArray[color][n] - Min[color]) * amplitude;
                    if (value < 0) value = 0;
                    else if (value > 1) value = 1;
                    byte byteValue = (byte)(255.0 * value);
                    int pos = n * 4;
                    imageData[pos + color - 1] = byteValue;
                }
            }
            for (int n = 0; n < length; n++)
            {
                int pos = n * 4 + 3;
                imageData[pos] = 255;
            }
            bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, Width, Height), imageData, Width * 4, 0);

            return bitmap;
        }
        public static BitmapSource GetImage(double[] imageArray, int Width, int Height, double Min = 0, double Max = 0)
        {
            WriteableBitmap bitmap = new WriteableBitmap(Width, Height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, BitmapPalettes.Gray256);
            byte[] imageData = new byte[Width * Height * 4];
            if (Min == 0 && Max == 0)
            {
                Min = imageArray.Min();
                Max = imageArray.Max();
            }

            int length = Width * Height;
            double amplitude = 1.0 / (Max - (double)Min);
            byte byteValue = 0;
            int pos = 0;
            double value = 0;
            for (int n = 0; n < length; n++)
            {
                value = (imageArray[n] - Min) * amplitude;
                if (value < 0) value = 0;
                else if (value > 1) value = 1;
                byteValue = (byte)(255.0 * value);
                pos = n * 4;
                imageData[pos] = byteValue;
                imageData[pos + 1] = byteValue;
                imageData[pos + 2] = byteValue;
                imageData[pos + 3] = 255;
            }

            bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, Width, Height), imageData, Width * 4, 0);

            return bitmap;
        }

        public static BitmapSource GetImage(double[][] brgbImageArray, int Width, int Height, double[] Min, double[] Max)
        {
            WriteableBitmap bitmap = new WriteableBitmap(Width, Height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, BitmapPalettes.Gray256);
            byte[] imageData = new byte[Width * Height * 4];
            if (Min == null || Max == null)
            {
                Min = new double[4];
                Max = new double[4];
            }
            for (int n = 1; n < 4; n++)
            {
                if (Min[n] == Max[n])
                {
                    var imageArray = brgbImageArray[n];
                    if (imageArray == null) continue;
                    Min[n] = double.MaxValue;
                    Max[n] = double.MinValue;
                    foreach (var v in imageArray)
                    {
                        if (v < Min[n]) Min[n] = v;
                        if (v > Max[n]) Max[n] = v;
                    }
                }
            }

            int length = Width * Height;
            for (int color = 1; color < 4; color++)
            {
                double amplitude = 1.0 / (Max[color] - Min[color]);
                if (brgbImageArray[color] == null)
                {
                    for(int n = 0;n < length; n++)
                    {
                        int pos = n * 4;
                        imageData[pos + color - 1] = 0;
                    }
                }
                else
                {
                    for (int n = 0; n < length; n++)
                    {
                        double value = (brgbImageArray[color][n] - Min[color]) * amplitude;
                        if (value < 0) value = 0;
                        else if (value > 1) value = 1;
                        byte byteValue = (byte)(255.0 * value);
                        int pos = n * 4;
                        imageData[pos + color - 1] = byteValue;
                    }
                }
            }
            for (int n = 0; n < length; n++)
            {
                int pos = n * 4 + 3;
                imageData[pos] = 255;
            }
            bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, Width, Height), imageData, Width * 4, 0);

            return bitmap;
        }
        public static double[,,] InitialCoef(int N, int M)
        {
            double[,,] returnCoef = new double[2, N, M];
            returnCoef[0, 1, 0] = 1;
            returnCoef[1, 0, 1] = 1;
            return returnCoef;
        }

        public static double[,] DistortImage(double[,] imageArray, double[,,] coef)
        {
            int N = coef.GetLength(1);
            int M = coef.GetLength(2);
            int width = coef.GetLength(0);
            int height = coef.GetLength(1);
            var returnArray = new int[width, height];

            var positionArray = new Tuple<double, double>[width, height];
            for(int y = 0;y < height; y++)
            {
                for(int x = 0;x < width; x++)
                {
                    var x_pos = 0.0;
                    var y_pos = 0.0;
                    for(int n = 0;n < N; n++)
                    {
                        for(int m = 0;m < M; m++)
                        {
                            x_pos += coef[0, x, y] * Math.Pow(x, n) * Math.Pow(y, m);
                            y_pos += coef[1, x, y] * Math.Pow(x, n) * Math.Pow(y, m);
                        }
                    }
                    if (x_pos < 0) x_pos = 0;
                    else if (x_pos >= width - 1) x_pos = width - 1;
                    if (y_pos < 0) y_pos = 0;
                    else if (y_pos >= height - 1) y_pos = height - 1;

                    positionArray[x, y] = new Tuple<double, double>(x_pos, y_pos);
                }
            }
            return Interpolate2DLinear(imageArray, positionArray);
        }

        public static double[,] Interpolate2DLinear(double[,] imageArray, Tuple<double, double>[,] xy_pos)
        {
            int width = imageArray.GetLength(0);
            int height = imageArray.GetLength(1);
            double[] xArray = new double[width];
            double[] yArray = new double[height];
            double[] xBaseArray = new double[width];
            double[] yBaseArray = new double[height];
            MathNet.Numerics.Interpolation.LinearSpline[] interpolate = new MathNet.Numerics.Interpolation.LinearSpline[height];
            for (int y = 0; y < height; y++)
            {
                yBaseArray[y] = y;
                for (int x = 0; x < width; x++)
                {
                    xBaseArray[x] = 0;
                    xArray[x] = imageArray[x, y];
                }
                yBaseArray[y] = y;
                interpolate[y] = MathNet.Numerics.Interpolation.LinearSpline.InterpolateSorted(xBaseArray, xArray);
            }
            int newWidth = xy_pos.GetLength(0);
            int newHeight = xy_pos.GetLength(1);
            double[,] returnArray = new double[newWidth, newHeight];
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    if (xy_pos[x, y].Item1 < 0 || xy_pos[x, y].Item1 >= width - 1 ||
                       xy_pos[x, y].Item2 < 0 || xy_pos[x, y].Item2 >= height - 1)
                    {
                        returnArray[x, y] = 0;
                    }
                    else
                    {
                        for (int yy = 0; y < height; y++)
                        {
                            yArray[yy] = interpolate[yy].Interpolate(xy_pos[x, y].Item1);
                        }
                        var temp_interpolate = MathNet.Numerics.Interpolation.LinearSpline.InterpolateSorted(yBaseArray, yArray);
                        returnArray[x, y] = temp_interpolate.Interpolate(xy_pos[x, y].Item2);
                    }
                }
            }
            return returnArray;
        }

        public static double[] FFT_abs(double[] imageData, int width, int height, bool center = true)
        {
            MathNet.Numerics.LinearAlgebra.Complex32.DenseMatrix matrix = new MathNet.Numerics.LinearAlgebra.Complex32.DenseMatrix(width, height);
            var enableMKL = MathNet.Numerics.Control.TryUseNativeMKL();

            var result = new double[width * height];
            if (enableMKL)
            {
                for(int y = 0;y < height; y++)
                {
                    for(int x = 0;x < width; x++)
                    {
                        matrix[x, y] = new MathNet.Numerics.Complex32((float)imageData[x + y * width], 0);
                    }
                }

                MathNet.Numerics.IntegralTransforms.Fourier.Forward2D(matrix);

            }
            else
            {
                var x_data = new MathNet.Numerics.Complex32[width];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        x_data[x] = new MathNet.Numerics.Complex32((float)imageData[x + y * width], 0);
                    }
                    MathNet.Numerics.IntegralTransforms.Fourier.Forward(x_data);
                    for (int x = 0; x < width; x++)
                    {
                        matrix[x, y] = x_data[x];
                    }
                }
                var y_data = new MathNet.Numerics.Complex32[height];
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        y_data[y] = matrix[x, y];
                    }
                    MathNet.Numerics.IntegralTransforms.Fourier.Forward(y_data);
                    for (int y = 0; y < height; y++)
                    {
                        matrix[x, y] = y_data[y];
                    }
                }
            }

            if (center)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int ypos = height / 2 - y;
                        if (ypos < 0) ypos += height;
                        int xpos = width / 2 - x;
                        if (xpos < 0) xpos += width;
                        result[xpos + ypos * width] = matrix[x, y].Magnitude;
                    }
                }
            }
            else
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        result[x + y * width] = matrix[x, y].Magnitude;
                    }
                }
            }
            return result;
        }

        public static double[] FFT_power(double[] imageData, int width, int height, bool center = true)
        {
            imageData = FFT_abs(imageData, width, height);
            return Array.ConvertAll<double, double>(imageData, (x) => x * x);
        }
    }
}
