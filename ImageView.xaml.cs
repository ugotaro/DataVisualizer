using BitMiracle.LibTiff.Classic;
using UGO.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;



namespace UGO.Utility
{
    /// <summary>
    /// ImageView.xaml の相互作用ロジック
    /// </summary>
    public partial class ImageView : Window, System.ComponentModel.INotifyPropertyChanged
    {
        int imageWidth, imageHeight;
        public int ImageWidth
        {
            get
            {
                return imageWidth;
            }
            set
            {
                if (value != imageWidth)
                {
                    imageWidth = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ImageWidth"));
                }
            }
        }

        public int ImageHeight
        {
            get
            {
                return imageHeight;
            }
            set
            {
                if (value != imageHeight)
                {
                    imageHeight = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ImageHeight"));
                }
            }
        }

        String ExtraInformation;

        double[] minValue = new double[] { 0, 0, 0, 0};
        double[] maxValue = new double[] { double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue };
        bool isKeep = false;

        CustomImage.ChannelType CurrentChannel
        {
            get;set;
            //get
            //{
            //    CustomImage.ChannelType channel = CustomImage.ChannelType.BlackWhite;
            //    if (RButton == null || GButton == null || BButton == null) return channel;
            //    Dispatcher.Invoke(new Action(() =>
            //    {
            //        if (RButton.IsChecked.Value)
            //        {
            //            channel = CustomImage.ChannelType.R;
            //        }
            //        else if (GButton.IsChecked.Value)
            //        {
            //            channel = CustomImage.ChannelType.G;
            //        }
            //        else if (BButton.IsChecked.Value)
            //        {
            //            channel = CustomImage.ChannelType.B;
            //        }
            //    }));
            //    return channel;
            //}
            //set
            //{
            //    Dispatcher.Invoke(new Action(() =>
            //    {
            //        switch (value)
            //        {
            //            case CustomImage.ChannelType.B:
            //                {
            //                    BButton.IsChecked = true;
            //                }break;
            //            case CustomImage.ChannelType.BlackWhite:
            //                {
            //                    BButton.IsChecked = false;
            //                    RButton.IsChecked = false;
            //                    GButton.IsChecked = false;
            //                }break;
            //            case CustomImage.ChannelType.G:
            //                {
            //                    GButton.IsChecked = true;
            //                }break;
            //            case CustomImage.ChannelType.R:
            //                {
            //                    RButton.IsChecked = true;
            //                }break;
            //        }
            //    }));
            //}
        }

        public double MinValue
        {
            get
            {
                return minValue[(int)CurrentChannel];
            }
            set
            {
                minValue[(int)CurrentChannel] = value;
                image.SetContrast(ContrastMin, ContrastMax, CurrentChannel);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MinValue"));
            }
        }

        public double MaxValue
        {
            get
            {
                return maxValue[(int)CurrentChannel];
            }
            set
            {
                maxValue[(int)CurrentChannel] = value;
                image.SetContrast(ContrastMin, ContrastMax, CurrentChannel);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaxValue"));
            }
        }

        public bool IsKeepChecked
        {
            get
            {
                return isKeep;
            }
            set
            {
                isKeep = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsKeepChecked"));
            }
        }

        double sliderMin = 0;
        double sliderMax = 255;
        const double SLIDER_MAX = 255;
        const double SLIDER_MIN = 0;

