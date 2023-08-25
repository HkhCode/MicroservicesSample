using System;
using System.Net.Http;
using Amazon.Runtime.Internal.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

namespace Play.Inventory.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        private static void AddCatalogClient(IServiceCollection services)
        {
            Random jitterer = new Random();
            services.AddHttpClient<CatalogClient>(client => client.BaseAddress = new Uri("https://localhost:5001"))
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(5, time => TimeSpan.FromSeconds(Math.Pow(2, time))
                   + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
            onRetry: (outcome, timespan, retryAttemp) =>
            {
                var serviceProvicer = services.BuildServiceProvider();
                serviceProvicer.GetService<ILogger<CatalogClient>>()?.LogWarning($"Delaying For {timespan.TotalSeconds} seconds , then making retry {retryAttemp}");
            }))
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                3,
                TimeSpan.FromSeconds(15),
                onBreak: (outcome, timespan) =>
                {
                    var serviceProvicer = services.BuildServiceProvider();
                    serviceProvicer.GetService<ILogger<CatalogClient>>()?.LogWarning($"Opening Cicuit For {timespan.TotalSeconds} seconds ...");
                },
                onReset: () =>
                {
                    var serviceProvicer = services.BuildServiceProvider();
                    serviceProvicer.GetService<ILogger<CatalogClient>>()?.LogWarning($"Closing The Circuit ... ");
                }
            ))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
        }


        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMongo()
            .AddMongoRepository<InventoryItem>("inventoryItems")
            .AddMongoRepository<CatalogItem>("catalogitems")
            .AddMassTransitWithRabbitMq();
            AddCatalogClient(services);
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Inventory.Service", Version = "v1" });
            });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Inventory.Service v1"));
            }

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
