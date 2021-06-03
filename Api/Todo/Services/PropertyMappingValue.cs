using System.Collections.Generic;

namespace Todo.Services
{
    public class PropertyMappingValue
    {
        public IEnumerable<string> DestinationProperties { get; }
        public bool Revert { get; }

        public PropertyMappingValue(IEnumerable<string> properties, bool revert = default)
        {
            DestinationProperties = properties;
            Revert = revert;
        }
    }
}
