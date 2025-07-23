using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Const;

//Best practice: manually control all ref by Add and Remove function
//Good practice: have 1 find call in many frame (save the find result between many frame)
//Quite good practice: have many find calls on 1 frame (save the find result to use in mutiple place)
//Bad practice: have only 1 find call in 1 frame (add a little function call overhead. ref make the object can't be GC)
//Careful. I do have safety check for object that marked destroed, but you should not forget to using Remove on OnDestroy after using Add on Awake
public static class ObjectFindCache<T> where T : UnityEngine.Object
{
    private static bool _isManuallyControl = false;
    private static T _cachedFindObject = null;
    private static int _cachedFindFrame = -1;
    private static List<T> _cachedObjects = new List<T>();

    public static void Add(T cachedObject)
    {
        _cachedObjects.Add(cachedObject);
        _isManuallyControl = true;
    }

    public static void Remove(T cachedObject)
    {
        _cachedObjects.Remove(cachedObject);
    }

    //should not call from Awake
    public static T Get(bool includeInactive = false, bool isCallFromAwake = false)
    {
#if UNITY_EDITOR
        if (_isManuallyControl && StackTraceUtility.ExtractStackTrace().Contains("Awake"))
        {
            if (!isCallFromAwake)
            {
                Debug.LogError($"Calling ObjectFindCache<{typeof(T)}>.Get() from Awake without isCallFromAwake set to true may return unexpected null, as the cached objects get cache in Awake too. Try use Start if can");
            }
            else
            {
                Debug.LogWarning($"Calling ObjectFindCache<{typeof(T)}>.Get() from Awake will trigger a FindObjectOfType, as the cached objects get cache in Awake too. Try use Start if can");
            }

        }
#endif
        for (int i = 0; i < _cachedObjects.Count; ++i)
        {
            if (!_cachedObjects[i])
            {
                _cachedObjects.RemoveAt(i);
                --i;
            }
            else if (!includeInactive || _cachedObjects[i].GameObject().activeInHierarchy)
            {
                return _cachedObjects[i];
            }
        }

        if (!includeInactive && _isManuallyControl && !isCallFromAwake)
            return null;

        if (!_cachedFindObject)
        {
            _cachedFindObject = null;
            int frame = Time.frameCount;
            if (frame != _cachedFindFrame)
            {
                _cachedFindFrame = frame;
                _cachedFindObject = UnityEngine.Object.FindObjectOfType<T>(true);
#if UNITY_EDITOR
                Debug.Log($"ObjectFindCache: {typeof(T)} is not cached and get find in frame {_cachedFindFrame}");
#endif
            }
        }

        return _cachedFindObject;
    }

    //should not call from Awake
    public static List<T> GetAll(bool includeInactive = false, bool isCallFromAwake = false)
    {
#if UNITY_EDITOR
        if (_isManuallyControl && StackTraceUtility.ExtractStackTrace().Contains("Awake"))
        {
            if (!isCallFromAwake)
            {
                Debug.LogError($"Calling ObjectFindCache<{typeof(T)}>.GetAll() from Awake without isCallFromAwake set to true may return unexpected null, as the cached objects get cache in Awake too. Try use Start if can");
            }
            else
            {
                Debug.LogWarning($"Calling ObjectFindCache<{typeof(T)}>.GetAll() from Awake will trigger a FindObjectOfType, as the cached objects get cache in Awake too. Try use Start if can");
            }

        }
#endif
        List<T> result = new List<T>();
        for (int i = 0; i < _cachedObjects.Count; ++i)
        {
            if (!_cachedObjects[i])
            {
                _cachedObjects.RemoveAt(i);
                --i;
            }
            else if (!includeInactive || _cachedObjects[i].GameObject().activeInHierarchy)
            {
                result.Add(_cachedObjects[i]);
            }
        }

        if (result.Count > 0)
            return result;

        if (!includeInactive && _isManuallyControl && !isCallFromAwake)
            return null;
        //Not yet implemented FindObjectsOfType result caching. Should manage this list manually with Add and Remove
        return UnityEngine.Object.FindObjectsOfType<T>(true).ToList();
    }
}

public static class MainCameraFindCache
{
    private static Camera _cachedMainCamera = null;
    private static int _cachedFrame = -1;

    public static Camera Get()
    {
        if (!_cachedMainCamera || !_cachedMainCamera.enabled || !_cachedMainCamera.CompareTag(UnityTag.MainCamera) || !_cachedMainCamera.gameObject.activeInHierarchy)
        {
            _cachedMainCamera = null;
            int frame = Time.frameCount;
            if (frame != _cachedFrame)
            {
                _cachedMainCamera = Camera.main;
                _cachedFrame = frame;
            }
        }
        return _cachedMainCamera;
    }
}
