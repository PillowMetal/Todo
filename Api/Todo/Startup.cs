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
using Microsoft.OpenApi.Models;
using Todo.Contexts;
using Todo.Services;
using static System.IO.Compression.CompressionLevel;
using static Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults;

namespace Todo
{
    public class Startup
    {
        private const string SwaggerEndpoint = "open-api-specification";
        private const string SwaggerTitle = "Todo API";

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
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = true;
                options.MimeTypes = MimeTypes.Concat(new[]
                {
                    usbeHateoasMediaType,
                    "application/vnd.usbe.todoitem.full+json",
                    "application/vnd.usbe.todoitem.full.hateoas+json",
                    "application/vnd.usbe.todoitem.friendly+json",
                    "application/vnd.usbe.todoitem.friendly.hateoas+json"
                });
            });

            _ = services.Configure<BrotliCompressionProviderOptions>(options => options.Level = Optimal);
            _ = services.Configure<GzipCompressionProviderOptions>(options => options.Level = Optimal);

            _ = services.AddResponseCaching();
            _ = services.AddHttpCacheHeaders(options => options.MaxAge = 30, options => options.MustRevalidate = true);

            _ = services.AddSwaggerGen(options => options.SwaggerDoc(SwaggerEndpoint, new OpenApiInfo
            {
                Title = SwaggerTitle,
                Version = "1"
            }));

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

            _ = app.UseResponseCompression();
            _ = app.UseResponseCaching();

            _ = app.UseSwagger();

            _ = app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint($"{SwaggerEndpoint}/swagger.json", SwaggerTitle);
                options.DocumentTitle += " - Todo";
            });

            _ = app.UseDefaultFiles();
            _ = app.UseStaticFiles();

            _ = app.UseHttpCacheHeaders();

            _ = app.UseRouting();

            _ = app.UseAuthorization();

            _ = app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
