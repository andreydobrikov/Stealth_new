using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PrefabLabSettings
{
    private const string SRC_FOLDER = "SourceFolder";
    private const string DEST_FOLDER = "DestinationFolder";
    private const string PREFAB_SUFFIX = "PrefabSuffix";
    private const string MODEL_METADATA = "ModelMetadata";
    private const string POST_PROCESS_ORDER = "PostprocessOrder";

    //A dictionary is convinient for looping thru each setting while reading/writing to the settingsfile
    private Dictionary<string, string> settings = new Dictionary<string, string>();

    public PrefabLabSettings(string srcFolder, string dstFolder, string pSuffix, string mdlMetaData, int processOrder)
    {
        this.SourceFolder = srcFolder;
        this.DestinationFolder = dstFolder;
        this.PrefabSuffix = pSuffix;
        this.ModelMetadata = mdlMetaData;
        this.PostProcessOrder = processOrder;
    }

    public static PrefabLabSettings Defaults
    {
        get
        {
            string srcFolder = "Models";
            string dstFolder = "Prefabs/";
            string pSuffix = "_prefab";
            string mdlMetadata = "_model";
            int processOrder = 1;

            return new PrefabLabSettings(srcFolder, dstFolder, pSuffix, mdlMetadata, processOrder);
        }
    }

    public void ReadSettingsFromDisk()
    {
        settings.Clear();

        StreamReader reader = new StreamReader(Filepath);
        if (reader != null)
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] keypairvalue = line.Split('=');

                if (keypairvalue.Length == 2)
                {
                    settings.Add(keypairvalue[0], keypairvalue[1]);
                }
                else
                {
                    //Error reading settings file.
                }
            }
        }

        reader.Close();
    }

    public void WriteSettingsToDisk()
    {
        StreamWriter writer = new StreamWriter(Filepath);

        if (writer != null)
        {
            foreach (var setting in settings)
            {
                writer.WriteLine(setting.Key + "=" + setting.Value);
            }

            writer.Flush();
            writer.Close();
        }
    }

    #region Properties

    /// <summary>
    /// Only use this importer for this folder. (relative) Example: "Models" (absolute => Project/Assets/Models)
    /// </summary>
    public string SourceFolder
    {
        get { return settings[SRC_FOLDER]; }
        set { settings[SRC_FOLDER] = value; }
    }

    /// <summary>
    /// Create the prefabs in this folder. (relative) Example: "Prefabs" (absolute => Project/Assets/Prefabs)
    /// </summary>
    public string DestinationFolder
    {
        get { return settings[DEST_FOLDER]; }
        set { settings[DEST_FOLDER] = value; }
    }

    public string PrefabSuffix
    {
        get { return settings[PREFAB_SUFFIX]; }
        set { settings[PREFAB_SUFFIX] = value; }
    }

    public string ModelMetadata
    {
        get { return settings[MODEL_METADATA]; }
        set { settings[MODEL_METADATA] = value; }
    }

    public int PostProcessOrder
    {
        get
        {
            int result;
            if (!int.TryParse(settings[POST_PROCESS_ORDER], out result))
                result = 1; //defaults to 1

            return result;
        }

        set
        {
            settings[POST_PROCESS_ORDER] = value.ToString();
        }
    }

    public static string Filepath
    {
        get { return Application.dataPath + "/Merlin's Prefab Lab/Editor/Settings.cfg"; }
    }

    #endregion
}