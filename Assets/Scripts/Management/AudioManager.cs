using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioSource testAudioSource1;
    public AudioSource testAudioSource2;
    public AudioResource audioResourceA;
    public AudioResource audioResourceB;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            testAudioSource1.resource = audioResourceA;
            testAudioSource1.Play();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            testAudioSource2.resource = audioResourceB;
            testAudioSource2.Play();
        }
    }
}
