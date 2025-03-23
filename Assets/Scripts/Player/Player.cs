using UnityEngine;

public class Player : MonoBehaviour
{
    private MainMenu mainMenu;

    void Start()
    {
        mainMenu = FindObjectOfType<MainMenu>();
        if (mainMenu == null)
        {
            Debug.LogError("MainMenu script not found in the scene");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("M key pressed by player");
            if (mainMenu != null)
            {
                //mainMenu.LoadMainMenuScene();
            }
            else
            {
                Debug.LogError("MainMenu reference is null");
            }
        }
    }
}