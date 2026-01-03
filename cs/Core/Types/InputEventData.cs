using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Input event data (engine-agnostic).
    /// Represents user input events in a format independent of any specific engine.
    /// Core primitive for cross-plugin input handling.
    /// </summary>
    public struct InputEventData : IEquatable<InputEventData>
    {
        /// <summary>
        /// Type of input event.
        /// </summary>
        public InputEventType EventType { get; set; }
        
        /// <summary>
        /// Mouse button involved in the event.
        /// </summary>
        public MouseButton MouseButton { get; set; }
        
        /// <summary>
        /// Keyboard key involved in the event.
        /// </summary>
        public KeyCode KeyCode { get; set; }
        
        /// <summary>
        /// Input modifiers (shift, ctrl, alt).
        /// </summary>
        public InputModifiers Modifiers { get; set; }
        
        /// <summary>
        /// Position of the input event.
        /// </summary>
        public Vector2 Position { get; set; }
        
        /// <summary>
        /// Whether the input is pressed (true) or released (false).
        /// </summary>
        public bool Pressed { get; set; }
        
        /// <summary>
        /// Whether shift key is pressed.
        /// </summary>
        public bool ShiftPressed { get; set; }
        
        /// <summary>
        /// Whether control key is pressed.
        /// </summary>
        public bool CtrlPressed { get; set; }
        
        /// <summary>
        /// Whether alt key is pressed.
        /// </summary>
        public bool AltPressed { get; set; }
        
        /// <summary>
        /// User ID associated with this input event.
        /// </summary>
        public UserId UserId { get; set; }
        
        /// <summary>
        /// Timestamp when the event occurred.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Determines if two InputEventData instances are equal.
        /// </summary>
        /// <param name="other">The other InputEventData to compare with.</param>
        /// <returns>True if the instances are equal; otherwise false.</returns>
        public bool Equals(InputEventData other)
        {
            return EventType == other.EventType &&
                   MouseButton == other.MouseButton &&
                   KeyCode == other.KeyCode &&
                   Modifiers == other.Modifiers &&
                   Position.Equals(other.Position) &&
                   Pressed == other.Pressed &&
                   ShiftPressed == other.ShiftPressed &&
                   CtrlPressed == other.CtrlPressed &&
                   AltPressed == other.AltPressed &&
                   UserId.Equals(other.UserId) &&
                   Timestamp == other.Timestamp;
        }

        /// <summary>
        /// Determines if two InputEventData instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>True if the instances are equal; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            return obj is InputEventData other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this InputEventData.
        /// </summary>
        /// <returns>A hash code based on all properties.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(EventType);
            hash.Add(MouseButton);
            hash.Add(KeyCode);
            hash.Add(Modifiers);
            hash.Add(Position);
            hash.Add(Pressed);
            hash.Add(ShiftPressed);
            hash.Add(CtrlPressed);
            hash.Add(AltPressed);
            hash.Add(UserId);
            hash.Add(Timestamp);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Determines if two InputEventData instances are equal.
        /// </summary>
        /// <param name="left">The first InputEventData.</param>
        /// <param name="right">The second InputEventData.</param>
        /// <returns>True if the instances are equal; otherwise false.</returns>
        public static bool operator ==(InputEventData left, InputEventData right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines if two InputEventData instances are not equal.
        /// </summary>
        /// <param name="left">The first InputEventData.</param>
        /// <param name="right">The second InputEventData.</param>
        /// <returns>True if the instances are not equal; otherwise false.</returns>
        public static bool operator !=(InputEventData left, InputEventData right)
        {
            return !left.Equals(right);
        }
    }
}
