using System;
using UnityEngine;
using UnityEngine.InputSystem;


enum ControlScheme : byte {
    Gamepad,
    Mouse,
    None
}

[RequireComponent(typeof(PlayerInput))]
public class Aiming : MonoBehaviour {
    @InputSystem_Actions _actions;
    @InputSystem_Actions.PlayerActions _playerMap;
    PlayerInput _input;

    // TODO: update these to IGNORE start and stop on mouse controls.
    void StartLFire(InputAction.CallbackContext ctx) {
        if (_activeScheme != ControlScheme.Gamepad) return;
        _leftGun.StartShooting();
    }

    void StopLFire(InputAction.CallbackContext ctx) {
        if (_activeScheme != ControlScheme.Gamepad) return;
        _leftGun.StopShooting();
    }

    void StartRFire(InputAction.CallbackContext obj) {
        if (_activeScheme != ControlScheme.Gamepad) return;
        _rightGun.StartShooting();
    }

    void StopRFire(InputAction.CallbackContext obj) {
        if (_activeScheme != ControlScheme.Gamepad) return;
        _rightGun.StopShooting();
    }

    Gun _leftGun;
    Gun _rightGun;

    ControlScheme _activeScheme;
    
    void Awake() {
        // find guns
        Gun[] guns = GetComponentsInChildren<Gun>();
        if (guns.Length != 2) {
            Debug.LogWarning($"Unexpected number of guns {guns.Length}");
        }
        
        // fine for prototype
        _leftGun  = guns[0];
        _rightGun = guns[1];
        
        _actions   = new @InputSystem_Actions();
        _playerMap = _actions.Player;

        _input = GetComponent<PlayerInput>();
        _input.onControlsChanged += ChangeControlScheme;
    }

    void ChangeControlScheme(PlayerInput obj) {
        _activeScheme = obj.currentControlScheme switch {
            "Gamepad"        => ControlScheme.Gamepad,
            "Keyboard&Mouse" => ControlScheme.Mouse,
            _                => throw new NotImplementedException()
        };

        if (!gameObject.activeSelf || _activeScheme != ControlScheme.Gamepad) return;
        _playerMap.LAim.started  += StartLFire;
        _playerMap.LAim.canceled += StopLFire;

        _playerMap.RAim.started  += StartRFire;
        _playerMap.RAim.canceled += StopRFire;
    }

    void OnEnable() {
        _actions.Enable();
        ChangeControlScheme(_input);
        _playerMap.LAim.performed += AimLeft;
        _playerMap.RAim.performed += AimRight;
    }

    void OnDisable() {
        _actions.Disable();
        _playerMap.LAim.started   -= StartLFire;
        _playerMap.LAim.canceled  -= StopLFire;
        _playerMap.LAim.performed -= AimLeft;

        _playerMap.RAim.started   -= StartRFire;
        _playerMap.RAim.canceled  -= StopRFire;
        _playerMap.RAim.performed -= AimRight;
    }

    // We deliver the aiming positions in absolute space. If we are on analog, then we need to offset this position
    // if we are on mouse, then we do not offset and pass directly to the gun's aiming functions
    void AimLeft(InputAction.CallbackContext ctx) {
        _lReticle = AimGun(_leftGun, ctx.ReadValue<Vector2>(), _activeScheme);
        _leftGun.Target = _lReticle;
    }

    void AimRight(InputAction.CallbackContext ctx) {
        _rReticle = AimGun(_rightGun, ctx.ReadValue<Vector2>(), _activeScheme);
        _rightGun.Target = _rReticle;
    }

    static Vector2 AimGun(in Gun gun, in Vector2 target, ControlScheme scheme) {
        if (target.magnitude <= Mathf.Epsilon) return gun.Target;
        if (scheme == ControlScheme.Mouse) {
            return Camera.main!.ScreenToWorldPoint(target);
        }

        return target + (Vector2)gun.transform.position;
    }

    Vector2 _lReticle;
    Vector2 _rReticle;

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_lReticle, 0.25f);
        Gizmos.DrawWireSphere(_rReticle, 0.25f);
    }
}