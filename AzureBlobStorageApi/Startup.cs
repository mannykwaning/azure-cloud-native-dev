using AzureBlobStorageApi.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;

namespace AzureBlobStorageApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Setup swagger 
            SetupSwaggerDocuments(services);

            //services.AddSingleton(CreateStorageAccountSettings);
            //services.AddSingleton(CreatePictureSettings);

            // Configure Repositories for injection

            services.AddControllers();

            // DEMO: Enable to allow manual handling of model binding errors
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // DEMO: Enable multi-stream read
            services.AddTransient<MultiStreamReadMiddleware>();

        }

        /// <summary>
        /// Creates picture settings
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        /*private IPictureSettings CreatePictureSettings(IServiceProvider arg)
        {
            return Configuration.GetSection(nameof(PictureSettings)).Get<PictureSettings>();
        }*/

        /// <summary>
        /// Creates storage account settings
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        /*private IStorageAccountSettings CreateStorageAccountSettings(IServiceProvider arg)
        {
            return Configuration.GetSection(nameof(StorageAccountSettings)).Get<StorageAccountSettings>();
        }*/

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            SetupSwaggerJsonGeneratgionAndUI(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }


        private void SetupSwaggerDocuments(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Azure Blob Storage Assignment III API",
                    Version = "v1",
                    Description = "Azure Blob Storage Assignment III solution"
                });

                // For UI to show the upload button
                c.MapType<Stream>(() => new OpenApiSchema { Type = "file" });

                c.CustomOperationIds(apiDesc =>
                {
                    return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
                });

                c.CustomSchemaIds(x => x.FullName);

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

            });
        }

        /// <summary>
        /// Swagger Json File and Swagger UI setup
        /// </summary>
        /// <param name="app"></param>
        private void SetupSwaggerJsonGeneratgionAndUI(IApplicationBuilder app)
        {
            app.UseSwagger(c =>
            {
                // Enabling V2 disables the file upload UI in swagger.
                c.SerializeAsV2 = false;
            });

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Azure Blob Storage Assignment III API V1");

                // Serve the Swagger UI at the app's root (http://localhost:<port>)
                c.RoutePrefix = string.Empty;
            });
        }
    }
}
