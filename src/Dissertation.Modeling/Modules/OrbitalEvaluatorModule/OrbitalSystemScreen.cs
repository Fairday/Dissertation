using ATS.WPF.Modules.Helpers;
using ATS.WPF.Shell.Model;
using Dissertation.Algorithms.Algorithms.Helpers;
using Dissertation.Algorithms.OrbitalMath;
using Dissertation.Modeling.Helpers;
using Dissertation.Modeling.UI;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Dissertation.Modeling.Modules.OrbitalEvaluatorModule
{
    public enum ViewType
    {
        Front, Top
    }

    public class OrbitalSystemScreen : Screen
    {
        public override void Initialized()
        {
            var source = ScreenSource as OrbitalSystemView;

            var topViewLayout = source.TopViewLayout;
            var frontViewLayout = source.FrontViewLayout;

            TopViewLayout = topViewLayout;
            FrontViewLayout = frontViewLayout;

            UIHelper.Dispatcher.Invoke(() =>
            {
                CreateAxises();
                CreateLatitudeBelts();
            });
        }

        public ICanvasPointWrapper CreatePoint(ViewType viewType, Brush brush)
        {
            return UIHelper.Dispatcher.Invoke(() =>
            {
                switch (viewType)
                {
                    case ViewType.Front:
                        {
                            var point = AddPoint(brush, FrontViewLayout);
                            var wrapper = new WpfCanvasPointWrapper(point);
                            return wrapper;
                        }
                    case ViewType.Top:
                        {
                            var point = AddPoint(brush, TopViewLayout);
                            var wrapper = new WpfCanvasPointWrapper(point);
                            return wrapper;
                        }
                    default: throw new KeyNotFoundException();
                }
            });
        }

        protected virtual void CreateAxises()
        {
            ///Horizontal axis
            AddAxis(-180, 180, 0, 0, TopViewLayout);
            ///Vertical axis
            AddAxis(0, 0, 180, -180, TopViewLayout);

            ///Horizontal axis
            AddAxis(-180, 180, 0, 0, FrontViewLayout);
            ///Vertical axis
            AddAxis(0, 0, 180, -180, FrontViewLayout);
        }
        protected virtual void CreateLatitudeBelts()
        {
            var averageEarthRadius = OM.AverageEarchRadius(0);
            var toCanvasLayoutNormalizer = new FullRangeNormalizer(0, averageEarthRadius, 0, 350);
            var verticalOffsetNormalizer = new FullRangeNormalizer(-averageEarthRadius, averageEarthRadius, -350, 350);

            for (int i = -9; i <= 9; i++)
            {
                var latitude = (i * 10d).ToRad();
                var latitudeEarchRadius = averageEarthRadius * Math.Cos(latitude);
                var verticalOffset = averageEarthRadius * Math.Sin(latitude);
                if (i >= 0)
                    AddLatitudeBeltWithTopView(toCanvasLayoutNormalizer.Normalize(latitudeEarchRadius), TopViewLayout);

                if (i == 0)
                    AddLatitudeBeltWithTopView(toCanvasLayoutNormalizer.Normalize(latitudeEarchRadius), FrontViewLayout);

                AddLatitudeBeltWithFrontView(toCanvasLayoutNormalizer.Normalize(latitudeEarchRadius), verticalOffsetNormalizer.Normalize(verticalOffset));
            }
        }
        protected virtual void AddAxis(double x1, double x2, double y1, double y2, Canvas layout)
        {
            Line axis = new Line();
            axis.Opacity = 0.5;
            axis.Stroke = Brushes.Gray;
            axis.X1 = x1;
            axis.X2 = x2;
            axis.Y1 = y1;
            axis.Y2 = y2;
            axis.StrokeThickness = 1;
            axis.SnapsToDevicePixels = true;
            axis.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            double left = (TopViewLayout.ActualWidth - axis.Width) / 2;
            Canvas.SetLeft(axis, left);
            double top = (TopViewLayout.ActualHeight - axis.Height) / 2;
            Canvas.SetTop(axis, top);
            layout.Children.Add(axis);
        }
        protected virtual UIElement AddPoint(Brush brush, Canvas layout)
        {
            var ellipse = new Ellipse();
            ellipse.Height = 3;
            ellipse.Width = 3;
            ellipse.Stroke = brush;
            ellipse.StrokeThickness = 1;
            ellipse.Fill = brush;
            ellipse.Effect = new DropShadowEffect()
            {
                ShadowDepth = 0,
                BlurRadius = 5,
            };
            double left = (TopViewLayout.ActualWidth - ellipse.Width) / 2;
            Canvas.SetLeft(ellipse, left);
            double top = (TopViewLayout.ActualHeight - ellipse.Height) / 2;
            Canvas.SetTop(ellipse, top);
            layout.Children.Add(ellipse);
            return ellipse;
        }
        protected virtual void AddLatitudeBeltWithTopView(double radius, Canvas layout)
        {
            var ellipse = new Ellipse();
            ellipse.Height = radius;
            ellipse.SnapsToDevicePixels = true;
            ellipse.Width = radius;
            ellipse.Opacity = 0.5;
            ellipse.Stroke = Brushes.Gray;
            double left = (TopViewLayout.ActualWidth - ellipse.Width) / 2;
            Canvas.SetLeft(ellipse, left);
            double top = (TopViewLayout.ActualHeight - ellipse.Height) / 2;
            Canvas.SetTop(ellipse, top);
            layout.Children.Add(ellipse);
        }
        protected virtual void AddLatitudeBeltWithFrontView(double radius, double verticalOffset)
        {
            Line line = new Line();
            line.SnapsToDevicePixels = true;
            line.Opacity = 0.5;
            line.Stroke = Brushes.Gray;
            line.X1 = -radius / 2;
            line.X2 = radius / 2;
            line.StrokeThickness = 1;
            line.SnapsToDevicePixels = true;
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            Canvas.SetTop(line, verticalOffset / 2);
            FrontViewLayout.Children.Add(line);
        }

        public Canvas TopViewLayout { get; private set; }
        public Canvas FrontViewLayout { get; private set; }
    }
}
