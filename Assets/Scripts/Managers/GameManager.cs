using UnityEngine;
using UnityEngine.Scripting;

public class GameManager : MonoBehaviour
{
    public CurrentLevel currentLevel = CurrentLevel.Hotel;
    public MapGenerationScript mapGenerationScript;
    public GameObject playerInstance;
    private AudioManager audioManager;

    void Awake()
    {
        audioManager = GetComponentInChildren<AudioManager>();
    }

    void Start()
    {
        mapGenerationScript.GenerateMap(currentLevel);
        audioManager.StartDynamicMusic();
    }
    
    // Use audioManager.TransitionToBossMusic() and audioManager.TransitionToDynamicMusic()
}
