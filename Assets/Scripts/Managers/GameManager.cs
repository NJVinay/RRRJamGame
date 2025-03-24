using UnityEngine;
using UnityEngine.Scripting;

public class GameManager : MonoBehaviour
{
    public CurrentLevel currentLevel = CurrentLevel.Hotel;
    public MapGenerationScript mapGenerationScript;
    public GameObject playerInstance;

    void Start()
    {
        mapGenerationScript.GenerateMap(currentLevel);
    }
}
