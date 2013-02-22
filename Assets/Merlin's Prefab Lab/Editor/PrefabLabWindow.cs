using System.IO;
using UnityEditor;
using UnityEngine;

public class PrefabLabWindow : EditorWindow
{
    private static PrefabLabSettings settings = PrefabLabSettings.Defaults;

    [MenuItem("Merlin/Prefab Lab")]
    public static void ShowWindow()
    {
        if (File.Exists(PrefabLabSettings.Filepath))
            settings.ReadSettingsFromDisk();
        else
            settings = PrefabLabSettings.Defaults;

        EditorWindow window = EditorWindow.GetWindow(typeof(PrefabLabWindow), true);

        Rect windowRect = new Rect(200, 300, 320, 460);

        window.position = windowRect;
        window.title = "Merlin's Prefab Lab";
    }

    //TODO: We need to do a lot of validation on the user typed strings
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 1000));

        //Configure SourceFolder
        GUILayout.Label("Source Folder", EditorStyles.boldLabel);
        GUILayout.Label(
            "All models in this folder or any subfolder will be processed."
            + System.Environment.NewLine +
            "The path should be relative to the 'Assets' folder.",
            EditorStyles.wordWrappedMiniLabel
            );

        settings.SourceFolder = EditorGUILayout.TextField(settings.SourceFolder);
        //if (GUILayout.Button("Change"))
        //{
        //    settings.SourceFolder = EditorUtility.OpenFolderPanel("Select the source folder", settings.SourceFolder, "defaultName");
        //}

        GUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        GUILayout.Label("e.g. 'Models' or 'Models/Originals'.", EditorStyles.wordWrappedMiniLabel);
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        //Configure DestinationFolder
        GUILayout.Label("Destination Folder", EditorStyles.boldLabel);
        GUILayout.Label(
            "Generated prefabs will be saved to this folder, maintaining the source folder structure." +
            System.Environment.NewLine +
            "The path should be relative to the 'Assets' folder.",
            EditorStyles.wordWrappedMiniLabel
            );

        settings.DestinationFolder = EditorGUILayout.TextField(settings.DestinationFolder);
        //if (GUILayout.Button("Change"))
        //{
        //    settings.DestinationFolder = EditorUtility.OpenFolderPanel("Select your destination", settings.DestinationFolder, "defaultName");
        //}

        GUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        GUILayout.Label("e.g. 'Prefabs' or 'Prefabs/Models'.", EditorStyles.wordWrappedMiniLabel);
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();


        //Configure PrefabPrefix
        GUILayout.Label("Prefab Suffix", EditorStyles.boldLabel);
        GUILayout.Label("The string that will be added to the end of the prefab name " +
        System.Environment.NewLine +
        "to create a clear distinction between model and prefab.",
        EditorStyles.wordWrappedMiniLabel
        );

        settings.PrefabSuffix = EditorGUILayout.TextField(settings.PrefabSuffix);

        EditorGUILayout.Space();
        EditorGUILayout.Space();


        //Configure postprocessor order
        GUILayout.Label("Postprocessor Order", EditorStyles.boldLabel);
        GUILayout.Label("When there are more asset postprocessors, the processing order could go wrong. Modify this value to change the order.", EditorStyles.wordWrappedMiniLabel);

        settings.PostProcessOrder = EditorGUILayout.IntField(settings.PostProcessOrder);

        GUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        GUILayout.Label("Note: Lower priorities will be imported first.", EditorStyles.wordWrappedMiniLabel);
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        //Apply Button
        GUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("Change"))
        {
            settings.WriteSettingsToDisk();
        }
        EditorGUILayout.Space();
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }
}