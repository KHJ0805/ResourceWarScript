using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class BaseUi : MonoBehaviour
{
    [Header("Common Settings")]
    private AudioSource buttonClickSound; // AudioSource
    private string buttonClickSoundPath = "Sound/BtnClickSound"; // AudioClip 경로
    private string audioMixerPath = "Sound/AudioMixer"; // AudioMixer 경로
    private AudioMixer mainAudioMixer;

    protected virtual void Start()
    {
        InitializeAudioSource();
    }

    private void InitializeAudioSource()
    {
        // AudioSource 동적으로 추가
        if (buttonClickSound == null)
        {
            buttonClickSound = gameObject.AddComponent<AudioSource>();
        }

        // AudioClip 로드
        AudioClip clip = Resources.Load<AudioClip>(buttonClickSoundPath);
        if (clip != null)
        {
            buttonClickSound.clip = clip;
            buttonClickSound.playOnAwake = false; // 자동 재생 방지
        }
        else
        {
            Debug.LogError($"Failed to load sound from path: {buttonClickSoundPath}");
        }

        // AudioMixer 로드
        mainAudioMixer = Resources.Load<AudioMixer>(audioMixerPath);
        if (mainAudioMixer != null)
        {
            // SFX 그룹 찾기
            AudioMixerGroup[] groups = mainAudioMixer.FindMatchingGroups("SFX");
            if (groups.Length > 0)
            {
                buttonClickSound.outputAudioMixerGroup = groups[0];
                Debug.Log("SFX group assigned to AudioSource.");
            }
            else
            {
                Debug.LogError("SFX group not found in AudioMixer.");
            }
        }
        else
        {
            Debug.LogError($"Failed to load Audio Mixer from path: {audioMixerPath}");
        }
    }

    protected void PlayButtonClickSound()
    {
        if (buttonClickSound != null && buttonClickSound.clip != null)
        {
            buttonClickSound.Play();
        }
        else
        {
            Debug.LogWarning("Button click sound is not set or AudioClip is missing.");
        }
    }

    protected void AddButtonListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                PlayButtonClickSound();
                action.Invoke();
            });
        }
    }
}
