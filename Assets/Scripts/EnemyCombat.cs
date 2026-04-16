using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public Camera cam;
    public float range = 3f;
    public int damage = 25;

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
            EnemyController enemy = hit.collider.GetComponentInParent<EnemyController>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}