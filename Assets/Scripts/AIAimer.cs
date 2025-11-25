using System;
using Gilzoide.UpdateManager;
using UnityEngine;

public class AIAimer : MonoBehaviour {
    const byte ENEMY_COLOUR = 1;
    Gun _gun;
    Transform _target;
    
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

    void FixedUpdate() {
        if (!_target) {
            _gun.StopShooting();
        }
        
        _gun.Target = _target.position;
    }
}