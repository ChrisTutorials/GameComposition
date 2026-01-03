# Architectural Tests - Service-Based Architecture Enforcement

This directory contains architectural tests that enforce Service-Based Architecture patterns across the GameComposition ecosystem. All tests follow DRY principles and use shared SSOT utilities for consistency and maintainability.

## 🏗️ **Architecture Foundation**

### **Single Source of Truth (SSOT) Utilities**
All architectural tests leverage shared utilities in `../Common/` to eliminate duplication:

- **`TestAssemblyHelper`** - Centralized assembly discovery and loading
- **`TestPathHelper`** - Unified path resolution for plugin directories  
- **`ArchitecturalTestBase`** - Base class with common test patterns

### **Service-Based Architecture Principles**
1. **Layer Separation**: Core business logic ↔ Godot engine integration via adapters
2. **Front-End Pattern**: UI nodes depend only on adapters, never services directly
3. **Dependency Injection**: Proper service lifecycle and IoC container usage
4. **Cross-Plugin Compatibility**: Consistent patterns across ecosystem

## 📁 **Domain-Based Test Organization**

### 🎨 **FrontEnd/** - UI Layer Architecture
**Pattern**: Front-end nodes should only own adapters, never business logic
- **FrontEndNodeAdapterPatternTests.cs** - Enforces adapter-only dependency pattern
- **Validates**: No direct service/workflow/presenter dependencies
- **Ensures**: Clean separation between presentation and business logic

### 🔧 **Services/** - Service Layer Patterns  
**Pattern**: Services own business logic and maintain proper lifecycle
- **ServiceOwnershipTests.cs** - Service ownership and lifecycle rules
- **ServiceArchitecturalTests.cs** - Service-specific architectural validation
- **Validates**: Proper service registration, disposal, and dependency patterns

### 🏗️ **Core/** - Fundamental Architecture Rules
**Pattern**: Core layer contains pure business logic without engine dependencies
- **CoreArchitecturalTests.cs** - Core layer architectural validation
- **AggregateRootArchitectureTests.cs** - Aggregate root pattern enforcement
- **ParameterizedArchitectureTests.cs** - Parameterized architectural validation
- **YagniViolationTests.cs** - YAGNI principle violation detection
- **DependencyVersionConsistencyTests.cs** - Dependency version consistency
- **MicrosoftExtensionsUsageTests.cs** - Microsoft.Extensions usage patterns
- **ArchitecturalRuleTests.cs** - General architectural rule validation

### 🌐 **CrossDomain/** - Inter-Domain Communication
**Pattern**: Domains communicate via events and interfaces, not direct coupling
- **CrossDomainEventBusArchitectureTests.cs** - Event bus communication patterns
- **CrossDomainPresenterArchitectureTests.cs** - Presenter cross-domain rules
- **CrossDomainSnapshotArchitectureTests.cs** - Snapshot immutability across domains
- **DomainEventArchitectureTests.cs** - Domain event architectural validation

### 🔌 **CrossPlugin/** - Ecosystem-Wide Compatibility
**Pattern**: All plugins follow consistent patterns for seamless integration
- **CrossPluginArchitecturalTests.cs** - Cross-plugin architectural validation
- **CrossPluginArchitectureTests.cs** - Plugin ecosystem rules

### 📸 **StateSnapshots/** - State Management Patterns
**Pattern**: Snapshots are immutable, State is mutable, never reference each other
- **StateInterfaceArchitectureTests.cs** - State interface architectural rules

### 🔗 **Dependencies/** - Dependency Management
**Pattern**: Proper dependency inversion and IoC container usage
- **DependencyInversionArchitectureTests.cs** - Dependency inversion principle
- **SolutionDependencyArchitectureTests.cs** - Solution-level dependency rules

### 💎 **ValueObjects/** - Immutable Value Patterns
**Pattern**: Value objects are immutable and equality-based
- **ValueObjectArchitectureTests.cs** - Value object architectural validation

### 🔒 **StrongTyping/** - Type Safety Patterns
**Pattern**: Strong typing eliminates primitive obsession
- **StrongTypingArchitectureTests.cs** - Strong typing architectural validation
- **IdTypeConsistencyTests.cs** - ID type consistency across ecosystem

### 📝 **Terminology/** - Naming Consistency
**Pattern**: Consistent terminology across entire ecosystem
- **GridTerminologyConsistencyTests.cs** - Grid-specific terminology rules
- **NamingArchitecturalTests.cs** - General naming conventions
- **NamespaceOrganizationArchitectureTests.cs** - Namespace organization rules

### 🎭 **Orchestrators/** - Workflow Orchestration
**Pattern**: Orchestrators coordinate workflows without business logic
- **OrchestratorArchitecturalTests.cs** - Orchestrator architectural validation

### ⚙️ **Installers/** - Dependency Installation
**Pattern**: Installers configure IoC containers properly
- **InstallerArchitecturalTests.cs** - Installer architectural validation

## 🎯 **Test Patterns & Standards**

### **Standard Test Structure**
```csharp
using BarkMoon.GameComposition.Tests.Common;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

// Explicitly alias to avoid conflicts
using ArchTypes = NetArchTest.Rules.Types;

namespace BarkMoon.GameComposition.Tests.Architectural.{Domain}
{
    public class ExampleArchitecturalTests // No inheritance required
    {
        [Fact]
        [Trait("Category", "Architectural")]
        [Trait("Domain", "Example")]
        public void Example_Architectural_Rule()
        {
            // Arrange - Use static SSOT assembly access
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies(); // Static call
            
            // Act - Use NetArchTest with aliasing
            var result = ArchitecturalTestHelpers.ForAssembly(assemblies.First())
                .Should()
                .ResideInNamespace("Expected.Namespace")
                .GetResult();
                
            // Assert - Use Shouldly for clear error messages
            result.IsSuccessful.ShouldBeTrue("Architectural rule violation explanation");
        }
    }
}
```