        public double SliderMin
        {
            get
            {
                return sliderMin;
            }
            set
            {
                sliderMin = value;
                if (lastImageData != null)
                {
                    image.SetContrast(ContrastMin, ContrastMax, CurrentChannel);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SliderMin"));
            }
        }

        public double SliderMax
        {
            get
            {
                return sliderMax;
            }
            set
            {
                sliderMax = value;
                if (lastImageData != null)
                {
                    image.SetContrast(ContrastMin, ContrastMax, CurrentChannel);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SliderMax"));
            }
        }

        public double ContrastMin
        {
            get
            {
                return (SliderMin - SLIDER_MIN) / (SLIDER_MAX - SLIDER_MIN) * (MaxValue - MinValue) + MinValue;
            }
        }

        public double ContrastMax
        {
            get
            {
                return (SliderMax - SLIDER_MIN) / (SLIDER_MAX - SLIDER_MIN) * (MaxValue - MinValue) + MinValue;
            }
        }

        public ImageView()
        {
            InitializeComponent();
            this.DataContext = this;
            

            contrastMinSlider.Minimum = SLIDER_MIN;
            contrastMinSlider.Maximum = SLIDER_MAX;
            contrastMaxSlider.Minimum = SLIDER_MIN;
            contrastMaxSlider.Maximum = SLIDER_MAX;

            channelGroup.Visibility = Visibility.Collapsed;
        }

        public void SetNewImage(ushort[] data, int width, int height, CustomImage.ChannelType channel = CustomImage.ChannelType.BlackWhite)
        {
            var doubleData = Array.ConvertAll<ushort, double>(data, (value) => value);
            SetNewImage(doubleData, width, height, channel);
        }

        public void SetNewImage(int[] data, int width, int height, CustomImage.ChannelType channel = CustomImage.ChannelType.BlackWhite)
        {
            double[] doubleData = Array.ConvertAll<int, double>(data, (x) => x);
            SetNewImage(data, width, height, channel);
        }

        public void SetNewImage(double[,] data, CustomImage.ChannelType channel = CustomImage.ChannelType.BlackWhite)
        {
            int width = data.GetLength(0);
            int height = data.GetLength(1);
            double[] array_1d = new double[data.GetLength(0) * data.GetLength(1)];
            for (int x = 0; x < data.GetLength(0); x++)
            {
                for (int y = 0; y < data.GetLength(1); y++)
                {
                    array_1d[x + y * data.GetLength(0)] = data[x, y];
                }
            }
            SetNewImage(array_1d, width, height, channel);
        }

        public void SetNewImage(double[] data, int width, int height, CustomImage.ChannelType channel = CustomImage.ChannelType.BlackWhite)
        {
            ImageWidth = width;
            ImageHeight = height;
            lastImageData[(int)channel] = data;
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    UpdateImage(channel);
                    if (channel != CustomImage.ChannelType.BlackWhite)
                    {
                        channelGroup.Visibility = Visibility.Visible;
                        if(CurrentChannel == CustomImage.ChannelType.BlackWhite)
                        {
                            RButton.IsChecked = true;
                        }
                    }
                    imageSlider.Visibility = Visibility.Collapsed;
                    numberLabel.Visibility = Visibility.Collapsed;
                }
                catch { }
            }));
        }

        public void SetNewImage(OpenCvSharp.Mat image)
        {
            ImageWidth = image.Width;
            ImageHeight = image.Height;
            if(image.Channels() == 3)
            {
                for (int col = 0;col < 3; col++)
                {
                    double[] data = new double[ImageWidth * ImageHeight];

                    for (int y = 0; y < ImageHeight; y++)
                    {
                        for (int x = 0; x < ImageWidth; x++)
                        {
                            data[x + y * ImageWidth] = image.At<OpenCvSharp.Vec3b>(y, x)[col];
                        }
                    }

                    lastImageData[col + 1] = data;
                    var channel = (CustomImage.ChannelType)(col + 1);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        try
                        {
                            UpdateImage(channel);
                            if (channel != CustomImage.ChannelType.BlackWhite)
                            {
                                channelGroup.Visibility = Visibility.Visible;
                                if (CurrentChannel == CustomImage.ChannelType.BlackWhite)
                                {
                                    RButton.IsChecked = true;
                                }
                            }
                            imageSlider.Visibility = Visibility.Collapsed;
                            numberLabel.Visibility = Visibility.Collapsed;
                        }
                        catch { }
                    }));
                }
            }
            else
            {
                double[] data = new double[ImageWidth * ImageHeight];
                for (int y = 0;y < ImageHeight; y++)
                {
                    for(int x = 0; x < ImageWidth; x++)
                    {
                        data[x + y * ImageWidth] = image.At<byte>(y, x);
                    }
                }
                lastImageData[(int)CustomImage.ChannelType.BlackWhite] = data;
                var channel = CustomImage.ChannelType.BlackWhite;
                Dispatcher.Invoke(new Action(() =>
                {
                    try
                    {
                        UpdateImage(channel);
                        if (channel != CustomImage.ChannelType.BlackWhite)
                        {
                            channelGroup.Visibility = Visibility.Visible;
                            if (CurrentChannel == CustomImage.ChannelType.BlackWhite)
                            {
                                RButton.IsChecked = true;
                            }
                        }
                        imageSlider.Visibility = Visibility.Collapsed;
                        numberLabel.Visibility = Visibility.Collapsed;
                    }
                    catch { }
                }));
            }

        }

        public void SetExtraInformation(String ExtraInformationArg) // is dispayed in the image and saved when saved
        {
            ExtraInformation = ExtraInformationArg;  // is this reallcopied or jsut a reference?
            image.SetExtraInformation(ExtraInformation);
        }

        List<Tuple<double[][], int, int>> imageDataArray = new List<Tuple<double[][], int, int>>();


        public void AddImage(ushort[] data, int width, int height)
        {
            AddImage(Array.ConvertAll<ushort, double>(data, (x)=>x), width, height);
        }

        public void AddImage(int[] data, int width, int height)
        {
            AddImage(Array.ConvertAll<int, double>(data, (x) => x), width, height);
        }

        public void AddImage(double[] data, int width, int height)
        {
            imageDataArray.Add(new Tuple<double[][], int, int>(new double[][] { data, null, null, null }, width, height));
            ImageWidth = width;
            ImageHeight = height;
            lastImageData[0] = data;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    CurrentChannel = CustomImage.ChannelType.BlackWhite;
                    channelGroup.Visibility = Visibility.Collapsed;
                    UpdateImage(CustomImage.ChannelType.BlackWhite);
                    imageSlider.Minimum = 0;
                    imageSlider.Maximum = imageDataArray.Count - 1;
                    imageSlider.Value = imageSlider.Maximum;
                    imageSlider.Visibility = Visibility.Visible;
                    numberLabel.Visibility = Visibility.Visible;
                }
                catch { }
            }));
        }


        public void AddImage(OpenCvSharp.Mat image)
        {
            ImageWidth = image.Width;
            ImageHeight = image.Height;
            if (image.Channels() == 3)
            {
                double[][] rgbdata = new double[3][];
                for (int col = 0; col < 3; col++)
                {
                    rgbdata[col] = new double[ImageWidth * ImageHeight];
                    for (int y = 0; y < ImageHeight; y++)
                    {
                        for (int x = 0; x < ImageWidth; x++)
                        {
                            rgbdata[col][x + y * ImageWidth] = image.At<OpenCvSharp.Vec3b>(y, x)[col];
                        }
                    }

                    lastImageData[col + 1] = rgbdata[col];
                }


                imageDataArray.Add(new Tuple<double[][], int, int>(rgbdata, image.Width, image.Height));
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        CurrentChannel = CustomImage.ChannelType.B;
                        channelGroup.Visibility = Visibility.Collapsed;
                        UpdateImage(CustomImage.ChannelType.B);
                        imageSlider.Minimum = 0;
                        imageSlider.Maximum = imageDataArray.Count - 1;
                        imageSlider.Value = imageSlider.Maximum;
                        imageSlider.Visibility = Visibility.Visible;
                        numberLabel.Visibility = Visibility.Visible;
                    }
                    catch { }
                }));
            }
            else
            {
                double[] data = new double[ImageWidth * ImageHeight];
                for (int y = 0; y < ImageHeight; y++)
                {
                    for (int x = 0; x < ImageWidth; x++)
                    {
                        data[x + y * ImageWidth] = image.At<byte>(y, x);
                    }
                }

                imageDataArray.Add(new Tuple<double[][], int, int>(new double[][] { data, null, null, null }, image.Width, image.Height));
                lastImageData[0] = data;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        CurrentChannel = CustomImage.ChannelType.BlackWhite;
                        channelGroup.Visibility = Visibility.Collapsed;
                        UpdateImage(CustomImage.ChannelType.BlackWhite);
                        imageSlider.Minimum = 0;
                        imageSlider.Maximum = imageDataArray.Count - 1;
                        imageSlider.Value = imageSlider.Maximum;
                        imageSlider.Visibility = Visibility.Visible;
                        numberLabel.Visibility = Visibility.Visible;
                    }
                    catch { }
                }));
            }
        }
        double[][] lastImageData = new double[4][];
        double[][] lastPostProcessData = new double[4][];
        private void UpdateImage(CustomImage.ChannelType channel)
        {
            if (lastImageData[(int)CurrentChannel] == null) return;
            double min, max;
            double[] data;
            switch (imageProcessType.SelectedIndex)
            {
                case 1:
                    {
                        data = Process.ImageProcessing.FFT_abs(lastImageData[(int)channel], ImageWidth, ImageHeight);
                    }
                    break;
                case 2:
                    {
                        data = Process.ImageProcessing.FFT_power(lastImageData[(int)channel], ImageWidth, ImageHeight);
                    }
                    break;
                default:
                    {
                        data = lastImageData[(int)channel].Clone() as double[];
                    }break;
            }

            if (logProcess_box != null)
            {
                if (logProcess_box.IsChecked.Value)
                {
                    data = Array.ConvertAll<double, double>(data, (x) => Math.Log10(Math.Max(x, 1e-10)));
                }
            }

            bool force = lastPostProcessData[(int)channel] == null;

            lastPostProcessData[(int)channel] = data.Clone() as double[];

            UpdateMinMax(data, out min, out max, channel, force);

            image.SetImage(data, ImageWidth, ImageHeight, channel, min, max);
        }

        private void UpdateMinMax(double[] data, out double min, out double max, CustomImage.ChannelType channel, bool forceUpdate = false)
        {
            if (!IsKeepChecked || forceUpdate)
            {
                if (channel == CurrentChannel ||  forceUpdate)
                {
                    GetMinMax(data, out min, out max, out double calculatedMin, out double calculatedMax);
                    minValue[(int)channel] = calculatedMin;
                    maxValue[(int)channel] = calculatedMax;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MinValue"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaxValue"));
                }
                else
                {
                    min = image.MinValue[(int)channel];
                    max = image.MaxValue[(int)channel];
                }
            }
            else
            {
                min = ((MaxValue - MinValue) * SliderMin / 256 + MinValue);
                max = ((MaxValue - MinValue) * SliderMax / 256 + MinValue);
            }
        }

        int lastValue = -1;
        private void ImageSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                int currentValue = (int)imageSlider.Value;
                if (lastValue != currentValue)
                {
                    var tuple = imageDataArray[currentValue];
                    ImageWidth = tuple.Item2;
                    ImageHeight = tuple.Item3;
                    lastImageData = tuple.Item1;
                    CurrentChannel = CustomImage.ChannelType.BlackWhite;
                    UpdateImage(CustomImage.ChannelType.BlackWhite);
                    System.Diagnostics.Debug.WriteLine("Image View: " + ((int)(imageSlider.Value)).ToString());
                    numberLabel.Content = currentValue.ToString();
                    lastValue = currentValue;
                }
            }
            catch { }
        }

        Point startPos;
        Point endPos = new Point(-1, -1);
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                endPos = e.GetPosition(image);
                
                UpdateCrosssectionalBar();
                UpdateCrosssectionGraph();
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(image);
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPos = e.GetPosition(image);
            endPos = startPos;
            UpdateCrosssectionGraph();
            UpdateCrosssectionalBar();
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        Line hCrosssectionLine = null;
        Line vCrosssectionLine = null;
        SolidColorBrush vCrossBrush = new SolidColorBrush(Colors.Yellow);
        SolidColorBrush hCrossBrush = new SolidColorBrush(Colors.Cyan);
        private void UpdateCrosssectionalBar()
        {
            if(hCrosssectionLine == null || vCrosssectionLine == null)
            {
                hCrosssectionLine = new Line();
                canvas.Children.Add(hCrosssectionLine);
                vCrosssectionLine = new Line();
                canvas.Children.Add(vCrosssectionLine);

                hCrosssectionLine.Stroke = hCrossBrush;
                vCrosssectionLine.Stroke = vCrossBrush;
                hCrosssectionLine.StrokeThickness = 0.5;
                vCrosssectionLine.StrokeThickness = 0.5;
                hCrosssectionLine.Opacity = 0.9;
                vCrosssectionLine.Opacity = 0.9;
                hCrosssectionLine.IsHitTestVisible = false;
                vCrosssectionLine.IsHitTestVisible = false;
            }

            if(endPos.X < 0 || endPos.Y < 0)
            {
                endPos.X = image.ActualWidth / 2;
                endPos.Y = image.ActualHeight / 2;
            }

            hCrosssectionLine.Visibility = hCrossSectionBox.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
            vCrosssectionLine.Visibility = vCrossSectionBox.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;

            hCrosssectionLine.Y1 = hCrosssectionLine.Y2 = endPos.Y;
            hCrosssectionLine.X1 = 0;
            hCrosssectionLine.X2 = image.ActualWidth;

            vCrosssectionLine.X1 = vCrosssectionLine.X2 = endPos.X;
            vCrosssectionLine.Y1 = 0;
            vCrosssectionLine.Y2 = image.ActualHeight;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {

                Microsoft.Win32.SaveFileDialog fileDialog = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "Tiff(*.tif)|*.tif"
                };
                var result = fileDialog.ShowDialog();
                if (result.Value)
                {
                    if(imageProcessType.SelectedIndex > 0 || logProcess_box.IsChecked.Value)
                    {
                        MessageBox.Show("Original image data is saved.");
                    }
                    string filenameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(fileDialog.FileName);
                    string path = System.IO.Path.GetDirectoryName(fileDialog.FileName);

                    string fileName = fileDialog.FileName;
                    using (Tiff output = Tiff.Open(fileName, "w"))  // saves the data
                    {
                        int imagePos = 0;
                        if (imageDataArray.Count > 1)
                        {
                            for (int pos = 0; pos < imageDataArray.Count; pos++)
                            {
                                var image = imageDataArray[pos].Item1;
                                var width = imageDataArray[pos].Item2;
                                var height = imageDataArray[pos].Item3;
                                for (int n = 0; n < image.Length; n++)
                                {
                                    if (image[n] != null)
                                    {
                                        SetTiffData(output, imagePos, image[n], width, height);
                                        imagePos++;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var image = lastImageData;
                            var width = ImageWidth;
                            var height = ImageHeight;
                            for (int n = 0; n < image.Length; n++)
                            {
                                if (image[n] != null)
                                {
                                    SetTiffData(output, imagePos, image[n], width, height);
                                    imagePos++;
                                }
                            }
                        }
                    }
                    this.Title =filenameWithoutExt;

                    // now save the metainformation in a text file
                    string[] lines = { ExtraInformation };
                    // WriteAllLines creates a file, writes a collection of strings to the file,
                    // and then closes the file.  You do NOT need to call Flush() or Close().
                    System.IO.File.WriteAllLines(path+ "\\" + filenameWithoutExt + ".txt" , lines);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public bool isActive=true;

        public event PropertyChangedEventHandler PropertyChanged;

        private void Window_Closed(object sender, EventArgs e)
        {
            isActive = false;
        }

        private static void SetTiffData(Tiff output, int pos, double[] image, int width, int height)
        {
            ushort samplesperpixel, bitspersample;
            double resolution;
            resolution = 72.0;
            bitspersample = 32;
            samplesperpixel = 1;
            output.SetDirectory((short)pos);
            output.SetField(TiffTag.IMAGEWIDTH, width);
            output.SetField(TiffTag.IMAGELENGTH, height);
            output.SetField(TiffTag.SAMPLESPERPIXEL, samplesperpixel);
            output.SetField(TiffTag.BITSPERSAMPLE, bitspersample);
            output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
            //              output.SetField(TiffTag.ROWSPERSTRIP, height);
            output.SetField(TiffTag.XRESOLUTION, resolution);
            output.SetField(TiffTag.YRESOLUTION, resolution);
            output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH);
            output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
            output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
            output.SetField(TiffTag.COMPRESSION, Compression.NONE);
            output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);
            output.SetField(TiffTag.FRAMECOUNT, pos);
            output.SetField(TiffTag.SAMPLEFORMAT, SampleFormat.IEEEFP);

            byte[] buf = new byte[width * height * sizeof(float)];

            float[] floatImageData = new float[width * height];
            for (int n = 0; n < image.Length; n++)
            {
                floatImageData[n] = (float)image[n];
            }
            Buffer.BlockCopy(floatImageData, 0, buf, 0, buf.Length);

            output.WriteEncodedStrip(0, buf, buf.Length);
            output.WriteDirectory();
        }

        private void GetMinMax(double[] image, out double min, out double max, out double minValue, out double maxValue)
        {
            minValue = image.Min();
            maxValue = image.Max();
            min = ((maxValue - minValue) * SliderMin / 256 + minValue);
            max = ((maxValue - minValue) * SliderMax / 256 + minValue);
        }

        private void imageProcessType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lastImageData != null)
                UpdateImage(CurrentChannel);
        }

        private void logProcess_box_Checked(object sender, RoutedEventArgs e)
        {

            if (lastImageData != null)
                UpdateImage(CurrentChannel);
        }

        private void resetContrastInformation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateMinMax(lastPostProcessData[(int)CurrentChannel], out double min, out double max, CurrentChannel, true);
            }
            catch { }
        }
        
        GraphWindow vGraph = null;
        private void vCrossSectionBox_Checked(object sender, RoutedEventArgs e)
        {
            if(vGraph == null)
            {
                vGraph = new GraphWindow("Vertical Crosssection");
                vGraph.GraphColor = vCrossBrush.Color;
                vGraph.Closed += new EventHandler((graph, args) =>
                {
                    vGraph = null;
                });
            }

            vGraph.Show();

            UpdateCrosssectionalBar();
            UpdateCrosssectionGraph();
        }

        GraphWindow hGraph = null;
        private void hCrossSectionBox_Checked(object sender, RoutedEventArgs e)
        {

            if(hGraph == null)
            {
                hGraph = new GraphWindow("Horizontal Crosssection");
                hGraph.GraphColor = hCrossBrush.Color;
                hGraph.Closed += new EventHandler((graph, args) =>
                {
                    hGraph = null;
                });
            }

            hGraph.Show();

            UpdateCrosssectionalBar();
            UpdateCrosssectionGraph();
        }

        private void UpdateCrosssectionGraph()
        {
            var screenPos = image.PointToScreen(endPos);
            var pixelPos = image.GetImagePixelPosition(screenPos);

            if (hGraph != null)
            {
                if (CurrentChannel == CustomImage.ChannelType.BlackWhite)
                {
                    double[] hData = new double[ImageWidth];
                    double[] xData = new double[ImageWidth];
                    int yPos = (int)pixelPos.Y;
                    for (int x = 0; x < ImageWidth; x++)
                    {
                        hData[x] = lastPostProcessData[0][x + yPos * ImageWidth];
                        xData[x] = x;
                    }

                    hGraph.SetGraph(xData, hData);
                }
                else
                {
                    double[][] hData = new double[3][];
                    double[][] xData = new double[3][];
                    int yPos = (int)pixelPos.Y;
                    for (int n = 0; n < 3;n++) {
                        hData[n] = new double[ImageWidth];
                        xData[n] = new double[ImageWidth];
                        for (int x = 0; x < ImageWidth; x++)
                        {
                            hData[n][x] = lastPostProcessData[n + 1][x + yPos * ImageWidth];
                            xData[n][x] = x;
                        }
                    }

                    hGraph.SetGraph(xData, hData, new Color[] { Colors.Blue, Colors.Green, Colors.Red });
                }
            }

            if (vGraph != null)
            {
                if (CurrentChannel == CustomImage.ChannelType.BlackWhite)
                {
                    double[] vData = new double[ImageHeight];
                    double[] xData = new double[ImageHeight];
                    int xPos = (int)pixelPos.X;
                    for (int y = 0; y < ImageHeight; y++)
                    {
                        vData[y] = lastPostProcessData[0][xPos + y * ImageWidth];
                        xData[y] = y;
                    }

                    vGraph.SetGraph(xData, vData);
                }
                else
                {
                    double[][] vData = new double[3][];
                    double[][] xData = new double[3][];

                    int xPos = (int)pixelPos.X;
                    for (int n = 0; n < 3; n++)
                    {
                        vData[n] = new double[ImageHeight];
                        xData[n] = new double[ImageHeight];
                        for (int y = 0; y < ImageHeight; y++)
                        {
                            vData[n][y] = lastPostProcessData[n + 1][xPos + y * ImageWidth];
                            xData[n][y] = y;
                        }
                    }

                    vGraph.SetGraph(xData, vData, new Color[] { Colors.Blue, Colors.Green, Colors.Red });
                }
            }
        }


        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scale = 1.0;
            Matrix matrix = ((MatrixTransform)imageGrid.RenderTransform).Matrix;

            // ScaleAt()の拡大中心点(引数3,4個目)に渡すための座標をとるときの基準Controlは、拡大縮小をしたいものの一つ上のControlにすること。
            // ここでは拡大縮小するGridを包んでいるScrollViewerを基準にした。
            var currentPosition = e.GetPosition(sender as System.Windows.IInputElement);

            // ホイール上に回す→拡大 / 下に回す→縮小
            if (e.Delta > 0) scale = 1.25;
            else scale = 1 / 1.25;

            System.Diagnostics.Debug.WriteLine($"倍率：{scale} 中心点：{currentPosition} 大きさ：({imageGrid.ActualWidth},{imageGrid.ActualHeight})");

            // 拡大実施
            matrix.ScaleAt(scale, scale, currentPosition.X, currentPosition.Y);
            imageGrid.RenderTransform = new MatrixTransform(matrix);
        }

        bool isDragging = false;
        Point dragStartPosition;
        private void ScrollViewer_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            dragStartPosition = e.GetPosition(sender as System.Windows.IInputElement);
        }

        private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging == false) return;

            var currentPosition = e.GetPosition(sender as System.Windows.IInputElement);

            double offsetX = currentPosition.X - dragStartPosition.X;
            double offsetY = currentPosition.Y - dragStartPosition.Y;

            var matrix = ((MatrixTransform)imageGrid.RenderTransform).Matrix;

            matrix.Translate(offsetX, offsetY);
            imageGrid.RenderTransform = new MatrixTransform(matrix);

            dragStartPosition = currentPosition;
        }

        private void ScrollViewer_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        private void ScrollViewer_MouseLeave(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void resetViewButton_Click(object sender, RoutedEventArgs e)
        {
            imageGrid.RenderTransform = new MatrixTransform(Matrix.Identity);
        }

        private void RButton_Checked(object sender, RoutedEventArgs e)
        {

            //(SliderMin - SLIDER_MIN) / (SLIDER_MAX - SLIDER_MIN) * (MaxValue - MinValue) + MinValue;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MinValue"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaxValue"));
            CustomImage.ChannelType channel = CustomImage.ChannelType.BlackWhite;
            if(sender == RButton)
            {
                channel = CustomImage.ChannelType.R;
            }else if(sender == GButton)
            {
                channel = CustomImage.ChannelType.G;
            }else if(sender == BButton)
            {
                channel = CustomImage.ChannelType.B;
            }
            int channelPos = (int)channel;
            var min = minValue[channelPos];
            var max = maxValue[channelPos];
            sliderMin = (image.MinValue[channelPos] - min) / (max - min) * (SLIDER_MAX - SLIDER_MIN) + SLIDER_MIN;
            sliderMax = (image.MaxValue[channelPos] - min) / (max - min) * (SLIDER_MAX - SLIDER_MIN) + SLIDER_MIN;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SliderMin"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SliderMax"));

            CurrentChannel = channel;
            //UpdateImage();
        }

        private double[] GetMinMax(double[,] imageData, out double min, out double max, out double minValue, out double maxValue)
        {
            double[] array_1d = new double[imageData.GetLength(0) * imageData.GetLength(1)];
            minValue = double.MaxValue;
            maxValue = double.MinValue;
            for(int x = 0;x < imageData.GetLength(0);x++)
            {
                for (int y = 0; y < imageData.GetLength(1); y++)
                {
                    double value = imageData[x, y];
                    if (value < minValue) minValue = value;
                    if (value > maxValue) maxValue = value;
                    array_1d[x + y * imageData.GetLength(0)] = value;
                }
            }
            min = (int)((maxValue - minValue) * SliderMin / 256 + minValue);
            max = (int)((maxValue - minValue) * SliderMax / 256 + minValue);
            return array_1d;
        }
    }
}
