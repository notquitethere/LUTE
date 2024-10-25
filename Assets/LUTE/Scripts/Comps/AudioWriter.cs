using System.Collections.Generic;
using UnityEngine;

public class AudioWriter : MonoBehaviour, IWriterListener
{
    // When true, a beep will be played on every written character glyph
    [SerializeField] protected bool playBeeps;
    [SerializeField] protected List<AudioClip> beepSounds = new List<AudioClip>();
    [SerializeField] protected AudioSource targetAudioSource;
    [Range(0, 1)]
    [SerializeField] protected float volume = 1f;

    protected float nextBeepTime;
    protected float targetVolume = 0f;

    protected virtual void Awake()
    {
        // Need to do this in Awake rather than Start due to init order issues

        targetAudioSource = LogaManager.Instance.SoundManager.GetSFXSource();
    }

    public virtual void OnGlyph()
    {
        if (playBeeps && beepSounds.Count > 0)
        {
            if (nextBeepTime < Time.realtimeSinceStartup)
            {
                targetAudioSource.clip = beepSounds[Random.Range(0, beepSounds.Count)];
                LogaManager.Instance.SoundManager.PlaySound(beepSounds[Random.Range(0, beepSounds.Count)], -1);
                float extend = targetAudioSource.clip.length;
                nextBeepTime = Time.realtimeSinceStartup + extend;
            }
        }
    }
}
