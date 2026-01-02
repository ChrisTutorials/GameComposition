# Plugin Installer Pattern Architecture

## Overview

The Plugin Installer Pattern establishes a consistent, testable, and maintainable approach for plugin service registration and lifecycle management. This pattern ensures that **service registry owns all orchestrators** while providing plugin autonomy through standardized installer classes.

## Architecture Pattern

### **Core Principle: Service Registry Ownership**
```
Service Registry (Owner)
├── Owns all orchestrators
├── Manages service lifetimes
├── Provides consumer access
└── Enforces architectural patterns
```

### **Plugin Autonomy Through Installers**
```
Plugin Installer Classes
├── Implement IPluginInstaller interface
├── Follow consistent registration pattern
├── Control plugin-specific services
└── Maintain plugin independence
```

## Complete Flow

### **1. Plugin Installation Phase**
```csharp
// Each plugin has its own installer
public class GridPlacementInstaller : IPluginInstaller
{
    public void ConfigureServices(ServiceRegistry registry)
    {
        // 1. Configuration (Singleton)
        RegisterConfiguration(registry);
        
        // 2. Core Services (Scoped)
        RegisterServices(registry);
        
        // 3. Orchestrators (Scoped)
        RegisterOrchestrators(registry);
        
        // 4. Event Bus (Singleton)
        RegisterEventBus(registry);
    }
}
```

### **2. Game-Level Composition Phase**
```csharp
// Game-level composition root installs all plugins
public class GameCompositionRoot
{
    private readonly ServiceRegistry _registry;
    
    public GameCompositionRoot()
    {
        _registry = new ServiceRegistry();
        InstallAllPlugins();
    }
    
    private void InstallAllPlugins()
    {
        var installers = new IPluginInstaller[]
        {
            new GridPlacementInstaller(),
            // new GameUserSessionsInstaller(),
            // new ItemDropsInstaller(),
        };
        
        foreach (var installer in installers)
        {
            installer.ConfigureServices(_registry);
        }
    }
}
```

### **3. Consumer Access Phase**
```csharp
// External consumers access through service registry
public class GameLevelSystem
{
    private readonly IServiceScope _scope;
    
    public GameLevelSystem(IServiceScope scope)
    {
        _scope = scope;
    }
    
    public void Initialize()
    {
        // Get orchestrators from service registry
        var targeting = _scope.GetService<TargetingWorkflowOrchestrator>();
        var placement = _scope.GetService<PlacementWorkflowOrchestrator>();
        
        // Coordinate systems through orchestrators
        CoordinateGameSystems(targeting, placement);
    }
}
```

## Registration Order Pattern

### **Enforced Order: Configuration → Services → Orchestrators → Event Bus**

```csharp
public void ConfigureServices(ServiceRegistry registry)
{
    // 1. Configuration (Singleton) - Must be first
    registry.RegisterSingleton(GridPlacementConfigurationFactory.CreateDefault());
    
    // 2. Core Services (Scoped) - Depend on configuration
    registry.RegisterScoped<TargetingService>();
    registry.RegisterScoped<PlacementService>();
    registry.RegisterScoped<ManipulationService2D>();
    
    // 3. Orchestrators (Scoped) - Depend on services
    registry.RegisterScoped<TargetingWorkflowOrchestrator>();
    registry.RegisterScoped<PlacementWorkflowOrchestrator>();
    registry.RegisterScoped<ManipulationWorkflowOrchestrator>();
    
    // 4. Event Bus (Singleton) - Shared by orchestrators
    if (!registry.IsRegistered<IEventBus>())
    {
        registry.RegisterSingleton<IEventBus, EventBus>();
    }
}
```

### **Why This Order Matters**

1. **Configuration First**: Services need configuration to initialize
2. **Services Second**: Orchestrators need services to coordinate
3. **Orchestrators Third**: Event bus needed for orchestrator communication
4. **Event Bus Last**: Shared singleton, registered once

## Service Lifetime Rules

### **Singleton Services**
- **Configuration**: Immutable settings shared across all users
- **Event Bus**: Single event coordination system
- **Registry Factories**: Shared service creation logic

### **Scoped Services**
- **All Orchestrators**: User-specific workflow coordination
- **Core Services**: User-specific business logic
- **State Management**: User-specific state containers

### **Why Orchestrators Are Scoped**
```csharp
// Each user gets their own orchestrator instances
using var scope = registry.CreateScope();
var user1Targeting = scope.GetService<TargetingWorkflowOrchestrator>();
var user2Targeting = scope.GetService<TargetingWorkflowOrchestrator>();

// Different instances for different users
user1Targeting.ShouldNotBe(user2Targeting);
user1Targeting.OrchestratorId.ShouldNotBe(user2Targeting.OrchestratorId);
```

## Architectural Enforcement

### **NetArchTest Validation**

#### **1. Installer Compliance Tests**
```csharp
[Fact]
public void All_Plugins_Should_Have_Installer_Class()
{
    // Verifies each plugin has installer implementing IPluginInstaller
    // Validates ConfigureServices method signature
    // Ensures consistent pattern across ecosystem
}
```

#### **2. Registration Order Tests**
```csharp
[Fact]
public void Installers_Should_Follow_Registration_Order_Pattern()
{
    // Uses TestServiceRegistry to capture registration order
    // Validates Configuration → Services → Orchestrators → Event Bus
    // Prevents dependency injection issues
}
```

#### **3. Lifetime Enforcement Tests**
```csharp
[Fact]
public void Orchestrators_Should_Be_Registered_As_Scoped()
{
    // Ensures all orchestrators are registered as scoped
    // Prevents shared state between users
    // Maintains proper isolation
}
```

