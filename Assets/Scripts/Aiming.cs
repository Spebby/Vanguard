using Gilzoide.UpdateManager;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputHelper;


[RequireComponent(typeof(PlayerInput))]
public class Aiming : MonoBehaviour, ILateUpdatable {
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
        this.RegisterInManager();
    }

    void OnDisable() {
        _actions.Disable();
        _playerMap.LAim.performed -= AimLeft;
        _playerMap.RAim.performed -= AimRight;
        
        _playerMap.LShoot.started  -= StartLFire;
        _playerMap.LShoot.canceled -= StopLFire;
        _playerMap.RShoot.started  -= StartRFire;
        _playerMap.RShoot.canceled -= StopRFire;
        this.UnregisterInManager();
    }
    #endregion
    
    void AimLeft(InputAction.CallbackContext ctx) {
        Vector2 input = ctx.ReadValue<Vector2>();
        if (input.sqrMagnitude > Mathf.Epsilon)
            _lAimDir = input.normalized;
    }

    void AimRight(InputAction.CallbackContext ctx) {
        Vector2 input = ctx.ReadValue<Vector2>();
        if (input.sqrMagnitude > Mathf.Epsilon)
            _rAimDir = input.normalized;
    }

    public void ManagedLateUpdate() {
        // Gamepad uses relative coordinates, mouse position uses screen coordinates.
        // Gamepad needs an offset, and mouse position needs to be mapped screenspace -> worldspace
        // If "no movement" has happened this input update, use the previous target (to prevent aiming at a default position)
        if (_activeScheme == ControlScheme.Mouse) {
            _lReticle = Camera.main!.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            _rReticle = _lReticle;
        } else {
            _lReticle = (Vector2)_leftGun.transform.position + _lAimDir;
            _rReticle = (Vector2)_rightGun.transform.position + _rAimDir;
        }

        _leftGun.Target  = _lReticle;
        _rightGun.Target = _rReticle;
    }

    Vector2 _lAimDir;
    Vector2 _rAimDir;
    Vector2 _lReticle;
    Vector2 _rReticle;

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_lReticle, 0.25f);
        Gizmos.DrawWireSphere(_rReticle, 0.25f);
    }
}