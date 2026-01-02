using System.Reflection;
using BarkMoon.GameComposition.Core.Services.DI;

namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Common interface for plugin installers to ensure consistent registration patterns.
    /// This is a marker interface - all installers should follow the same method signature.
    /// </summary>
    public interface IPluginInstaller
    {
        /// <summary>
        /// Configures services for this plugin in the provided service registry.
        /// All installers must follow this exact signature for consistency.
        /// </summary>
        /// <param name="registry">The service registry to configure.</param>
        void ConfigureServices(ServiceRegistry registry);
    }
}
