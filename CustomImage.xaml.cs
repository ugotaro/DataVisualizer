using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UGO.View
{
    /// <summary>
    /// CustomImage.xaml の相互作用ロジック
    /// </summary>
    public partial class CustomImage : UserControl
    {
        public enum ChannelType
        {
            BlackWhite = 0,
            B = 1,
            G = 2,
            R = 3
        };

        Label mousePositionLabel = new Label();

        public int ImageWidth { get; private set; }
        public int ImageHeight { get; private set; }
        //public int[][] ImageData { get; private set; }
        public double[][] ImageData { get; private set; }
        public int xPos=0, yPos=0;
        public String ExtraInformation;
        public double[] MinValue { get; private set; } = new double[] { 0, 0, 0, 0 };
        public double[] MaxValue { get; private set; } = new double[] { 0, 0, 0, 0 };
        private ChannelType lastUpdateChannel;

        public CustomImage()
        {
            InitializeComponent();
            canvas.Children.Add(mousePositionLabel);
            mousePositionLabel.Foreground = new SolidColorBrush(Colors.Green);
            ImageData = new double[4][];
            
        }

        WriteableBitmap bitmapBuffer = null;

        private void SetImage(int[] imageData, int width, int height, ChannelType channel, double min = 0, double max = 0)
        {
            SetImage(Array.ConvertAll<int, double>(imageData, (x) => x), width, height, channel, min, max);
        }

        public void SetImage(double[] imageData, int width, int height, ChannelType channel, double min = 0, double max = 0)
        {
            try
            {
                double maxValue = double.MinValue;
                double minValue = double.MaxValue;
                if (min == max)
                {
                    foreach (var value in imageData)
                    {
                        if (maxValue < value) maxValue = value;
                        if (minValue > value) minValue = value;
                    }
                }
                else
                {
                    minValue = min;
                    maxValue = max;
                }

                ImageData[(int)channel] = imageData;
                MinValue[(int)channel] = minValue;
                MaxValue[(int)channel] = maxValue;

                Dispatcher.Invoke(new Action(() =>
                {
                    try
                    {
                        if (channel == ChannelType.BlackWhite)
                        {
                            bitmapBuffer = new WriteableBitmap(Process.ImageProcessing.GetImage(imageData, width, height, minValue, maxValue));
                        }
                        else
                        {
                            bitmapBuffer = new WriteableBitmap(Process.ImageProcessing.GetImage(ImageData, width, height, MinValue, MaxValue));
                        }
                        image.Source = bitmapBuffer;
                        image.InvalidateVisual();
                        ImageWidth = width;
                        ImageHeight = height;
                        UpdateLabel(channel);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

        }

        public void SetContrast(double min, double max, ChannelType channel)
        {
            if (ImageWidth == 0 || ImageHeight == 0) return;
            if (ImageData[(int)channel] == null) return;
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {

                    if (channel == ChannelType.BlackWhite)
                    {
                        MinValue[0] = min;
                        MaxValue[0] = max;
                        bitmapBuffer = new WriteableBitmap(Process.ImageProcessing.GetImage(ImageData[0], ImageWidth, ImageHeight, MinValue[0], MaxValue[0]));
                    }
                    else
                    {
                        MinValue[(int)channel] = min;
                        MaxValue[(int)channel] = max;
                        bitmapBuffer = new WriteableBitmap(Process.ImageProcessing.GetImage(ImageData, ImageWidth, ImageHeight, MinValue, MaxValue));
                    }
                    image.Source = bitmapBuffer;
                    image.InvalidateVisual();
                    UpdateLabel(channel);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }));
        }


        public void SetExtraInformation(String ExtraInformationArg) // is dispayed in the image and saved when saved
        {
            ExtraInformation = ExtraInformationArg;  // is this reallcopied or jsut a reference?
        }


        public Point GetImagePixelPosition(Point screenPosition)
        {
            int x = 0;
            int y = 0;
            Dispatcher.Invoke(new Action(() =>
            {
                var visualPos = image.PointFromScreen(screenPosition);
                x = (int)(visualPos.X / image.ActualWidth * ImageWidth);
                y = (int)(visualPos.Y / image.ActualHeight * ImageHeight);
            }));

            x = Math.Max(0, x);
            x = Math.Min(ImageWidth - 1, x);
            y = Math.Max(0, y);
            y = Math.Min(ImageHeight - 1, y);
            return new Point(x, y);

        }
        private void UpdateLabel(ChannelType channel)
        {
            try
            {
                lastUpdateChannel = channel;
                if (ImageData[(int)channel] == null) return;
                int bsize = 3;
                int minX = (xPos - bsize < 0) ? 0 : xPos - bsize;
                int minY = (yPos - bsize < 0) ? 0 : yPos - bsize;
                int maxX = (xPos + bsize >= ImageWidth) ? ImageWidth - 1 : xPos + bsize;
                int maxY = (yPos + bsize >= ImageHeight) ? ImageHeight - 1 : yPos + bsize;
                int n = 0;
                int px, py;

                double mysum = 0, mymin = double.MaxValue, val = 0;
                for (px = minX; px <= maxX; px++)
                {
                    for (py = minY; py <= maxY; py++)
                    {
                        if (ImageData != null)
                            val = ImageData[(int)channel][px + ImageWidth * py];
                        if (val < mymin) mymin = val;
                        mysum += val;
                        n++;
                    }
                }
                int dataIdx = xPos + ImageWidth * yPos;
                if (dataIdx < 0) dataIdx = 0;
                if (dataIdx >= ImageWidth * ImageHeight) dataIdx = ImageWidth * ImageHeight - 1;
                var idxValue = 0.0;
                if (ImageData != null) idxValue = ImageData[(int)channel][dataIdx];
                mousePositionLabel.Content = "(" + xPos.ToString() + "," + yPos.ToString() + ")\n" + idxValue + "\n" +(int) (((double) mysum)/((double) n)+0.5 - mymin) + "(7x7) avg.\n" + ExtraInformation;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var position = e.GetPosition(image);
                xPos = (int)(position.X / image.ActualWidth * ImageWidth);
                yPos = (int)(position.Y / image.ActualHeight * ImageHeight);
                UpdateLabel(lastUpdateChannel);
                Canvas.SetLeft(mousePositionLabel, 0);
                Canvas.SetTop(mousePositionLabel, 0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                mousePositionLabel.Visibility = System.Windows.Visibility.Visible;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                mousePositionLabel.Visibility = System.Windows.Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }


    }
}
