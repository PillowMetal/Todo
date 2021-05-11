using System.Collections.Generic;

namespace Todo.Services
{
    public class PropertyMapping<TSource, TDestination> : IPropertyMapping
    {
        public IReadOnlyDictionary<string, PropertyMappingValue> MappingDictionary { get; }

        public PropertyMapping(IReadOnlyDictionary<string, PropertyMappingValue> dictionary) => MappingDictionary = dictionary;
    }
}
