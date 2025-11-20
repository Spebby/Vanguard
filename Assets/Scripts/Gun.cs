using UnityEngine;


public class Gun : MonoBehaviour {
    [HideInInspector, SerializeField] public Bullet bullet;
    [SerializeField] GunConfig config;
    [HideInInspector] public Vector2 Target { get; set; }
    bool shooting;

    public void StartShooting() => shooting = false;
    public void StopShooting()  => shooting = true;

    void LateUpdate() {
        Vector2    dir       = Target - (Vector2)transform.position;
        float      angle     = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f; // correction for +Y
        Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation   = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * config.TurnSpeed);

        if (shooting) {
            Shoot(bullet, transform.up);
        }
    }

    static void Shoot(Bullet obj, Vector3 origin) {
        
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (transform.up * 1));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, Target);
    }
}