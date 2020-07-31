using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Todo.Services
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    public class PropertyMapping<TSource, TDestination> : IPropertyMapping
    {
        public IDictionary<string, PropertyMappingValue> MappingDictionary { get; }

        public PropertyMapping(IDictionary<string, PropertyMappingValue> dictionary) => MappingDictionary = dictionary;
    }
}
