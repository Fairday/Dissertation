using System.Collections.Generic;

namespace Dissertation.Algorithms.Model
{
    public class ObservationAreaModel
    {
        public List<Latitude> Latitudes { get; set; }
        public List<Longitude> Longitudes { get; set; }

        public ObservationAreaModel()
        {
            Latitudes = new List<Latitude>();
            Longitudes = new List<Longitude>();
        }
    }
}
