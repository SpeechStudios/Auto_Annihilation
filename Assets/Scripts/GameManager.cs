using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public float TimerStart = 60f;
    public SoundManager SoundManager;
    public GameObject Car;
    public GameObject InitialEnemies;
    public GameObject PauseMenu, GameMenu;


    public List<MeshRenderer> CarComponents;

    public Image BlackOutScreen;

    public EnemySpawner Spawner;
    public Animator TimeAnimator;
    public TextMeshProUGUI TimerText;
    public TextMeshProUGUI Corruption;
    public Volume PPVolume;
    public Gradient AtmosphereGradient;
    public ParticleSystem Atmosphere;
    public List<Enemy> EnemyList;
    public Loading loading;



    public readonly float SpawnAccelleration = 100f;
    public readonly float EnemySpawnRate = 1.5f;
    private readonly float CorruptionLimit = 50f;
    private bool GameOverBool;
    private bool Invunrability;



    private VehicleControl VehicleControls;
    private VehicleCamera VehicleCamera;
    private Shooting Shooting;


    private ColorAdjustments ColorGrading;
    [HideInInspector] public string GameOverString;
    GameStateBase CurrentState;
    public GameStateLoading LoadingState = new();
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
        PPVolume.profile.TryGet(out ColorGrading);
        VehicleControls = Car.GetComponent<VehicleControl>();
        VehicleCamera = Car.GetComponentInChildren<VehicleCamera>();
        Shooting = Car.GetComponent<Shooting>();
        CurrentState =  LoadingState;
        CurrentState.EnterState();
        AudioListener.volume = 0.8f;
    }

    void Update()
    {
        StateManager();
    }

    public void CheckInvunrability()
    {
        if (VehicleControls.currentHealth <= VehicleControls.MaxHealth/2)
        {
            if(!Invunrability)
            {
                VehicleControls.Invunrable = true;
                Invunrability = true;
                StartCoroutine(CarInvunvrable());
            }
        }
    }
    public void GameOver()
    {
        if (!GameOverBool)
        {
            SoundManager.EnableCarSounds(false);
            GameOverBool = true;
            StartCoroutine(FadeIn());
            TimerText.text = GameOverString;
            TimeAnimator.SetBool("GameOver", true);
        }
    }
    public void SetCorruption()
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
        SoundManager.BackgroundMusic.volume = percentageCorruption;
    }
    void StateManager()
    {
        CurrentState.UpdateState();
    }
    public void SwitchState(GameStateBase state)
    {
        CurrentState = state;
        state.EnterState();
    }



    private void EnableMeshRenderer(bool enable)
    {
        foreach (var item in CarComponents)
        {
            item.enabled = enable;
        }
    }
    public void EnableStartComponents(bool enable)
    {
        Car.GetComponent<Rigidbody>().isKinematic = !enable;
        Car.GetComponent<Rigidbody>().useGravity = enable;
        Cursor.visible = !enable;
        VehicleControls.enabled = enable;
        SoundManager.EnableCarSounds(enable);
        Shooting.enabled = enable;
        Spawner.enabled = enable;
    }

    public void StartGame()
    {
        StartCoroutine("StartTheGame");
    }
    public void Pause_UnPause()
    {
        if(!PauseMenu.activeInHierarchy)
        {
            PauseMenu.SetActive(true);
            SoundManager.BackgroundMusic.Pause();
            SoundManager.EnableCarSounds(false);
            Shooting.enabled = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0;
        }
        else
        {
            PauseMenu.SetActive(false);
            Time.timeScale = 1;
            SoundManager.BackgroundMusic.Play();
            SoundManager.EnableCarSounds(true);
            Shooting.enabled = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
    public void Quit()
    {
        Application.Quit();
    }

    IEnumerator StartTheGame()
    {
        loading.enabled = false;
        StartCoroutine(FadeIn());
        yield return new WaitForSeconds(2f);
        loading.LoadingMenu.SetActive(false);
        GameMenu.SetActive(true);
        SwitchState(SurviveState);
        StartCoroutine(Fadeout());
        Car.GetComponent<Animation>().Play();
        InitialEnemies.SetActive(true);
        SoundManager.BackgroundMusic.enabled = true;
        TimeAnimator.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        EnableStartComponents(true);
        Shooting.TargeterPrefab.SetActive(true);

    }
    IEnumerator FadeIn()
    {
        for (float f = 0; f < 1.05f; f += 0.01f)
        {
            Color c = BlackOutScreen.color;
            c.a = f;
            BlackOutScreen.color = c;
            yield return new WaitForSeconds(0.01f);
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    IEnumerator Fadeout()
    {
        for (float f = 1; f > -0.05f; f -= 0.01f)
        {
            Color c = BlackOutScreen.color;
            c.a = f;
            BlackOutScreen.color = c;
            yield return new WaitForSeconds(0.01f);
        }
    }
    IEnumerator CarInvunvrable()
    {
        Physics.IgnoreLayerCollision(3, 9, true);
        foreach (var item in VehicleControls.GetComponentInChildren<Ram>().EnemiesOnRam)
        {
            item.Damage(item.CurrentHealth, DamageType.Instant);
        }
        Camera.main.transform.GetComponent<VehicleCamera>().ResetCar();
        EnableMeshRenderer(false);
        VehicleControls.carParticles.InvunrableVFX.Play();
        for (int i = 0; i < 9; i++)
        {
            yield return new WaitForSeconds(0.25f);
            EnableMeshRenderer(true);
            yield return new WaitForSeconds(0.25f);
            EnableMeshRenderer(false);
        }
        VehicleControls.carParticles.InvunrableVFX.Stop();
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(true);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(false);
        yield return new WaitForSeconds(0.25f);
        EnableMeshRenderer(true);
        VehicleControls.Invunrable = false;
        Physics.IgnoreLayerCollision(3, 9, false);
    }
}
