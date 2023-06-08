using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlanetSelectionUI : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private SceneAsset PlanetScene;
#endif
    [field: SerializeField] public string PlanetSceneName { get; private set; } = null;
    private Button selectBtn;
    private Outline selectedOutline;

    private void Awake()
    {
#if UNITY_EDITOR
        PlanetSceneName = PlanetScene.name;
#endif
        selectBtn = GetComponent<Button>();
        selectedOutline = GetComponent<Outline>();
    }
    private void Start()
    {
        selectBtn.onClick.AddListener(() => {
            MainMenuManager.SelectGamePlanet(this);
        });
    }

    public void DeselectBtn()
    {
        selectedOutline.enabled = false;
        selectBtn.interactable = true;
    }
    public void SelectBtn()
    {
        selectedOutline.enabled = true;
        selectBtn.interactable = false;
    }
}
