using System;
using System.Linq;
using System.Reflection;
using BarkMoon.GameComposition.Core.Services.DI;
using BarkMoon.GameComposition.Core.Interfaces;
using BarkMoon.GridPlacement.Core.UserScope;

namespace BarkMoon.GameComposition.Core.Examples
{
    /// <summary>
    /// Example game-level composition root that demonstrates the plugin installer pattern.
    /// Shows how external systems should install and access plugins through service registry.
    /// </summary>
    public class GameCompositionRoot
    {
        private readonly ServiceRegistry _registry;
        private readonly IPluginInstaller[] _installers;

        /// <summary>
        /// Initializes the game composition root with all available plugins.
        /// </summary>
        public GameCompositionRoot()
        {
            _registry = new ServiceRegistry();
            _installers = DiscoverAndInstallPlugins();
        }

        /// <summary>
        /// Creates a new service scope for user-specific operations.
        /// </summary>
        public IServiceScope CreateScope() => _registry.CreateScope();

        /// <summary>
        /// Discovers all plugin installers and installs them in the service registry.
        /// </summary>
        private IPluginInstaller[] DiscoverAndInstallPlugins()
        {
            // In a real application, this would discover plugins dynamically
            // For this example, we'll manually install GridPlacement
            var installers = new IPluginInstaller[]
            {
                new GridPlacementInstaller()
                // Add other plugin installers here as they become available:
                // new GameUserSessionsInstaller(),
                // new ItemDropsInstaller(),
                // new ArtisanCraftInstaller(),
            };

            // Install each plugin
            foreach (var installer in installers)
            {
                try
                {
                    installer.ConfigureServices(_registry);
                    Console.WriteLine($"Successfully installed plugin: {installer.GetType().Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to install plugin {installer.GetType().Name}: {ex.Message}");
                    throw;
                }
            }

            return installers;
        }

        /// <summary>
        /// Example of how external systems access orchestrators through service registry.
        /// </summary>
        public void ExampleGameLevelIntegration()
        {
            // Create scope for this game session
            using var scope = CreateScope();

            // Access GridPlacement orchestrators through service registry (correct pattern)
            var targetingOrchestrator = scope.GetService<BarkMoon.GridPlacement.Core.Workflows.TargetingWorkflowOrchestrator>();
            var placementOrchestrator = scope.GetService<BarkMoon.GridPlacement.Core.Workflows.PlacementWorkflowOrchestrator>();
            var manipulationOrchestrator = scope.GetService<BarkMoon.GridPlacement.Core.Workflows.ManipulationWorkflowOrchestrator>();

            // Use orchestrators for game-level coordination
            if (targetingOrchestrator != null && placementOrchestrator != null)
            {
                Console.WriteLine($"Coordinating game systems with orchestrators:");
                Console.WriteLine($"  - Targeting: {targetingOrchestrator.OrchestratorId}");
                Console.WriteLine($"  - Placement: {placementOrchestrator.OrchestratorId}");
                Console.WriteLine($"  - Manipulation: {manipulationOrchestrator?.OrchestratorId}");
            }

            // Example: Generic orchestrator access through common interface
            var allOrchestrators = new[]
            {
                scope.GetService<BarkMoon.GridPlacement.Core.Workflows.TargetingWorkflowOrchestrator>(),
                scope.GetService<BarkMoon.GridPlacement.Core.Workflows.PlacementWorkflowOrchestrator>(),
                scope.GetService<BarkMoon.GridPlacement.Core.Workflows.ManipulationWorkflowOrchestrator>(),
                scope.GetService<BarkMoon.GridPlacement.Core.Workflows.CursorWorkflowOrchestrator>(),
                scope.GetService<BarkMoon.GridPlacement.Core.Workflows.GridPlacementMasterOrchestrator>()
            };

            Console.WriteLine($"Total orchestrators available: {allOrchestrators.Count(o => o != null)}");
        }

