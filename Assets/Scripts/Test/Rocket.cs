using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public Vector3 Target;
    public float LookAtSpeed;
    [HideInInspector] public float RocketSpeed;
    [HideInInspector] public float ExplosionDamage;
    public GameObject Explosion;
    //[HideInInspector] public LayerMask WeaponHitMask;
    void Start()
    {
        StartCoroutine(LookAt());
    }
    private void Update()
    {
        transform.position += transform.forward * RocketSpeed * Time.deltaTime;
    }

    IEnumerator LookAt()
    {
        Quaternion lookRotation = Quaternion.LookRotation(Target - transform.position);

        float time = 0;
        while(time < 1)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time);
            time += Time.deltaTime * LookAtSpeed;
            yield return null;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        GameObject explosion = Instantiate(Explosion, transform.position, transform.rotation);
        explosion.GetComponent<ExplosionScript>().Damage = ExplosionDamage;
        explosion.GetComponent<ExplosionScript>().dmgType = DamageType.Rocket;
        Invoke("DestroySoon", 2f);
    }
    void DestroySoon()
    {
        Destroy(gameObject);
    }
}
