using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    private static SoundManager instance = null;
    public static SoundManager Instance
    {
        get { return instance; }
    }

    public static SoundManager SM;

    public AudioClip selectSound;
    public AudioClip fillSound;
    public AudioClip rotateSound;
    public AudioClip rollbackSound;
    public AudioClip stageClearSound;
    public AudioSource audioSrc;

    void Awake()
    {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
            return;
        }
        else {
            instance = this;
            SM = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void playSelectSound()
    {
        audioSrc.volume = 1f;
        audioSrc.clip = selectSound;
        audioSrc.Play();
    }

    public void playFillSound()
    {
        audioSrc.volume = 1f;
        audioSrc.clip = fillSound;
        audioSrc.Play();
    }

    public void playRotateSound()
    {
        audioSrc.volume = 0.4f;
        audioSrc.clip = rotateSound;
        audioSrc.Play();
    }

    public void playRollbackSound()
    {
        audioSrc.volume = 1f;
        audioSrc.clip = rollbackSound;
        audioSrc.Play();
    }

    public void playStageClearSound()
    {
        audioSrc.volume = 1f;
        audioSrc.clip = stageClearSound;
        audioSrc.Play();
    }
}
