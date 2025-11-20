using Gilzoide.UpdateManager;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputHelper;


[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Runtime.Editor")]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour, IFixedUpdatable {
    @InputSystem_Actions _actions;
    @InputSystem_Actions.PlayerActions _playerMap;
    PlayerInput _input;
    ControlScheme _activeScheme;

    [SerializeField] SpriteRenderer body;
    [SerializeField] internal VehicleConfig config;
    float _moveDir;
    
    DodgeManager _dodgeManager;
    
    void UpdateMoveDir(InputAction.CallbackContext ctx) => _moveDir = ctx.ReadValue<float>();
    void UpdateControlScheme(PlayerInput obj) => _activeScheme = GetControlScheme(obj);

    #region Unity Boilerplate
    void Awake() {
        _actions   = new @InputSystem_Actions();
        _playerMap = _actions.Player;

        _input                   =  GetComponent<PlayerInput>();
        _input.onControlsChanged += UpdateControlScheme;

        if (config.Dodges > 0) {
            _dodgeManager = new DodgeManager(body, config);
        }
    }

    void OnEnable() {
        this.RegisterInManager();
        UpdateControlScheme(_input);

        _actions.Enable();
        _playerMap.Move.performed += UpdateMoveDir;
        if (_dodgeManager != null) _playerMap.Dodge.performed += _dodgeManager.Dodge;
    }

    void OnDisable() {
        this.UnregisterInManager();
        
        _actions.Disable();
        _playerMap.Move.performed -= UpdateMoveDir;
        if (_dodgeManager != null) _playerMap.Dodge.performed -= _dodgeManager.Dodge;
    }

    void OnDestroy() {
        _dodgeManager = null;
    }
    #endregion

    
    public void ManagedFixedUpdate() {
        transform.position += (Vector3)(Vector2.right * (_moveDir * config.Speed * Time.fixedDeltaTime));
    }
}
