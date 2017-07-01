using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fibon.Messages.Commands;
using Fibon.Service.Framework;
using Fibon.Service.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RawRabbit.vNext;
using RawRabbit.vNext.Disposable;

namespace Fibon.Service
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.Configure<RabbitMqOptions>(Configuration.GetSection("rabbitmq"));

            ConfigureRabbitMq(services);            
        }

        private void ConfigurationRabbitMqSubscribtions(IApplicationBuilder app)
        {
           var client = app.ApplicationServices.GetService<IBusClient>();
            var handler = app.ApplicationServices.GetService<ICommandHandler<CalculateValueCommand>>();

            client.SubscribeAsync<CalculateValueCommand>(async (msg,context)=>
            {
                await handler.HandleAsync(msg);
            }           
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
            ConfigurationRabbitMqSubscribtions(app);
        }

        private void ConfigureRabbitMq(IServiceCollection services)
        {
            var options = new RabbitMqOptions();

            var section =  Configuration.GetSection("rabbitmq");
            section.Bind(options);

            var client = BusClientFactory.CreateDefault(options);

            services.AddSingleton<IBusClient>(_ => client);
            services.AddScoped<ICommandHandler<CalculateValueCommand>,CalculatedValueCommandHandler>();
        }
    }
}
