using System;

namespace Dissertation.Algorithms.Model
{
    public class PeriodicityViewTaskParameters
    {
        public OrbitParameters OrbitParameters { get; }
        public ObservationAreaModel ObservationAreaModel { get; }

        public PeriodicityViewAlgorithmType PeriodicityViewAlgorithmType
        {
            get
            {
                if (ObservationAreaModel.Longitudes.Count != 0)
                    return PeriodicityViewAlgorithmType.EarthArea;
                else if (ObservationAreaModel.Latitudes.Count > 1)
                    return PeriodicityViewAlgorithmType.LattitudeRange;
                else
                    return PeriodicityViewAlgorithmType.Lattitude;
            }
        }

        public PeriodicityViewTaskParameters(OrbitParameters orbitParameters, ObservationAreaModel observationAreaModel)
        {
            OrbitParameters = orbitParameters;

            //if (observationAreaModel.Latitudes.Count == 0)
            //    throw new Exception("Не задано ни одной широты");

            //if (observationAreaModel.Longitudes.Count > 1 && observationAreaModel.Latitudes.Count != observationAreaModel.Longitudes.Count)
            //    throw new Exception("Количество заданных широт и долгот не равно");

            ObservationAreaModel = observationAreaModel;
        }
    }
}
