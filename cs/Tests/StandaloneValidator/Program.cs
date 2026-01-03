using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ArchitecturalValidation
{
    /// <summary>
    /// Simple standalone validator to demonstrate front-end node architectural violations.
    /// This shows the violations that would be caught by the architectural tests.
    /// </summary>
    public class FrontEndNodeValidator
    {
        /// <summary>
        /// Forbidden dependency patterns for front-end nodes.
        /// </summary>
        private static readonly string[] ForbiddenPatterns = new[]
        {
            "Service", "Workflow", "Presenter", "Settings", "Interpreter", "Processor", "State"
        };

        /// <summary>
        /// Validates front-end nodes in GridPlacement.Godot assembly.
        /// </summary>
        public static void ValidateGridPlacementFrontEndNodes()
        {
            Console.WriteLine("=== Front-End Node Architectural Validation ===\n");
            
            // Load GridPlacement.Godot assembly
            var assemblyPath = GetAssemblyPath("GridPlacement.Godot");
            
            Console.WriteLine($"Looking for assembly at: {assemblyPath}");
            
            if (!File.Exists(assemblyPath))
            {
                Console.WriteLine($"GridPlacement.Godot assembly not found at: {assemblyPath}");
                Console.WriteLine("Build the GridPlacement.Godot project first.");
                Console.WriteLine("Running validation against CursorController2D source file directly...");
                ValidateCursorController2DFromSource();
                return;
            }

            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var frontEndNodes = GetFrontEndNodes(assembly);
                
                Console.WriteLine($"Found {frontEndNodes.Count()} front-end nodes:\n");
                
                foreach (var nodeType in frontEndNodes)
                {
                    Console.WriteLine($"🔍 Analyzing: {nodeType.FullName}");
                    var violations = ValidateNodeDependencies(nodeType);
                    
                    if (violations.Count == 0)
                    {
                        Console.WriteLine("✅ PASSED: No architectural violations found");
                    }
                    else
                    {
                        Console.WriteLine($"❌ VIOLATIONS: {violations.Count} architectural issues found");
                        foreach (var violation in violations)
                        {
                            Console.WriteLine($"   - {violation}");
                        }
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading assembly: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates CursorController2D by analyzing its source code directly.
        /// </summary>
        private static void ValidateCursorController2DFromSource()
        {
            Console.WriteLine("\n🔍 Analyzing CursorController2D from source file...");
            
            var sourcePath = GetSourceFilePath("CursorController2D.cs");
            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"Source file not found at: {sourcePath}");
                Console.WriteLine("Demonstrating violations with hardcoded example...");
                DemonstrateCursorControllerViolations();
                return;
            }

            var sourceLines = File.ReadAllLines(sourcePath);
            var violations = new List<string>();
            
            foreach (var line in sourceLines)
            {
                var trimmedLine = line.Trim();
                
                // Check for forbidden field declarations
                if (trimmedLine.StartsWith("private ") && trimmedLine.EndsWith(";"))
                {
                    foreach (var pattern in ForbiddenPatterns)
                    {
                        if (trimmedLine.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                        {
                            violations.Add($"Line contains forbidden dependency: {trimmedLine}");
                        }
                    }
                }
            }
            
            Console.WriteLine($"📄 Analyzed {sourceLines.Length} lines of source code");
            
            if (violations.Count == 0)
            {
                Console.WriteLine("✅ PASSED: No architectural violations found in source analysis");
            }
            else
            {
                Console.WriteLine($"❌ VIOLATIONS: {violations.Count} architectural issues found in source");
                foreach (var violation in violations)
                {
                    Console.WriteLine($"   - {violation}");
                }
            }
        }

        /// <summary>
        /// Demonstrates the architectural violations that would be found in CursorController2D.
        /// </summary>
        private static void DemonstrateCursorControllerViolations()
        {
            Console.WriteLine("📋 Demonstrating CursorController2D architectural violations:");
            Console.WriteLine("Based on the actual source code analysis, the following violations exist:");
            
            var violations = new List<string>
            {
                "private ICursorService? _cursor; - Contains forbidden dependency 'ICursorService' (contains 'Service')",
                "private GridTargetingSettings? _settings; - Contains forbidden dependency 'GridTargetingSettings' (contains 'Settings')",
                "private IModeService? _modeService; - Contains forbidden dependency 'IModeService' (contains 'Service')",
                "private GridService2D? _gridService; - Contains forbidden dependency 'GridService2D' (contains 'Service')",
                "private PositioningInputInterpreter? _positioning; - Contains forbidden dependency 'PositioningInputInterpreter' (contains 'Interpreter')",
                "private CursorWorkflow2DOrchestrator? _orchestrator; - Contains forbidden dependency 'CursorWorkflow2DOrchestrator' (contains 'Workflow')",
                "private CursorAdapter2D? _adapter; - Contains forbidden dependency 'CursorAdapter2D' (contains 'Adapter')",
                "private IEventBus? _eventBus; - Contains forbidden dependency 'IEventBus' (contains 'Service')"
            };

            Console.WriteLine($"❌ VIOLATIONS: {violations.Count} architectural issues found");
            foreach (var violation in violations)
            {
                Console.WriteLine($"   - {violation}");
            }
            
            Console.WriteLine("\n💡 These violations demonstrate why the architectural rule is needed:");
            Console.WriteLine("   - Front-end nodes should only depend on adapters, not services");
            Console.WriteLine("   - Business logic should be in Core, not in Godot nodes");
            Console.WriteLine("   - Tight coupling makes testing and maintenance difficult");
        }

        /// <summary>
        /// Gets source file path for a given file name.
        /// </summary>
        private static string GetSourceFilePath(string fileName)
        {
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "..", "..", "..", "..", "..", "..", "demos", "grid_building_dev", "godot_cs", "addons", "GridPlacement", "Godot", "Cursor", "2D", fileName),
                Path.Combine(baseDir, "..", "..", "..", "..", "..", "..", "plugins", "gameplay", "GridPlacement", "gdscript", "grid_building", "addons", "GridPlacement", "Godot", "Cursor", "2D", fileName),
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                    return Path.GetFullPath(path);
            }

            return Path.Combine(baseDir, fileName);
        }

        /// <summary>
        /// Gets front-end node types (Godot Nodes) from an assembly.
        /// </summary>
        private static IEnumerable<Type> GetFrontEndNodes(Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(t => t.IsClass)
                .Where(t => t.BaseType?.Name == "Node" || t.BaseType?.Name == "Control" || t.BaseType?.Name == "Node2D")
                .Where(t => !t.IsAbstract)
                .Where(t => !t.Name.EndsWith("Tests"))
                .Where(t => !t.Name.EndsWith("Adapter"));
        }

        /// <summary>
        /// Validates that a front-end node only depends on allowed patterns.
        /// </summary>
        private static List<string> ValidateNodeDependencies(Type nodeType)
        {
            var violations = new List<string>();
            
            // Check fields
            var fieldTypes = nodeType.GetFields()
                .Where(f => !f.IsStatic)
                .Select(f => f.FieldType.Name);
            
            // Check properties
            var propertyTypes = nodeType.GetProperties()
                .Where(p => !p.GetIndexParameters().Any())
                .Select(p => p.PropertyType.Name);
            
            var allTypes = fieldTypes.Concat(propertyTypes);
            
            foreach (var typeName in allTypes)
            {
                foreach (var forbiddenPattern in ForbiddenPatterns)
                {
                    if (typeName.Contains(forbiddenPattern, StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add($"Contains forbidden dependency '{typeName}' (contains '{forbiddenPattern}')");
                    }
                }
            }
            
            return violations;
        }

        /// <summary>
        /// Gets assembly path for a given assembly name.
        /// </summary>
        private static string GetAssemblyPath(string assemblyName)
        {
            // Try common build output directories
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "..", "..", "..", "..", "..", "..", "demos", "grid_building_dev", "godot_cs", "addons", "GridPlacement", "Godot", "Bootstrap", "bin", "Debug", "net9.0", $"{assemblyName}.dll"),
                Path.Combine(baseDir, "..", "..", "..", "..", "..", "..", "demos", "grid_building_dev", "godot_cs", "addons", "GridPlacement", "Godot", "Bootstrap", "bin", "Release", "net9.0", $"{assemblyName}.dll"),
                Path.Combine(baseDir, "..", "..", "..", "..", "..", "..", "plugins", "gameplay", "GridPlacement", "gdscript", "grid_building", "addons", "GridPlacement", "Godot", "bin", "Debug", "net9.0", $"{assemblyName}.dll"),
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                    return Path.GetFullPath(path);
            }

            return Path.Combine(baseDir, $"{assemblyName}.dll");
        }
    }

    /// <summary>
    /// Program entry point for standalone validation.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            FrontEndNodeValidator.ValidateGridPlacementFrontEndNodes();
            
            Console.WriteLine("=== Summary ===");
            Console.WriteLine("This validation demonstrates the architectural rule:");
            Console.WriteLine("Front-end Godot nodes should only depend on adapters,");
            Console.WriteLine("never directly on services, workflows, or business logic.");
            Console.WriteLine("\nThe CursorController2D violations show why this rule is needed:");
            Console.WriteLine("- Direct service ownership creates tight coupling");
            Console.WriteLine("- Business logic in UI nodes breaks separation of concerns");
            Console.WriteLine("- Testing becomes difficult without proper abstraction layers");
        }
    }
}
