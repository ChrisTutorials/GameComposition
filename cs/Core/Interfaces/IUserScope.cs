using GameUserSessions.Core;

namespace GameComposition.Core.Interfaces
{
    /// <summary>
    /// Represents a per-user scope within a GridBuilding session.
    /// Holds state and services that are specific to a single user of
    /// the systems (human player, AI, or tool), not ownership of buildings.
    /// </summary>
    public interface IUserScope : IServiceContext
    {
        /// <summary>
        /// Identifier of the user this scope belongs to.
        /// </summary>
        GameUserSessions.Core.UserId UserId { get; }
    }
}
