using System;

namespace BarkMoon.GameComposition.Core.Types
{
    /// <summary>
    /// Input event types (Source device)
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
        Unknown = 0,
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        Num0, Num1, Num2, Num3, Num4, Num5, Num6, Num7, Num8, Num9,
        Escape, Space, Return, Enter, Backspace, Tab,
        Up, Down, Left, Right,
        Delete, Insert, Home, End, PageUp, PageDown,
        F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
        Shift, Control, Alt
    }
}
