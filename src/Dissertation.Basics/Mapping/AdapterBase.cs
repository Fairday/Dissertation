namespace Dissertation.Basics.Mapping
{
    public abstract class AdapterBase<TModel, TDto> : IAdapter<TModel, TDto>
    {
        public abstract TDto ToDto(IMapper mapper, TModel model);
        public abstract TModel ToModel(IMapper mapper, TDto dto);
    }
}
