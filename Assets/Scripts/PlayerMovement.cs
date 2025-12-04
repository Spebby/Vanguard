using Gilzoide.UpdateManager;
using UnityEngine;
using UnityEngine.InputSystem;


[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Runtime.Editor")]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour, IFixedUpdatable {
    @InputSystem_Actions _actions;
    @InputSystem_Actions.PlayerActions _playerMap;

    [SerializeField] SpriteRenderer body;
    [SerializeField] internal VehicleConfig config;
    float _moveDir;
    
    DodgeManager _dodgeManager;
    [SerializeField] Vector3 lBound;
    [SerializeField] Vector3 rBound;
    
    void UpdateMoveDir(InputAction.CallbackContext ctx) => _moveDir = ctx.ReadValue<float>();

    #region Unity Boilerplate
    void Awake() {
        _actions   = new @InputSystem_Actions();
        _playerMap = _actions.Player;

        if (config.Dodges > 0) {
            _dodgeManager = new DodgeManager(body, config);
        }
    }

    void OnEnable() {
        this.RegisterInManager();

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
        float newX = transform.position.x + _moveDir * config.Speed * Time.fixedDeltaTime;
        transform.position = new Vector3(Mathf.Clamp(newX, lBound.x, rBound.x), transform.position.y, transform.position.z);
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lBound, 0.1f);
        Gizmos.DrawWireSphere(rBound, 0.1f);
    }
}
