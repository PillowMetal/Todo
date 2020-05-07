using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Todo.Contexts;
using Todo.Services;

namespace Todo
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public static void ConfigureServices(IServiceCollection services)
        {
            _ = services.AddDbContext<TodoContext>(options => options.UseInMemoryDatabase("TodoList"));

            _ = services
                .AddControllers(options => options.ReturnHttpNotAcceptable = true)
                .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver())
                .AddXmlDataContractSerializerFormatters();

            _ = services.Configure<MvcOptions>(options => options
                .OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>().First()
                .SupportedMediaTypes.Add("application/vnd.usbe.hateoas+json"));

            _ = services.AddTransient<IPropertyMappingService, PropertyMappingService>();
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                _ = app.UseDeveloperExceptionPage();

            _ = app.UseDefaultFiles();
            _ = app.UseStaticFiles();

            _ = app.UseHttpsRedirection();

            _ = app.UseRouting();

            _ = app.UseAuthorization();

            _ = app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
