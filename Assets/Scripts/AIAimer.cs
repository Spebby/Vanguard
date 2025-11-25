using System;
using Gilzoide.UpdateManager;
using UnityEngine;

public class AIAimer : MonoBehaviour, IFixedUpdatable {
    const byte ENEMY_COLOUR = 1;
    Gun _gun;
    Transform _target;

    void OnEnable() => this.RegisterInManager();
    void OnDisable() => this.UnregisterInManager();
    
    void Awake() {
        _gun = GetComponentInChildren<Gun>();
        if (!_gun) Debug.LogError($"Missing gun on {gameObject}");
    }

    void Start() {
        _target = GameObject.FindWithTag("Player").transform;
        if (!_target) {
            throw new Exception($"Cannot find player target!");
        }

        _gun.Colour = ENEMY_COLOUR;
    }

    public void ManagedFixedUpdate() {
        if (!_target) {
            this.UnregisterInManager();
            _gun.StopShooting();
        }
        
        _gun.Target = _target.position;
    }
}