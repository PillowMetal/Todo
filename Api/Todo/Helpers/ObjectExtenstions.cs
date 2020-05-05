#nullable enable
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using static System.String;

namespace Todo.Helpers
{
    public static class ObjectExtenstions
    {
        public static ExpandoObject ShapeData<T>(this T source, string fields)
        {
            var dataShapedObject = new ExpandoObject();

            if (IsNullOrWhiteSpace(fields))
                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    _ = dataShapedObject.TryAdd(propertyInfo.Name, propertyInfo.GetValue(source));
            else
                foreach (string field in fields.Split(','))
                {
                    PropertyInfo? propertyInfo = typeof(T).GetProperty(field.Trim(), BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                    if (propertyInfo == null)
                        throw new Exception($"Property {field.Trim()} wasn't found on {typeof(T)}");

                    _ = dataShapedObject.TryAdd(propertyInfo.Name, propertyInfo.GetValue(source));
                }

            return dataShapedObject;
        }
    }
}
