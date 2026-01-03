using NetArchTest.Rules;
using Shouldly;
using System.Linq;
using System.Reflection;
using System.IO;
using BarkMoon.GameComposition.Tests.Common;
using Xunit;

// Explicitly alias the NetArchTest Types class to avoid conflict
using ArchTypes = NetArchTest.Rules.Types;

namespace BarkMoon.GameComposition.Core.Tests.Architectural
{
    /// <summary>
    /// Architectural tests that enforce grid terminology consistency across the ecosystem.
    /// Uses DRY patterns with reusable validation rules and helper methods.
    /// </summary>
    public class GridTerminologyConsistencyTests : ArchitecturalTestBase
    {
        // DRY: Centralized validation rules configuration
        private static readonly TerminologyRule[] _gridRules = new[]
        {
            new TerminologyRule
            {
                ForbiddenPattern = "GridPosition",
                Replacement = "GridTile",
                Description = "discrete grid coordinates",
                TargetTypes = MemberType.Property | MemberType.Field | MemberType.Parameter
            },
            new TerminologyRule
            {
                ForbiddenPattern = "GridPosition",
                Replacement = "GridTile", 
                Description = "discrete grid coordinates",
                TargetTypes = MemberType.Property | MemberType.Field | MemberType.Parameter,
                AppliesTo = t => t.IsValueType && t.Name.Contains("Snapshot", StringComparison.OrdinalIgnoreCase)
            }
        };

        private static readonly TerminologyRule[] _worldRules = new[]
        {
            new TerminologyRule
            {
                RequiredPattern = "WorldPosition",
                ExpectedType = typeof(Vector2),
                Description = "continuous world coordinates",
                TargetTypes = MemberType.Property | MemberType.Field
            }
        };

        /// <summary>
        /// Rule 1: Grid positioning should use GridTile terminology, not GridPosition across ALL plugins.
        /// GridPosition suggests mutable state, GridTile suggests discrete coordinates.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void Grid_Positioning_Should_Use_GridTile_Not_GridPosition_Across_All_Plugins()
        {
            // Arrange - Load all plugin assemblies
            var assemblies = AllAssemblies;
            var allViolations = ValidateTerminologyRules(_gridRules, assemblies);

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"Grid positioning terminology must use GridTile instead of GridPosition across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Rule 2: World positioning should use Vector2 for continuous positions across ALL plugins.
        /// Grid positioning should use Vector2I for discrete coordinates.
        /// </summary>
        [Fact]
        [Trait("Category", "Architectural")]
        public void World_Positioning_Should_Use_Vector2_For_Continuous_Positions_Across_All_Plugins()
        {
            // Arrange - Load all plugin assemblies
            var assemblies = AllAssemblies;
            var allViolations = ValidateTerminologyRules(_worldRules, assemblies);

            // Assert across all plugins
            if (allViolations.Count > 0)
            {
                var errorMessage = $"World positioning terminology must use Vector2 for continuous positions across all plugins. Violations:\n{string.Join("\n", allViolations)}";
                throw new InvalidOperationException(errorMessage);
            }
        }

        #region DRY Helper Methods

        /// <summary>
        /// Validates terminology rules across all relevant assemblies using DRY patterns.
        /// </summary>
        private static List<string> ValidateTerminologyRules(TerminologyRule[] rules, IEnumerable<Assembly> assemblies)
        {
            var violations = new List<string>();

            foreach (var rule in rules)
            {
                var ruleViolations = ValidateRule(rule, assemblies);
                violations.AddRange(ruleViolations);
            }

            return violations;
        }

        /// <summary>
        /// Validates a single terminology rule across assemblies.
        /// </summary>
        private static List<string> ValidateRule(TerminologyRule rule, IEnumerable<Assembly> assemblies)
        {
            var violations = new List<string>();

            foreach (var assembly in assemblies)
            {
                var types = GetRelevantTypes(assembly, rule);
                
                foreach (var type in types)
                {
                    var typeViolations = ValidateType(rule, type);
                    violations.AddRange(typeViolations);
                }
            }

            return violations;
        }

        /// <summary>
        /// Gets types that should be validated for a given rule.
        /// </summary>
        private static IEnumerable<Type> GetRelevantTypes(Assembly assembly, TerminologyRule rule)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass || t.IsValueType);

            // Apply rule-specific filters
            if (rule.AppliesTo != null)
            {
                types = types.Where(rule.AppliesTo);
            }

            // Filter by pattern presence
            if (rule.ForbiddenPattern != null)
            {
                types = types.Where(t => HasMemberWithPattern(t, rule.ForbiddenPattern));
            }
            else if (rule.RequiredPattern != null)
            {
                types = types.Where(t => HasMemberWithPattern(t, rule.RequiredPattern));
            }

            return types;
        }

