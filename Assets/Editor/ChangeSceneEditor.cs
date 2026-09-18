using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(ChangeScene))]
[CanEditMultipleObjects]
public class ChangeSceneEditor : Editor
{
    private SerializedProperty targetSceneName;
    private SerializedProperty playerTag;
    private string[] sceneNames;

    private void OnEnable()
    {
        targetSceneName = serializedObject.FindProperty("targetSceneName");
        playerTag = serializedObject.FindProperty("playerTag");
        RefreshScenes();
        if (string.IsNullOrEmpty(targetSceneName.stringValue))
        {
            targetSceneName.stringValue = SceneManager.GetActiveScene().name;
            serializedObject.ApplyModifiedProperties();
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        RefreshScenes();
        if (sceneNames.Length == 0)
        {
            EditorGUILayout.HelpBox("No scene assets were found in the project.", MessageType.Warning);
            EditorGUILayout.PropertyField(playerTag);
            serializedObject.ApplyModifiedProperties();
            return;
        }
        int selectedIndex = System.Array.IndexOf(sceneNames, targetSceneName.stringValue);
        selectedIndex = Mathf.Clamp(selectedIndex, 0, sceneNames.Length - 1);
        selectedIndex = EditorGUILayout.Popup("Target Scene", selectedIndex, sceneNames);
        targetSceneName.stringValue = sceneNames[selectedIndex];
        EditorGUILayout.PropertyField(playerTag);
        serializedObject.ApplyModifiedProperties();
    }

    private void RefreshScenes()
    {
        List<string> names = new List<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            names.Add(Path.GetFileNameWithoutExtension(path));
        }
        names.Sort();
        sceneNames = names.ToArray();
    }
}