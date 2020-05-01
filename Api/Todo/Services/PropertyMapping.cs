using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Todo.Services
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    public class PropertyMapping<TSource, TDestination> : IPropertyMapping
    {
        public Dictionary<string, PropertyMappingValue> MappingDictionary { get; set; }

        public PropertyMapping(Dictionary<string, PropertyMappingValue> dictionary) => MappingDictionary = dictionary;
    }
}
