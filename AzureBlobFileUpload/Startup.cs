using AzureBlobFileUpload.Repositories;
using AzureBlobFileUpload.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;

namespace AzureBlobFileUpload
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Setup Swagger
            SetupSwaggerDocuments(services);

            // Configure Settings
            services.AddSingleton(CreateStorageAccountSettings);
            services.AddSingleton(CreatePictureSettings);

            // Configure Repositories
            services.AddScoped(typeof(IStorageRepository), typeof(StorageRepository));

            services.AddControllers();
        }

        /// <summary>
        /// Creates the storage account settings
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private IStorageAccountSettings CreateStorageAccountSettings(IServiceProvider arg)
        {
            return Configuration.GetSection(nameof(StorageAccountSettings)).Get<StorageAccountSettings>();
        }

        /// <summary>
        /// Creates the picture settings
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private IFileSettings CreatePictureSettings(IServiceProvider arg)
        {
            return Configuration.GetSection(nameof(FileSettings)).Get<FileSettings>();
        }

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

        /// <summary>
        /// Sets up the swagger documents
        /// </summary>
        /// <param name="services">The service collection</param>
        private static void SetupSwaggerDocuments(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "HW3 Azure Blob Storage API",
                    Version = "v1",
                    Description = "Azure Blob Storage Api for Assignment III"
                });

                // Needed for the UI to show the upload button
                c.MapType<Stream>(() => new OpenApiSchema { Type = "file" });

                // Use method name as operationId so that ADD REST Client... will work
                c.CustomOperationIds(apiDesc =>
                {
                    return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
                });

                // Support for duplicate schema ids in .NET Core
                // See http://wegotcode.com/microsoft/swagger-fix-for-dotnetcore/
                c.CustomSchemaIds(x => x.FullName);

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

            });
        }

        /// <summary>
        /// Sets up the Swagger JSON file and Swagger Interactive UI
        /// </summary>
        /// <param name="app">The application builder</param>
        private static void SetupSwaggerJsonGeneratgionAndUI(IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = false;
            });

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "HW3 Azure Blob Storage API");

                // Serve the Swagger UI at the app's root (http://localhost:<port>)
                c.RoutePrefix = string.Empty;
            });
        }
    }
}
