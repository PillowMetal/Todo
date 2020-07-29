using System.Collections.Generic;

namespace Todo.Services
{
    public interface IPropertyMappingService
    {
        IDictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();
        bool IsValidMapping<TSource, TDestination>(string orderBy);
        bool HasProperties<T>(string fields);
    }
}
