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
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy, Dictionary<string, PropertyMappingValue> propertyMapping)
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

                foreach (string property in propertyMapping[propertyName].DestinationProperties)
                {
                    descending = propertyMapping[propertyName].Revert ? !descending : descending;
                    orderByString += Concat(IsNullOrWhiteSpace(orderByString) ? Empty : ", ", property, descending ? " descending" : " ascending");
                }
            }

            return source.OrderBy(orderByString);
        }
    }
}
