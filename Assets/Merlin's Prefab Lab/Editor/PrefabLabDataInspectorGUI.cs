using UnityEditor;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(PrefabLabData))]
public class PrefabLabDataInspectorGUI : Editor
{
    public override void OnInspectorGUI()
    {
        PrefabLabData PLD = target as PrefabLabData;

        if (PLD.Type == PrefabLabObjectType.Old)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("This is an old prefab. Please update the prefab to see new features", MessageType.Info, true);
        }
        else if (PLD.Type == PrefabLabObjectType.Root)
        {
            EditorGUILayout.Space();
            
            PLD.ImportAnimationsAutomatically = EditorGUILayout.Toggle("Import Animations", PLD.ImportAnimationsAutomatically, EditorStyles.toggle);
            
            EditorGUILayout.Space();

            if (PLD.SkinnedMeshes)
            {
                EditorGUILayout.HelpBox("SkinnedMeshRenderers are not supported yet. To update a skinned mesh correctly select the skinned GameObject and follow instructions. TIP: Try to avoid adding components and GameObjects to the skinned object and it's bones.", MessageType.Warning, true);
            }
        }
        else if (PLD.Type == PrefabLabObjectType.SkinnedMesh)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("SkinnedMeshRenderers are not supported yet. To update a skinned mesh correctly you have to delete this gameObject, and all the bones used by this component. Apply the Prefab and update the prefab", MessageType.Warning, true);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Update Prefab"))
        {
            string path = AssetDatabase.GetAssetPath(PrefabUpdater.GetInstanceIDOfSceneAndProjectObject(PLD.gameObject));

            PrefabLabSettings settings = PrefabUpdater.GetPrefabLabSettings();

            PrefabUpdater.UpdatePrefab(settings, path);
        }

        if (GUILayout.Button("Select Model"))
        {
            PrefabLabSettings settings = PrefabUpdater.GetPrefabLabSettings();

            Object[] newSelection = PrefabUpdater.GetModelsOrPrefabs(
                new Object[] { AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(PrefabUpdater.GetInstanceIDOfSceneAndProjectObject(PLD.gameObject)), typeof(Object)) }, settings);

            if (newSelection != null)
            {
                Selection.objects = newSelection;
            }
            else
            {
                Debug.Log("There was no prefab lab object found");
            }
        }

        EditorGUILayout.Space();
    }
}
