using Gilzoide.UpdateManager;
using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;


public class DodgeManager : IFixedUpdatable {
    GameObject _parent;
    Collider2D _collider;
    SpriteRenderer _renderer;
    VehicleConfig _config;

    int _currentDodges;
    float _regenTimer;
    float _dodgeCooldown;

    readonly Color _dodgeColor = Color.gray2;
    
    public DodgeManager(SpriteRenderer renderer, VehicleConfig config) {
        _renderer = renderer;
        _parent   = renderer.transform.parent.gameObject;
        _config   = config;
        _collider = _parent.GetComponentInParent<Collider2D>();

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

        TweenSettings i = new(duration: _config.DodgeInvulnerability * 0.75f, ease: Ease.OutSine);
        TweenSettings o = new (duration:_config.DodgeInvulnerability * 0.25f, ease: Ease.InSine);
        
        Sequence.Create()
                .Chain(Tween.Scale(_parent.transform, 1.5f, i))
                .Chain(Tween.Scale(_parent.transform, 1.0f, o))
                .Group(Tween.Color(_renderer, Color.white, o))
                .ChainCallback(() => {
                     _collider.enabled = true;
                 });
        
        _collider.enabled = false;
        _currentDodges--;
        _renderer.color = _dodgeColor;
        _dodgeCooldown        = _config.DodgeCooldown + _config.DodgeInvulnerability;
    }

    public void ManagedFixedUpdate() {
        _dodgeCooldown        -= Time.deltaTime;
        _regenTimer           -= Time.fixedDeltaTime;
        
        if (!(_regenTimer <= Mathf.Epsilon) || _currentDodges >= _config.Dodges) return;
        _currentDodges++;
        _regenTimer = _config.DodgeRegen;
    }
}