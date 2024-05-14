using UnityEngine;

public class SoundManager : MonoBehaviour
{
    protected AudioSource audioSourceMusic;

    void Reset()
    {
        int audioSourceCount = this.GetComponents<AudioSource>().Length;
        for (int i = 0; i < 3 - audioSourceCount; i++)
            gameObject.AddComponent<AudioSource>();

    }

    protected virtual void Awake()
    {
        Reset();
        AudioSource[] audioSources = GetComponents<AudioSource>();
        audioSourceMusic = audioSources[0];
    }
    protected virtual void Start()
    {
        audioSourceMusic.playOnAwake = false;
        audioSourceMusic.loop = true;
    }

    /// Plays game music using an audio clip - one music clip may be played at a time.
    public void PlayMusic(AudioClip musicClip, bool loop, float fadeDuration, float atTime)
    {
        if (audioSourceMusic == null || audioSourceMusic.clip == musicClip)
        {
            return;
        }

        if (Mathf.Approximately(fadeDuration, 0f))
        {
            audioSourceMusic.clip = musicClip;
            audioSourceMusic.loop = loop;
            audioSourceMusic.time = atTime;  // May be inaccurate if the audio source is compressed http://docs.unity3d.com/ScriptReference/AudioSource-time.html BK
            audioSourceMusic.Play();
        }
        else
        {
            float startVolume = audioSourceMusic.volume;

            LeanTween.value(gameObject, startVolume, 0f, fadeDuration)
                .setOnUpdate((v) =>
                {
                    // Fade out current music
                    audioSourceMusic.volume = v;
                }).setOnComplete(() =>
                {
                    // Play new music
                    audioSourceMusic.volume = startVolume;
                    audioSourceMusic.clip = musicClip;
                    audioSourceMusic.loop = loop;
                    audioSourceMusic.time = atTime;  // May be inaccurate if the audio source is compressed http://docs.unity3d.com/ScriptReference/AudioSource-time.html BK
                    audioSourceMusic.Play();
                });
        }
    }

    public virtual void SetAudioPitch(float pitch, float duration, System.Action onComplete)
    {
        if (Mathf.Approximately(duration, 0f))
        {
            audioSourceMusic.pitch = pitch;
            if (onComplete != null)
            {
                onComplete();
            }
            return;
        }

        LeanTween.value(gameObject,
            audioSourceMusic.pitch,
            pitch,
            duration).setOnUpdate((p) =>
            {
                audioSourceMusic.pitch = p;
            }).setOnComplete(() =>
            {
                if (onComplete != null)
                {
                    onComplete();
                }
            });
    }
    public virtual void SetAudioVolume(float volume, float duration, System.Action onComplete)
    {
        if (Mathf.Approximately(duration, 0f))
        {
            if (onComplete != null)
            {
                onComplete();
            }
            audioSourceMusic.volume = volume;
            return;
        }

        LeanTween.value(gameObject,
            audioSourceMusic.volume,
            volume,
            duration).setOnUpdate((v) =>
            {
                audioSourceMusic.volume = v;
            }).setOnComplete(() =>
            {
                if (onComplete != null)
                {
                    onComplete();
                }
            });
    }

    public virtual void StopMusic()
    {
        audioSourceMusic.Stop();
        audioSourceMusic.clip = null;
    }

    public virtual float GetVolume()
    {
        return audioSourceMusic.volume;
    }
}
