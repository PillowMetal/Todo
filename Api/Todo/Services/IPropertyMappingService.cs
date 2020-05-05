using System.Collections.Generic;

namespace Todo.Services
{
    public interface IPropertyMappingService
    {
        Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();
        bool IsValidMapping<TSource, TDestination>(string orderBy);
        bool HasProperties<T>(string fields);
    }
}
