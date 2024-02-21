using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public bool IsPlayingSoundEffect => soundEffects.isPlaying;
    [SerializeField] private List<AudioSound> audios;
    [SerializeField] private AudioClip mainMenuClip;
    [SerializeField] private AudioClip gameplayClip;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource soundEffects;

    private bool isInit;

    private void Awake()
    {
        if (Instance==null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    public void Init()
    {
        if (isInit)
        {
            return;
        }

        isInit = true;
        PlayerData.OnUpdatedMusic += PlayBackgroundMusic;
        
        PlayBackgroundMusic();
    }

    private void OnDisable()
    {
        PlayerData.OnUpdatedMusic -= PlayBackgroundMusic;
    }

    public void PlayBackgroundMusic()
    {
        if (SceneManager.IsGameplayScene)
        {
            audioSource.clip = gameplayClip;
        }
        else
        {
            audioSource.clip = mainMenuClip;
        }
        
        if (DataManager.Instance.PlayerData.PlayMusic)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
    }

    public void PlaySoundEffect(string _key)
    {
        if (string.IsNullOrEmpty(_key))
        {
            return;
        }
        AudioSound _audio = audios.Find(_element => _element.Key == _key);

        if (_audio == null)
        {
            Debug.LogError("Cant find audio inside audios list for key: " + _key);
            return;
        }
        
        PlaySoundEffect(_audio.AudioClip,_audio.Volume);
    }

    public void PlaySoundEffect(AudioClip _clip, float _volume = 1)
    {
        if (soundEffects.isPlaying)
        {
            return;
        }
        if (_clip==null)
        {
            return;
        }
        if (!DataManager.Instance.PlayerData.PlaySoundEffect)
        {
            return;
        }

        soundEffects.clip = _clip;
        soundEffects.volume = _volume;
        soundEffects.Play();
    }

    public void StopBackgroundMusic()
    {
        audioSource.Stop();
    }

    public void PlayMainMenuMusic()
    {
        if (!DataManager.Instance.PlayerData.PlayMusic)
        {
            return;
        }
        audioSource.clip = mainMenuClip;
        audioSource.Play();
    }
}
