using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;
using Serilog;
using CDPHGenServices.Interfaces;
using CDPHGenServices;
using CDPHGenServices.Models;

namespace CDPHCCDService
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

            //services.AddControllers();
            services.AddControllers().AddXmlDataContractSerializerFormatters()
            .AddXmlSerializerFormatters()
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // If using Kestrel:
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // The following line enables Application Insights telemetry collection.
            services.AddApplicationInsightsTelemetry();

            services.AddSingleton<IConfiguration>(Configuration);


            services.AddTransient<IBlobStorageService>(s => new BlobStorageService(Configuration.GetValue<string>("ConnectionStrings:AccessKey"), Configuration.GetValue<string>("BlobStorageContainer")));

            FhirConverterConfig fhirConfig = new FhirConverterConfig();
            Configuration.Bind("FhirConverter", fhirConfig);
            services.AddTransient<ICcdToFhireService>(s => new CcdToFhireService(fhirConfig));

            services.AddSwaggerGen(options => 
            {
                options.SwaggerDoc(name: "v1", info: new OpenApiInfo
                { Title = "CDPH CCD Service API", Version = "v1"});
            });

            services.AddControllers();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(options => 
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json",
                "CDPH CCD Service API Version 1");

                options.SupportedSubmitMethods(new[] {
                    SubmitMethod.Post
                });
            });

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
