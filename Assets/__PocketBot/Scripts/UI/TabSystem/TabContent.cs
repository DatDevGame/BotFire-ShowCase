using System;
using UnityEngine;

namespace LatteGames.Tab
{
    public class TabContent : MonoBehaviour
    {
        [SerializeField] CanvasGroupVisibility canvasGroupVisibility;
        public Action<bool> OnSetActive;
        public bool isActive;

        public virtual void SetActive(bool isActive)
        {
            this.isActive = isActive;
            if (isActive)
            {
                canvasGroupVisibility.ShowImmediately();
            }
            else
            {
                canvasGroupVisibility.HideImmediately();
            }
            OnSetActive?.Invoke(isActive);
        }
    }
}