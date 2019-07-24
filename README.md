# SitecoreHost.MessageBus.Plugin
Plugin for SitecoreHost that allows to connect to Sitecore message bus, tested with IdentityServer as a host.

Simple Sitecore Host plugin, contains two parts:
* Command to send a test message `dotnet Sitecore.IdentityServer.Host.dll mb-test "message text"`
* Plugin that subscribes to these messages (also handles `/mb/status` url)

Messages are stored in Sitecore Messaging database, default Sitecore message queue can be used.
