using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;
using TaskListApp.Data;
using TaskListApp.CustomSettings;

namespace TaskListApp
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
            services.AddControllers();

            // Register Task Repository
            //services.AddTransient<ITasksRepository, TasksRepository>();

            // Entity Framework: Register the context with dependency injection
            services.AddDbContext<TasksDatabaseContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Setup swagger 
            SetupSwaggerDocuments(services);

            // Custom Settings
            SetupCustomSettings(services);

            // Setup app insights
            //SetupApplicationInsights(services);

        }

        /// <summary>
        /// Application Insights Integration
        /// </summary>
        /// <param name="services"></param>
        /*private void SetupApplicationInsights(IServiceCollection services)
        {
            // Settings access
            ApplicationInsights applicationInsightsSettings = Configuration.GetSection("ApplicationInsights").Get<ApplicationInsights>();

            // Setup app insights
            services.AddApplicationInsightsTelemetry(applicationInsightsSettings.InstrumentationKey);

            // Setup live monitoring key to enable authentication and event filtering
            services.ConfigureTelemetryModule<QuickPulseTelemetryModule>((module, _) => module.AuthenticationApiKey = applicationInsightsSettings.AuthenticationApiKey);
        }*/

        private void SetupCustomSettings(IServiceCollection services)
        {
            // Support IOptions<T> injection
            services.AddOptions();

            // Adding class representation of settings for customer limits in json settings
            services.Configure<TasksLimits>(Configuration.GetSection(nameof(TasksLimits)));

            // Support Generic IConfiguration access for generic string access
            services.AddSingleton<IConfiguration>(Configuration);
        }

        /// <summary>
        /// Sets up the swagger documents
        /// </summary>
        /// <param name="services">The service collection</param>
        private static void SetupSwaggerDocuments(IServiceCollection services)
        {
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Tasks List Application",
                    Version = "v1",
                    Description = "Creates and manages taks lists",
                });

                // Use method name as operationId so that ADD REST Client... will work
                c.CustomOperationIds(apiDesc =>
                {
                    return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
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
        /// Sets up Swagger JSON file and Swagger Interactive UI
        /// </summary>
        /// <param name="app"></param>
        private static void SetupSwaggerJsonGeneratgionAndUI(IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c =>
            {
                // Use the older 2.0 format so the ADD REST Client... will work
                c.SerializeAsV2 = true;
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task List App v1");

                // Serve the Swagger UI at the app's root (http://localhost:<port>)
                c.RoutePrefix = string.Empty;
            });
        }
    }
}
