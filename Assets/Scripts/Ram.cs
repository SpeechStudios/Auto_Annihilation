using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Ram : MonoBehaviour
{
    public Rigidbody CarRb;
    public Transform RamPosition;
    public List<Enemy> EnemiesOnRam;

    private void OnTriggerEnter(Collider other)
    {
        if (CarRb.velocity.magnitude > 7)
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                Enemy enemy = other.GetComponent<Enemy>();
                if (!enemy.Rammed)
                {
                    enemy.Rammed = true;
                    enemy.SwitchState(enemy.RammedState);
                    enemy.Damage(Mathf.RoundToInt(CarRb.velocity.magnitude), DamageType.Ram);
                    enemy.SwitchState(enemy.RammedState);
                    EnemiesOnRam.Add(other.GetComponent<Enemy>());
                    GameObject enemyObj = other.gameObject;
                    enemyObj.transform.parent = transform;
                    enemyObj.transform.position = new Vector3(enemyObj.transform.position.x, RamPosition.position.y + 0.5f, RamPosition.position.z);
                }
            }
        }
    }
    public void RamEnemiesIntoWall()
    {
        if (CarRb.velocity.magnitude > 15)
        {
            foreach (var item in EnemiesOnRam)
            {
                item.Damage(Mathf.RoundToInt(CarRb.velocity.magnitude)*2,DamageType.Ram);
            }
        }
    }
}
