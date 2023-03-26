using UnityEngine;
using UnityEngine.AI;
public abstract class EnemyAIBase
{
    public abstract void EnterState(Enemy enemy);
    public abstract void UpdateState(Enemy enemy);
    public void DisableCharachter(Enemy enemy, bool disable)
    {
        enemy.Animator.applyRootMotion = !disable;
        enemy.GetComponent<Collider>().isTrigger = disable;
        enemy.GetComponent<Rigidbody>().useGravity = !disable;
        enemy.GetComponent<NavMeshAgent>().enabled = !disable;
    }
}
public class EnemySpawnState : EnemyAIBase
{
    private float Timer;
    public override void EnterState(Enemy enemy)
    {
        enemy.Source.PlayOneShot(enemy.EnemySpawnSFX);
    }

    public override void UpdateState(Enemy enemy)
    {
        Timer += Time.deltaTime;
        enemy.transform.Translate(Vector3.up * Time.deltaTime);
        if(Timer > 2f)
        {
            enemy.SwitchState(enemy.RunState);
            enemy.GetComponent<Collider>().enabled = true;
        }
    }
}
public class EnemyRunState : EnemyAIBase
{
    float Timer;
    float RandomSoundTime;
    public override void EnterState(Enemy enemy)
    {
        DisableCharachter(enemy, false);
        //enemy.Animator.SetBool("Spawn", false);
        enemy.Animator.SetBool("StuckHit", false);
        enemy.Animator.SetBool("Run", true);
        RandomSoundTime = Random.Range(0.5f, 2f);
    }

    public override void UpdateState(Enemy enemy)
    {
        enemy.SetDestination();
        SFX(enemy);
        float dist = Vector3.Distance(enemy.transform.position, enemy.Player.position);
        float random = Random.Range(0f, 5f);
        if (dist < random)
        {
            enemy.SwitchState(enemy.AttackState);
        }
    }
    void SFX(Enemy enemy)
    {
        Timer += Time.deltaTime;

        if(Timer > RandomSoundTime)
        {
            int RandomClip = Random.Range(0, enemy.DefaultSFX.Count -1);
            enemy.Source.PlayOneShot(enemy.DefaultSFX[RandomClip]);
            RandomSoundTime = Random.Range(0.5f, 2f);
            Timer = 0;
        }
    }

}
public class EnemyAttackState: EnemyAIBase
{
    private float Timer;
    public override void EnterState(Enemy enemy)
    {
        enemy.DamageReduction = 4;
        enemy.Animator.SetTrigger("Attack");
        enemy.Animator.SetBool("Run", false);
        enemy.Source.PlayOneShot(enemy.EnemyStikeSFX);
        Timer = 0;
    }

    public override void UpdateState(Enemy enemy)
    {
        Timer += Time.deltaTime;
        float dist = Vector3.Distance(enemy.transform.position, enemy.Player.position);
        if (dist > 5)
        {
            enemy.Animator.SetBool("StuckHit", false);
            enemy.SwitchState(enemy.RunState);
            enemy.Source.priority = 128;
            enemy.Source.spatialBlend = 1;
        }
        if(Timer > enemy.AttackRate)
        {
            enemy.Animator.SetBool("StuckHit", true);
            enemy.Source.priority = 110;
            enemy.Source.spatialBlend = 0;
            enemy.Source.PlayOneShot(enemy.EnemyPunchSFX);
            Timer = 0;
        }
    }
}
public class EnemyRammedState : EnemyAIBase
{
    private float Timer;
    public override void EnterState(Enemy enemy)
    {
        enemy.DamageReduction = 8;
        enemy.Animator.applyRootMotion = false;
        enemy.Animator.SetBool("Run", false);
        enemy.Animator.SetBool("StuckHit", true);
        DisableCharachter(enemy, true);
        enemy.transform.LookAt(enemy.Player);
    }

    public override void UpdateState(Enemy enemy)
    {
        Timer += Time.deltaTime;
        if (Timer > enemy.AttackRate)
        {
            Timer = 0;
        }
    }
}

