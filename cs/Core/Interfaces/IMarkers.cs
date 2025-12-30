namespace BarkMoon.GameComposition.Core.Interfaces
{
    /// <summary>
    /// Marker interface for state objects (Layer 1/2 Data).
    /// State = Pure Data Only - no business logic allowed.
    /// </summary>
    public interface IState
    {
    }

    /// <summary>
    /// Marker interface for service objects (Layer 2 Business Logic).
    /// Services = Business Logic - all logic goes here.
    /// </summary>
    public interface IService
    {
    }

    /// <summary>
    /// Marker interface for workflow adapters (Layer 3 Bridge).
    /// </summary>
    public interface IWorkflowAdapter
    {
    }

    /// <summary>
    /// Marker interface for high-level technical systems.
    /// </summary>
    public interface ISystem
    {
    }


    /// <summary>
    /// Defines a component that can be initialized.
    /// </summary>
    public interface IInitializable
    {
        void Initialize();
    }
}
