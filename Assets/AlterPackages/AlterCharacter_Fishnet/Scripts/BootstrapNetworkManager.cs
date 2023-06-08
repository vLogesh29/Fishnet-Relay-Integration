using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class BootstrapNetworkManager : NetworkBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private List<SceneAsset> constDontDestroyOnLoadScenes = new();
#endif
    [SerializeField] private List<string> constDontDestroyOnLoadSceneNames;

    public static BootstrapNetworkManager Instance { get; private set; } = null;
    public event Action<NetworkConnection, bool> OnClientLoadedScenes;
    
    private string changedScene;

    private void Awake()
    { 
        Instance = this;

#if UNITY_EDITOR
        constDontDestroyOnLoadSceneNames = constDontDestroyOnLoadScenes.ConvertAll<string>(x => x.name);
#endif
    }
    private void Start()
    {
        SceneManager.OnLoadEnd += SceneManager_OnLoadEnd;
        //SceneManager.OnActiveSceneSet += (asServer) => EventOnClientLoadedScenes(asServer);
    }

    private void EventOnClientLoadedScenes(bool asServer)
    {
        UnitySceneManager.SetActiveScene(UnitySceneManager.GetSceneByName(changedScene));

        Debug.Log($"--> client subscription check isNull:{OnClientLoadedScenes == null}, " +
                    $"List:{OnClientLoadedScenes?.GetInvocationList()}, " +
                    $"Count:{OnClientLoadedScenes?.GetInvocationList()?.Length}");

        OnClientLoadedScenes?.Invoke(ClientManager.Connection, asServer);
        // asServer:false -> as it supposed to spawn player for a client only };
    }

    private void SceneManager_OnLoadEnd(SceneLoadEndEventArgs obj)
    {
        List<string> loadedscenes = obj.LoadedScenes.ToList().ConvertAll<string>(x => x.name);//.Concat(obj.SkippedSceneNames).ToList();
        Debug.Log($"LoadEnd, contains changed scene = {loadedscenes.Contains(changedScene)}");
        if (loadedscenes.Contains(changedScene))
            EventOnClientLoadedScenes(true);
    }

    /*
public static void ChangeNetworkScene(string sceneName, string[] scenesToClose)
{
   instance.CloseScenes(scenesToClose);

   SceneLoadData sld = new SceneLoadData(sceneName);
   NetworkConnection[] conns = instance.ServerManager.Clients.Values.ToArray();
   instance.SceneManager.LoadConnectionScenes(conns, sld);
}

[ServerRpc(RequireOwnership = false)]
void CloseScenes(string[] scenesToClose)
{
   CloseScenesObserver(scenesToClose);
}

[ObserversRpc]
void CloseScenesObserver(string[] scenesToClose)
{
   foreach (var sceneName in scenesToClose)
   {
       UnitySceneManager.UnloadSceneAsync(sceneName);
   }
}
*/

    public void ChangeNetworkScene(string sceneName, List<string> scenesToDontDestroyOnLoad = null)
    {
        changedScene = sceneName;

        if (scenesToDontDestroyOnLoad == null)
            scenesToDontDestroyOnLoad = new List<string> {sceneName};
        else if (!scenesToDontDestroyOnLoad.Contains(sceneName))
            scenesToDontDestroyOnLoad.Add(sceneName);

        CloseScenes(scenesToDontDestroyOnLoad.ToArray());

        SceneLoadData sld = new SceneLoadData(sceneName);
        NetworkConnection[] conns = ServerManager.Clients.Values.ToArray();
        SceneManager.LoadConnectionScenes(conns, sld);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CloseScenes(string[] scenesToDontDestroyOnLoad)
    {
        CloseScenesObserver(scenesToDontDestroyOnLoad);
    }

    // RunLocally: so that initially server only is present can also run this, without client prefab
    [ObserversRpc(RunLocally = true)] 
    private void CloseScenesObserver(string[] scenesToDontDestroyOnLoad)
    {
        int noOfActiveScenes = UnitySceneManager.sceneCount;

        for (int i = 0; i < noOfActiveScenes; i++)
        {
            Scene scene = UnitySceneManager.GetSceneAt(i);

            if (constDontDestroyOnLoadSceneNames.Contains(scene.name) ||
                (scenesToDontDestroyOnLoad != null && scenesToDontDestroyOnLoad.Contains(scene.name)))
            {
                Debug.Log($"Scene:\"{scene.name}\" Dont-Destroy-On-Load!");
                continue;
            }

            UnitySceneManager.UnloadSceneAsync(scene);
        }
    }
}