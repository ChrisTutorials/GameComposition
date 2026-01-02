using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Turn input event types (Source device)
    /// </summary>
    public enum InputSourceType
    {
        /// <summary>
        /// Keyboard input.
        /// </summary>
        Key = 0,

        /// <summary>
        /// Mouse input.
        /// </summary>
        Mouse = 1,

        /// <summary>
        /// Gamepad/controller input.
        /// </summary>
        Gamepad = 2,

        /// <summary>
        /// Touch/gesture input.
        /// </summary>
        Gesture = 3
    }

    /// <summary>
    /// Type of input interaction/event
    /// </summary>
    public enum InputEventType
    {
        None = 0,
        Press = 1,
        Release = 2,
        Move = 3,
        Scroll = 4,
        Key = 5
    }

    /// <summary>
    /// Gesture types for touch input
    /// </summary>
    public enum GestureType
    {
        None = 0,
        Tap = 1,
        DoubleTap = 2,
        LongPress = 3,
        Drag = 4,
        Pinch = 5,
        Rotate = 6,
        Swipe = 7
    }

    [Flags]
    public enum InputModifiers
    {
        None = 0,
        Shift = 1,
        Ctrl = 2,
        Alt = 4,
        Meta = 8
    }

    /// <summary>
    /// Engine-agnostic KeyCode definitions.
    /// Maps closely to standard keyboard layouts.
    /// </summary>
    public enum KeyCode
    {
        /// <summary>Unknown or unrecognized key</summary>
        Unknown = 0,
        
        /// <summary>Letter keys A-Z</summary>
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        
        /// <summary>Number keys 0-9</summary>
        Num0, Num1, Num2, Num3, Num4, Num5, Num6, Num7, Num8, Num9,
        
        /// <summary>Special keys</summary>
        Escape, Space, Return, Enter, Backspace, Tab,
        
        /// <summary>Arrow keys</summary>
        Up, Down, Left, Right,
        
        /// <summary>Editing keys</summary>
        Delete, Insert, Home, End, PageUp, PageDown,
        
        /// <summary>Function keys F1-F12</summary>
        F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
        
        /// <summary>Modifier keys</summary>
        Shift, Control, Alt
    }
}
