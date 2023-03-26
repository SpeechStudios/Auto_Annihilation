using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum DamageType {MiniGun,Cannon,Rocket,EnemyPunch,EnemyStrike, Instant, Ram}

public class Interfaces : MonoBehaviour
{
    public static float RandomizeDamage(float originalDamage)
    {
        float randomDamage = Random.Range((originalDamage / 100) * 80, (originalDamage / 100) * 120);
        return (Mathf.RoundToInt(randomDamage));
    }
}
public interface IDamageable
{
    void Damage(float damage, DamageType dmgType);
}
public interface ITargetable
{
    void Highlight();
}

