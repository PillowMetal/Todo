using System;
using System.Collections.Generic;
using System.Linq;
using Todo.Entities;
using Todo.Models;
using static System.Reflection.BindingFlags;
using static System.String;

namespace Todo.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly IDictionary<string, PropertyMappingValue> _todoItemPropertyMapping = new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
        {
            { "Id", new PropertyMappingValue(new List<string> { "Id" }) },
            { "Name", new PropertyMappingValue(new List<string> { "Name" }) },
            { "Tags", new PropertyMappingValue(new List<string> { "Project", "Context" }) },
            { "Age", new PropertyMappingValue(new List<string> { "Date" }, true) },
            { "IsComplete", new PropertyMappingValue(new List<string> { "IsComplete" }) }
        };

        private readonly IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService() => _propertyMappings.Add(new PropertyMapping<TodoItemDto, TodoItem>(_todoItemPropertyMapping));

        public IDictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>() => _propertyMappings
            .OfType<PropertyMapping<TSource, TDestination>>().First().MappingDictionary;

        public bool IsValidMapping<TSource, TDestination>(string orderBy)
        {
            if (IsNullOrWhiteSpace(orderBy))
                return true;

            IDictionary<string, PropertyMappingValue> propertyMapping = GetPropertyMapping<TSource, TDestination>();

            return orderBy.Split(',')
                .Select(clause => clause.Trim())
                .Select(trimmed => new { trimmed, index = trimmed.IndexOf(" ", StringComparison.OrdinalIgnoreCase) })
                .Select(property => property.index == -1 ? property.trimmed : property.trimmed.Remove(property.index))
                .All(propertyName => propertyMapping.ContainsKey(propertyName));
        }

        public bool HasProperties<T>(string fields) => IsNullOrWhiteSpace(fields) || fields
            .Split(',').All(field => typeof(T).GetProperty(field.Trim(), Public | Instance | IgnoreCase) != null);
    }
}
