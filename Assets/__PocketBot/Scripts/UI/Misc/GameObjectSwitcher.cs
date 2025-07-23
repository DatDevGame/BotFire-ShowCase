using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectSwitcher : MonoBehaviour
{
    [SerializeField] List<GameObject> gameObjects;

    public void ChangeObject(int goIndex)
    {
        for (int i = 0; i < gameObjects.Count; i++)
            gameObjects[i].SetActive(i == goIndex);
    }
}
