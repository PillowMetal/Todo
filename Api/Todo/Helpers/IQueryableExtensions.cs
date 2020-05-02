using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Todo.Services;
using static System.String;
using static System.StringComparison;

namespace Todo.Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy, Dictionary<string, PropertyMappingValue> dictionary)
        {
            if (IsNullOrWhiteSpace(orderBy))
                return source;

            string orderByString = Empty;
            string[] split = orderBy.Split(',');

            foreach (string clause in split)
            {
                string trimmed = clause.Trim();
                bool descending = trimmed.EndsWith(" desc", OrdinalIgnoreCase);
                int index = trimmed.IndexOf(" ", OrdinalIgnoreCase);
                string propertyName = index == -1 ? trimmed : trimmed.Remove(index);

                if (!dictionary.ContainsKey(propertyName))
                    throw new ArgumentException($"Key mapping for {propertyName} does not exist.");

                foreach (string property in dictionary[propertyName].DestinationProperties)
                {
                    descending = dictionary[propertyName].Revert ? !descending : descending;
                    orderByString += Concat(IsNullOrWhiteSpace(orderByString) ? Empty : ", ", property, descending ? " descending" : " ascending");
                }
            }

            return source.OrderBy(orderByString);
        }
    }
}
