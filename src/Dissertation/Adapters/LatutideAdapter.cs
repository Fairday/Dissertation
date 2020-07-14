using Dissertation.Algorithms.Model;
using Dissertation.Basics.Mapping;
using Dissertation.Model;

namespace Dissertation.Adapters
{
    public class LatutideAdapter : AdapterBase<LatitudeModel, Latitude>
    {
        public override Latitude ToDto(IMapper mapper, LatitudeModel model)
        {
            var latitude = new Latitude(model.Value);
            return latitude;
        }

        public override LatitudeModel ToModel(IMapper mapper, Latitude dto)
        {
            throw new System.NotImplementedException();
        }
    }
}