        /// <summary>
        /// Validates a single type against a terminology rule.
        /// </summary>
        private static List<string> ValidateType(TerminologyRule rule, Type type)
        {
            var violations = new List<string>();

            // Validate properties
            if (rule.TargetTypes.HasFlag(MemberType.Property))
            {
                var propertyViolations = ValidateMembers<PropertyInfo>(
                    type.GetProperties(),
                    rule,
                    p => p.Name,
                    p => p.PropertyType);
                violations.AddRange(propertyViolations);
            }

            // Validate fields
            if (rule.TargetTypes.HasFlag(MemberType.Field))
            {
                var fieldViolations = ValidateMembers<FieldInfo>(
                    type.GetFields(),
                    rule,
                    f => f.Name,
                    f => f.FieldType);
                violations.AddRange(fieldViolations);
            }

            // Validate constructor parameters (for snapshots)
            if (rule.TargetTypes.HasFlag(MemberType.Parameter) && 
                type.IsValueType && 
                type.Name.Contains("Snapshot", StringComparison.OrdinalIgnoreCase))
            {
                var parameterViolations = ValidateConstructorParameters(type, rule);
                violations.AddRange(parameterViolations);
            }

            return violations;
        }

        /// <summary>
        /// Generic member validation using DRY patterns.
        /// </summary>
        private static List<string> ValidateMembers<TMember>(
            IEnumerable<TMember> members,
            TerminologyRule rule,
            Func<TMember, string> getName,
            Func<TMember, Type> getType) where TMember : MemberInfo
        {
            var violations = new List<string>();

            foreach (var member in members)
            {
                var memberName = getName(member);
                var memberType = getType(member);

                if (rule.ForbiddenPattern != null && 
                    memberName.Contains(rule.ForbiddenPattern, StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add(
                        $"{member.DeclaringType?.FullName}.{memberName}: " +
                        $"Should use '{rule.Replacement}' instead of '{rule.ForbiddenPattern}' for {rule.Description}");
                }
                else if (rule.RequiredPattern != null && 
                         memberName.Contains(rule.RequiredPattern, StringComparison.OrdinalIgnoreCase))
                {
                    if (rule.ExpectedType != null && memberType != rule.ExpectedType)
                    {
                        violations.Add(
                            $"{member.DeclaringType?.FullName}.{memberName}: " +
                            $"{rule.RequiredPattern} should use {rule.ExpectedType.Name} for {rule.Description}, found {memberType.Name}");
                    }
                }
            }

            return violations;
        }

        /// <summary>
        /// Validates constructor parameters for snapshot types.
        /// </summary>
        private static List<string> ValidateConstructorParameters(Type type, TerminologyRule rule)
        {
            var violations = new List<string>();
            var constructors = type.GetConstructors();

            foreach (var constructor in constructors)
            {
                foreach (var parameter in constructor.GetParameters())
                {
                    var parameterName = parameter.Name!;
                    var parameterType = parameter.ParameterType;

                    if (rule.ForbiddenPattern != null && 
                        parameterName.Contains(rule.ForbiddenPattern, StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add(
                            $"{type.FullName} constructor parameter '{parameterName}': " +
                            $"Should use '{rule.Replacement}' instead of '{rule.ForbiddenPattern}' for {rule.Description}");
                    }
                    else if (rule.RequiredPattern != null && 
                             parameterName.Contains(rule.RequiredPattern, StringComparison.OrdinalIgnoreCase))
                    {
                        if (rule.ExpectedType != null && parameterType != rule.ExpectedType)
                        {
                            violations.Add(
                                $"{type.FullName} constructor parameter '{parameterName}': " +
                                $"{rule.RequiredPattern} should use {rule.ExpectedType.Name} for {rule.Description}, found {parameterType.Name}");
                        }
                    }
                }
            }

            return violations;
        }

        /// <summary>
        /// Checks if a type has any member matching the given pattern.
        /// </summary>
        private static bool HasMemberWithPattern(Type type, string pattern)
        {
            return type.GetProperties().Any(p => p.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase)) ||
                   type.GetFields().Any(f => f.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase)) ||
                   (type.IsValueType && type.Name.Contains("Snapshot", StringComparison.OrdinalIgnoreCase) &&
                    type.GetConstructors().Any(c => c.GetParameters().Any(p => p.Name!.Contains(pattern, StringComparison.OrdinalIgnoreCase))));
        }

        #endregion
    }

    #region DRY Data Structures

    /// <summary>
    /// Defines a terminology validation rule using DRY configuration.
    /// </summary>
    public class TerminologyRule
    {
        public string? ForbiddenPattern { get; set; }
        public string? RequiredPattern { get; set; }
        public string? Replacement { get; set; }
        public Type? ExpectedType { get; set; }
        public string Description { get; set; } = string.Empty;
        public MemberType TargetTypes { get; set; } = MemberType.Property | MemberType.Field;
        public Func<Type, bool>? AppliesTo { get; set; }
    }

    /// <summary>
    /// Flags for different member types to validate.
    /// </summary>
    [Flags]
    public enum MemberType
    {
        Property = 1,
        Field = 2,
        Parameter = 4
    }

    #endregion
}
