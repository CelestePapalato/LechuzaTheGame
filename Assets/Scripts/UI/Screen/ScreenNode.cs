using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lechuza.UI
{
    public class ScreenNode : MonoBehaviour, IWindowHost
    {
        public ScreenNodeSO nodeData;
        public string NodeId => nodeData != null ? nodeData.nodeId : "";

        [SerializeField] protected WindowBase firstActiveWindow;

        private Canvas canvas;
        private GraphicRaycaster graphicRaycaster;

        protected FlowStack<WindowBase> windowStack = new();
        protected WindowBase currentActiveWindow;
        protected bool poppedByController = false;

        protected bool isScenePoppingEnabled  = true;
        protected bool isWindowPoppingEnabled = true;
        protected bool isActive = false;

        public event Action<ScreenNodeSO> OpenRequested;

        public UnityEvent OnOpen;
        public UnityEvent OnClose;
        public UnityEvent OnShow;
        public UnityEvent OnHide;

        protected virtual void Awake()
        {
            canvas = GetComponent<Canvas>();
            graphicRaycaster = GetComponent<GraphicRaycaster>();

            var windowProviders = GetComponentsInChildren<IWindowProvider>(includeInactive: true);
            foreach (var provider in windowProviders)
                provider.windowHost = this;
        }

        protected virtual void OnDestroy() { }

        public void GoTo(string nodeId)
        {
            var target = nodeData?.GetTransition(nodeId);
            if (target != null)
                OpenRequested?.Invoke(target);
        }

        public void GoTo(ScreenNodeSO target) => OpenRequested?.Invoke(target);

        public virtual void Open()
        {
            OnOpen?.Invoke();
            OnOpenStart();
            OnOpenEnd();
        }

        public virtual void Close()
        {
            OnCloseComplete();
            OnClose?.Invoke();
        }

        public virtual void Hide()
        {
            OnHide?.Invoke();
            EnableButtonInteraction(false);
        }

        public virtual void Show()
        {
            OnShow?.Invoke();
            EnableButtonInteraction(true);
            OnOpenStart();
        }

        public void SetSortOrder(int n = 0)
        {
            canvas.sortingOrder = n;
        }

        public virtual void EnableButtonInteraction(bool enable = true)
        {
            isActive = enable;
            if (graphicRaycaster)
                graphicRaycaster.enabled = enable;
            currentActiveWindow?.SetWindowActiveState(enable);
            if (enable)
                StartCoroutine(SetFirstSelectedButton(currentActiveWindow?.FirstButton()));
        }

        protected virtual void OnOpenStart()
        {
            isActive = true;
            gameObject.SetActive(true);

            if (canvas) canvas.enabled = true;
            if (graphicRaycaster) graphicRaycaster.enabled = true;

            poppedByController = false;
            isScenePoppingEnabled = nodeData.enableScenePopping;
            isWindowPoppingEnabled = nodeData.enableWindowPopping;

            Time.timeScale = nodeData.stopGame ? 0 : 1;

            if (firstActiveWindow != null)
                PushWindow(firstActiveWindow);

            OnOpenEnd(); // se tendría que ejecutar al final de una animación
        }

        protected virtual void OnOpenEnd()
        {
            currentActiveWindow?.SetWindowActiveState(true);
        }

        protected virtual void OnSceneClosing() { }

        protected virtual void OnCloseComplete()
        {
            OnSceneClosing();

            canvas.enabled = false;
            isActive = false;
            if (graphicRaycaster != null)
                graphicRaycaster.enabled = false;

            poppedByController = true;
            Time.timeScale = 1;
            PopAllWindows();
            StopAllCoroutines();

            gameObject.SetActive(false);
        }

        public void PushWindow(WindowBase window)
        {
            if (window == null) return;

            if (currentActiveWindow == window)
            {
                StartCoroutine(SetFirstSelectedButton(window.FirstButton()));
                return;
            }

            OnWindowPush();
            CleanLoopFromHistory(window);
            windowStack.Push(window);
            SetNewActiveWindow(window);
        }

        public void PopWindow()
        {
            if (!isWindowPoppingEnabled) return;
            OnWindowPop();

            if (windowStack.Count > 1)
            {
                windowStack.Pop();
                SetNewActiveWindow(windowStack.Peek());
            }
            else
            {
                if (!isScenePoppingEnabled) return;
                windowStack.Pop();
                currentActiveWindow?.SetWindowActiveState(false);
                currentActiveWindow = null;
                Close();
            }
        }

        protected virtual void OnWindowPop()  { }
        protected virtual void OnWindowPush() { }

        public void PopAllWindows()
        {
            if (!isWindowPoppingEnabled) return;
            while (windowStack.Count > 0)
                PopWindow();
        }

        public void ClearWindowStack()
        {
            if (!isWindowPoppingEnabled) return;

            bool wasPoppedByController = poppedByController;
            poppedByController = true;

            while (windowStack.Count > 0)
                PopWindow();

            poppedByController = wasPoppedByController;
        }

        public void PopScene()
        {
            if (!isScenePoppingEnabled) return;
            poppedByController = false;
            isWindowPoppingEnabled = true;
            PopAllWindows();
        }

        private void CleanLoopFromHistory(WindowBase window)
        {
            if (!windowStack.Contains(window)) return;
            while (windowStack.Count > 0 && windowStack.Peek() != window)
                PopWindow();
        }

        protected void SetNewActiveWindow(WindowBase window)
        {
            if (window == null)
            {
                currentActiveWindow?.SetWindowActiveState(false);
                currentActiveWindow = null;
                return;
            }

            currentActiveWindow?.SetWindowActiveState(false);
            currentActiveWindow = window;
            currentActiveWindow.gameObject.SetActive(true);
            currentActiveWindow.SetWindowActiveState(isActive);

            var firstSelected = currentActiveWindow.FirstButton();
            if (firstSelected && gameObject.activeInHierarchy)
                StartCoroutine(SetFirstSelectedButton(firstSelected));
        }

        protected IEnumerator SetFirstSelectedButton(GameObject button)
        {
            if (!isActive) yield break;
            EventSystem.current.SetSelectedGameObject(null);
            if (button == null) yield break;
            yield return null;
            if (button.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(button);
        }

        protected IEnumerator DoNextFrame(Action action)
        {
            yield return null;
            action?.Invoke();
        }
    }
}
