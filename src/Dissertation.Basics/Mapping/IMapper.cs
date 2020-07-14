namespace Dissertation.Basics.Mapping
{
    public interface IMapper
    {
        TModel ToModel<TModel, TDto>(TDto dto);
        TDto ToDto<TModel, TDto>(TModel model);
        IMapper RegisterAdapter<TModel, TDto, TAdapter>(TAdapter adapter)
            where TAdapter : IAdapter<TModel, TDto>;
    }
}
