using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using HyrphusQ.Helpers;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CheatDeleteData : MonoBehaviour
{
    [SerializeField]
    private Button m_Button;

    private void Awake()
    {
        m_Button.onClick.AddListener(() => {
            var sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                var rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (var rootGameObject in rootGameObjects)
                {
                    if (rootGameObject == EventSystem.current?.gameObject)
                        continue;
                    DestroyImmediate(rootGameObject);
                }
            }
            var rootGameObjectsDontDestroyOnLoad = gameObject.scene.GetRootGameObjects();
            foreach (var rootGameObject in rootGameObjectsDontDestroyOnLoad)
            {
                if (rootGameObject == gameObject)
                    continue;
                DestroyImmediate(rootGameObject);
            }
            var dirInfo = new DirectoryInfo(Application.persistentDataPath);
            var files = dirInfo.GetFiles();
            foreach (var file in files)
            {
                File.Delete(file.FullName);
            }
            PlayerPrefs.DeleteAll();
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
            var typeName = typeof(SavedDataSO<>).Name;
            var guids = AssetDatabase.FindAssets($"t:{typeName}");
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var savableSO = AssetDatabase.LoadAssetAtPath<SavedDataSO>(assetPath);
                if (savableSO != null)
                {
                    savableSO.Delete();
                }
            }
#else
            Application.Quit();
#endif
        });
    }
}