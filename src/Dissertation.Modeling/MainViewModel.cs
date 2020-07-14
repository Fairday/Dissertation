using ATS.MVVM.Collections;
using ATS.MVVM.Command;
using ATS.MVVM.Core;
using Dissertation.Algorithms.Algorithms.Helpers;
using Dissertation.Algorithms.OrbitalMath;
using Dissertation.Modeling.Algorithms;
using Dissertation.Modeling.Helpers;
using Dissertation.Modeling.Model;
using Dissertation.Modeling.UI;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Dissertation.Modeling
{
    public class MainViewModel : VirtualBindableBase
    {
        private bool _Canceled;
        private readonly Canvas _TopViewLayout;
        private readonly Canvas _FrontViewLayout;
        private readonly Viewport3DX _Viewport3DX;

        public ICanvasPointWrapper TopViewEarchPoint { get => Get(); set => Set(value); }
        public ICanvasPointWrapper TopViewSatellite { get => Get(); set => Set(value); }
        public ICanvasPointWrapper FrontViewEarchPoint { get => Get(); set => Set(value); }
        public ICanvasPointWrapper FrontViewSatellite { get => Get(); set => Set(value); }
        public ICommand StartModelingCommand { get => Get(); set => Set(value); }
        public ICommand StopModelingCommand { get => Get(); set => Set(value); }
        public EffectsManager EffectsManager { get => Get(); set => Set(value); }
        public Camera Camera { get => Get(); set => Set(value); }
        public double T { get => Get(); set => Set(value); }
        public double SatelliteLatitude { get => Get(); set => Set(value); }
        public double SatelliteLongitude { get => Get(); set => Set(value); }
        public double TraverseAngle { get => Get(); set => Set(value); }
        public double CosTraverseAngle { get => Get(); set => Set(value); }
        public double CosAzimuth { get => Get(); set => Set(value); }
        public double Azimuth { get => Get(); set => Set(value); }
        public ObservableList<double> ObservationTimes { get => Get(); set => Set(value); }

        private PointGeometryModel3D CreatePoint(float x, float y, float z, System.Windows.Media.Color color)
        {
            var point = new PointGeometryModel3D()
            {
                Size = new Size(5, 5),
                Color = color,
                Figure = PointFigure.Ellipse,
                FigureRatio = 0.12,
                ToolTip = $"{x},{y},{z}",
                Geometry = new PointGeometry3D()
                {
                    Indices = new IntCollection(),
                    Positions = new Vector3Collection()
                    {
                        new Vector3(x, y, z),
                    },
                }
            };

            return point;
        }

        public MainViewModel(Canvas topViewLayout, Canvas frontViewLayout, Viewport3DX viewport3DX)
        {
            ObservationTimes = new ObservableList<double>();
            _Viewport3DX = viewport3DX;
            _TopViewLayout = topViewLayout;
            _FrontViewLayout = frontViewLayout;

            #region 3d
            EffectsManager = new DefaultEffectsManager();
            Camera = new PerspectiveCamera();

            var gm = new MeshGeometryModel3D();
            MeshBuilder meshBuilder = new MeshBuilder();
            meshBuilder.AddSphere(new Vector3(0, 0, 0), 0.5);
            gm.Geometry = meshBuilder.ToMesh();
            gm.Material = PhongMaterials.Blue;

            LineBuilder lb = new LineBuilder();
            lb.AddBox(new Vector3(0, 0, 0), 1, 1, 1);
            LineGeometry3D lineGeometry = lb.ToLineGeometry3D();

            LineGeometryModel3D lgm = new LineGeometryModel3D()
            {
                Geometry = lineGeometry,
                Thickness = 1.5,
                Color = Colors.Black,
                Transform = new System.Windows.Media.Media3D.TranslateTransform3D(0, 0, 0),
            };

            var p1 = CreatePoint(0, 0, 0, Colors.Black);
            var p2 = CreatePoint(1, 0, 0, Colors.Red);
            var p3 = CreatePoint(0, 1, 0, Colors.Green);
            var p4 = CreatePoint(0, 0, 1, Colors.Yellow);
            var p5 = CreatePoint(-1, 0, 0, Colors.LightSkyBlue);
            var p6 = CreatePoint(0, -1, 0, Colors.Blue);
            var p7 = CreatePoint(0, 0, -1, Colors.DarkGray);

            viewport3DX.Items.Add(p1);
            viewport3DX.Items.Add(p2);
            viewport3DX.Items.Add(p3);
            viewport3DX.Items.Add(p4);
            viewport3DX.Items.Add(p5);
            viewport3DX.Items.Add(p6);
            viewport3DX.Items.Add(p7);
            viewport3DX.Items.Add(gm);
            #endregion

            StartModelingCommand = AsyncCommandCreator.Create(() =>
            {
                _ = Start();
            }, dispatcher: Application.Current.Dispatcher);


            StopModelingCommand = AsyncCommandCreator.Create(() =>
            {
                Stop();
            });
            PrepareLayout();
        }

        private void PrepareLayout()
        {
            var averageEarthRadius = OM.AverageEarchRadius(0);
            var toCanvasLayoutNormalizer = new FullRangeNormalizer(0, averageEarthRadius, 0, 300);
            var verticalOffsetNormalizer = new FullRangeNormalizer(-averageEarthRadius, averageEarthRadius, -300, 300);

            for (int i = -9; i <= 9; i++)
            {
                var latitude = (i * 10d).ToRad();
                var latitudeEarchRadius = averageEarthRadius * Math.Cos(latitude);
                var verticalOffset = averageEarthRadius * Math.Sin(latitude);
                if (i >= 0)
                    AddLatitudeBeltWithTopView(toCanvasLayoutNormalizer.Normalize(latitudeEarchRadius), _TopViewLayout);

                if (i == 0)
                    AddLatitudeBeltWithTopView(toCanvasLayoutNormalizer.Normalize(latitudeEarchRadius), _FrontViewLayout);

                AddLatitudeBeltWithFrontView(toCanvasLayoutNormalizer.Normalize(latitudeEarchRadius), verticalOffsetNormalizer.Normalize(verticalOffset));
            }

            var UIPoint1 = AddPoint(Brushes.Green, _TopViewLayout);
            var UIPoint2 = AddPoint(Brushes.Red, _TopViewLayout);
            TopViewSatellite = new WpfCanvasPointWrapper(UIPoint1);
            TopViewEarchPoint = new WpfCanvasPointWrapper(UIPoint2);

            UIPoint1 = AddPoint(Brushes.Green, _FrontViewLayout);
            UIPoint2 = AddPoint(Brushes.Red, _FrontViewLayout);
            FrontViewSatellite = new WpfCanvasPointWrapper(UIPoint1);
            FrontViewEarchPoint = new WpfCanvasPointWrapper(UIPoint2);

            ///Horizontal axis
            AddAxis(-150, 150, 0, 0, _TopViewLayout);
            ///Vertical axis
            AddAxis(0, 0, 150, -150, _TopViewLayout);

            ///Horizontal axis
            AddAxis(-150, 150, 0, 0, _FrontViewLayout);
            ///Vertical axis
            AddAxis(0, 0, 150, -150, _FrontViewLayout);
        }
        private void AddAxis(double x1, double x2, double y1, double y2, Canvas layout)
        {
            Line axis = new Line();
            axis.Opacity = 0.3;
            axis.Stroke = Brushes.Black;
            axis.X1 = x1;
            axis.X2 = x2;
            axis.Y1 = y1;
            axis.Y2 = y2;
            axis.StrokeThickness = 1;
            axis.SnapsToDevicePixels = true;
            axis.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            double left = (_TopViewLayout.ActualWidth - axis.Width) / 2;
            Canvas.SetLeft(axis, left);
            double top = (_TopViewLayout.ActualHeight - axis.Height) / 2;
            Canvas.SetTop(axis, top);
            layout.Children.Add(axis);
        }

        private UIElement AddPoint(Brush brush, Canvas layout)
        {
            var ellipse = new Ellipse();
            ellipse.Height = 3;
            ellipse.Width = 3;
            ellipse.Stroke = brush;
            ellipse.StrokeThickness = 1;
            ellipse.Fill = brush;
            ellipse.Effect = new DropShadowEffect()
            {
                ShadowDepth = 0, BlurRadius = 5,
            };
            double left = (_TopViewLayout.ActualWidth - ellipse.Width) / 2;
            Canvas.SetLeft(ellipse, left);
            double top = (_TopViewLayout.ActualHeight - ellipse.Height) / 2;
            Canvas.SetTop(ellipse, top);
            layout.Children.Add(ellipse);
            return ellipse;
        }

        private void AddLatitudeBeltWithTopView(double radius, Canvas layout)
        {
            var ellipse = new Ellipse();
            ellipse.Opacity = 0.1;
            ellipse.Height = radius;
            ellipse.Width = radius;
            ellipse.Stroke = Brushes.Black;
            double left = (_TopViewLayout.ActualWidth - ellipse.Width) / 2;
            Canvas.SetLeft(ellipse, left);
            double top = (_TopViewLayout.ActualHeight - ellipse.Height) / 2;
            Canvas.SetTop(ellipse, top);
            layout.Children.Add(ellipse);
        }

        private void AddLatitudeBeltWithFrontView(double radius, double verticalOffset)
        {
            Line line = new Line();
            line.Opacity = 0.3;
            line.Stroke = Brushes.Black;
            line.X1 = -radius / 2;
            line.X2 = radius / 2;
            line.StrokeThickness = 1;
            line.SnapsToDevicePixels = true;
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            Canvas.SetTop(line, verticalOffset / 2);
            _FrontViewLayout.Children.Add(line);
        }

        private async Task Start()
        {
            //await Task.Run(async () =>
            //{
            //    _Canceled = false;

            //    var m = 1;
            //    var n = 1;
            //    var inclination = 0;

            //    var averageEarthRadius = OM.AverageEarchRadius(inclination);
            //    var halfBand = (9d).ToRad();

            //    var toCanvasLayoutNormalizer = new FullRangeNormalizer(0, averageEarthRadius, 0, 150);

            //    var csc = new CoordinateSystemConverter();
            //    var moveModelingAlgorithm = new MoveModelingAlgorithm(orbit);
            //    var halfSizingAlgorithm = new HalfSizingAlgorithm(0.0000001);

            //    var targetEarchPoint = new EarchAnglePoint(0, 0, true);
            //    var earthAngleSpeed = Dissertation.Algorithms.Resources.Constants.we;

            //    T = 0; //sec
            //    var dt = 25;
            //    var startSatellitePosition = new SatellitePosition(0, 0);
            //    var satellitePosition = startSatellitePosition;
            //    int cosAngleSign;
            //    bool isSignChanged = false;

            //    Func<double, double> traverseExecutor = (currentTime) =>
            //    {
            //        var currentSatellitePosition = moveModelingAlgorithm.Move(startSatellitePosition, currentTime);
            //        var currentsatelliteSpeendOnEarchPoint = csc.ProjectSatelliteSpeed(inclination, currentSatellitePosition.LongitudeAscentNode,
            //            currentSatellitePosition.LatitudeArgument);
            //        return csc.CosBy(targetEarchPoint, currentsatelliteSpeendOnEarchPoint);
            //    };

            //    //Позиция точки на земной поверхности с учетом вращения Земли
            //    var targetEarchPointLocation = csc.Calculate(averageEarthRadius, targetEarchPoint.Latitude,
            //        targetEarchPoint.Longitude, T, earthAngleSpeed);

            //    //Проекция подспутниковой точки (используется единичный вектор координат) -> (широта, долгота)
            //    var satelliteOnEarchPoint = csc.ProjectSatellitePoint(inclination, satellitePosition.LongitudeAscentNode,
            //        satellitePosition.LatitudeArgument);

            //    //Проекция подспутниковой точки (используется единичный вектор скорости) -> (широта, долгота)
            //    var satelliteSpeendOnEarchPoint = csc.ProjectSatelliteSpeed(inclination, satellitePosition.LongitudeAscentNode,
            //        satellitePosition.LatitudeArgument);

            //    //Географические координаты спутника
            //    var satellitePoint = csc.Calculate(averageEarthRadius, satelliteOnEarchPoint.Latitude,
            //        satelliteOnEarchPoint.Longitude, T, earthAngleSpeed);

            //    //Координаты точки на орбите
            //    var orbitSatellitePoint = csc.SatelliteLocationToCoordinates(inclination, satellitePosition.LongitudeAscentNode,
            //        satellitePosition.LatitudeArgument, orbit.Radius);

            //    //Широта спутника
            //    SatelliteLatitude = Math.Round(satelliteOnEarchPoint.Latitude.ToGrad(), 3);
            //    //Долгота спутника
            //    SatelliteLongitude = Math.Round(satelliteOnEarchPoint.Longitude.ToGrad(), 3);

            //    CosTraverseAngle = csc.CosBy(targetEarchPoint, satelliteSpeendOnEarchPoint);
            //    TraverseAngle = Math.Acos(CosTraverseAngle);
            //    CosAzimuth = csc.CosBy(targetEarchPoint, satelliteOnEarchPoint);
            //    Azimuth = Math.Acos(CosAzimuth);

            //    cosAngleSign = Math.Sign(CosTraverseAngle);

            //    while (!_Canceled)
            //    {
            //        //Позиция точки на земной поверхности с учетом вращения Земли
            //        targetEarchPointLocation = csc.Calculate(averageEarthRadius, targetEarchPoint.Latitude,
            //            targetEarchPoint.Longitude, T, earthAngleSpeed);

            //        //Проекция подспутниковой точки (широта, долгота)
            //        satelliteOnEarchPoint = csc.ProjectSatellitePoint(inclination, satellitePosition.LongitudeAscentNode, 
            //            satellitePosition.LatitudeArgument);

            //        //Проекция подспутниковой точки (широта, долгота)
            //        satelliteSpeendOnEarchPoint = csc.ProjectSatelliteSpeed(inclination, satellitePosition.LongitudeAscentNode,
            //            satellitePosition.LatitudeArgument);

            //        //Географические координаты спутника
            //        satellitePoint = csc.Calculate(averageEarthRadius, satelliteOnEarchPoint.Latitude,
            //            satelliteOnEarchPoint.Longitude, T, earthAngleSpeed);

            //        //Координаты точки на орбите
            //        orbitSatellitePoint = csc.SatelliteLocationToCoordinates(inclination, satellitePosition.LongitudeAscentNode,
            //            satellitePosition.LatitudeArgument, orbit.Radius);

            //        //Широта спутника
            //        SatelliteLatitude = Math.Round(satelliteOnEarchPoint.Latitude.ToGrad(), 3);
            //        //Долгота спутника
            //        SatelliteLongitude = Math.Round(satelliteOnEarchPoint.Longitude.ToGrad(), 3);

            //        CosTraverseAngle = csc.CosBy(targetEarchPoint, satelliteSpeendOnEarchPoint);
            //        TraverseAngle = Math.Acos(CosTraverseAngle);
            //        CosAzimuth = csc.CosBy(targetEarchPoint, satelliteOnEarchPoint);
            //        Azimuth = Math.Acos(CosAzimuth);

            //        var newSign = Math.Sign(CosTraverseAngle);
            //        if (newSign != cosAngleSign)
            //            isSignChanged = true;
            //        cosAngleSign = newSign;

            //        if (isSignChanged)
            //        {
            //            if (Math.Abs(Azimuth) < halfBand)
            //            {
            //                var computedMoment = halfSizingAlgorithm.Compute(T - dt, T, traverseExecutor);

            //                Application.Current.Dispatcher.Invoke(() =>
            //                {
            //                    ObservationTimes.Add(Math.Round(computedMoment.Parameter, 3));
            //                });
            //            }

            //            isSignChanged = false;
            //        }

            //        T = orbit.ModEraTier.Normalize(T + dt);
            //        //Пересчет текущей фазовой позиции спутника с учетом прошедшего времени
            //        satellitePosition = moveModelingAlgorithm.Move(startSatellitePosition, T);

            //        //Дальше построение графиков
            //        var S_normalizedX = toCanvasLayoutNormalizer.Normalize(satellitePoint.X);
            //        var S_normalizedY = toCanvasLayoutNormalizer.Normalize(satellitePoint.Y);
            //        var S_normalizedZ = toCanvasLayoutNormalizer.Normalize(satellitePoint.Z);

            //        TopViewSatellite.SetCoordinateLocation(Direction.Top, new CoordinateLocation(-S_normalizedX, S_normalizedY, S_normalizedZ), SatelliteLongitude > 270 || SatelliteLongitude < 90);
            //        FrontViewSatellite.SetCoordinateLocation(Direction.Front, new CoordinateLocation(-S_normalizedX, S_normalizedY, S_normalizedZ), SatelliteLongitude > 270 || SatelliteLongitude < 90);

            //        //Satellite.X = -S_normalizedX;
            //        //Satellite.Y = S_normalizedY;
            //        //Satellite.Z = S_normalizedZ;

            //        var P_normalizedX = toCanvasLayoutNormalizer.Normalize(targetEarchPointLocation.X);
            //        var P_normalizedY = toCanvasLayoutNormalizer.Normalize(targetEarchPointLocation.Y);
            //        var P_normalizedZ = toCanvasLayoutNormalizer.Normalize(targetEarchPointLocation.Z);
            //        //EarchPoint.X = -P_normalizedX;
            //        //EarchPoint.Y = P_normalizedY;

            //        TopViewEarchPoint.SetCoordinateLocation(Direction.Top, new CoordinateLocation(-P_normalizedX, P_normalizedY, P_normalizedZ), true);
            //        FrontViewEarchPoint.SetCoordinateLocation(Direction.Front, new CoordinateLocation(-P_normalizedX, P_normalizedY, P_normalizedZ), true);

            //        //Application.Current?.Dispatcher.Invoke(() =>
            //        //{
            //        //    var uiPoint = new Border()
            //        //    {
            //        //        Width = 2,
            //        //        Height = 2,
            //        //        CornerRadius = new CornerRadius(1),
            //        //        Background = Brushes.Green,
            //        //    };
            //        //    Canvas.SetLeft(uiPoint, Satellite.Y);
            //        //    Canvas.SetTop(uiPoint, Satellite.X);
            //        //    _Canvas.Children.Add(uiPoint);
            //        //});

            //        //Application.Current?.Dispatcher.Invoke(() =>
            //        //{
            //        //    var uiPoint = new Border()
            //        //    {
            //        //        Width = 2,
            //        //        Height = 2,
            //        //        CornerRadius = new CornerRadius(1),
            //        //        Background = Brushes.Red,
            //        //    };
            //        //    Canvas.SetLeft(uiPoint, orbitSatellitePoint.Y / 30);
            //        //    Canvas.SetTop(uiPoint, -orbitSatellitePoint.X / 30);
            //        //    _Canvas.Children.Add(uiPoint);

            //        //    var viewX = (float)OM.ReduceValueToInterval(orbitSatellitePoint.X, -orbit.Radius, orbit.Radius, -1, 1);
            //        //    var viewY = (float)OM.ReduceValueToInterval(orbitSatellitePoint.Z, -orbit.Radius, orbit.Radius, -1, 1); 
            //        //    var viewZ = (float)OM.ReduceValueToInterval(orbitSatellitePoint.Y, -orbit.Radius, orbit.Radius, -1, 1);

            //        //    var p1 = new PointGeometryModel3D()
            //        //    {
            //        //        Size = new Size(5, 5),
            //        //        Color = Colors.Red,
            //        //        Figure = PointFigure.Ellipse,
            //        //        FigureRatio = 0.12,
            //        //        Geometry = new PointGeometry3D()
            //        //        {
            //        //            Indices = new IntCollection(),
            //        //            Positions = new Vector3Collection()
            //        //            {
            //        //                new Vector3(viewX, viewY, viewZ),
            //        //            },
            //        //        }
            //        //    };
            //        //    _Viewport3DX.Items.Add(p1);
            //        //});
            //        await Task.Delay(5);
            //    }

            //    _Canceled = false;
            //});
        }

        private void Stop()
        {
            _Canceled = true;
        }
    }
}
