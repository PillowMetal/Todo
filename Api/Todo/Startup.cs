using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Todo.Contexts;
using Todo.Services;

namespace Todo
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "ASP0000:Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'", Justification = "<Pending>")]
        public static void ConfigureServices(IServiceCollection services)
        {
            _ = services.AddDbContext<TodoContext>(options => options.UseInMemoryDatabase("TodoList"));

            _ = services
                .AddControllers(options =>
                {
                    options.ReturnHttpNotAcceptable = true;
                    options.OutputFormatters.OfType<SystemTextJsonOutputFormatter>().First().SupportedMediaTypes.Add("application/vnd.usbe.hateoas+json");

                    options.InputFormatters.Insert(0, new ServiceCollection()
                        .AddLogging().AddControllers().AddNewtonsoftJson().Services.BuildServiceProvider()
                        .GetRequiredService<IOptions<MvcOptions>>().Value.InputFormatters.OfType<NewtonsoftJsonPatchInputFormatter>().First());
                })
                .AddXmlDataContractSerializerFormatters()
                .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandling = ReferenceHandling.Preserve);

            _ = services.AddTransient<IPropertyMappingService, PropertyMappingService>();
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _ = env.IsDevelopment()
                ? app.UseDeveloperExceptionPage()
                : app.UseExceptionHandler(builder => builder.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                }));

            _ = app.UseDefaultFiles();
            _ = app.UseStaticFiles();

            _ = app.UseHttpsRedirection();

            _ = app.UseRouting();

            _ = app.UseAuthorization();

            _ = app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
