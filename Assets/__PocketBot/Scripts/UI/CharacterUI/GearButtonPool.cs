using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GearButtonPool : Singleton<GearButton>, ILoadingResource
{
    [SerializeField]
    private int m_DefaultCapacity = 30;
    [SerializeField]
    private GearButton m_GearButtonPrefab;
    [SerializeField]
    private RectTransform m_Container;

    private static ObjectPool<GearButton> m_GearButtonPool;

    private IAsyncTask InstantiateDefaultGearButton()
    {
        m_GearButtonPool = new ObjectPool<GearButton>(OnCreateItem, OnGetItem, OnReleaseItem, defaultCapacity: 0);
        var asyncOperation = InstantiateAsync(m_GearButtonPrefab, m_DefaultCapacity, m_Container);
        asyncOperation.completed += OnInstantiateCompleted;

        GearButton OnCreateItem()
        {
            return Instantiate(m_GearButtonPrefab, m_Container);
        }
        void OnGetItem(GearButton item)
        {
            item.CanvasGroup.alpha = 1;
        }
        void OnReleaseItem(GearButton item)
        {
            item.transform.position = Vector3.zero;
            item.transform.SetParent(m_Container);
            item.CanvasGroup.alpha = 0;
            item.Button.interactable = true;
            item.Button.onClick.RemoveAllListeners();
            item.IgnoreLayout(false);
            item.DeSelect();
            item.gameObject.SetActive(false);
        }
        void OnInstantiateCompleted(AsyncOperation operation)
        {
            for (int i = 0; i < asyncOperation.Result.Length; i++)
            {
                m_GearButtonPool.Release(asyncOperation.Result[i]);
            }
        }
        return new InstantiateAsyncTask<GearButton>(asyncOperation);
    }

    public static GearButton Get()
    {
        return m_GearButtonPool.Get();
    }

    public static void Release(GearButton gearButton)
    {
        m_GearButtonPool.Release(gearButton);
    }

    public IAsyncTask Load()
    {
        return InstantiateDefaultGearButton();
    }
}