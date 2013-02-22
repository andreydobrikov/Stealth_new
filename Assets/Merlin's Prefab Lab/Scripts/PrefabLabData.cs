using UnityEngine;

public class PrefabLabData : MonoBehaviour
{
    public string Data;
    public PrefabLabObjectType Type = PrefabLabObjectType.Old;
    public bool ImportAnimationsAutomatically = true;
    public bool SkinnedMeshes = false;
}

public enum PrefabLabObjectType
{
    Root,
    SkinnedMesh,
    NotSpecified,
    Old
}