using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionScript : MonoBehaviour
{
    public float Radius;
    public LayerMask Layer;
    [HideInInspector] public float Damage;
    [HideInInspector] public DamageType dmgType;
    private void Start()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, Radius,Layer);
        foreach (var item in hitColliders)
        {
            if(item.GetComponent<IDamageable>() !=null)
            {
                item.GetComponent<IDamageable>().Damage(Damage, dmgType);
            }
        }
    }
}
