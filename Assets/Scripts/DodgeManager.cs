using Gilzoide.UpdateManager;
using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;


public class DodgeManager : IFixedUpdatable {
    GameObject _parent;
    SpriteRenderer _renderer;
    VehicleConfig _config;

    int _currentDodges;
    float _regenTimer;
    float _dodgeCooldown;
    float _dodgeInvulnerability;

    readonly Color _dodgeColor = Color.gray2;
    
    public DodgeManager(SpriteRenderer renderer, VehicleConfig config) {
        _renderer = renderer;
        _parent = renderer.transform.parent.gameObject;
        _config = config;

        _currentDodges = config.Dodges;
        _regenTimer    = config.DodgeRegen;
        
        this.RegisterInManager();
    }

    ~DodgeManager() {
        _renderer = null;
        _config = null;
        
        this.UnregisterInManager();
    }

    public void Dodge(InputAction.CallbackContext ctx) {
        if (_currentDodges == 0 || _dodgeCooldown > Mathf.Epsilon) return;

        Sequence.Create()
                .Chain(Tween.Scale(_parent.transform, 1.5f, _config.DodgeInvulnerability * 0.75f, Ease.OutSine))
                .Chain(Tween.Scale(_parent.transform, 1.0f, _config.DodgeInvulnerability * 0.25f, Ease.InSine))
                .ChainCallback(() => _renderer.color = Color.white);
        
        _currentDodges--;
        _renderer.color = _dodgeColor;
        _dodgeInvulnerability = _config.DodgeInvulnerability;
        _dodgeCooldown        = _config.DodgeCooldown + _config.DodgeInvulnerability;
    }

    public void ManagedFixedUpdate() {
        _dodgeCooldown        -= Time.deltaTime;
        _regenTimer           -= Time.fixedDeltaTime;
        _dodgeInvulnerability -= Time.deltaTime;
        
        if (!(_regenTimer <= Mathf.Epsilon) || _currentDodges >= _config.Dodges) return;
        _currentDodges++;
        _regenTimer = _config.DodgeRegen;
    }
}