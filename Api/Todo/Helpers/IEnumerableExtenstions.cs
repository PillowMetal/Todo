#nullable enable
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using static System.String;

namespace Todo.Helpers
{
    public static class IEnumerableExtenstions
    {
        public static IEnumerable<ExpandoObject> ShapeData<T>(this IEnumerable<T> source, string fields)
        {
            var propertyInfos = new List<PropertyInfo>();

            if (IsNullOrWhiteSpace(fields))
                propertyInfos.AddRange(typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance));
            else
                foreach (string field in fields.Split(','))
                {
                    PropertyInfo? propertyInfo = typeof(T).GetProperty(field.Trim(), BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                    if (propertyInfo == null)
                        throw new Exception($"Property {field.Trim()} wasn't found on {typeof(T)}");

                    propertyInfos.Add(propertyInfo);
                }

            var expandoObjects = new List<ExpandoObject>();

            foreach (T sourceObject in source)
            {
                var dataShapedObject = new ExpandoObject();

                foreach (PropertyInfo propertyInfo in propertyInfos)
                    _ = dataShapedObject.TryAdd(propertyInfo.Name, propertyInfo.GetValue(sourceObject));

                expandoObjects.Add(dataShapedObject);
            }

            return expandoObjects;
        }
    }
}