#### **4. Event Bus Uniqueness Tests**
```csharp
[Fact]
public void Event_Bus_Should_Be_Registered_As_Singleton_Once()
{
    // Ensures IEventBus registered exactly once per plugin
    // Prevents multiple event bus instances
    // Maintains event coordination consistency
}
```

## Benefits of This Pattern

### **1. Clear Ownership**
- ✅ **Service Registry owns orchestrators** - Single source of truth
- ✅ **Plugin autonomy** - Each plugin controls its installation
- ✅ **No interface pollution** - No wrapper interfaces needed

### **2. Consistent Patterns**
- ✅ **Standardized installer interface** - All plugins follow same pattern
- ✅ **Predictable registration order** - Prevents dependency issues
- ✅ **Enforced lifetimes** - Architectural tests ensure compliance

### **3. Testability**
- ✅ **Easy to mock** - Service registry can be mocked in tests
- ✅ **Isolated testing** - Each plugin can be tested independently
- ✅ **Architectural validation** - NetArchTest ensures pattern compliance

### **4. Cross-Plugin Compatibility**
- ✅ **Unified access** - All plugins accessed through same registry
- ✅ **No plugin coupling** - Plugins don't depend on each other
- ✅ **Ecosystem consistency** - Same pattern across all plugins

## Migration Guide

### **From Direct Registration to Installer Pattern**

#### **Before (Incorrect)**
```csharp
// Direct registration in game code - BAD PATTERN
var registry = new ServiceRegistry();
registry.RegisterScoped<TargetingService>();
registry.RegisterScoped<TargetingWorkflowOrchestrator>();
// ... scattered registration
```

#### **After (Correct)**
```csharp
// Plugin installer pattern - GOOD PATTERN
public class GameCompositionRoot
{
    public GameCompositionRoot()
    {
        var registry = new ServiceRegistry();
        var installers = new IPluginInstaller[]
        {
            new GridPlacementInstaller(),
            // Add more plugins as needed
        };
        
        foreach (var installer in installers)
        {
            installer.ConfigureServices(registry);
        }
    }
}
```

### **From Interface Wrapper to Service Registry**

#### **Before (Incorrect)**
```csharp
// Wrapper interface - BAD PATTERN
public interface IGridPlacementPlugin
{
    IOrchestrator TargetingOrchestrator { get; }
    IOrchestrator PlacementOrchestrator { get; }
}
```

#### **After (Correct)**
```csharp
// Service registry access - GOOD PATTERN
using var scope = registry.CreateScope();
var targeting = scope.GetService<TargetingWorkflowOrchestrator>();
var placement = scope.GetService<PlacementWorkflowOrchestrator>();
```

## Testing Strategy

### **1. Unit Tests**
```csharp
[Fact]
public void Installer_Should_Register_Services_In_Correct_Order()
{
    var installer = new GridPlacementInstaller();
    var testRegistry = new TestServiceRegistry();
    
    installer.ConfigureServices(testRegistry);
    
    var order = testRegistry.GetRegistrationOrder();
    // Validate order: Configuration → Services → Orchestrators → Event Bus
}
```

### **2. Integration Tests**
```csharp
[Fact]
public void Service_Registry_Should_Provide_All_Orchestrators()
{
    var registry = new ServiceRegistry();
    var installer = new GridPlacementInstaller();
    
    installer.ConfigureServices(registry);
    using var scope = registry.CreateScope();
    
    var orchestrators = new[]
    {
        scope.GetService<TargetingWorkflowOrchestrator>(),
        scope.GetService<PlacementWorkflowOrchestrator>(),
        // ... all orchestrators
    };
    
    orchestrators.All(o => o != null).ShouldBeTrue();
}
```

### **3. Architectural Tests**
```csharp
[Fact]
public void All_Plugins_Should_Follow_Installer_Pattern()
{
    var pluginAssemblies = GetPluginAssemblies();
    
    foreach (var assembly in pluginAssemblies)
    {
        ArchitecturalRulesTests.All_Plugins_Should_Have_Installer_Class(assembly);
        ArchitecturalRulesTests.Installers_Should_Follow_Registration_Order_Pattern(assembly);
    }
}
```

## Best Practices

### **1. Installer Design**
- ✅ **Implement IPluginInstaller** - Consistent interface
- ✅ **Follow registration order** - Prevent dependency issues
- ✅ **Use proper lifetimes** - Singleton for config, Scoped for orchestrators
- ✅ **Handle null gracefully** - Validate registry parameter

### **2. Service Registration**
- ✅ **Configuration first** - Services need config to initialize
- ✅ **Services second** - Orchestrators need services
- ✅ **Orchestrators third** - Event coordination
- ✅ **Event bus last** - Shared singleton

### **3. Consumer Access**
- ✅ **Use service registry** - Never direct instantiation
- ✅ **Create scopes** - Proper user isolation
- ✅ **Dispose scopes** - Resource cleanup
- ✅ **Access by type** - Strongly typed resolution

### **4. Testing**
- ✅ **Test installer pattern** - Verify compliance
- ✅ **Test registration order** - Prevent issues
- ✅ **Test service resolution** - Verify accessibility
- ✅ **Test lifetimes** - Verify proper scoping

## Summary

The Plugin Installer Pattern provides:

- **✅ Clear Architecture**: Service registry owns orchestrators, installers handle registration
- **✅ Plugin Autonomy**: Each plugin controls its own installation
- **✅ Consistent Patterns**: All plugins follow same installer interface
- **✅ Architectural Enforcement**: NetArchTest validates compliance
- **✅ Cross-Plugin Compatibility**: Unified access through service registry
- **✅ Testability**: Easy to test and maintain

This pattern establishes the foundation for a scalable, maintainable plugin ecosystem while preserving architectural consistency and testability.
