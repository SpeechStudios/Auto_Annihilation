using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour, IDamageable, ITargetable
{
    public float MaxHealth = 100;
    public float MyDamageValue = 25f;
    public float AttackRate = 1f;
    public GameObject HealthBar;
    public GameObject SpawnerVFX;
    public GameObject HitVFX;
    public GameObject Strike;
    public Transform StrikePosition;
    public ParticleSystem PS_StuckAttack1, PS_StuckAttack2;
    public Gradient HealthColors;
    public GameObject DeathEffect;
    [Header("SFX")]
    public List<AudioClip> DefaultSFX;
    public AudioClip EnemySpawnSFX;
    public AudioClip EnemyStikeSFX;
    public AudioClip EnemyPunchSFX;
    public AudioClip EnemyDeathSFX;
    public AudioClip EnemyRammedSFX;


    [HideInInspector] public AudioSource Source;
    [HideInInspector] public float DamageReduction = 1f;
    [HideInInspector] public float CurrentHealth;
    [HideInInspector] public bool Rammed;
    [HideInInspector] public NavMeshAgent Agent;
    [HideInInspector] public Rigidbody Rb;
    [HideInInspector] public Collider Col;
    [HideInInspector] public Animator Animator;
    [HideInInspector] public Transform Player;
    private readonly List<GameObject> HitVFXPool = new();
    private Material Mat;
    private bool Highlighted;


    EnemyAIBase CurrentState;
    public EnemySpawnState SpawnState = new();
    public EnemyRunState RunState = new();
    public EnemyAttackState AttackState = new();
    public EnemyRammedState RammedState = new();


    void Start()
    {
        Source = GetComponent<AudioSource>();
        Mat = GetComponentInChildren<SkinnedMeshRenderer>().material;
        Agent = GetComponent<NavMeshAgent>();
        Rb = GetComponent<Rigidbody>();
        Col = GetComponent<Collider>();
        Animator = GetComponent<Animator>();
        Player = GameObject.FindGameObjectWithTag("Car").transform;
        GameManager.Instance.EnemyList.Add(this);

        Instantiate(SpawnerVFX, transform.position + (Vector3.up * 2f), SpawnerVFX.transform.rotation);
        CurrentHealth = MaxHealth;
        CurrentState = SpawnState;
        CurrentState.EnterState(this);
    }
    void Update()
    {
        //Skin Animation
        Mat.SetTextureScale("_MainTex", new Vector2(0.5f + Mathf.PingPong((Time.time / 7.5f), 1f), 0.5f + Mathf.PingPong((Time.time / 7.5f), 1f)));
        Vector3 playerLocation = new Vector3(Player.transform.position.x, 7 , Player.transform.position.z);
        HealthBar.transform.LookAt(playerLocation);
        StateManager();
        CheckDeath();
    }
    public void Damage(float damage, DamageType dmgType)
    {
        float ActualDamage = Interfaces.RandomizeDamage(damage);
        CurrentHealth -= ActualDamage;
        GameManager.Instance.CheckWeaponDamage(damage, dmgType);
        if (CurrentHealth > 0)
        {
            GetHit(1);
            if (damage > 40)
            {
                GetHit(3);
            }
        }

        if (HealthBar)
        {
            if (!HealthBar.activeInHierarchy)
            {
                HealthBar.SetActive(true);
            }
            HealthBar.GetComponent<Slider>().value = CurrentHealth / MaxHealth;
            HealthBar.GetComponent<Slider>().fillRect.GetComponent<Image>().color = HealthColors.Evaluate(CurrentHealth / MaxHealth);
        }
    }
    public void CheckDeath()
    {
        if (CurrentHealth <= 0)
        {
            if (GameObject.FindGameObjectWithTag("Ram").GetComponent<Ram>().EnemiesOnRam.Contains(this))
            {
                GameObject.FindGameObjectWithTag("Ram").GetComponent<Ram>().EnemiesOnRam.Remove(this);
            }
            GameManager.Instance.EnemyList.Remove(this);
            foreach (var item in HitVFXPool)
            {
                Destroy(item);
            }
            AudioSource.PlayClipAtPoint(EnemyDeathSFX, transform.position);
            Instantiate(DeathEffect, transform.position + Vector3.up, transform.rotation);
            Destroy(gameObject);
        }
    }
    public void Highlight()
    {
        Mat.SetColor("_EmissionColor", Mat.color * 10);
        Highlighted = true;
    }
    public void FixedUpdate()
    {
        if (Highlighted)
        {
            Mat.SetColor("_EmissionColor", Mat.color * 3);
            Highlighted = false;
        }
    }
    void StateManager()
    {
        CurrentState.UpdateState(this);
    }
    public void SwitchState(EnemyAIBase state)
    {
        CurrentState = state;
        state.EnterState(this);
    }
    public void SetDestination()
    {
        if (Agent)
        {
            NavMeshPath path = new();
            if (Physics.Raycast(Player.position, -Player.up, out RaycastHit hit, Mathf.Infinity))
            {
                float Range = GameManager.Instance.EnemyList.Count * 0.1f;
                float Dist = Vector3.Distance(transform.position, Player.position) / 10;
                if (Range > 15)
                {
                    Range = 15;
                }
                Vector3 point = hit.point + (Player.forward * Random.Range(-Range, Range) * Dist) + (Player.right * Random.Range(-Range, Range) * Dist);
                Agent.CalculatePath(point, path);
                Agent.SetPath(path);
            }
        }
    }
    public void GetHit(int Amount)
    {
        for (int i = 0; i < Amount; i++)
        {
            GameObject obj;
            if(HitVFXPool.Count > 0)
            {
                HitVFXPool[0].transform.position = transform.position + Vector3.up;
                HitVFXPool[0].SetActive(true);
                obj = HitVFXPool[0];
                HitVFXPool.RemoveAt(0);
            }
            else
            {
                obj = Instantiate(HitVFX, transform.position + Vector3.up, transform.rotation);
                StartCoroutine("PlaceInPool", obj);
            }
            float randX = Random.Range(-0.4f, 0.4f);
            float randY = Random.Range(-0.6f, 0.6f);
            obj.transform.Translate(new Vector3(randX, randY));
        }
    }
    public void Attack()
    {
       GameObject obj = Instantiate(Strike, StrikePosition.position, StrikePosition.rotation);
        obj.GetComponent<EnemyStrike>().Damage = MyDamageValue;
    }
    public void StuckAttack1()
    {
        PS_StuckAttack1.Play();
        Player.GetComponent<IDamageable>().Damage(MyDamageValue / DamageReduction, DamageType.EnemyPunch);
    }
    public void StuckAttack2()
    {
        PS_StuckAttack2.Play();
        Player.GetComponent<IDamageable>().Damage(MyDamageValue / DamageReduction, DamageType.EnemyPunch);
    }
    IEnumerator PlaceInPool(GameObject obj)
    {
        yield return new WaitForSeconds(1f);
        obj.SetActive(false);
        HitVFXPool.Add(obj);
    }
}
