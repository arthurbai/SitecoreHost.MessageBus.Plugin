using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Sitecore.Framework.Runtime.Commands;
using Sitecore.Framework.Runtime.Configuration;
using Sitecore.MessageBus.Plugin.Configuration;
using Sitecore.MessageBus.Plugin.Messaging;

namespace Sitecore.MessageBus.Plugin.Commands
{
    [Command("mb-test")]
    public class TestMessageBusConnection : SitecoreCommand
    {
        private readonly IConsole _console;
        private readonly IBus _bus;

        public TestMessageBusConnection(IConsole console, ISitecoreConfiguration configuration)
        {
            this._console = console;
            var settings = new PluginSettings();
            configuration.GetSection(PluginSettings.SectionName).Bind(settings);
            var sqlSettings = settings.SqlConnection;

            var activator = new BuiltinHandlerActivator();

            var bus = Configure.With(activator)
                .Logging(l => l.ColoredConsole())
                .Transport(t => t.UseSqlServer(sqlSettings.ConnectionString, sqlSettings.QueueName))
                .Routing(r =>
                    r.TypeBased().MapAssemblyOf<TestMessage>(sqlSettings.QueueName)
                )
                .Options(o =>
                {
                    o.SetNumberOfWorkers(0);
                    o.SetMaxParallelism(1);
                })
                .Subscriptions(s => s.StoreInSqlServer(sqlSettings.ConnectionString, "Sitecore_Subscriptions", true))
                .Start();

            this._bus = bus;
        }

        [Argument(0, Name = nameof(Message), Description = "Text to send")]
        public string Message { get; set; }

        public override Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            if (this._bus == null)
            {
                this._console.ForegroundColor = ConsoleColor.Red;
                this._console.WriteLine("bus is null");
                this._console.ForegroundColor = ConsoleColor.White;
                return Task.FromResult(1);
            }

            var message = new TestMessage {Name = $"{this.Message ?? "test message"}, sent {DateTime.Now:s}"};
            var header = new Dictionary<string, string> {{"key", "val"}};

            this._console.ForegroundColor = ConsoleColor.Cyan;
            this._console.WriteLine("Sending test message");
            this._console.ForegroundColor = ConsoleColor.Yellow;
            this._console.WriteLine(message.Name);
            this._console.ForegroundColor = ConsoleColor.White;

            this._bus.Publish(message, header).GetAwaiter().GetResult();

            return Task.FromResult(0);
        }
    }
}