using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource BackgroundMusic;
    public List<AudioClip> GameSounds;
    public List<AudioSource> CarSounds;

    public AudioClip TryGetClip(int index)
    {
        if(GameSounds[index] == null)
        {
            return null;
        }
        return GameSounds[index];
    }
    public void EnableCarSounds(bool Enable)
    {
        foreach (var AudioSource in CarSounds)
        {
            AudioSource.enabled = Enable;
            if(Enable == true)
            {
                if (AudioSource.gameObject.name == "Cannon")
                    return;
                AudioSource.Play();
            }
            else
            {
                AudioSource.Pause();
            }
        }
    }
}
