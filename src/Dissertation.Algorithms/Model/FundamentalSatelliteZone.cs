namespace Dissertation.Algorithms.Model
{
    /// <summary>
    /// Параметры фундаментальной области Эk
    /// </summary>
    public class FundamentalSatelliteZone
    {
        /// <summary>
        /// Относительное расположение области Эk
        /// </summary>
        public double Tetta1 { get; }

        /// <summary>
        /// Долгота трассы спутника
        /// </summary>
        public double Tetta2 { get; }

        /// <summary>
        /// Количество смещений до совпадения с область Э1
        /// </summary>
        public int Gk { get; }

        public SatelliteOld BoundedSattelite { get; }

        public FundamentalSatelliteZone(double sr1, int gk, double sr2, SatelliteOld sattelite)
        {
            BoundedSattelite = sattelite;
            Tetta1 = sr1;
            Tetta2 = sr2;
            Gk = gk;
        }
    }
}
