using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;
using Sitecore.Framework.Runtime.Configuration;
using Sitecore.MessageBus.Plugin.Configuration;
using Sitecore.MessageBus.Plugin.Messaging;

namespace Sitecore.MessageBus.Plugin
{
    public class ConfigureSitecore
    {
        private readonly ILogger<ConfigureSitecore> _logger;
        private readonly PluginSettings _settings;
        
        public ConfigureSitecore(ILogger<ConfigureSitecore> logger, ISitecoreConfiguration configuration)
        {
            this._logger = logger;
            this._settings = new PluginSettings();
            configuration.GetSection(PluginSettings.SectionName).Bind(_settings);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var sqlSettings = this._settings.SqlConnection;

            services.AutoRegisterHandlersFromAssemblyOf<TestHandler>();
            services.AddRebus(config =>
                config.Logging(l => l.ColoredConsole())
                    //.Sagas(s => s.StoreInSqlServer(settings.ConnectionString, "Sitecore_Sagas", "Sitecore_SagasIndex"))
                    .Transport(t => t.UseSqlServer(sqlSettings.ConnectionString, sqlSettings.QueueName))
                    .Routing(r =>
                        r.TypeBased().MapAssemblyOf<TestMessage>(sqlSettings.QueueName)
                    )
                    .Options(o =>
                    {
                        o.SetNumberOfWorkers(1);
                        o.SetMaxParallelism(1);
                    })
                    .Subscriptions(s => s.StoreInSqlServer(sqlSettings.ConnectionString, "Sitecore_Subscriptions", true))
            );
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRebus(async bus => await bus.Subscribe<TestMessage>());
            
            app.MapWhen(
                ctx => ctx.Request.Path.Value == "/mb/status",
                builder =>
                {
                    builder.Run(async (context) =>
                    {
                        await context.Response.WriteAsync("Rebus subscription is active, check log file");
                    });
                }
            );
        }
    }
}
