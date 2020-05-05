using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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
                propertyInfos.AddRange(fields.Split(',').Select(field =>
                    typeof(T).GetProperty(field.Trim(), BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)));

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
