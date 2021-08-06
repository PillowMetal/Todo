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
        public static IReadOnlyCollection<ExpandoObject> ShapeData<T>(this IEnumerable<T> source, string fields, string keys = null)
        {
            List<PropertyInfo> propertyInfos = new();

            if (IsNullOrWhiteSpace(fields))
                propertyInfos.AddRange(typeof(T).GetProperties(Public | Instance));
            else
            {
                if (!IsNullOrWhiteSpace(keys))
                    propertyInfos.AddRange(keys.Split(',').Select(static key => typeof(T).GetProperty(key.Trim(), Public | Instance | IgnoreCase)));

                propertyInfos.AddRange(fields.Split(',').Select(static field => typeof(T).GetProperty(field.Trim(), Public | Instance | IgnoreCase)));
                propertyInfos = propertyInfos.Distinct().ToList();
            }

            List<ExpandoObject> expandoObjects = new();

            foreach (T sourceObject in source)
            {
                ExpandoObject expandoObject = new();

                propertyInfos.ForEach(propertyInfo => expandoObject.TryAdd(propertyInfo.Name.ToLowerFirstChar(), propertyInfo.GetValue(sourceObject)));
                expandoObjects.Add(expandoObject);
            }

            return expandoObjects;
        }
    }
}
