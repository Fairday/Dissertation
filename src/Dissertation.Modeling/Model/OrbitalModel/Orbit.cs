using ATS.MVVM.Command;
using ATS.MVVM.Core;
using Dissertation.Algorithms.Algorithms.Helpers;
using Dissertation.Algorithms.Algorithms.Model;
using Dissertation.Algorithms.Algorithms.Newton;
using Dissertation.Algorithms.Model;
using Dissertation.Algorithms.OrbitalMath;
using Dissertation.Algorithms.Resources;
using Dissertation.Modeling.Helpers;
using Dissertation.Modeling.Model.Basics;
using System;
using System.Windows.Input;

namespace Dissertation.Modeling.Model.OrbitalModel
{
    public class Orbit : VirtualBindableBase
    {
        public InputOrbitParameters InputOrbitParameters { get => Get(); private set => Set(value); }
        /// <summary>
        /// Количество витков
        /// </summary>
        public int NCoil => InputOrbitParameters.NCoil;
        /// <summary>
        /// Количество суток
        /// </summary>
        public int NDay => InputOrbitParameters.NDay;
        /// <summary>
        /// Радиус орбиты
        /// </summary>
        public double Radius { get => Get(); set => Set(value); }
        /// <summary>
        /// Высота орбиты
        /// </summary>
        public double Height { get => Get(); set => Set(value); }
        /// <summary>
        /// Межвитковое расстояние
        /// </summary>
        public double DxTurn { get => Get(); private set => Set(value); }
        /// <summary>
        /// Межузловое расстояние
        /// </summary>
        public double DxNode { get => Get(); private set => Set(value); }
        /// <summary>
        /// Межвитковое расстояние (градусы)
        /// </summary>
        public double DxTurnGrad { get => Get(); private set => Set(value); }
        /// <summary>
        /// Межузловое расстояние (градусы)
        /// </summary>
        public double DxNodeGrad { get => Get(); private set => Set(value); }
        /// <summary>
        /// Период орбиты
        /// </summary>
        public double EraTurn { get => Get(); private set => Set(value); }
        /// <summary>
        /// Период повторяемости трассы
        /// </summary>
        public double EraTier { get => Get(); private set => Set(value); }
        /// <summary>
        /// Нормализатор времемени по величине периода повторяемости трассы
        /// </summary>
        public RangeNormalizer ModEraTier { get => Get(); private set => Set(value); }
        /// <summary>
        /// Угловая скорость изменения восходящего узла спутника    
        /// </summary>
        public double DeltaLongitudeAscent { get => Get(); private set => Set(value); }
        /// <summary>
        /// Угловая скорость изменения аргумента широты спутника   
        /// </summary>
        public double DeltaLatitudeArgument { get => Get(); private set => Set(value); }
        /// <summary>
        /// Угловая скорость вращения спутника по орбите
        /// </summary>
        public double OrbitSatelliteAngleSpeed { get => Get(); private set => Set(value); }
        /// <summary>
        /// Прецессия ДВУ орбиты
        /// </summary>
        public double OrbitPrecession { get => Get(); private set => Set(value); }
        /// <summary>
        /// Значение межвита, подсчитанное через угловую скорость вращения Земли
        /// </summary>
        public double DxTurnRestored { get => Get(); private set => Set(value); }
        /// <summary>
        /// Радуис Земли
        /// </summary>
        public double EarchRadius { get => Get(); private set => Set(value); }

        //TODO: this is temp solution
        public Angle LeftCapture { get => Get(); private set => Set(value); }
        //TODO: this is temp solution
        public Angle RightCapture { get => Get(); private set => Set(value); }
        //TODO: this is temp solution
        public Angle FullCaptue { get => Get(); private set => Set(value); }

        //TODO: this is temp solution
        public void InitializeCaptures(ICaptureZone captureZone)
        {
            LeftCapture = new Angle(captureZone.AlphaLeft, true);
            RightCapture = new Angle(captureZone.AlphaRight, true);
            FullCaptue = new Angle(captureZone.Alpha, true);
        }

        public ICommand RecalculateOrbitCommand { get => Get(); private set => Set(value); }
        public bool RecalculateOrbitRadius { get => Get(); set => Set(value); }

        public Orbit(InputOrbitParameters inputOrbitParameters)
        {
            InputOrbitParameters = inputOrbitParameters;

            RecalculateOrbitRadius = true;

            RecalculateOrbitCommand = new RelayCommand(() =>
            {
                RecalculateOrbit();
            });

            CalculateOrbitParameters();
        }

        private void RecalculateOrbit()
        {
            CalculateOrbitParameters();
        }

        private void CalculateOrbitParameters()
        {
            ///Расчет межузловое расстояния
            DxNode = 2 * Math.PI / InputOrbitParameters.NCoil;
            DxNodeGrad = DxNode.ToGrad();
            ///Расчет межвиткого расстояния
            DxTurn = DxNode * InputOrbitParameters.NDay;
            DxTurnGrad = DxTurn.ToGrad();

            if (RecalculateOrbitRadius)
            {
                EarchRadius = OM.AverageEarchRadius(InputOrbitParameters.Inclination);
                ///Расчет радиуса орбиты
                var orbitAverageHeightAlgorithm = new OrbitAverageHeightAlgorithm(new NewtonAlgorithm());
                var orbitGeometryInfo = orbitAverageHeightAlgorithm.CalculateOrbitAverageHeight(AccuracyModel.CalculationAccuracy, new OrbitParameters(InputOrbitParameters.NCoil, InputOrbitParameters.NDay, InputOrbitParameters.Inclination), EarchRadius);
                Radius = orbitGeometryInfo.Radius;
                Height = orbitGeometryInfo.AverageHeight;
            }
            ///Расчет периода орбиты
            EraTurn = OM.Tdr(Radius, InputOrbitParameters.Inclination);
            ///Расчет период повторяемости трассы
            EraTier = EraTurn * InputOrbitParameters.NCoil;
            ModEraTier = new RangeNormalizer(EraTier);
            ///Расчет угловой скорости изменения восходящего узла спутника
            DeltaLongitudeAscent = -DxTurn / EraTurn;
            ///Расчет угловой скорости изменения аргумента широты спутника
            DeltaLatitudeArgument = 2 * Math.PI / EraTurn;
            ///Расчет угловой скорости вращения спутника по орбите
            OrbitSatelliteAngleSpeed = 2 * Math.PI / EraTurn;

            OrbitPrecession = -3 * Math.PI * Constants.J2 * Math.Pow(Constants.Re / Radius, 2) * Math.Cos(InputOrbitParameters.InclinationAngle.Rad);
            DxTurnRestored = Constants.we * EraTurn + (InputOrbitParameters.Inclination > 90 ? - Math.Abs(OrbitPrecession) : Math.Abs(OrbitPrecession));
        }
    }
}
