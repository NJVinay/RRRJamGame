using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public WeaponsManager weaponsManager;
    public AudioSource baseAudioSource;
    public AudioClip baseAudioClip;
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

    void Start()
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

        // Debug
        Invoke(nameof(StartDynamicMusic), 1); //1 is only for debug purposes
        //Invoke(nameof(DebugRefreshAttachments), 0.5f);
    }

    void StartDynamicMusic()
    {
        baseAudioSource.clip = baseAudioClip;
        baseAudioSource.PlayScheduled(AudioSettings.dspTime);        
        nextLoopStart = AudioSettings.dspTime +baseAudioClip.length;
        
        StartCoroutine(InvokeAtDSPTime(RefreshActiveAudioSources));
    }

    IEnumerator InvokeAtDSPTime(Action method)
    {
        while (true) // Keep invoking at regular intervals
        {
            yield return new WaitUntil(() => AudioSettings.dspTime >= nextLoopStart);

            method?.Invoke(); // Call the method
            nextLoopStart += baseAudioClip.length; // Schedule next execution
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
}
