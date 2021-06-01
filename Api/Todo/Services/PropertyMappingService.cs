using System;
using System.Collections.Generic;
using System.Linq;
using Todo.Entities;
using Todo.Models;
using static System.Reflection.BindingFlags;
using static System.String;
using static System.StringComparer;

namespace Todo.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly Dictionary<string, PropertyMappingValue> _todoItemPropertyMapping = new(OrdinalIgnoreCase)
        {
            { "Id", new PropertyMappingValue(new[] { "Id" }) },
            { "Name", new PropertyMappingValue(new[] { "Name" }) },
            { "Tags", new PropertyMappingValue(new[] { "Project", "Context" }) },
            { "Age", new PropertyMappingValue(new[] { "Date" }, true) },
            { "IsComplete", new PropertyMappingValue(new[] { "IsComplete" }) }
        };

        private readonly List<IPropertyMapping> _propertyMappings = new();

        public PropertyMappingService() => _propertyMappings.Add(new PropertyMapping<TodoItemDto, TodoItem>(_todoItemPropertyMapping));

        public IReadOnlyDictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>() => _propertyMappings
            .OfType<PropertyMapping<TSource, TDestination>>().Single().MappingDictionary;

        public bool IsValidMapping<TSource, TDestination>(string orderBy) => IsNullOrWhiteSpace(orderBy) || orderBy.Split(',')
            .Select(clause => clause.Trim())
            .Select(trimmed => new { trimmed, index = trimmed.IndexOf(" ", StringComparison.Ordinal) })
            .Select(property => property.index == -1 ? property.trimmed : property.trimmed.Remove(property.index))
            .All(propertyName => GetPropertyMapping<TSource, TDestination>().ContainsKey(propertyName));

        public bool HasProperties<T>(string fields) => IsNullOrWhiteSpace(fields) || fields
            .Split(',').All(field => typeof(T).GetProperty(field.Trim(), Public | Instance | IgnoreCase) != null);
    }
}
