using System.IO;
using UnityEditor;
using UnityEngine;

public class PrefabLabDropDownItems
{
    [MenuItem("Assets/Merlin/Update Prefab")]
    public static void UpdatePrefab()
    {
        //Read PrefabUpdater settings
        PrefabLabSettings settings = PrefabLabSettings.Defaults;
        if (File.Exists(PrefabLabSettings.Filepath))
            settings.ReadSettingsFromDisk();

        PrefabUpdater.UpdatePrefabs(Selection.objects, settings);
    }

    [MenuItem("Assets/Merlin/Select Prefab or Original Model")]
    public static void SelectPrefabOrModel()
    {
        //Read PrefabUpdater settings
        PrefabLabSettings settings = PrefabLabSettings.Defaults;
        if (File.Exists(PrefabLabSettings.Filepath))
            settings.ReadSettingsFromDisk();

        Object[] newSelection = PrefabUpdater.GetModelsOrPrefabs(Selection.objects, settings);

        if (newSelection != null)
        {
            Selection.objects = newSelection;
        }
        else
        {
            Debug.Log("There was no prefab lab object found");
        }
    }
}
