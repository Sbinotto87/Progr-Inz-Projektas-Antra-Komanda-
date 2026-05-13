using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public Camera cam;
    public float range = 3f;
    public float damage = 25f;

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            // Try boss first
            BossController boss = hit.collider.GetComponentInParent<BossController>();
            if (boss != null)
            {
                float dmg = damage;
                var c = CheatsManager.Instance;
                if (c != null && c.CheatsEnabled)
                {
                    if (c.OneShotKill) dmg = boss.maxHealth + 1f;
                    else dmg *= c.DamageMultiplier;
                }
                boss.TakeDamage(dmg);
                return;
            }

            EnemyController enemy = hit.collider.GetComponentInParent<EnemyController>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}