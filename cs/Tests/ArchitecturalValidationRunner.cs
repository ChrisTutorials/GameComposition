using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BarkMoon.GameComposition.Tests.Common;
using System.Collections.Generic;

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
            var assemblyPath = TestPathHelper.GetAssemblyPath("GridPlacement.Godot");
            
            if (!File.Exists(assemblyPath))
            {
                Console.WriteLine($"GridPlacement.Godot assembly not found at: {assemblyPath}");
                Console.WriteLine("Build the GridPlacement.Godot project first.");
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
