using System;
using System.Collections.Generic;
using System.Linq;

namespace Dissertation.Basics.Mapping
{
    public class Mapper : IMapper
    {
        private Dictionary<string, object> _RegisteredAdapters = new Dictionary<string, object>();

        public TDto ToDto<TModel, TDto>(TModel model)
        {
            var adapter = GetAdapter<TModel, TDto>();
            return adapter.ToDto(this, model);
        }

        public TModel ToModel<TModel, TDto>(TDto dto)
        {
            var adapter = GetAdapter<TModel, TDto>();
            return adapter.ToModel(this, dto);
        }

        public IMapper RegisterAdapter<TModel, TDto, TAdapter>(TAdapter adapter)
            where TAdapter : IAdapter<TModel, TDto>
        {
            var hash = typeof(TModel).FullName + typeof(TDto).FullName;
            if (!_RegisteredAdapters.ContainsKey(hash))
            {
                _RegisteredAdapters[hash] = adapter;
            }
            else
            {
                throw new Exception("Adapter is already exist");
            }
            return this;
        }

        private IAdapter<TModel, TDto> GetAdapter<TModel, TDto>()
        {
            var hash = typeof(TModel).FullName + typeof(TDto).FullName;
            if (!_RegisteredAdapters.ContainsKey(hash))
            {
                throw new Exception("Adapter is not exist");
            }
            else
            {
                return (IAdapter<TModel, TDto>)_RegisteredAdapters.First(kvp => kvp.Key == hash).Value;
            }
        }
    }
}
