using UnityEngine;

[CustomInspectorName(displayName: "PBMonoPrefabItemModule")]
public class PBMonoPrefabItemModule : ModelPrefabItemModule
{
    [SerializeField]
    protected PBPart partModelPrefab;

    public override T GetModelPrefab<T>()
    {
        return partModelPrefab as T;
    }
}
