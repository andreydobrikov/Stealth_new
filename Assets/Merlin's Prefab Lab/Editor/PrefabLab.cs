using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/*----------------------------------
 * Description
 * ---------------------------------
 * This importer creates prefabs for all imported models.
 * This way we can preserve our prefabs AND keep them up-to-date with their models
 * The prefabs are named "modelname" + 'PrefabNameExtension'
 * You should set the Source and Destination Folder to what you want for each project!
 * Use the source and dest folders to separate 3dmax files from the prefabs 
 * ---------------------------------
 * Code Flow
 * ---------------------------------
 * When importing a model into Unity,
 * this importer does the following:
 * - Check if the imported object should be used with this importer
 * - Check if a prefab of the model already exists. If not, create the prefab
 * - Search in the prefab for the model
 * - Sync with the newly imported model
 * - Preserve the prefab (Keep all usergenerated content like ParticleSystems, Lights, Scripts etc. Basicly every Component)
 * - Update the prefab to apply the changes
 * 
 * !!! This importer uses MetaData scripts to store information about the model.
 * !!! We need this, there is no other way to check if a gameobject is new, old or user generated content
 */


public class PrefabUpdater : AssetPostprocessor
{
    private static string NewModelNameSuffix = "_new";
    private static string lostObjectsName = "Lost GameObjects";

    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        //Read PrefabUpdater settings
        PrefabLabSettings settings = PrefabLabSettings.Defaults;
        if (File.Exists(PrefabLabSettings.Filepath))
            settings.ReadSettingsFromDisk();

        //Move assets

        List<string> filteredMovedFromAssets;
        List<string> filteredMoveAssets = filterAssets(movedAssets, settings.SourceFolder, PrefabType.ModelPrefab, movedFromAssetPaths, out filteredMovedFromAssets);

        for (int i = 0; i < filteredMoveAssets.Count; i++)
        {
            moveAndRenameAsset(filteredMoveAssets[i], filteredMovedFromAssets[i], settings);
        }

        //Delete assets
        List<string> filteredDeletedAssets = filterAssets(deletedAssets, settings.SourceFolder);

        foreach (string asset in filteredDeletedAssets)
        {
            deletePrefab(asset, settings);
        }

        //Import assets
        List<string> filteredImportAssets = filterAssets(importedAssets, settings.SourceFolder, PrefabType.ModelPrefab);

        foreach (string asset in filteredImportAssets)
        {
            UpdatePrefab(asset, settings);
        }



