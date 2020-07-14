using OxyPlot;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Dissertation.Algorithms.Metrica.Visuals
{
    public partial class GraphicWindow : Window
    {
        private Plot _Plot;
        private static Random rnd = new Random();

        public GraphicWindow(string graphicTitle, IList<DataPoint> points, string axisXTitle, string axisYTitle)
        {

            GraphicTitle = graphicTitle;
            AxisXTitle = axisXTitle;
            AxisYTitle = axisYTitle;
            InitializeComponent();

            _Plot = PART_Oxyplot;
            var ls = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                Color = Colors.Black,
                ItemsSource = points,
            };
            _Plot.Series.Add(ls);
        }

        public GraphicWindow(string graphicTitle, IList<List<DataPoint>> points, string axisXTitle, string axisYTitle, string legendTitle, string[] seriesTitles, LegendPlacement legendPlacement = LegendPlacement.Inside)
        {
            GraphicTitle = graphicTitle;
            AxisXTitle = axisXTitle;
            AxisYTitle = axisYTitle;
            InitializeComponent();

            _Plot = PART_Oxyplot;
            _Plot.LegendTitle = legendTitle;
            _Plot.LegendPlacement = legendPlacement;
            _Plot.LegendPosition = LegendPosition.RightTop;

            for (int i = 0; i < points.Count(); i++)
            {
                Color randomColor = Color.FromArgb((byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));

                var ls = new LineSeries()
                {
                    LineStyle = LineStyle.Solid,
                    Color = randomColor,
                    ItemsSource = points[i],
                    Title = seriesTitles[i],
                };
                _Plot.Series.Add(ls);
            }
        }

        public string GraphicTitle
        {
            get { return (string)GetValue(GraphicTitleProperty); }
            set { SetValue(GraphicTitleProperty, value); }
        }

        public static readonly DependencyProperty GraphicTitleProperty =
            DependencyProperty.Register("GraphicTitle", typeof(string), typeof(GraphicWindow), new PropertyMetadata(""));

        public string AxisXTitle
        {
            get { return (string)GetValue(AxisXTitleProperty); }
            set { SetValue(AxisXTitleProperty, value); }
        }

        public static readonly DependencyProperty AxisXTitleProperty =
            DependencyProperty.Register("AxisXTitle", typeof(string), typeof(GraphicWindow), new PropertyMetadata(""));

        public string AxisYTitle
        {
            get { return (string)GetValue(AxisYTitleProperty); }
            set { SetValue(AxisYTitleProperty, value); }
        }

        public static readonly DependencyProperty AxisYTitleProperty =
            DependencyProperty.Register("AxisYTitle", typeof(string), typeof(GraphicWindow), new PropertyMetadata(""));
    }
}
