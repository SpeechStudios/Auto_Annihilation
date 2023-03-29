using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public GameObject LoadingMenu;
    public GameObject PressAnyKey;
    public Slider LoadingBar;
    public Image ScreenshotHolder;
    public List<Sprite> Screenshots;
    private float SS_Timer;
    private float ScreenshotTransitionTime = 5f;
    private void Awake()
    {
        ScreenshotHolder.sprite = SelectNewScreenShot();
    }
    private void Update()
    {
        if(!LoadingMenu.activeInHierarchy)
        {
            return;
        }
        ScreenshotTransition();
    }
    public void LoadGame()
    {
        StartCoroutine("LoadScene");
    }
    private void ScreenshotTransition()
    {
        SS_Timer += Time.deltaTime;
        if(SS_Timer>ScreenshotTransitionTime)
        {
            StartCoroutine(TransitionScreenShot());
            SS_Timer = 0;
        }
    }
    IEnumerator LoadScene()
    {
        float RandomFloat = Random.Range(0, 0.3f);
        for (float f = 0; f < RandomFloat; f += 0.01f)
        {
            LoadingBar.value += 0.01f;
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(Random.Range(0.5f, 2f));
        float RandomFloat2 = Random.Range(0.3f, 0.7f);
        for (float i = LoadingBar.value; i < RandomFloat2; i += 0.01f)
        {
            LoadingBar.value = i;
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(Random.Range(0.5f, 3f));
        for (float i = LoadingBar.value; i < 1; i+=0.01f)
        {
            LoadingBar.value = i;
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(Random.Range(0.5f, 1f));
        yield return null;
        PressAnyKey.SetActive(true);
    }
    IEnumerator TransitionScreenShot()
    {
        for (float f = 1; f > -0.05f; f-= 0.01f)
        {
            Color c = ScreenshotHolder.color;
            c.a = f;
            ScreenshotHolder.color = c;
            yield return new WaitForSeconds(0.01f);
        }
        ScreenshotHolder.sprite = SelectNewScreenShot();
        for (float f = 0; f < 1.05f; f += 0.01f)
        {
            Color c = ScreenshotHolder.color;
            c.a = f;
            ScreenshotHolder.color = c;
            yield return new WaitForSeconds(0.01f);
        }
    }
    private Sprite SelectNewScreenShot()
    {
        int randomScreenShot = Random.Range(0, Screenshots.Count);
        if (ScreenshotHolder.sprite == Screenshots[randomScreenShot])
        {
           return SelectNewScreenShot();         
        }
        return Screenshots[randomScreenShot];
    }

}
