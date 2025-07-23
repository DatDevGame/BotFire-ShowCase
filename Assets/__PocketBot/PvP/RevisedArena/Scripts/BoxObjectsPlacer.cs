using System.Collections;
using System.Collections.Generic;
using LatteGames;
using LatteGames.Utils;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BoxObjectsPlacer : BoxObjectPlacer
{
    [SerializeField]
    private List<GameObject> objectTemplates;

    public override void UpdatePlacing()
    {
        transform.DestroyChildren();
        int i = 0;
        IterateSpawnPoint((index, position) =>
        {
            GameObject newObject = null;
            GameObject objectTemplate = objectTemplates[i % objectTemplates.Count];
#if UNITY_EDITOR
            newObject = Application.isPlaying ? Instantiate(objectTemplate, transform) : (PrefabUtility.IsPartOfPrefabAsset(objectTemplate) ? PrefabUtility.InstantiatePrefab(objectTemplate, transform) as GameObject : Instantiate(objectTemplate, transform));
#else
            newObject = Instantiate(objectTemplate, transform);
#endif
            newObject.transform.localPosition = position;
            i++;
        });
    }
}