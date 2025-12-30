using System;
using System.Collections.Generic;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Configuration for system composition and initialization.
    /// </summary>
    public class CompositionConfig
    {
        public string SystemId { get; set; } = string.Empty;
        public bool EnableLogging { get; set; }
    }

    /// <summary>
    /// Represents the current state of the composition system.
    /// </summary>
    public class CompositionState
    {
        public bool IsInitialized { get; set; }
        public IReadOnlyList<string> ActiveServices { get; set; } = new List<string>();
    }

    /// <summary>
    /// Describes a service to be registered.
    /// </summary>
    public class ServiceDescriptor
    {
        public string ServiceId { get; set; } = string.Empty;
        public Type ServiceType { get; set; } = typeof(object);
    }

    /// <summary>
    /// Defines a workflow that spans multiple domains.
    /// </summary>
    public class MultiDomainWorkflow
    {
        public string WorkflowId { get; set; } = string.Empty;
        public IReadOnlyList<string> Steps { get; set; } = new List<string>();
    }

    /// <summary>
    /// Base interface for all composed services.
    /// </summary>
    public interface IComposedService
    {
        string ServiceId { get; }
    }
}
