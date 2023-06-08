using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController_Fishnet : MonoBehaviour
{
    public static SceneController_Fishnet Instance { get; private set; } = null;
    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private List<string> worldSceneNames = new();


    // Start is called before the first frame update
    public string GetWorldName()
    {
        string currSceneName = SceneManager.GetActiveScene().name;
        if (!worldSceneNames.Contains(currSceneName))
            Debug.LogError($"Current Scene ({currSceneName}) is not initialied in list of WorldNames in SceneManager!");
        return currSceneName;
    }
}
