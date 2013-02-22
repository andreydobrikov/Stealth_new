using UnityEditor;
using System.Collections;

public class GlobalImportSettings : AssetPostprocessor
{

    public void OnPreprocessModel()
    {
        ModelImporter modelImporter = (ModelImporter) assetImporter;    
                
        modelImporter.globalScale = 1;
        modelImporter.swapUVChannels = false;
        //modelImporter.generateAnimations = ModelImporterGenerateAnimations.None; 
    }   

}

