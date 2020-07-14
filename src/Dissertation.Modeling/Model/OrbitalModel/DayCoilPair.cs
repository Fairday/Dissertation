namespace Dissertation.Modeling.Model.OrbitalModel
{
    public struct DayCoilPair
    {
        public DayCoilPair(int nCoil, int nDay)
        {
            NDay = nDay;
            NCoil = nCoil;
        }

        public int NDay { get; }
        public int NCoil { get; }
    }
}
