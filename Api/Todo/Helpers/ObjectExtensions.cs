#nullable enable
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using static System.String;

namespace Todo.Helpers
{
    public static class ObjectExtensions
    {
        public static ExpandoObject ShapeData<T>(this T source, string? fields = null)
        {
            var dataShapedObject = new ExpandoObject();

            PropertyInfo? idPropertyInfo = typeof(T).GetProperty("id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            _ = dataShapedObject.TryAdd((idPropertyInfo?.Name).ToLowerFirstChar(), idPropertyInfo?.GetValue(source));

            if (IsNullOrWhiteSpace(fields))
                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    _ = dataShapedObject.TryAdd(propertyInfo.Name.ToLowerFirstChar(), propertyInfo.GetValue(source));
            else
                foreach (PropertyInfo? propertyInfo in fields.Split(',').Select(field =>
                    typeof(T).GetProperty(field.Trim(), BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)))
                    _ = dataShapedObject.TryAdd((propertyInfo?.Name).ToLowerFirstChar(), propertyInfo?.GetValue(source));

            return dataShapedObject;
        }
    }
}
