using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Services;

/// <summary>
/// Engine-agnostic object service interface for game object management.
/// 
/// This service handles all engine-specific operations that core services
/// cannot perform, such as object instantiation, world management, and
/// engine integration. It provides a clean abstraction layer between
/// pure C# business logic and engine presentation concerns.
/// 
/// ## Responsibilities:
/// - Object instantiation from templates/prefabs
/// - Object parenting and hierarchy management
/// - Object lifecycle management (creation/destruction)
/// - Position and transform management
/// - Engine-specific error handling and validation
/// 
/// ## Non-Responsibilities:
/// - Business logic or validation rules (handled by core services)
/// - Placement validation or collision detection
/// - Game state management or decision making
/// </summary>
public interface IObjectService
{
    /// <summary>
    /// Instantiates an object from a template/prefab.
    /// </summary>
    /// <param name="template">The template/prefab to instantiate</param>
    /// <returns>Instantiated object or null if instantiation failed</returns>
    object? InstantiateObject(object template);

    /// <summary>
    /// Adds an object to a parent object with proper error handling.
    /// </summary>
    /// <param name="parent">The parent object</param>
    /// <param name="child">The child object to add</param>
    void AddObjectToWorld(object parent, object child);

    /// <summary>
    /// Removes an object from its parent with proper error handling.
    /// </summary>
    /// <param name="parent">The parent object</param>
    /// <param name="child">The child object to remove</param>
    void RemoveObjectFromWorld(object parent, object child);

    /// <summary>
    /// Safely destroys an object and its resources.
    /// </summary>
    /// <param name="obj">The object to destroy</param>
    void DestroyObject(object obj);

    /// <summary>
    /// Sets the position of an object with validation.
    /// </summary>
    /// <param name="obj">The object to position</param>
    /// <param name="position">The position to set</param>
    void SetObjectPosition(object obj, object position);

    /// <summary>
    /// Gets whether an object is currently active in the world.
    /// </summary>
    /// <param name="obj">The object to check</param>
    /// <returns>True if the object is active in the world</returns>
    bool IsObjectInWorld(object obj);

    /// <summary>
    /// Creates a new object of the specified type.
    /// </summary>
    /// <typeparam name="T">The object type to create</typeparam>
    /// <returns>New object instance or null if creation failed</returns>
    T? CreateObject<T>() where T : class, new();

    /// <summary>
    /// Gets validation issues if any engine dependencies are missing.
    /// </summary>
    /// <returns>Collection of validation issue descriptions</returns>
    IEnumerable<string> GetValidationIssues();
}
