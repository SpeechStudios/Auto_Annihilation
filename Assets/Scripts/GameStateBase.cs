using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameStateBase
{
    public abstract void EnterState(GameManager gm);
    public abstract void UpdateState(GameManager gm);
}
public class GameStateStart : GameStateBase
{
    public override void EnterState(GameManager gm)
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
        gm.Spawner.SpawnTime = gm.EnemySpawnRate;
        gm.SwitchState(gm.SurviveState);
    }

    public override void UpdateState(GameManager gm)
    {

    }
}
public class GameStateSurvive : GameStateBase
{
    float Timer;
    public override void EnterState(GameManager gm)
    {
        Timer = gm.TimerStart;
    }

    public override void UpdateState(GameManager gm)
    {
        gm.CheckInvunrability();
        gm.Spawner.SpawnTime -= (Time.deltaTime / (gm.SpawnAccelleration - gm.EnemyList.Count));
        Timer -= Time.deltaTime;
        float minutes = Mathf.FloorToInt(Timer / 60);
        float seconds = Mathf.FloorToInt(Timer % 60);
        gm.TimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds) + " SURVIVE";
        if (Timer < 0)
        {
            gm.SwitchState(gm.ClenseState);
        }
    }
}
public class GameStateClense : GameStateBase
{
    public override void EnterState(GameManager gm)
    {
        gm.TimerText.gameObject.transform.position = Vector3.zero;
        gm.TimeAnimator.SetTrigger("ClenseCorruption");
        gm.TimerText.text = "<size=80%>Clense Corruption";
        gm.Spawner.enabled = false;
    }

    public override void UpdateState(GameManager gm)
    {
        gm.CheckInvunrability();
        if (gm.EnemyList.Count == 0)
        {
            gm.SwitchState(gm.WinState);
        }
    }
}
public class GameStateLose : GameStateBase
{
    public override void EnterState(GameManager gm)
    {
        gm.GameOver(gm.GameOverString);
    }

    public override void UpdateState(GameManager gm)
    {

    }
}
public class GameStateWin : GameStateBase
{
    public override void EnterState(GameManager gm)
    {
        gm.GameOverString = "You Won!";
        gm.GameOver(gm.GameOverString);
    }

    public override void UpdateState(GameManager gm)
    {

    }
}