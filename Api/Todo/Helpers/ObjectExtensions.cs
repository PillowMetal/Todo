﻿using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using static System.Reflection.BindingFlags;
using static System.String;

namespace Todo.Helpers
{
    public static class ObjectExtensions
    {
        public static ExpandoObject ShapeData<T>(this T source, string fields = null, string keys = null)
        {
            ExpandoObject expandoObject = new();

            if (IsNullOrWhiteSpace(fields))
                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties(Public | Instance))
                    _ = expandoObject.TryAdd(propertyInfo.Name.ToLowerFirstChar(), propertyInfo.GetValue(source));
            else
            {
                if (!IsNullOrWhiteSpace(keys))
                    foreach (PropertyInfo propertyInfo in keys.Split(',').Select(static key => typeof(T).GetProperty(key.Trim(), Public | Instance | IgnoreCase)))
                        _ = expandoObject.TryAdd((propertyInfo?.Name).ToLowerFirstChar(), propertyInfo?.GetValue(source));

                foreach (PropertyInfo propertyInfo in fields.Split(',').Select(static field => typeof(T).GetProperty(field.Trim(), Public | Instance | IgnoreCase)))
                    _ = expandoObject.TryAdd((propertyInfo?.Name).ToLowerFirstChar(), propertyInfo?.GetValue(source));
            }

            return expandoObject;
        }
    }
}
