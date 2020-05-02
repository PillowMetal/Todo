using System.Collections.Generic;
using System.Linq;
using Todo.Entities;
using Todo.Models;
using static System.StringComparer;

namespace Todo.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly Dictionary<string, PropertyMappingValue> _todoItemPropertyMapping = new Dictionary<string, PropertyMappingValue>(OrdinalIgnoreCase)
        {
            { "Id", new PropertyMappingValue(new List<string> { "Id" }) },
            { "Name", new PropertyMappingValue(new List<string> { "Name" }) },
            { "Tags", new PropertyMappingValue(new List<string> { "Project", "Context" }) },
            { "Age", new PropertyMappingValue(new List<string> { "Date" }, true) },
            { "IsComplete", new PropertyMappingValue(new List<string> { "IsComplete" }) }
        };

        private readonly IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService() => _propertyMappings.Add(new PropertyMapping<TodoItemDto, TodoItem>(_todoItemPropertyMapping));

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>() => _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>().First().MappingDictionary;
    }
}