### **SSOT Usage Patterns**

#### **Assembly Access**
```csharp
// All assemblies including Godot (for front-end tests)
var assemblies = ArchitecturalTestHelpers.GetAssembliesWithGodot();

// Core assemblies only (no Godot dependencies)
var coreAssemblies = ArchitecturalTestHelpers.GetCoreAssemblies();

// All relevant assemblies for comprehensive testing
var allAssemblies = ArchitecturalTestHelpers.GetAllAssemblies();
```

#### **Path Resolution**
```csharp
// Get specific assembly path
var assemblyPath = TestPathHelper.GetAssemblyPath("GridPlacement.Core");

// Get build directories
var coreBuildDir = TestPathHelper.GetGameCompositionCoreBuildDirectory();
var godotBuildDir = TestPathHelper.GetGridPlacementGodotBuildDirectory();
```

#### **Common Validation Patterns**
```csharp
// Namespace validation
ArchitecturalTestHelpers.ValidateNamespaceConvention("BarkMoon.GridPlacement.Core");

// Core assembly isolation
ArchitecturalTestHelpers.ValidateCoreAssembliesDoNotDependOnGodot();

// Type finding helpers
var snapshotTypes = ArchitecturalTestHelpers.FindTypesWithSuffix("Snapshot");
var serviceTypes = ArchitecturalTestHelpers.FindTypesImplementing<IService>();
```

## 🚀 **Running Tests**

### **All Architectural Tests**
```bash
dotnet test --filter "Category=Architectural"
```

### **Domain-Specific Tests**
```bash
# Front-end node patterns
dotnet test --filter "Category=Architectural&Domain=FrontEnd"

# Service layer patterns  
dotnet test --filter "Category=Architectural&Domain=Services"

# Cross-plugin compatibility
dotnet test --filter "Category=Architectural&Domain=CrossPlugin"
```

### **Pattern-Specific Tests**
```bash
# Strong typing validation
dotnet test --filter "Category=Architectural&Pattern=StrongTyping"

# Value object patterns
dotnet test --filter "Category=Architectural&Pattern=ValueObjects"
```

## 📋 **Test Classification**

### **Traits Used**
- **`Category=Architectural`** - All architectural tests
- **`Domain={DomainName}`** - Domain classification (FrontEnd, Services, Core, etc.)
- **`Pattern={PatternName}`** - Pattern classification (StrongTyping, ValueObjects, etc.)

### **Priority Levels**
- **Critical** - Core architecture violations that break ecosystem compatibility
- **High** - Important patterns that maintain code quality and consistency  
- **Medium** - Best practices that improve maintainability
- **Low** - Nice-to-have patterns that enhance developer experience

## 🔧 **Adding New Tests**

### **1. Choose Domain & Pattern**
Select appropriate domain folder and pattern classification:
- Domain determines *what* is being tested (FrontEnd, Services, Core, etc.)
- Pattern determines *how* it's being tested (StrongTyping, ValueObjects, etc.)

### **2. Follow Standard Template**
```csharp
using BarkMoon.GameComposition.Tests.Common;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

// Explicitly alias to avoid conflicts
using ArchTypes = NetArchTest.Rules.Types;

namespace BarkMoon.GameComposition.Tests.Architectural.{Domain}
{
    public class {TestName} // No inheritance required
    {
        [Fact]
        [Trait("Category", "Architectural")]
        [Trait("Domain", "{Domain}")]
        [Trait("Pattern", "{Pattern}")]
        public void {Rule_Name}_Should_{Expected_Behavior}()
        {
            // Use static SSOT patterns and helpers
            var assemblies = ArchitecturalTestHelpers.GetAllAssemblies();
            var result = ArchitecturalTestHelpers.ForAssembly(assemblies.First())
                .Should()
                .ResideInNamespace("Expected.Namespace")
                .GetResult();
                
            result.IsSuccessful.ShouldBeTrue("Architectural rule violation explanation");
        }
    }
}
```

### **3. Apply SSOT Principles**
- **Use static utilities** - No inheritance required
- **Use helper methods** - `ArchitecturalTestHelpers.GetAllAssemblies()`, etc.
- **Leverage common patterns** - Type finding, validation helpers
- **Follow established naming conventions**

### **4. Document Clearly**
- **XML documentation** explaining the architectural rule
- **Clear test names** following `Subject_Should_ExpectedBehavior` pattern
- **Descriptive assertions** with helpful failure messages

## 🎯 **Impact & Benefits**

### **Architecture Enforcement**
- **Consistent Patterns** - All plugins follow Service-Based Architecture
- **Quality Assurance** - Violations caught during development, not production
- **Ecosystem Compatibility** - Plugins integrate seamlessly

### **Developer Experience**  
- **Clear Expectations** - Explicit architectural rules and guidance
- **Fast Feedback** - Immediate validation of architectural decisions
- **Maintainable Codebase** - Automated enforcement prevents architectural drift

### **Operational Benefits**
- **Reduced Technical Debt** - Proactive architecture validation
- **Easier Onboarding** - Clear patterns for new developers
- **Confidence in Changes** - Automated validation prevents regressions

## 📚 **Additional Resources**

- **[../Common/README.md](Common/README.md)** - SSOT utilities documentation
- **[architecture-config.yaml](architecture-config.yaml)** - Configuration for architectural rules
- **[ORGANIZATION_SUMMARY.md](ORGANIZATION_SUMMARY.md)** - Detailed organization patterns

This architectural test suite ensures the GameComposition ecosystem maintains high-quality, consistent Service-Based Architecture patterns across all plugins and components.
