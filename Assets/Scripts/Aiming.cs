using UnityEngine;
using UnityEngine.InputSystem;
using static InputHelper;


[RequireComponent(typeof(PlayerInput))]
public class Aiming : MonoBehaviour {
    @InputSystem_Actions _actions;
    @InputSystem_Actions.PlayerActions _playerMap;
    PlayerInput _input;
    ControlScheme _activeScheme;

    void UpdateControlScheme(PlayerInput obj) => _activeScheme = GetControlScheme(obj);
    void StartLFire(InputAction.CallbackContext ctx) => _leftGun.StartShooting();
    void StopLFire(InputAction.CallbackContext ctx)  => _leftGun.StopShooting();
    void StartRFire(InputAction.CallbackContext obj) => _rightGun.StartShooting();
    void StopRFire(InputAction.CallbackContext obj)  => _rightGun.StopShooting();

    Gun _leftGun;
    Gun _rightGun;

    
    #region Unity Boilerplate
    void Awake() {
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
        _input.onControlsChanged += UpdateControlScheme;
    }

    void OnEnable() {
        UpdateControlScheme(_input);
        _actions.Enable();
        _playerMap.LAim.performed += AimLeft;
        _playerMap.RAim.performed += AimRight;

        _playerMap.LShoot.started  += StartLFire;
        _playerMap.LShoot.canceled += StopLFire;
        _playerMap.RShoot.started  += StartRFire;
        _playerMap.RShoot.canceled += StopRFire;
    }

    void OnDisable() {
        _actions.Disable();
        _playerMap.LAim.performed -= AimLeft;
        _playerMap.RAim.performed -= AimRight;
        
        _playerMap.LShoot.started  -= StartLFire;
        _playerMap.LShoot.canceled -= StopLFire;
        _playerMap.RShoot.started  -= StartRFire;
        _playerMap.RShoot.canceled -= StopRFire;
    }
    #endregion
    
    void AimLeft(InputAction.CallbackContext ctx) {
        _lReticle = AimGun(_leftGun, ctx.ReadValue<Vector2>(), _activeScheme);
        _leftGun.Target = _lReticle;
    }

    void AimRight(InputAction.CallbackContext ctx) {
        _rReticle = AimGun(_rightGun, ctx.ReadValue<Vector2>(), _activeScheme);
        _rightGun.Target = _rReticle;
    }

    // Gamepad uses relative coordinates, mouse position uses screen coordinates.
    // Gamepad needs an offset, and mouse position needs to be mapped screenspace -> worldspace
    // If "no movement" has happened this input update, use the previous target (to prevent aiming at a default position)
    static Vector2 AimGun(in Gun gun, in Vector2 target, ControlScheme scheme) {
        // This check is done first b/c Unity switches inputs before firing event that control schemes have changed
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