using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Todo.Contexts;
using Todo.Services;
using static System.IO.Compression.CompressionLevel;
using static Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults;

namespace Todo
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        [SuppressMessage("Design", "ASP0000:Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'", Justification = "<Pending>")]
        public static void ConfigureServices(IServiceCollection services)
        {
            _ = services.AddDbContext<TodoContext>(options => options.UseInMemoryDatabase("Todo"));

            const string usbeHateoasMediaType = "application/vnd.usbe.hateoas+json";

            _ = services
                .AddControllers(options =>
                {
                    options.ReturnHttpNotAcceptable = true;
                    options.OutputFormatters.OfType<SystemTextJsonOutputFormatter>().First().SupportedMediaTypes.Add(usbeHateoasMediaType);

                    options.InputFormatters.Insert(0, new ServiceCollection()
                        .AddLogging().AddControllers().AddNewtonsoftJson().Services.BuildServiceProvider()
                        .GetRequiredService<IOptions<MvcOptions>>().Value.InputFormatters.OfType<NewtonsoftJsonPatchInputFormatter>().First());
                })
                .AddXmlDataContractSerializerFormatters();

            _ = services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.Providers.Add<BrotliCompressionProvider>();
                options.EnableForHttps = true;
                options.MimeTypes = MimeTypes.Concat(new[] { usbeHateoasMediaType });
            });

            _ = services.Configure<GzipCompressionProviderOptions>(options => options.Level = Optimal);
            _ = services.Configure<BrotliCompressionProviderOptions>(options => options.Level = Optimal);

            _ = services.AddResponseCaching();
            _ = services.AddHttpCacheHeaders(options => options.MaxAge = 60, options => options.MustRevalidate = true);
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

            _ = app.UseHttpsRedirection();
            _ = app.UseDefaultFiles();
            _ = app.UseStaticFiles();

            _ = app.UseResponseCompression();
            _ = app.UseResponseCaching();
            _ = app.UseHttpCacheHeaders();

            _ = app.UseRouting();

            _ = app.UseAuthorization();

            _ = app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
