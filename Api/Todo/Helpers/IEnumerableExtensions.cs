﻿#nullable enable
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using static System.Reflection.BindingFlags;
using static System.String;
using static System.StringComparison;

namespace Todo.Helpers
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<ExpandoObject> ShapeData<T>(this IEnumerable<T> source, string fields)
        {
            var propertyInfos = new List<PropertyInfo?>();

            if (IsNullOrWhiteSpace(fields))
                propertyInfos.AddRange(typeof(T).GetProperties(Public | Instance));
            else
            {
                propertyInfos.AddRange(fields.Split(',').Select(field =>
                    typeof(T).GetProperty(field.Trim(), Public | Instance | IgnoreCase)));

                if (propertyInfos.All(propertyInfo => !(propertyInfo?.Name ?? Empty).Equals("id", OrdinalIgnoreCase)))
                    propertyInfos.Insert(0, typeof(T).GetProperty("id", Public | Instance | IgnoreCase));
            }

            var expandoObjects = new List<ExpandoObject>();

            foreach (T sourceObject in source)
            {
                var dataShapedObject = new ExpandoObject();

                foreach (PropertyInfo? propertyInfo in propertyInfos)
                    _ = dataShapedObject.TryAdd((propertyInfo?.Name).ToLowerFirstChar(), propertyInfo?.GetValue(sourceObject));

                expandoObjects.Add(dataShapedObject);
            }

            return expandoObjects;
        }
    }
}