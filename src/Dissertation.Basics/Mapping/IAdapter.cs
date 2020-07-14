namespace Dissertation.Basics.Mapping
{
    public interface IAdapter<TModel, TDto>
    {
        TModel ToModel(IMapper mapper, TDto dto);
        TDto ToDto(IMapper mapper, TModel model);
    }
}
