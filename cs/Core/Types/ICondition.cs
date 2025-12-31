namespace BarkMoon.GameComposition.Core.Types;

/// <summary>
/// Generic condition system for cross-plugin compatibility.
/// 
/// A condition represents a reusable rule that evaluates whether something
/// should be allowed or not. Think of it as a "gatekeeper" that checks if
/// requirements are met before an action can proceed.
/// 
/// For detailed examples, see: /docs/game-composition/content/examples.md
/// </summary>
public interface ICondition<TContext>
{
    /// <summary>
    /// Unique identifier for this condition.
    /// Used for configuration, debugging, and registry lookups.
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// Display name for this condition.
    /// Human-readable name shown in UI, tooltips, or debug output.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Description of what this condition checks.
    /// Detailed explanation used for tooltips and documentation.
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Priority for condition evaluation (higher = evaluated first).
    /// Used for optimization by checking important conditions first.
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// Whether this condition is inverted (negated).
    /// When true, the condition result is inverted before returning.
    /// </summary>
    bool IsInverted { get; }
    
    /// <summary>
    /// Logical operator for combining with other conditions.
    /// Determines how this condition interacts with others in condition sets.
    /// </summary>
    ConditionOperator Operator { get; }
    
    /// <summary>
    /// Checks if this condition is satisfied given the context.
    /// 
    /// Core method that evaluates the condition's rule against the provided context.
    /// Implementation should be pure (no side effects) and deterministic.
    /// </summary>
    /// <param name="context">The context to evaluate against.</param>
    /// <returns>True if the condition is satisfied (considering inversion).</returns>
    bool IsSatisfied(TContext context);
    
    /// <summary>
    /// Validates that this condition is properly configured.
    /// 
    /// Called before evaluation to ensure the condition has all required properties.
    /// </summary>
    /// <returns>True if the condition is valid and ready for evaluation.</returns>
    bool IsValid();
}

/// <summary>
/// Logical operators for combining conditions.
/// </summary>
public enum ConditionOperator
{
    /// <summary>
    /// All conditions must be satisfied (AND)
    /// </summary>
    All,
    
    /// <summary>
    /// At least one condition must be satisfied (OR)
    /// </summary>
    Any,
    
    /// <summary>
    /// No conditions should be satisfied (NOT)
    /// </summary>
    None,

    /// <summary>
    /// Alias for All, maintained for backward compatibility
    /// </summary>
    And = All,

    /// <summary>
    /// Alias for Any, maintained for backward compatibility
    /// </summary>
    Or = Any
}

/// <summary>
/// Base implementation of ICondition with common functionality.
/// 
/// Provides foundation for creating specific conditions while handling
/// common concerns like inversion, validation, and basic properties.
/// 
/// For detailed examples, see: /docs/game-composition/content/examples.md
/// </summary>
/// <typeparam name="TContext">The context type this condition evaluates</typeparam>
public abstract class ConditionBase<TContext> : ICondition<TContext>
{
    public string Id { get; protected set; } = string.Empty;
    public string Name { get; protected set; } = string.Empty;
    public string Description { get; protected set; } = string.Empty;
    public int Priority { get; protected set; } = 0;
    public bool IsInverted { get; protected set; } = false;
    public ConditionOperator Operator { get; protected set; } = ConditionOperator.All;
    
    /// <summary>
    /// Creates a new condition with basic properties
    /// </summary>
    /// <param name="id">Condition ID</param>
    /// <param name="name">Display name</param>
    /// <param name="description">Description</param>
    /// <param name="priority">Evaluation priority</param>
    /// <param name="isInverted">Whether condition is inverted</param>
    /// <param name="operator">Logical operator</param>
    protected ConditionBase(
        string id, 
        string name, 
        string description, 
        int priority = 0, 
        bool isInverted = false, 
        ConditionOperator @operator = ConditionOperator.All)
    {
        Id = id;
        Name = name;
        Description = description;
        Priority = priority;
        IsInverted = isInverted;
        Operator = @operator;
    }
    
    /// <summary>
    /// Template method for condition evaluation
    /// </summary>
    /// <param name="context">Context to evaluate</param>
    /// <returns>True if condition is satisfied</returns>
    public bool IsSatisfied(TContext context)
    {
        var result = EvaluateCondition(context);
        return IsInverted ? !result : result;
    }
    
    /// <summary>
    /// Abstract method for actual condition evaluation
    /// </summary>
    /// <param name="context">Context to evaluate</param>
    /// <returns>True if condition is satisfied</returns>
    protected abstract bool EvaluateCondition(TContext context);
    
    /// <summary>
    /// Base validation for common condition properties
    /// </summary>
    /// <returns>True if condition is valid</returns>
    public virtual bool IsValid()
    {
        return !string.IsNullOrEmpty(Id) && 
               !string.IsNullOrEmpty(Name) && 
               Priority >= 0;
    }
    
    public override string ToString()
    {
        var invertedStr = IsInverted ? "NOT " : "";
        return $"{invertedStr}{Name} ({Id})";
    }
}
