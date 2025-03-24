using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private WeaponsManager weaponsManager;
    public AudioSource baseAudioSource;
    public AudioSource auxiliaryAudioSource;
    public AudioClip baseAudioClip;
    public AudioSource bossAudioSource;
    public List<AudioClip> menuThemeClips;
    public AudioMixerGroup musicAudioMixerGroup;

    // Lists containing module clips for each track
    public List<AudioClip> barrelSlotClips; 
    public List<AudioClip> sightClips; 
    public List<AudioClip> underbarrelClips; 
    public List<AudioClip> magazineClips; 
    public List<AudioClip> miscClips;
    private List<AudioClip>[] audioClipsBank = new List<AudioClip>[5];
    private AudioSource[] musicAudioSources = new AudioSource[10];

    private int[] audioSourcesToTrack = new int[10];
    private List<int> audioSourcesIndexesToStop = new List<int>();
    private double nextLoopStart;
    private bool toBossMusic = false;

    //Debug
    [Space(20)]
    public bool testDynamicMusic = false;
    public bool testMenuTheme = false;

    void Awake()
    {
        FetchManagers();
    }

    void Start()
    {
        if (testDynamicMusic) StartDynamicMusic();
        if (testMenuTheme) StartMenuThemeIntro();
    }

    public void TransitionToBossMusic()
    {
        bossAudioSource.Play();
        StartCoroutine(FadeAudioSource(bossAudioSource, 0.75f, 4f));
        StartCoroutine(FadeAudioSource(baseAudioSource, 0f, 4f));

        for (int i = 0; i < 10; i++)
        {
            StartCoroutine(FadeAudioSource(musicAudioSources[i], 0f, 4f));
        }
    }

    public void TransitionToDynamicMusic()
    {
        StartCoroutine(FadeAudioSource(bossAudioSource, 0f, 4f));
        StartCoroutine(FadeAudioSource(baseAudioSource, 1f, 4f));

        for (int i = 0; i < 10; i++)
        {
            StartCoroutine(FadeAudioSource(musicAudioSources[i], 1f, 4f));
        }
    }

    public float fadeSpeed = 1.5f;

    IEnumerator FadeAudioSource(AudioSource audioSource, float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float time = 0f;
        while (time < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = targetVolume;
    }

    public void StartMenuThemeIntro()
    {
        baseAudioSource.clip = menuThemeClips[0];
        baseAudioSource.loop = false;

        double startTime = AudioSettings.dspTime + 1;
        baseAudioSource.PlayScheduled(startTime);
        auxiliaryAudioSource.PlayScheduled(startTime + menuThemeClips[0].samples / (double)menuThemeClips[0].frequency);
    }

    public void StopMenuTheme()
    {
        baseAudioSource.Stop();
        auxiliaryAudioSource.Stop();
        baseAudioSource.loop = true;
    }

    public void StartDynamicMusic()
    {
        audioClipsBank[0] = barrelSlotClips;
        audioClipsBank[1] = sightClips;
        audioClipsBank[2] = underbarrelClips;
        audioClipsBank[3] = magazineClips;
        audioClipsBank[4] = miscClips;

        for (int i = 0; i < 10; i++)
        {
            AudioSource tempAudioSource = gameObject.AddComponent<AudioSource>();
            tempAudioSource.playOnAwake = false;
            tempAudioSource.loop = true;
            tempAudioSource.outputAudioMixerGroup = musicAudioMixerGroup;

            musicAudioSources[i] = tempAudioSource;
            audioSourcesToTrack[i] = 10;
        }
        
        baseAudioSource.clip = baseAudioClip;
        double startTime = AudioSettings.dspTime +2; // 2 is for debug purposes
        baseAudioSource.PlayScheduled(startTime);       
        nextLoopStart = startTime +baseAudioClip.samples / (double)baseAudioClip.frequency;
        
        StartCoroutine(InvokeAtDSPTime(RefreshActiveAudioSources));

        //Debug
        if (testDynamicMusic) Invoke(nameof(DebugRefreshAttachments), 0.5f);
    }

    public void StopDynamicMusic()
    {
        for (int i = 0; i < 10; i++)
        {
            musicAudioSources[i].Stop();
        }

        StopAllCoroutines();
        baseAudioSource.Stop();
        audioSourcesIndexesToStop.Clear();
    }

    IEnumerator InvokeAtDSPTime(Action method)
    {
        while (true) // Keep invoking at regular intervals
        {
            yield return new WaitUntil(() => AudioSettings.dspTime >= nextLoopStart);

            method?.Invoke(); // Call the method
        }
    }
    
    void RefreshActiveAudioSources()
    {
        while (audioSourcesIndexesToStop.Count > 0)
        {
            int currentIndex = audioSourcesIndexesToStop[0];

            musicAudioSources[currentIndex].clip = null;
            musicAudioSources[currentIndex].Stop();

            audioSourcesToTrack[currentIndex] = 10;
            audioSourcesIndexesToStop.RemoveAt(0);

            Debug.Log("AS " + currentIndex + " stopped.");
        }

        nextLoopStart = AudioSettings.dspTime +baseAudioClip.samples / (double)baseAudioClip.frequency;
    }

    public void UpdateDynamicMusic()
    {
        for (int i = 0; i < 5; i++)
        {
            if (weaponsManager.currentAttachments[i] != null)
            {
                int indexOccurrencies = audioSourcesToTrack.Count(n => n == i);
                
                // Check if the track is passing from null to some clip
                if (indexOccurrencies == 0)
                {
                    SchedulePlayAtNewAudioSource(i);
                }
                // Check if the track is passing from a clip to different clip
                else if (indexOccurrencies == 1)
                {
                    int currentSourceIndex = Array.IndexOf(audioSourcesToTrack, i);

                    if (musicAudioSources[currentSourceIndex].clip != audioClipsBank[i][weaponsManager.currentAttachments[i].Index])
                    {
                        SchedulePlayAtNewAudioSource(i);
                        ScheduleStopAudioSource(currentSourceIndex, i);
                    }
                }
            }
            else
            {
                if (Array.Exists(audioSourcesToTrack, element => element == i))
                {
                    int currentIndex = Array.IndexOf(audioSourcesToTrack, i);

                    ScheduleStopAudioSource(currentIndex, i);
                }
            }
        }
    }

    private void SchedulePlayAtNewAudioSource(int index)
    {
        int currentIndex = FindAvailableAudioSource();
        audioSourcesToTrack[currentIndex] = index;

        musicAudioSources[currentIndex].clip = audioClipsBank[index][weaponsManager.currentAttachments[index].Index];
        musicAudioSources[currentIndex].PlayScheduled(nextLoopStart);
        Debug.Log("Scheduled PLAY: track " + index + " at AS " + currentIndex);
    }

    private void ScheduleStopAudioSource(int index, int trackIndex) //trackindex only has debug purposes
    {
        if (!audioSourcesIndexesToStop.Contains(index))
        {
            audioSourcesIndexesToStop.Add(index);
            Debug.Log("Scheduled STOP: track " + trackIndex + " at AS " + index);
        }
    }

    private int FindAvailableAudioSource()
    {
        //Inefficient loop, I have to edit it soon
        for (int i = 0; i < 10; i++)
        {
            if (audioSourcesToTrack[i] == 10) return i;
        }

        Debug.Log("Error: Available AudioSource not found !");
        return 0;
    }

    private void DebugRefreshAttachments()
    {
        weaponsManager.WeaponCheck();
        Invoke(nameof(DebugRefreshAttachments), 0.5f);
    }

    void OnSceneLoaded()
    {
        FetchManagers();
    }

    public void FetchManagers()
    {
        weaponsManager = GameObject.FindWithTag("PlayerWeapon").GetComponent<WeaponsManager>();
    }
}
