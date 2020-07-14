using Dissertation.Algorithms.Model;
using Dissertation.Basics.Mapping;
using Dissertation.Model;

namespace Dissertation.Adapters
{
    public class SatelliteAdapter : AdapterBase<SatelliteModel, SatelliteOld>
    {
        public override SatelliteOld ToDto(IMapper mapper, SatelliteModel model)
        {
            var satellite = new SatelliteOld(model.NodeLongitude, model.LatitudeArgument, model.Number);
            return satellite;
        }

        public override SatelliteModel ToModel(IMapper mapper, SatelliteOld dto)
        {
            var satelliteModel = new SatelliteModel()
            {
                LatitudeArgument = dto.LatitudeArgument,
                NodeLongitude = dto.NodeLongitude,
                Number = dto.Number,
            };
            return satelliteModel;
        }
    }
}
