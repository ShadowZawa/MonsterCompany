using UnityEngine;
using UnityEngine.UI;

public class SettingUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingPanel;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Settings")]
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    private void Start()
    {
        LoadSettings();
        InitializeUI();
    }

    private void LoadSettings()
    {
        // 從 PlayerPrefs 載入設定，如果不存在則使用預設值 1.0
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        // 應用設定到 SoundManager
        ApplySettings();
    }

    private void InitializeUI()
    {
        // 設置 Slider 的初始值
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    private void ApplySettings()
    {
        // 應用音樂音量
        if (SoundManager.instance != null)
        {
            SoundManager.instance.SetMusicVolume(musicVolume);
            SoundManager.instance.SetSFXVolume(sfxVolume);
        }
    }

    public void OnMusicVolumeChanged(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        
        // 即時更新音樂音量
        if (SoundManager.instance != null)
        {
            SoundManager.instance.SetMusicVolume(musicVolume);
        }

        // 保存設定
        SaveSettings();
    }

    public void OnSFXVolumeChanged(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        
        // 即時更新 SFX 音量
        if (SoundManager.instance != null)
        {
            SoundManager.instance.SetSFXVolume(sfxVolume);
        }
        
        // 保存設定
        SaveSettings();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save(); // 確保立即寫入（特別是在移動平台）
    }

    public void ToggleSettingPanel()
    {
        if (settingPanel != null)
        {
            bool isActive = settingPanel.activeSelf;
            settingPanel.SetActive(!isActive);
        }
    }

    public void ResetToDefault()
    {
        musicVolume = 1f;
        sfxVolume = 1f;

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = musicVolume;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = sfxVolume;

        ApplySettings();
        SaveSettings();
    }

    // 公開方法讓其他腳本獲取當前音量設定
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;

    private void OnDestroy()
    {
        // 移除監聽器避免記憶體洩漏
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }
}