using System.Collections.Generic;

namespace Todo.Services
{
    public class PropertyMapping<TSource, TDestination> : IPropertyMapping
    {
        public IDictionary<string, PropertyMappingValue> MappingDictionary { get; }

        public PropertyMapping(IDictionary<string, PropertyMappingValue> dictionary) => MappingDictionary = dictionary;
    }
}
