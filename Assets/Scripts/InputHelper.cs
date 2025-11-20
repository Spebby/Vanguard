using System;
using UnityEngine.InputSystem;


public static class InputHelper {
    public enum ControlScheme : byte {
        Gamepad,
        Mouse
    }

    public static ControlScheme GetControlScheme(PlayerInput obj) {
        return obj.currentControlScheme switch {
            "Gamepad"        => ControlScheme.Gamepad,
            "Keyboard&Mouse" => ControlScheme.Mouse,
            _                => throw new NotImplementedException()
        };
    }
}