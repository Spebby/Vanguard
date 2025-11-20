using System;
using Gilzoide.UpdateManager;
using UnityEngine;


public class Gun : MonoBehaviour, IFixedUpdatable, ILateUpdatable {
    [SerializeField] GunConfig config;
    [HideInInspector] public Vector2 Target { get; set; }
    public Vector3 Tip => transform.up + transform.position;
    
    bool _shooting;
    float _fireDelay;

    #region Unity Boilerplate
    void OnEnable() => this.RegisterInManager();
    void OnDisable() => this.UnregisterInManager();
    #endregion
    
    public void StartShooting() => _shooting = true;
    public void StopShooting()  => _shooting = false;
    void Shoot(Bullet obj, Vector3 origin, Quaternion rotation) {
        Bullet b = Instantiate(obj.gameObject, origin, rotation).GetComponent<Bullet>();
        b.Init(config.Speed, config.Lifetime, config.Damage);
    }

    public void ManagedLateUpdate() {
        Vector2    dir       = Target - (Vector2)transform.position;
        float      angle     = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f; // correction for +Y
        Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation   = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * config.TurnSpeed);
    }
    
    public void ManagedFixedUpdate() {
        _fireDelay -= Time.deltaTime;
        if (_shooting && _fireDelay <= 0) {
            Shoot(config.bulletPrefab, Tip, transform.rotation);
            _fireDelay = 1f / config.FiringSpeed;
        }
    }
    
    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (transform.up * 1));
    }
}