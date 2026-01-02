# Architectural Test Organization

## Overview

The architectural test suite has been reorganized from a single god class into domain-specific test files for better maintainability, clarity, and scalability.

## Before: God Class Problem

```csharp
// OLD: Single massive class with 25+ test methods
public class ArchitecturalRulesTests
{
    // Core framework tests
    [Fact] public void Core_Should_Use_Microsoft_Extensions() { ... }
    [Fact] public void Core_Should_Be_Engine_Agnostic() { ... }
    
    // Orchestrator tests  
    [Fact] public void Orchestrators_Should_Own_Only_Services() { ... }
    [Fact] public void Orchestrators_Should_Implement_IOrchestrator() { ... }
    
    // Service tests
    [Fact] public void Services_Should_Own_Typed_State() { ... }
    [Fact] public void Services_Should_Emit_Domain_Events() { ... }
    
    // Installer tests
    [Fact] public void All_Plugins_Should_Have_Installer() { ... }
    [Fact] public void Installers_Should_Follow_Order() { ... }
    
    // Naming tests
    [Fact] public void Dimensional_Types_Should_Have_Suffix() { ... }
    [Fact] public void Snapshot_Classes_Should_Follow_Convention() { ... }
    
    // ... 15+ more test methods
}
```

**Problems:**
- **Single Responsibility Violation**: One class handling all architectural domains
- **Maintainability**: Hard to find and modify specific domain tests
- **Scalability**: Adding new domains makes the class even larger
- **Readability**: 1000+ lines in a single file

## After: Domain-Specific Organization

```csharp
// NEW: Organized by architectural domains
namespace BarkMoon.GameComposition.ArchitecturalTests
{
    // Core framework architectural rules
    public class Core.CoreArchitecturalTests { ... }
    
    // Orchestrator pattern architectural rules
    public class Orchestrators.OrchestratorArchitecturalTests { ... }
    
    // Service pattern architectural rules
    public class Services.ServiceArchitecturalTests { ... }
    
    // Plugin installer architectural rules
    public class Installers.InstallerArchitecturalTests { ... }
    
    // Naming convention architectural rules
    public class Naming.NamingArchitecturalTests { ... }
}
```

## Domain Breakdown

### **1. Core Architectural Tests**
**File**: `Core/CoreArchitecturalTests.cs`
**Purpose**: Fundamental GameComposition framework rules
**Tests**:
- `Core_Should_Use_Microsoft_Extensions_Not_Custom_Implementations`
- `Core_Should_Be_Engine_Agnostic`
- `Services_Should_Be_Interface_First`
- `Dependencies_Should_Follow_Layered_Architecture`
- `Types_Should_Be_Immutable_Structs_Where_Appropriate`

### **2. Orchestrator Architectural Tests**
**File**: `Orchestrators/OrchestratorArchitecturalTests.cs`
**Purpose**: Orchestrator pattern validation across plugins
**Tests**:
- `Orchestrators_Should_Own_Only_Services_And_EventBus`
- `Orchestrators_Should_Implement_IOrchestrator_Interface`
- `Each_Domain_Should_Have_Dedicated_Orchestrator`
- `Orchestrators_Should_Be_Registered_In_Service_Registry`
- `All_Orchestrators_Should_Have_Unique_Identifiers`

### **3. Service Architectural Tests**
**File**: `Services/ServiceArchitecturalTests.cs`
**Purpose**: Service pattern and state ownership validation
**Tests**:
- `Services_Should_Only_Emit_Domain_Appropriate_Events`
- `All_Services_Should_Own_Typed_State_And_Provide_Snapshots`
- `All_State_Classes_Should_Be_Pure_Data_And_Paired_With_Services`

### **4. Installer Architectural Tests**
**File**: `Installers/InstallerArchitecturalTests.cs`
**Purpose**: Plugin installer pattern validation
**Tests**:
- `All_Plugins_Should_Have_Installer_Class`
- `Installers_Should_Follow_Registration_Order_Pattern`
- `Orchestrators_Should_Be_Registered_As_Scoped`
- `Configurations_Should_Be_Registered_As_Singleton`
- `Event_Bus_Should_Be_Registered_As_Singleton_Once`

### **5. Naming Architectural Tests**
**File**: `Naming/NamingArchitecturalTests.cs`
**Purpose**: Naming convention and terminology validation
**Tests**:
- `Dimensional_Types_Should_Have_Dimensional_Suffix`
- `ID_Types_Should_Use_Numeric_IDs_Not_Strings`
- `Cursor_Domain_Should_Use_Consistent_Cursor_Terminology`
- `Snapshot_Classes_Should_Follow_Proper_Naming_Convention`
- `Service_Classes_Should_Have_Dimensional_Suffix_At_End`
- `State_Classes_Should_Have_Dimensional_Suffix_At_End`

## Test Coordinator

### **ArchitecturalTestCoordinator**
**File**: `ArchitecturalTestCoordinator.cs`
**Purpose**: Orchestrates all domain-specific tests and provides integration validation

