using Dissertation.Algorithms.Model;
using Dissertation.Basics.Mapping;
using Dissertation.Model;

namespace Dissertation.Adapters
{
    public class OrbitInfoAdapter : AdapterBase<OrbitInfo, OrbitParameters>
    {
        public override OrbitParameters ToDto(IMapper mapper, OrbitInfo model)
        {
            var op = new OrbitParameters(model.m, model.n, model.i);
            return op;
        }

        public override OrbitInfo ToModel(IMapper mapper, OrbitParameters dto)
        {
            throw new System.NotImplementedException();
        }
    }
}
