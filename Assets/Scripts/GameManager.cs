using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject PlayerBody;
    public List<MeshRenderer> CarComponents;
    public VehicleControl PlayerControls;
    public EnemySpawner Spawner;
    public GameObject BlackOutScreen;
    public Animator TimeAnimator;
    public TextMeshProUGUI TimerText;
    public TextMeshProUGUI Corruption;
    public List<Enemy> EnemyList;
    public PostProcessVolume PPVolume;
    public Gradient AtmosphereGradient;
    public ParticleSystem Atmosphere;
    ColorGrading ColorGrading;

    public float TimerStart = 60f;
    public readonly float SpawnAccelleration = 100f;
    public readonly float EnemySpawnRate = 1.5f;
    private readonly float CorruptionLimit = 50f;
    private bool GameOverBool;
    private bool Invunrability;

    private float MiniGunDamage;
    private float CannonDamage;
    private float RocketLauncherDamage;
    private float RamDamage;
    private float StrikeDamge;
    private float PunchDamage;

    [HideInInspector] public string GameOverString;
    GameStateBase CurrentState;
    public GameStateStart StartState = new();
    public GameStateSurvive SurviveState = new();
    public GameStateClense ClenseState = new();
    public GameStateLose LoseState = new();
    public GameStateWin WinState = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        StartCoroutine(Fadeout());
        PPVolume.profile.TryGetSettings(out ColorGrading);

        CurrentState = StartState;
        CurrentState.EnterState(this);
    }

    void Update()
    {
        StateManager();
        SetCorruption();
    }
    public void CheckWeaponDamage(float Damage, DamageType dmgType)
    {
        if(dmgType == DamageType.MiniGun)
        {
            MiniGunDamage += Damage;
        }
        if (dmgType == DamageType.Cannon)
        {
            CannonDamage += Damage;
        }
        if (dmgType == DamageType.Rocket)
        {
            RocketLauncherDamage += Damage;
        }
        if (dmgType == DamageType.Ram)
        {
            RamDamage += Damage;
        }
        if (dmgType == DamageType.EnemyPunch)
        {
            PunchDamage += Damage;
        }
        if (dmgType == DamageType.EnemyStrike)
        {
            StrikeDamge += Damage;
        }

    }
    public void CheckInvunrability()
    {
        if (PlayerControls.currentHealth <= PlayerControls.MaxHealth/2)
        {
            if(!Invunrability)
            {
                PlayerControls.Invunrable = true;
                Invunrability = true;
                StartCoroutine(CarInvunvrable());
            }
        }
    }
    public void GameOver(string WinOrLose)
    {
        if (!GameOverBool)
        {
            GameOverBool = true;
            StartCoroutine(FadeIn());
            TimerText.text = WinOrLose;
            TimeAnimator.SetBool("GameOver", true);
        }
    }
    private void SetCorruption()
    {
        float percentageCorruption = EnemyList.Count/CorruptionLimit;
        Corruption.text = EnemyList.Count + $"/{CorruptionLimit} Corruption";
        if(EnemyList.Count == CorruptionLimit)
        {
            GameOverString = "The Land Has Been Corrupted, You Lose";
            SwitchState(LoseState);
        }
        ColorGrading.colorFilter.value = AtmosphereGradient.Evaluate(percentageCorruption);
        var emission = Atmosphere.emission;
        emission.rateOverTime = 10f * EnemyList.Count;
    }
    void StateManager()
    {
        CurrentState.UpdateState(this);
    }
    public void SwitchState(GameStateBase state)
    {
        CurrentState = state;
        state.EnterState(this);
    }

    IEnumerator Fadeout()
    {
        for (float f = 1; f > -0.05f; f-=0.05f)
        {
            Color c = BlackOutScreen.GetComponent<Image>().color;
            c.a = f;
            BlackOutScreen.GetComponent<Image>().color = c;
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator FadeIn()
    {
        for (float f = 0; f < 1.05f; f += 0.05f)
        {
            Color c = BlackOutScreen.GetComponent<Image>().color;
            c.a = f;
            BlackOutScreen.GetComponent<Image>().color = c;
            yield return new WaitForSeconds(0.05f);
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    IEnumerator CarInvunvrable()
    {
        foreach (var item in PlayerControls.GetComponentInChildren<Ram>().EnemiesOnRam)
        {
            item.Damage(item.CurrentHealth, DamageType.Instant);
        }
        Camera.main.transform.GetComponent<VehicleCamera>().RestCar();
        Physics.IgnoreLayerCollision(3, 9,true);
        EnableMeshRenderer(false);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(true);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(false);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(true);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(false);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(true);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(false);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(true);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(false);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(true);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(false);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(true);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(false);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(true);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(false);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(true);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(false);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(true);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(false);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(true);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(false);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(true);
        PlayerControls.Invunrable = false;
        Physics.IgnoreLayerCollision(3, 9,false);
    }
    void EnableMeshRenderer(bool enable)
    {
        foreach (var item in CarComponents)
        {
            item.enabled = enable;
        }
    }
    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
}
