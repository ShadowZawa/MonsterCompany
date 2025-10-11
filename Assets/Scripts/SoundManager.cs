using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Background Music")]
    public AudioSource musicSource;
    public AudioClip defaultMusic;
    public bool playOnAwake = true;
    public bool persistAcrossScenes = true;

    [Header("One-shot Settings")]
    public AudioSource sfxSourcePrefab; // 用來在指定位置播放短音效的 prefab（需包含 AudioSource）
    private float globalSFXVolume = 1f; // 全局音效音量

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (persistAcrossScenes)
                DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 如果沒有指定 musicSource，建立一個
        if (musicSource == null)
        {
            GameObject go = new GameObject("MusicSource");
            go.transform.SetParent(transform);
            musicSource = go.AddComponent<AudioSource>();
            musicSource.loop = true;
            
        }
    }

    void Start()
    {
        if (playOnAwake && defaultMusic != null)
        {
            PlayMusic(defaultMusic);
        }
    }

    #region Background Music
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.volume = Mathf.Clamp01(volume);
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Stop();
    }

    public void SetMusicVolume(float v)
    {
        musicSource.volume = Mathf.Clamp01(v);
    }

    public void SetSFXVolume(float v)
    {
        globalSFXVolume = Mathf.Clamp01(v);
    }

    public float GetSFXVolume()
    {
        return globalSFXVolume;
    }
    #endregion

    #region Positional SFX
    // 在指定世界座標播放一次性音效
    public AudioSource PlayOneShotAt(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return null;

        AudioSource src = null;
        if (sfxSourcePrefab != null)
        {
            GameObject go = Instantiate(sfxSourcePrefab.gameObject, position, Quaternion.identity);
            src = go.GetComponent<AudioSource>();
        }
        else
        {
            GameObject go = new GameObject("SFX");
            go.transform.position = position;
            src = go.AddComponent<AudioSource>();
        }

        src.clip = clip;
        src.spatialBlend = 1f; // 3D 聲音
        src.volume = Mathf.Clamp01(volume * globalSFXVolume); // 應用全局音效音量
        src.Play();
        Destroy(src.gameObject, clip.length + 0.1f);
        return src;
    }

    // 以指定父物件為位置（例如武器、人物）播放短音效
    public AudioSource PlayOneShotOn(AudioClip clip, Transform parent, float volume = 1f)
    {
        if (clip == null || parent == null) return null;
        AudioSource src = null;
        if (sfxSourcePrefab != null)
        {
            GameObject go = Instantiate(sfxSourcePrefab.gameObject, parent.position, Quaternion.identity, parent);
            src = go.GetComponent<AudioSource>();
        }
        else
        {
            GameObject go = new GameObject("SFX");
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            src = go.AddComponent<AudioSource>();
        }
        src.clip = clip;
        src.spatialBlend = 1f;
        src.volume = Mathf.Clamp01(volume * globalSFXVolume); // 應用全局音效音量
        src.Play();
        Destroy(src.gameObject, clip.length + 0.1f);
        return src;
    }
    #endregion
}
