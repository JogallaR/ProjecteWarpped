
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

public class ButtonSound : MonoBehaviour,
    IPointerEnterHandler,
    IPointerClickHandler
{
    [Header("Sons")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    [Header("Configuració")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup sfxGroup;

    private static AudioSource sharedAudioSource;

    private void Awake()
    {
        if (sharedAudioSource == null)
        {
            GameObject audioObject = new GameObject("ButtonAudioSource");
            sharedAudioSource = audioObject.AddComponent<AudioSource>();

            sharedAudioSource.playOnAwake = false;
            sharedAudioSource.loop = false;
            sharedAudioSource.spatialBlend = 0f;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlaySound(hoverSound);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            PlaySound(clickSound);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || sfxGroup == null)
            return;

        if (sharedAudioSource == null)
            return;

        sharedAudioSource.outputAudioMixerGroup = sfxGroup;
        sharedAudioSource.PlayOneShot(clip, volume);
    }
}
