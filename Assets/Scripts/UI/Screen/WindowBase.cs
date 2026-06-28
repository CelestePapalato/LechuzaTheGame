using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace EcosDeLaMazmorra.UI
{
    public class WindowBase : MonoBehaviour
    {
        protected bool active = false;

        [SerializeField] bool updateStateOnEnable  = true;
        [SerializeField] bool updateStateOnDisable = true;
        [SerializeField] GameObject firstButton;
        [SerializeField] bool rememberLastSelectedButton = true;

        private GameObject lastSelectedButton;

        public UnityAction<bool> windowActiveStatusChanged;
        public UnityEvent OnWindowActivated;
        public UnityEvent OnWindowDeactivated;

        protected virtual void OnEnable()
        {
            if (updateStateOnEnable)
                SetWindowActiveState(true);
        }

        protected virtual void OnDisable()
        {
            if (updateStateOnDisable)
                SetWindowActiveState(false);
            lastSelectedButton = null;
        }

        public virtual void SetWindowActiveState(bool value)
        {
            active = value;
            windowActiveStatusChanged?.Invoke(value);
            SetInteractableAllButtons(value);
            if (active) OnWindowActivated?.Invoke();
            else        OnWindowDeactivated?.Invoke();
        }

        private void LateUpdate()
        {
            if (active && rememberLastSelectedButton && EventSystem.current != null)
            {
                GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
                if (currentSelected != lastSelectedButton)
                {
                    if (currentSelected != null && currentSelected.activeInHierarchy)
                        if (currentSelected.transform.IsChildOf(transform))
                            lastSelectedButton = currentSelected;
                }
            }
        }

        public bool isActive() => active;

        public virtual GameObject FirstButton()
        {
            if (rememberLastSelectedButton && lastSelectedButton != null && lastSelectedButton.activeInHierarchy)
                return lastSelectedButton;
            return firstButton;
        }

        protected void SetInteractableAllButtons(bool value)
        {
            var buttons = GetComponentsInChildren<UnityEngine.UI.Selectable>(false);
            foreach (var button in buttons)
                button.interactable = value;
        }

        protected void SetInteractableAllButtons(bool value, Transform parent)
        {
            var buttons = parent.GetComponentsInChildren<UnityEngine.UI.Selectable>(false);
            foreach (var button in buttons)
                button.interactable = value;
        }
    }
}