        /// <summary>
        /// Example of cross-plugin coordination through service registry.
        /// </summary>
        public void ExampleCrossPluginCoordination()
        {
            using var scope = CreateScope();

            // Get orchestrators from different plugins (all through same registry)
            var gridPlacementOrchestrators = new[]
            {
                scope.GetService<BarkMoon.GridPlacement.Core.Workflows.TargetingWorkflowOrchestrator>(),
                scope.GetService<BarkMoon.GridPlacement.Core.Workflows.PlacementWorkflowOrchestrator>()
            };

            // When other plugins are available, they would be accessed the same way:
            // var userSessionsOrchestrator = scope.GetService<GameUserSessions.Workflows.UserSessionOrchestrator>();
            // var itemDropsOrchestrator = scope.GetService<ItemDrops.Workflows.ItemDropsOrchestrator>();

            Console.WriteLine($"Cross-plugin coordination available through unified service registry");
            Console.WriteLine($"GridPlacement orchestrators: {gridPlacementOrchestrators.Count(o => o != null)}");
        }

        /// <summary>
        /// Validates that all plugins follow the installer pattern correctly.
        /// </summary>
        public void ValidatePluginInstallation()
        {
            Console.WriteLine("Validating plugin installation patterns...");

            foreach (var installer in _installers)
            {
                var installerType = installer.GetType();
                
                // Verify installer implements IPluginInstaller
                if (!typeof(IPluginInstaller).IsAssignableFrom(installerType))
                {
                    throw new InvalidOperationException($"Installer {installerType.Name} does not implement IPluginInstaller");
                }

                // Verify installer has correct method signature
                var configureMethod = installerType.GetMethod("ConfigureServices");
                if (configureMethod == null)
                {
                    throw new InvalidOperationException($"Installer {installerType.Name} missing ConfigureServices method");
                }

                var parameters = configureMethod.GetParameters();
                if (parameters.Length != 1 || parameters[0].ParameterType != typeof(ServiceRegistry))
                {
                    throw new InvalidOperationException($"Installer {installerType.Name} has incorrect ConfigureServices signature");
                }

                Console.WriteLine($"✓ {installerType.Name} follows correct installer pattern");
            }

            Console.WriteLine("All installers validated successfully!");
        }

        /// <summary>
        /// Demonstrates proper cleanup and disposal of resources.
        /// </summary>
        public void Dispose()
        {
            _registry?.Dispose();
            Console.WriteLine("Game composition root disposed successfully");
        }
    }

    /// <summary>
    /// Example usage of the game composition root in a real application.
    /// </summary>
    public class GameApplication
    {
        private GameCompositionRoot _compositionRoot;

        public void Initialize()
        {
            Console.WriteLine("Initializing game application...");

            // Create composition root (installs all plugins)
            _compositionRoot = new GameCompositionRoot();

            // Validate installation
            _compositionRoot.ValidatePluginInstallation();

            // Example integration
            _compositionRoot.ExampleGameLevelIntegration();
            _compositionRoot.ExampleCrossPluginCoordination();

            Console.WriteLine("Game application initialized successfully!");
        }

        public void Shutdown()
        {
            Console.WriteLine("Shutting down game application...");
            _compositionRoot?.Dispose();
            Console.WriteLine("Game application shutdown complete!");
        }

        /// <summary>
        /// Example of creating user sessions with scoped services.
        /// </summary>
        public void CreateUserSession(string userId)
        {
            using var scope = _compositionRoot.CreateScope();
            
            // Each user gets their own scoped orchestrators
            var targetingOrchestrator = scope.GetService<BarkMoon.GridPlacement.Core.Workflows.TargetingWorkflowOrchestrator>();
            
            Console.WriteLine($"Created user session {userId} with orchestrator {targetingOrchestrator?.OrchestratorId}");
            
            // User session automatically disposed when scope ends
        }
    }
}
