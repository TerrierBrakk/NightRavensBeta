using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(PropSpawner))]
public class UiElementsSpawnObjectsEditor : Editor
{

    private PropSpawner _spawner;
    private VisualElement _RootElement;
    private VisualTreeAsset _VisualTree;

    private List<Editor> objectPreviewEditors;

    public void OnEnable()
    {
        _spawner = (PropSpawner)target;

        _RootElement = new VisualElement();
        _VisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UiElementsSpawnObjectsEditor.xml");

        //Cargamos los estilos
        StyleSheet stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/UiElementsSpawnObjects.uss");
        _RootElement.styleSheets.Add(stylesheet);
    }


    public override VisualElement CreateInspectorGUI()
    {
        _RootElement.Clear();

        _VisualTree.CloneTree(_RootElement);

        UQueryBuilder<VisualElement> builder = _RootElement.Query(classes: new string[] { "prefab-button" });
        builder.ForEach(AddButtonIcon);

        return _RootElement;
    }

    public void AddButtonIcon(VisualElement button)
    {
        IMGUIContainer icon = new IMGUIContainer(() =>
        {

            string path = "Assets/Props/" + button.name + ".prebab";

            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            Editor editor = GetPreviewEditor(asset);

            editor.OnPreviewGUI(GUILayoutUtility.GetRect(90, 90), null);
        });

        icon.focusable = false;

        button.hierarchy.ElementAt(0).Add(icon);

        //spawn the prefab cuando el boton se toca
        button.RegisterCallback<MouseDownEvent>(evnt =>
        {
            SpawnPrefab(button.name);
        }, TrickleDown.TrickleDown);
    }

    
}