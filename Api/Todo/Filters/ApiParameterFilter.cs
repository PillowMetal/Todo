using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using static System.StringComparison;

namespace Todo.Filters
{
    public class ApiParameterFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context) => context.ApiDescription.ParameterDescriptions
            .Where(static description => description.ParameterDescriptor.BindingInfo.BindingSource?.Id == "Header")
            .Where(static description => description.ParameterDescriptor.BindingInfo.BinderModelName == "Accept")
            .ToList()
            .ForEach(description => operation.Parameters
                .Remove(operation.Parameters.Single(parameter => parameter.Name.Equals(description.Name, OrdinalIgnoreCase))));
    }
}
