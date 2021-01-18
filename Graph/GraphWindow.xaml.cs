using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace UGO.Utility
{
    /// <summary>
    /// GraphWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class GraphWindow : Window, System.ComponentModel.INotifyPropertyChanged
    {
        public GraphWindow(string title = "Graph")
        {
            InitializeComponent();

            this.Title = title;
        }

        public Color GraphColor { get; set; }

        private string information;
        public string GraphInformation
        {
            get
            {
                return information;
            }
            set
            {
                information = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GraphInformation"));
            }
        }

        public void SetGraph(double[] xValues, double[] yValues)
        {
            var xSeries = new OxyPlot.Series.LineSeries();
            for (int n = 0; n < xValues.Length; n++)
            {
                xSeries.Points.Add(new OxyPlot.DataPoint(xValues[n], yValues[n]));
            }

            xSeries.Color = new OxyPlot.OxyColor();
            xSeries.Color = OxyPlot.OxyColor.FromArgb(GraphColor.A, GraphColor.R, GraphColor.G, GraphColor.B);


            var tmp = new OxyPlot.PlotModel();
            tmp.Series.Add(xSeries);


            Dispatcher.Invoke(new Action(() =>
            {
                plotView.Model = tmp;
                plotView.InvalidatePlot();
            }));
        }

        public void SetGraph(double[][] xValues, double[][] yValues, Color[] colors = null)
        {

            int colorNum = xValues.GetLength(0);
            var tmp = new OxyPlot.PlotModel();
            for (int c = 0; c < colorNum;c++)
            {
                var xSeries = new OxyPlot.Series.LineSeries();
                for (int n = 0; n < xValues[c].Length; n++)
                {
                    xSeries.Points.Add(new OxyPlot.DataPoint(xValues[c][n], yValues[c][n]));
                }

                xSeries.Color = new OxyPlot.OxyColor();
                if (colors == null)
                {
                    xSeries.Color = OxyPlot.OxyColor.FromArgb(GraphColor.A, GraphColor.R, GraphColor.G, GraphColor.B);
                }
                else
                {
                    xSeries.Color = OxyPlot.OxyColor.FromArgb(colors[c].A, colors[c].R, colors[c].G, colors[c].B);
                }
                tmp.Series.Add(xSeries);

            }
            Dispatcher.Invoke(new Action(() =>
            {
                plotView.Model = tmp;
                plotView.InvalidatePlot();
            }));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
