using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ILoadingResource
{
    IAsyncTask Load();
}
public class PBLoadingResourceManager : Singleton<PBLoadingResourceManager>
{
    private void Start()
    {
        var loadingResources = transform.parent.GetComponentsInChildren<ILoadingResource>();
        var tasks = new List<IAsyncTask>(loadingResources.Length);
        foreach (var loadingResource in loadingResources)
        {
            tasks.Add(loadingResource.Load());
        }
        if (LoadingScreenUI.s_Instance != null)
            LoadingScreenUI.Load(new CompositeAsyncTask(tasks.ToArray()));
    }
}