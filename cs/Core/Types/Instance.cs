using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Runtime instance of a template. Engine-agnostic representation of any instantiated object.
    /// </summary>
    public class Instance
    {
        /// <summary>
        /// Unique identifier for this instance
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Reference to the template this instance was created from
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// Asset path to the template (PackedScene, prefab, etc.)
        /// </summary>
        public string? TemplatePath { get; set; }

        /// <summary>
        /// Current position of this instance
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Whether this instance is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Instance-specific data that doesn't belong in the template
        /// </summary>
        public Dictionary<string, object> InstanceData { get; set; } = new();

        /// <summary>
        /// Creates a new instance with default values
        /// </summary>
        public Instance()
        {
            Id = System.Guid.NewGuid().ToString();
            Position = Vector2.Zero;
        }

        /// <summary>
        /// Creates a new instance from a template
        /// </summary>
        /// <param name="templateId">The template ID to create from</param>
        /// <param name="templatePath">Optional template asset path</param>
        public Instance(string templateId, string? templatePath = null)
        {
            Id = System.Guid.NewGuid().ToString();
            TemplateId = templateId;
            TemplatePath = templatePath;
            Position = Vector2.Zero;
        }
    }

    /// <summary>
    /// Service interface for template resolution and management
    /// </summary>
    public interface ITemplateService
    {
        /// <summary>
        /// Gets a template by its ID
        /// </summary>
        /// <typeparam name="T">The template type</typeparam>
        /// <param name="templateId">The template ID</param>
        /// <returns>The template instance or null if not found</returns>
        T? GetTemplate<T>(string templateId) where T : class;

        /// <summary>
        /// Registers a template with the service
        /// </summary>
        /// <typeparam name="T">The template type</typeparam>
        /// <param name="templateId">The template ID</param>
        /// <param name="template">The template instance</param>
        void RegisterTemplate<T>(string templateId, T template) where T : class;

        /// <summary>
        /// Checks if a template is registered
        /// </summary>
        /// <param name="templateId">The template ID</param>
        /// <returns>True if the template exists</returns>
        bool HasTemplate(string templateId);

        /// <summary>
        /// Removes a template from the registry
        /// </summary>
        /// <param name="templateId">The template ID</param>
        /// <returns>True if the template was removed</returns>
        bool RemoveTemplate(string templateId);
    }
}