```csharp
public class ArchitecturalTestCoordinator
{
    // Domain-specific test classes
    public class CoreTests : Core.CoreArchitecturalTests { }
    public class OrchestratorTests : Orchestrators.OrchestratorArchitecturalTests { }
    public class ServiceTests : Services.ServiceArchitecturalTests { }
    public class InstallerTests : Installers.InstallerArchitecturalTests { }
    public class NamingTests : Naming.NamingArchitecturalTests { }
    
    // Cross-domain integration tests
    public class CrossDomainArchitecturalTests { ... }
}
```

## Legacy Compatibility

### **Backward Compatibility**
The original `ArchitecturalRulesTests.cs` is maintained as a deprecated wrapper:

```csharp
public class ArchitecturalRulesTests
{
    [Fact(DisplayName = "001GC: Core Test [LEGACY]")]
    public void Core_Should_Use_Microsoft_Extensions_Not_Custom_Implementations()
    {
        var coreTests = new Core.CoreArchitecturalTests();
        coreTests.Core_Should_Use_Microsoft_Extensions_Not_Custom_Implementations();
    }
}
```

**Benefits:**
- ✅ **No Breaking Changes**: Existing test runners continue to work
- ✅ **Clear Migration Path**: Legacy tests marked with `[LEGACY]` tags
- ✅ **Gradual Transition**: Teams can migrate to domain-specific tests over time

## Usage Patterns

### **Running All Architectural Tests**
```bash
# Run all domain-specific tests
dotnet test --filter "FullyQualifiedName~ArchitecturalTests"

# Run specific domain tests
dotnet test --filter "FullyQualifiedName~Core.CoreArchitecturalTests"
dotnet test --filter "FullyQualifiedName~Orchestrators.OrchestratorArchitecturalTests"
```

### **Adding New Tests**
```csharp
// Add to appropriate domain class
public class Core.CoreArchitecturalTests
{
    [Fact(DisplayName = "006GC: New Core Rule")]
    public void New_Core_Architectural_Rule()
    {
        // Test implementation
    }
}
```

### **Plugin Integration**
```csharp
// In plugin test suite
public class GridPlacementArchitecturalTests
{
    [Fact]
    public void GridPlacement_Should_Follow_All_Architectural_Rules()
    {
        var assembly = Assembly.LoadFrom("BarkMoon.GridPlacement.Core.dll");
        
        // Run all domain-specific tests
        Core.CoreArchitecturalTests.Core_Should_Be_Engine_Agnostic();
        Orchestrators.OrchestratorArchitecturalTests.Each_Domain_Should_Have_Dedicated_Orchestrator(assembly);
        Services.ServiceArchitecturalTests.Services_Should_Only_Emit_Domain_Appropriate_Events(assembly);
        Installers.InstallerArchitecturalTests.All_Plugins_Should_Have_Installer_Class(assembly);
        Naming.NamingArchitecturalTests.Dimensional_Types_Should_Have_Dimensional_Suffix(assembly);
    }
}
```

## Benefits of New Organization

### **1. Single Responsibility Principle**
- Each test class handles one architectural domain
- Clear separation of concerns
- Easier to understand and maintain

### **2. Improved Maintainability**
- **Find Tests Quickly**: Know exactly which file contains which tests
- **Modify Safely**: Changes to one domain don't risk breaking others
- **Add New Tests**: Clear where to add new architectural rules

### **3. Better Scalability**
- **New Domains**: Add new test classes without affecting existing ones
- **Growing Test Suites**: Each domain can grow independently
- **Team Collaboration**: Different team members can own different domains

### **4. Enhanced Readability**
- **Smaller Files**: Each file is 100-200 lines instead of 1000+
- **Focused Content**: Each file contains related tests only
- **Clear Documentation**: Domain-specific documentation and examples

### **5. Test Organization**
- **Selective Execution**: Run only specific domain tests when needed
- **Parallel Testing**: Different domains can run in parallel
- **CI/CD Integration**: Target specific domains in pipelines

## Migration Guide

### **For Developers**
1. **Use Domain-Specific Classes**: Import and use the new test classes directly
2. **Update Test References**: Point to the new domain-specific test methods
3. **Run Legacy Tests**: Continue using legacy tests during transition

### **For CI/CD**
1. **Update Test Filters**: Target domain-specific test classes
2. **Parallel Execution**: Configure different domains to run in parallel
3. **Legacy Support**: Keep legacy tests running during migration period

### **For Documentation**
1. **Update References**: Point documentation to new domain-specific classes
2. **Examples**: Use domain-specific examples in documentation
3. **Migration Notes**: Document the transition from god class to domain organization

## File Structure

```
Tests/Architectural/
├── Core/
│   └── CoreArchitecturalTests.cs
├── Orchestrators/
│   └── OrchestratorArchitecturalTests.cs
├── Services/
│   └── ServiceArchitecturalTests.cs
├── Installers/
│   └── InstallerArchitecturalTests.cs
├── Naming/
│   └── NamingArchitecturalTests.cs
├── ArchitecturalTestCoordinator.cs
└── ArchitecturalRulesTests.cs (legacy wrapper)
```

This organization provides a clean, maintainable, and scalable foundation for architectural testing while preserving backward compatibility.
