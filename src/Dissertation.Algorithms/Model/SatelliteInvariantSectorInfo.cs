using Dissertation.Algorithms.Algorithms.Helpers;
using System;

namespace Dissertation.Algorithms.Model
{
    /// <summary>
    /// Инвариант потока наблюдений
    /// </summary>
    public class SatelliteInvariantSectorInfo
    {
        /// <summary>
        /// Участок наблюдения
        /// </summary>
        public double sGrad => s.ToGrad();

        /// <summary>
        /// Участок наблюдения
        /// </summary>
        public double s { get; }

        /// <summary>
        /// Число восходящих узлов
        /// </summary>
        public int d { get; }

        /// <summary>
        /// Число нисходящих узлов
        /// </summary>
        public int? f { get; }

        /// <summary>
        /// Число нисходящих узлов правее Hp
        /// </summary>
        public int? q { get; }

        /// <summary>
        /// Кратность покрытия точки r
        /// </summary>
        public int? l { get; }

        public SatelliteInvariantSectorInfo(double s, int d, int? f, int? q, int? l, ObservationVariant observationVariant)
        {
            this.s = s;
            this.d = d;
            this.f = f;
            this.q = q;
            this.l = l;
            ObservationVariant = observationVariant;
        }

        public SatelliteInvariantSectorInfo(double s, int d, int? f, int? q, int? l, Func<SatelliteInvariantSectorInfo, ObservationVariant> observationVariantInitializer)
        {
            this.s = s;
            this.d = d;
            this.f = f;
            this.q = q;
            this.l = l;
            ObservationVariant = observationVariantInitializer(this);
        }

        /// <summary>
        /// Вариант наблюдения
        /// </summary>
        public ObservationVariant ObservationVariant { get; private set; }

        public override string ToString()
        {
            return $"sGrad: {Math.Round(sGrad, 3)}, s: {s}, d: {d}, f: {f}, q: {q}, l: {l}";
        }
    }
}
