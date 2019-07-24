using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Sitecore.MessageBus.Plugin.Messaging
{
    public class TestHandler : IHandleMessages<TestMessage>
    {
        private readonly ILogger<TestHandler> _logger;

        public TestHandler(ILogger<TestHandler> logger)
        {
            this._logger = logger;
        }

        public Task Handle(TestMessage message)
        {
            new Rebus.Logging.ConsoleLoggerFactory(true).GetLogger<TestHandler>().Warn("TestHandler: " + message.Name);

            this._logger.LogInformation("TestHandler: " + message.Name);

            return Task.CompletedTask;
        }
    }
}
