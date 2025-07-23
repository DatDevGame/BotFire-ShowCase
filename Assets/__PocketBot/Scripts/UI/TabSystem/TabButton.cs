using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LatteGames.Tab
{
    public class TabButton : MonoBehaviour
    {
        protected Button button;

        public virtual void Initialize(Action onClickAction)
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(() => onClickAction.Invoke());
        }

        public virtual void SetState(bool isActive)
        {

        }
    }
}
