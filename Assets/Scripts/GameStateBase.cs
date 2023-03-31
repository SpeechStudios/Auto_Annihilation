using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameStateBase
{
    public abstract void EnterState();
    public abstract void UpdateState();
    public void PauseInteraction()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.Pause_UnPause();
        }
    }
    public void EndGameState()
    {
        GameManager.Instance.Car.GetComponent<VehicleControl>().enabled = false;
        GameManager.Instance.Car.GetComponent<Shooting>().enabled = false;
        GameManager.Instance.Car.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }
}
public class GameStateLoading : GameStateBase
{
    public override void EnterState()
    {
        Cursor.lockState = CursorLockMode.None;
        GameManager.Instance.EnableStartComponents(false);
        GameManager.Instance.loading.LoadGame();
    }

    public override void UpdateState()
    {
        if (GameManager.Instance.loading.enabled)
        {
            if (Input.anyKey)
            {
                GameManager.Instance.StartGame();
            }
        }
    }

}
public class GameStateSurvive : GameStateBase
{
    float Timer;
    public override void EnterState()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        GameManager.Instance.Spawner.SpawnTime = GameManager.Instance.EnemySpawnRate;
        Timer = GameManager.Instance.TimerStart;
    }

    public override void UpdateState()
    {
        GameManager.Instance.CheckInvunrability();
        GameManager.Instance.Spawner.SpawnTime -= (Time.deltaTime / (GameManager.Instance.SpawnAccelleration - GameManager.Instance.EnemyList.Count));
        Timer -= Time.deltaTime;
        float minutes = Mathf.FloorToInt(Timer / 60);
        float seconds = Mathf.FloorToInt(Timer % 60);
        GameManager.Instance.TimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds) + " SURVIVE";
        if (Timer < 0)
        {
            GameManager.Instance.SwitchState(GameManager.Instance.ClenseState);
        }
        PauseInteraction();
        GameManager.Instance.SetCorruption();
    }
}
public class GameStateClense : GameStateBase
{
    public override void EnterState()
    {
        GameManager.Instance.TimerText.gameObject.transform.position = Vector3.zero;
        GameManager.Instance.TimeAnimator.SetTrigger("ClenseCorruption");
        GameManager.Instance.TimerText.text = "<size=80%>Clense Corruption";
        GameManager.Instance.Spawner.enabled = false;
    }

    public override void UpdateState()
    {
        GameManager.Instance.CheckInvunrability();
        if (GameManager.Instance.EnemyList.Count == 0)
        {
            GameManager.Instance.SwitchState(GameManager.Instance.WinState);
        }
        PauseInteraction();
        GameManager.Instance.SetCorruption();
    }
}
public class GameStateLose : GameStateBase
{
    public override void EnterState()
    {
        GameManager.Instance.GameOver();
        EndGameState();
    }

    public override void UpdateState()
    {
        AudioListener.volume = Mathf.MoveTowards(AudioListener.volume, 0.2f, Time.deltaTime / 6);
    }
}
public class GameStateWin : GameStateBase
{
    public override void EnterState()
    {
        GameManager.Instance.GameOverString = "You Won!";
        GameManager.Instance.GameOver();
        EndGameState();
    }

    public override void UpdateState()
    {
        AudioListener.volume = Mathf.MoveTowards(AudioListener.volume, 0.2f, Time.deltaTime / 6);
    }
}
