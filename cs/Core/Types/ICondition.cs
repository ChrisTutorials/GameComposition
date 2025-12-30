namespace BarkMoon.GameComposition.Core.Types;

/// <summary>
/// Generic condition system for cross-plugin compatibility.
/// 
/// This interface provides a standardized way to evaluate conditions
/// across different plugins without requiring custom condition implementations.
/// 
/// Plugins can use this for drop conditions, crafting requirements,
/// trading prerequisites, and other conditional logic.
/// </summary>
public interface ICondition<TContext>
{
    /// <summary>
    /// Unique identifier for this condition
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// Display name for this condition
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Description of what this condition checks
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Priority for condition evaluation (higher = evaluated first)
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// Whether this condition is inverted (negated)
    /// </summary>
    bool IsInverted { get; }
    
    /// <summary>
    /// Logical operator for combining with other conditions
    /// </summary>
    ConditionOperator Operator { get; }
    
    /// <summary>
    /// Checks if this condition is satisfied given the context
    /// </summary>
    /// <param name="context">The context to evaluate against</param>
    /// <returns>True if the condition is satisfied</returns>
    bool IsSatisfied(TContext context);
    
    /// <summary>
    /// Validates that this condition is properly configured
    /// </summary>
    /// <returns>True if the condition is valid</returns>
    bool IsValid();
}

/// <summary>
/// Logical operators for combining conditions
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
/// Base implementation of ICondition for easier condition creation
/// </summary>
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
