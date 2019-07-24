namespace Sitecore.MessageBus.Plugin.Configuration
{
    public class PluginSettings
    {
        public static readonly string SectionName = "Sitecore:MessageBus";

        public SqlConnectionSettings SqlConnection { get; set; }
    }
}
