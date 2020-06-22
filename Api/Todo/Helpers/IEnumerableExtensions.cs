#nullable enable
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using static System.Reflection.BindingFlags;
using static System.String;

namespace Todo.Helpers
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<ExpandoObject> ShapeData<T>(this IEnumerable<T> source, string? fields, string? keys = null)
        {
            var propertyInfos = new List<PropertyInfo?>();

            if (IsNullOrWhiteSpace(fields))
                propertyInfos.AddRange(typeof(T).GetProperties(Public | Instance));
            else
            {
                if (!IsNullOrWhiteSpace(keys))
                    propertyInfos.AddRange(keys.Split(",").Select(key => typeof(T).GetProperty(key.Trim(), Public | Instance | IgnoreCase)));

                propertyInfos.AddRange(fields.Split(',').Select(field => typeof(T).GetProperty(field.Trim(), Public | Instance | IgnoreCase)));
                propertyInfos = propertyInfos.Distinct().ToList();
            }

            var expandoObjects = new List<ExpandoObject>();

            foreach (T sourceObject in source)
            {
                var expandoObject = new ExpandoObject();

                foreach (PropertyInfo? propertyInfo in propertyInfos)
                    _ = expandoObject.TryAdd((propertyInfo?.Name).ToLowerFirstChar(), propertyInfo?.GetValue(sourceObject));

                expandoObjects.Add(expandoObject);
            }

            return expandoObjects;
        }
    }
}
