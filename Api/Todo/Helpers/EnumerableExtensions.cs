using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using static System.Reflection.BindingFlags;
using static System.String;

namespace Todo.Helpers
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<ExpandoObject> ShapeData<T>(this IEnumerable<T> source, string fields, string keys = null)
        {
            List<PropertyInfo> propertyInfos = new();

            if (IsNullOrWhiteSpace(fields))
                propertyInfos.AddRange(typeof(T).GetProperties(Public | Instance));
            else
            {
                if (!IsNullOrWhiteSpace(keys))
                    propertyInfos.AddRange(keys.Split(',').Select(key => typeof(T).GetProperty(key.Trim(), Public | Instance | IgnoreCase)));

                propertyInfos.AddRange(fields.Split(',').Select(field => typeof(T).GetProperty(field.Trim(), Public | Instance | IgnoreCase)));
                propertyInfos = propertyInfos.Distinct().ToList();
            }

            List<ExpandoObject> expandoObjects = new();

            foreach (T sourceObject in source)
            {
                ExpandoObject expandoObject = new();

                foreach (PropertyInfo propertyInfo in propertyInfos)
                    _ = expandoObject.TryAdd(propertyInfo.Name.ToLowerFirstChar(), propertyInfo.GetValue(sourceObject));

                expandoObjects.Add(expandoObject);
            }

            return expandoObjects;
        }
    }
}