        //Debug
        /*foreach (string str in importedAssets)
            Debug.Log("Reimported Asset: " + str);

        foreach (string str in deletedAssets)
            Debug.Log("Deleted Asset: " + str);

        for (int i = 0; i < movedAssets.Length; i++)
            Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);*/
    }

    #region Move and Rename

    private static void moveAndRenameAsset(string movedAsset, string movedFromAssetPath, PrefabLabSettings settings)
    {
        //Substract subdirs from imported asset
        string oldRelativeSubdir = movedFromAssetPath.Remove(0, ("Assets/" + settings.SourceFolder).Length + 1);
        oldRelativeSubdir = oldRelativeSubdir.Remove(oldRelativeSubdir.LastIndexOf("/") + 1);
        string oldRelativeDestinationFolder = settings.DestinationFolder + "/" + oldRelativeSubdir;

        string oldAssetName = Path.GetFileNameWithoutExtension(movedFromAssetPath);
        string oldAssetPath;

        if (findExistingPrefab(oldAssetName, oldRelativeDestinationFolder, settings.PrefabSuffix, out oldAssetPath))
        {
            string relativeSubdir = movedAsset.Remove(0, ("Assets/" + settings.SourceFolder).Length + 1);
            relativeSubdir = relativeSubdir.Remove(relativeSubdir.LastIndexOf("/") + 1);
            string relativeDestinationFolder = settings.DestinationFolder + "/" + relativeSubdir;
            string absoluteSubdir = Application.dataPath + "/" + relativeDestinationFolder;

            string newAssetName = Path.GetFileNameWithoutExtension(movedAsset);
            string newAssetPath = "Assets/" + relativeDestinationFolder + newAssetName + settings.PrefabSuffix + ".prefab";
            oldAssetPath = "Assets/" + oldAssetPath;

            Directory.CreateDirectory(absoluteSubdir);

            AssetDatabase.MoveAsset(oldAssetPath, newAssetPath);

            Debug.Log("From: " + oldAssetPath + " To: " + newAssetPath + " absoluteSubdir: " + absoluteSubdir);

            if (oldAssetName != newAssetName)
            {
                GameObject GO = (GameObject)AssetDatabase.LoadAssetAtPath(newAssetPath, typeof(GameObject));
                GetChildByName(oldAssetName, GO).name = newAssetName;
            }
        }
    }

    #endregion

    #region Delete

    private static void deletePrefab(string assetPath, PrefabLabSettings settings)
    {
        string prefabPath;
        if (findExistingPrefab(assetPath, settings, out prefabPath))
        {
            AssetDatabase.DeleteAsset("Assets/" + prefabPath);
        }
    }

    #endregion

    #region Import


    public static void UpdatePrefab(PrefabLabSettings settings, string prefabPath)
    {
        string modelPath;
        if (findExistingModel(prefabPath, settings, out modelPath))
        {
            UpdatePrefab("Assets/" + modelPath, settings);
        }
    }

    public static void UpdatePrefab(string modelPath, PrefabLabSettings settings)
    {
        //Substract subdirs from imported asset
        string relativeSubdir = modelPath.Remove(0, ("Assets/" + settings.SourceFolder).Length + 1);
        relativeSubdir = relativeSubdir.Remove(relativeSubdir.LastIndexOf("/") + 1);
        string relativeSourceFolder = settings.DestinationFolder + "/" + relativeSubdir;
        string absoluteSubdir = Application.dataPath + "/" + relativeSourceFolder;

        //Prepare the imported model for modification
        GameObject importedModel = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath(modelPath, typeof(GameObject)));
        AddMetaData(importedModel);

        GameObject lostObjects = null;

        Object originalPrefab;
        if (!FindExistingPrefab(importedModel.name, relativeSourceFolder, settings.PrefabSuffix, out originalPrefab))
        {
            //Create subdirs
            Directory.CreateDirectory(absoluteSubdir);

            AssetDatabase.Refresh();

            //Prefab not found, create one for me please!
            string prefabFilepath = "Assets/" + relativeSourceFolder + importedModel.name + settings.PrefabSuffix + ".prefab";
            Object emptyPrefab = PrefabUtility.CreateEmptyPrefab(prefabFilepath);
            AssetDatabase.ImportAsset(prefabFilepath);

            //Creating an empty prefab is really empty so we need a gameobject to start with, called parent
            GameObject parent = new GameObject(importedModel.name + settings.PrefabSuffix);
            PrefabLabData PLD = parent.AddComponent<PrefabLabData>();
            PLD.Type = PrefabLabObjectType.Root;

            //Mark as max model
            UpdateMetaDataRecursively(importedModel, settings.ModelMetadata, PLD);
            //Copy newModel to prefab
            importedModel.transform.parent = parent.transform;

            originalPrefab = PrefabUtility.ReplacePrefab(parent, emptyPrefab, ReplacePrefabOptions.ReplaceNameBased);

            //Clean up
            GameObject.DestroyImmediate(parent);
        }
        else
        {
            GameObject prefab = (GameObject)GameObject.Instantiate(originalPrefab);
            prefab.name = originalPrefab.name;

            PrefabLabData PLD = prefab.GetComponent<PrefabLabData>();
            if (PLD == null)
            {
                PLD = prefab.AddComponent<PrefabLabData>();
                PLD.Type = PrefabLabObjectType.Root;
            }

            //Find LostObjects root or create it
            lostObjects = GetChildByName(lostObjectsName, prefab);

            if (lostObjects == null)
            {
                lostObjects = new GameObject(lostObjectsName);
                lostObjects.transform.parent = prefab.transform;
            }

            //Find newModel root in merlin's prefab and do magic
            GameObject prefabModelRoot = GetChildByName(importedModel.name, prefab);

            if (prefabModelRoot != null)
            {
                //Check if object changed from one object to multiple
                if ((prefabModelRoot.transform.GetChildCount() == 0 && importedModel.transform.GetChildCount() != 0) || (prefabModelRoot.transform.GetChildCount() != 0 && importedModel.transform.GetChildCount() == 0))
                {
                    prefabModelRoot.transform.parent = lostObjects.transform;

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6
                    prefabModelRoot.SetActiveRecursively(false);
#else
                    prefabModelRoot.SetActive(false);
#endif
                    prefabModelRoot = null;

                    Debug.LogWarning("Unity treats model files with one object differently than files with multiple objects. You changed in the number of objects in " + importedModel.name + ". We recovered the old gameobjects in LostObjects.");
                }
            }

            if (prefabModelRoot == null)
            {
                UpdateMetaDataRecursively(importedModel, settings.ModelMetadata, PLD);

                importedModel.transform.parent = prefab.transform;
                importedModel.transform.localPosition = Vector3.zero;
            }
            else
            {
                //Mark the newly imported as new to know later on if a modelpart has just been imported or its an old prefabpart
                UpdateMetaDataRecursively(importedModel, NewModelNameSuffix, PLD);

                //Does all the magic to childs
                RebuildPrefab(importedModel, prefabModelRoot, modelPath, PLD);
                RescueAndRemove(importedModel, prefabModelRoot, lostObjects);
                RenameNewToCurrent(prefabModelRoot.gameObject, settings);

                //Don't forget the parent
                CopyComponents(importedModel, prefabModelRoot, modelPath, PLD);
            }

            //Clean up if no lostobjects have been found
            if (lostObjects.transform.childCount > 0)
            {
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6
                lostObjects.SetActiveRecursively(false);
#else
                lostObjects.SetActive(false);
#endif

                Debug.Log("Prefab Lab Warning: There are " + lostObjects.transform.childCount + " objects found which don't have a parent anymore because it has been deleted in the model." +
                    System.Environment.NewLine +
                    "To recover these objects, browse to the gameobject \"" + lostObjectsName + "\" in the prefab " + prefab.name + ".", originalPrefab);
            }
            else
            {
                GameObject.DestroyImmediate(lostObjects);
            }

            //Save created or modified prefab
            PrefabUtility.ReplacePrefab(prefab, originalPrefab, ReplacePrefabOptions.ReplaceNameBased);

            //Clean up
            GameObject.DestroyImmediate(importedModel);
            GameObject.DestroyImmediate(prefab);
        }
    }

    public static void UpdatePrefabs(Object[] objects, PrefabLabSettings settings)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            string assetPath = AssetDatabase.GetAssetPath(objects[i].GetInstanceID());

            if (assetPath.IndexOf("Assets/" + settings.DestinationFolder) != -1)
            {
                if (PrefabUtility.GetPrefabType(objects[i]) == PrefabType.Prefab)
                {
                    string modelPath;
                    if (findExistingModel(assetPath, settings, out modelPath))
                    {
                        UpdatePrefab("Assets/" + modelPath, settings);
                    }
                }
            }

            if (assetPath.IndexOf("Assets/" + settings.SourceFolder) != -1)
            {
                if (PrefabUtility.GetPrefabType(objects[i]) == PrefabType.ModelPrefab)
                {
                    UpdatePrefab(assetPath, settings);
                }
            }
        }
    }

    public static Object[] GetModelsOrPrefabs(Object[] objects, PrefabLabSettings settings)
    {
        List<Object> newObjects = new List<Object>();

        for (int i = 0; i < objects.Length; i++)
        {
            string assetPath = AssetDatabase.GetAssetPath(objects[i].GetInstanceID());

            if (assetPath.IndexOf("Assets/" + settings.DestinationFolder) != -1)
            {
                if (PrefabUtility.GetPrefabType(objects[i]) == PrefabType.Prefab)
                {
                    string modelPath;
                    if (findExistingModel(assetPath, settings, out modelPath))
                    {
                        newObjects.Add(AssetDatabase.LoadMainAssetAtPath("Assets/" + modelPath));
                    }
                }
            }

            if (assetPath.IndexOf("Assets/" + settings.SourceFolder) != -1)
            {
                if (PrefabUtility.GetPrefabType(objects[i]) == PrefabType.ModelPrefab)
                {
                    string prefabPath;
                    if (findExistingPrefab(assetPath, settings, out prefabPath))
                    {
                        newObjects.Add(AssetDatabase.LoadMainAssetAtPath("Assets/" + prefabPath));
                    }
                }
            }
        }

        return newObjects.Count == 0 ? null : newObjects.ToArray();
    }

    public override int GetPostprocessOrder()
    {
        //Read PrefabUpdater settings
        PrefabLabSettings settings = PrefabLabSettings.Defaults;
        if (File.Exists(PrefabLabSettings.Filepath))
            settings.ReadSettingsFromDisk();

        return settings.PostProcessOrder;
    }

    private static bool findExistingModel(string prefabPath, PrefabLabSettings settings, out string foundModelPath)
    {
        foundModelPath = null;

        string relativeSubdir = prefabPath.Remove(0, ("Assets/" + settings.DestinationFolder).Length + 1);
        relativeSubdir = relativeSubdir.Remove(relativeSubdir.LastIndexOf("/") + 1);
        string relativeSourceFolder = settings.SourceFolder + "/" + relativeSubdir;
        string prefabName = Path.GetFileNameWithoutExtension(prefabPath);
        string modelName = prefabName.Remove(prefabName.Length - settings.PrefabSuffix.Length);

        //find filepath
        DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/" + relativeSourceFolder);

        if (di.Exists)
        {
            foreach (FileInfo fi in di.GetFiles())
            {
                if (Path.GetFileNameWithoutExtension(fi.Name) == modelName)
                {
                    foundModelPath = relativeSourceFolder + fi.Name;
                    return true;
                }
            }
        }

        return false;
    }

    private static bool findExistingPrefab(string modelPath, PrefabLabSettings settings, out string foundPrefabPath)
    {
        foundPrefabPath = null;

        string relativeSubdir = modelPath.Remove(0, ("Assets/" + settings.SourceFolder).Length + 1);
        relativeSubdir = relativeSubdir.Remove(relativeSubdir.LastIndexOf("/") + 1);
        string relativeDestinationFolder = settings.DestinationFolder + "/" + relativeSubdir;
        string modelName = Path.GetFileNameWithoutExtension(modelPath);

        //find filepath
        DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/" + relativeDestinationFolder);

        if (di.Exists)
        {
            foreach (FileInfo fi in di.GetFiles())
            {
                if (Path.GetFileNameWithoutExtension(fi.Name) == modelName + settings.PrefabSuffix)
                {
                    foundPrefabPath = relativeDestinationFolder + fi.Name;
                    return true;
                }
            }
        }

        return false;
    }

    private static bool findExistingPrefab(string modelname, string subfolder, string prefabSuffix, out string foundPrefabPath)
    {
        bool result = false;

        //find filepath
        DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/" + subfolder);

        foundPrefabPath = null;

        string prefabFilename = "";
        if (di.Exists)
        {
            foreach (FileInfo fi in di.GetFiles())
            {
                if (fi.Name == modelname + prefabSuffix + ".prefab")
                {
                    prefabFilename = fi.Name;
                    break;
                }
            }
        }

        //get gameobject
        if (prefabFilename.Length > 0)
        {
            foundPrefabPath = subfolder + prefabFilename;

            result = true;
        }

        return result;
    }

    //Returns true if found and sets the foundPrefab object 
    private static bool FindExistingPrefab(string modelname, string subfolder, string prefabSuffix, out Object foundPrefab)
    {
        string foundPrefabPath;

        bool result = findExistingPrefab(modelname, subfolder, prefabSuffix, out foundPrefabPath);

        foundPrefab = AssetDatabase.LoadAssetAtPath("Assets/" + foundPrefabPath, typeof(Object));

        return result;
    }

    //Recursive copy newly found modelparts to the prefab
    //and synch Transform, Materials etc. with current parts
    private static void RebuildPrefab(GameObject importedModel, GameObject prefab, string assetPath, PrefabLabData PLD)
    {
        for (int newModelIndex = importedModel.transform.childCount - 1; newModelIndex >= 0; newModelIndex--)
        {
            bool hasMatch = false;

            for (int oldPrefabIndex = prefab.transform.childCount - 1; oldPrefabIndex >= 0; oldPrefabIndex--)
            {
                GameObject importedModelChild = importedModel.transform.GetChild(newModelIndex).gameObject;
                GameObject prefabChild = prefab.transform.GetChild(oldPrefabIndex).gameObject;

                if (importedModelChild.name == prefabChild.name)
                {
                    CopyComponents(importedModelChild, prefabChild, assetPath, PLD);

                    hasMatch = true;

                    RebuildPrefab(importedModelChild, prefabChild, assetPath, PLD);
                    break;
                }
            }

            //New gameobject in importedModel detected, copy it to the prefab
            if (!hasMatch)
            {
                ParentGOWithoutTransformChanges(prefab, importedModel.transform.GetChild(newModelIndex).gameObject);
            }
        }
    }

    //Recursive remove old prefab parts and move the userdata on it to lostObjects
    private static void RescueAndRemove(GameObject importedModel, GameObject prefab, GameObject lostObjects)
    {
        //Check prefab for removed parts
        for (int oldPrefabIndex = prefab.transform.childCount - 1; oldPrefabIndex >= 0; oldPrefabIndex--)
        {
            bool hasMatch = false;

            for (int newModelIndex = importedModel.transform.childCount - 1; newModelIndex >= 0; newModelIndex--)
            {
                GameObject newModelChild = importedModel.transform.GetChild(newModelIndex).gameObject;
                GameObject oldPrefabChild = prefab.transform.GetChild(oldPrefabIndex).gameObject;

                if (newModelChild.name == oldPrefabChild.name)
                {
                    hasMatch = true;

                    RescueAndRemove(newModelChild, oldPrefabChild, lostObjects);
                    break;
                }
            }

            //Removed gameobject in the prefab detected. Gather userdata and add it to lost gameobjects
            if (!hasMatch)
            {
                PrefabLabData metadata = prefab.transform.GetChild(oldPrefabIndex).gameObject.GetComponent<PrefabLabData>();
                if (metadata != null)
                {
                    if (metadata.Data != NewModelNameSuffix)    //Check whether the modelpart has just been added to the prefab by Rebuild() or not
                    {
                        //Gather userdata before removing
                        FindAndSaveLostObjects(prefab.transform.GetChild(oldPrefabIndex).gameObject, lostObjects);

                        GameObject.DestroyImmediate(prefab.transform.GetChild(oldPrefabIndex).gameObject);
                    }
                }
            }
        }
    }

    private static void CopyTransform(GameObject source, GameObject destination)
    {
        destination.transform.localPosition = source.transform.localPosition;
        destination.transform.localRotation = source.transform.localRotation;
        destination.transform.localScale = source.transform.localScale;
    }

    private static void CopyMaterials(GameObject source, GameObject destination)
    {
        if (source.renderer != null && destination.renderer != null)
            destination.renderer.sharedMaterials = source.renderer.sharedMaterials;
    }

    private static void CopyAnimations(GameObject source, GameObject destination)
    {
        if (source.animation == null && destination.animation != null)
        {
            //Remove animation component
            Component.DestroyImmediate(destination.animation);
        }
        else if (source.animation != null && destination.animation == null)
        {
            //Add & sync animation component
            Animation anim = destination.AddComponent<Animation>();
            anim.clip = source.animation.clip;
            foreach (AnimationState animState in source.animation)
            {
                anim.AddClip(animState.clip, animState.name);
            }
        }
        else if (source.animation != null && destination.animation != null)
        {
            AnimationClip clip = destination.animation.clip;
            bool auto = destination.animation.playAutomatically;
            bool phys = destination.animation.animatePhysics;
            AnimationCullingType cull = destination.animation.cullingType;

            //Remove old
            foreach (AnimationState item in destination.animation)
            {
                destination.animation.RemoveClip(item.clip);
            }

            foreach (AnimationState item in source.animation)
            {
                destination.animation.AddClip(item.clip, item.name);
            }

            //Add & sync animation component
            destination.animation.clip = clip;
            destination.animation.playAutomatically = auto;
            destination.animation.animatePhysics = phys;
            destination.animation.cullingType = cull;
        }
        else
        {
            //source.anmation && destination.animation are null;
            //Do nothing
        }
    }

    private static void CopyColliders(GameObject source, GameObject destination)
    {
        if (destination.GetComponent<Collider>() == null)
        {
            MeshCollider srcCollider = source.GetComponent<MeshCollider>();
            if (srcCollider != null)
            {
                MeshCollider dstCollider = destination.AddComponent<MeshCollider>();
                dstCollider.sharedMesh = srcCollider.sharedMesh;
            }
        }
    }

    private static void CopyMetaData(GameObject source, GameObject destination)
    {
        PrefabLabData destPLD = destination.GetComponent<PrefabLabData>();
        PrefabLabData sourcePLD = source.GetComponent<PrefabLabData>();

        if (destPLD != null)
        {
            destPLD.Type = sourcePLD.Type;
        }
        else
        {
            destPLD = destination.AddComponent<PrefabLabData>();
            destPLD.Type = PrefabLabObjectType.NotSpecified;
        }
    }

    private static void CopyComponents(GameObject source, GameObject destination, string assetPath, PrefabLabData PLD)
    {
        CopyTransform(source, destination);

        CopyMetaData(source, destination);

        if (GetModelImporter(assetPath).importMaterials)
            CopyMaterials(source, destination);

        if (GetModelImporter(assetPath).generateAnimations != ModelImporterGenerateAnimations.None && PLD.ImportAnimationsAutomatically)
            CopyAnimations(source, destination);

        if (GetModelImporter(assetPath).addCollider)
            CopyColliders(source, destination);
    }

    //Recursively add metadata to gameobjects
    private static void AddMetaData(GameObject root)
    {
        root.AddComponent<PrefabLabData>();

        for (int i = 0; i < root.transform.childCount; i++)
        {
            GameObject child = root.transform.GetChild(i).gameObject;
            AddMetaData(child);
        }
    }

    //Recursively find all gameobjects in root and add suffix to name
    private static void UpdateMetaDataRecursively(GameObject root, string suffix, PrefabLabData rootPLD)
    {
        UpdateMetaDataRecursively(root, suffix, rootPLD, true);
    }

    private static void UpdateMetaDataRecursively(GameObject root, string suffix, PrefabLabData rootPLD, bool firstExecution)
    {
        PrefabLabData metadata = root.GetComponent<PrefabLabData>();
        metadata.Data = suffix;

        if (firstExecution)
        {
            rootPLD.SkinnedMeshes = false;
        }

        if (metadata.Type == PrefabLabObjectType.Old)
            metadata.Type = PrefabLabObjectType.NotSpecified;

        if (root.GetComponent<SkinnedMeshRenderer>() != null)
        {
            metadata.Type = PrefabLabObjectType.SkinnedMesh;
            rootPLD.SkinnedMeshes = true;
        }

        for (int i = 0; i < root.transform.childCount; i++)
        {
            GameObject child = root.transform.GetChild(i).gameObject;
            UpdateMetaDataRecursively(child, suffix, rootPLD, false);
        }
    }

    //Rename MetaData.Data from NewModelNameSuffix to CurrentModelNameSuffix
    //(New and Current exist so we can detect whether an gameobject has just been imported or its an older part of the prefab)
    private static void RenameNewToCurrent(GameObject root, PrefabLabSettings settings)
    {
        PrefabLabData metadata = root.GetComponent<PrefabLabData>();

        if (metadata != null)
        {
            if (metadata.Data == NewModelNameSuffix)
            {
                metadata.Data = settings.ModelMetadata;
            }
        }

        for (int i = 0; i < root.transform.childCount; i++)
        {
            RenameNewToCurrent(root.transform.GetChild(i).gameObject, settings);
        }
    }

    //Find and move userdata from the source to lostObjects
    private static void FindAndSaveLostObjects(GameObject source, GameObject lostObjectsStorage)
    {
        for (int i = source.transform.childCount - 1; i >= 0; i--)
        {
            if (source.transform.GetChild(i).gameObject.GetComponent<PrefabLabData>() == null)
                ParentGOWithoutTransformChanges(lostObjectsStorage, source.transform.GetChild(i).gameObject);
            else
                FindAndSaveLostObjects(source.transform.GetChild(i).gameObject, lostObjectsStorage);
        }
    }

    //Parent one GameObject to another without modifying the transform
    private static void ParentGOWithoutTransformChanges(GameObject parent, GameObject childToBe)
    {
        Vector3 position = childToBe.transform.localPosition;
        Quaternion rotation = childToBe.transform.localRotation;
        Vector3 scale = childToBe.transform.localScale;

        childToBe.transform.parent = parent.transform;

        childToBe.transform.localScale = scale;
        childToBe.transform.localPosition = position;
        childToBe.transform.localRotation = rotation;

    }

    //Find a child in go by its name
    private static GameObject GetChildByName(string name, GameObject go)
    {
        GameObject result = null;
        Transform t = go.transform.FindChild(name);

        if (t != null)
            result = t.gameObject;

        return result;
    }

    private static ModelImporter GetModelImporter(string assetPath)
    {
        return (ModelImporter)AssetImporter.GetAtPath(assetPath);
    }

    #endregion

    private static List<string> filterAssets(string[] assetPaths, string assetPath, PrefabType prefabType, string[] movedFromAssetPaths, out List<string> filteredMovedFromAssets)
    {
        List<string> filteredAssets = new List<string>();
        filteredMovedFromAssets = new List<string>();

        for (int i = 0; i < assetPaths.Length; i++)
        {
            //Skip imports outside the sourcefolder
            if (assetPaths[i].IndexOf("Assets/" + assetPath) != -1)
            {
                if (PrefabUtility.GetPrefabType(AssetDatabase.LoadAssetAtPath(assetPaths[i], typeof(Object))) == prefabType)
                {
                    filteredAssets.Add(assetPaths[i]);
                    filteredMovedFromAssets.Add(movedFromAssetPaths[i]);
                }
            }
        }

        return filteredAssets;
    }

    private static List<string> filterAssets(string[] assetPaths, string sourcePath)
    {
        List<string> filteredAssets = new List<string>();

        for (int i = 0; i < assetPaths.Length; i++)
        {
            //Skip imports outside the sourcefolder
            if (assetPaths[i].IndexOf("Assets/" + sourcePath) != -1)
            {
                filteredAssets.Add(assetPaths[i]);
            }
        }
        return filteredAssets;
    }

    private static List<string> filterAssets(string[] assetPaths, string sourcePath, PrefabType prefabType)
    {
        List<string> prefabTypeFilteredAssets = new List<string>();

        List<string> filteredAssets = filterAssets(assetPaths, sourcePath);

        foreach (string s in filteredAssets)
        {
            if (PrefabUtility.GetPrefabType(AssetDatabase.LoadAssetAtPath(s, typeof(Object))) == prefabType)
            {
                prefabTypeFilteredAssets.Add(s);
            }
        }

        return prefabTypeFilteredAssets;
    }

    public static int GetInstanceIDOfSceneAndProjectObject(GameObject GO)
    {
        int instanceID;
        if (AssetDatabase.IsMainAsset(GO.GetInstanceID()) || AssetDatabase.IsSubAsset(GO.GetInstanceID()))
        {
            instanceID = GO.GetInstanceID();
        }
        else
        {
            GameObject prefab = GO;

            UnityEngine.Object prefabObject = null;

            if (prefab != null)
            {
                prefabObject = PrefabUtility.GetPrefabParent(prefab);
            }

            instanceID = prefabObject.GetInstanceID();
        }

        return instanceID;
    }

    public static PrefabLabSettings GetPrefabLabSettings()
    {
        //Read PrefabUpdater settings
        PrefabLabSettings settings = PrefabLabSettings.Defaults;
        if (File.Exists(PrefabLabSettings.Filepath))
            settings.ReadSettingsFromDisk();
        return settings;
    }

}
