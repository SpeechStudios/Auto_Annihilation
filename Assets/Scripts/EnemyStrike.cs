using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStrike : MonoBehaviour
{
    public float Damage;
    private SphereCollider col;
    private void Awake()
    {
        col = GetComponent<SphereCollider>();
    }


    void Update()
    {
        col.center += Vector3.forward * Time.deltaTime * 4;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && other.GetComponentInParent<IDamageable>() != null)
        {
            other.GetComponentInParent<IDamageable>().Damage(Damage, DamageType.EnemyStrike);
        }
    }
}
