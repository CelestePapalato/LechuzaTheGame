using System.Collections.Generic;
using UnityEngine;

namespace EcosDeLaMazmorra.UI
{
    public class ScreenManager : MonoBehaviour
    {
        public static ScreenManager Instance { get; private set; }
        public string CurrentNodeId => currentNodeData?.nodeId;

        [SerializeField] ScreenNodeSO startNode;

        private Dictionary<string, ScreenNode> nodeInstances = new();
        private ScreenNodeSO currentNodeData;
        private ScreenNode currentNode;
        private FlowStack<ScreenNodeSO> history;

        private bool managementActive = true;
        private int minimumHistorySize = 1;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Init(startNode);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Init(ScreenNodeSO root)
        {
            history = new FlowStack<ScreenNodeSO>(OnRemovedLoop);

            if (root == null)
            {
                Debug.LogError("[ScreenManager] startNode not assigned.");
                minimumHistorySize = 0;
                return;
            }

            currentNodeData = root;

            if (nodeInstances.TryGetValue(root.nodeId, out var existing))
                currentNode = existing;
            else
            {
                currentNode = root.GetNode(transform);
                nodeInstances[root.nodeId] = currentNode;
            }

            minimumHistorySize = 1;
            history.Push(currentNodeData);
            currentNode?.SetSortOrder(history.Count - 1);
            currentNode?.Open();
        }

        public static bool GoTo(string nodeId) =>
            Instance != null && Instance.GoToInternal(nodeId);

        public static bool GoTo(ScreenNodeSO targetData) =>
            Instance != null && Instance.GoToInternal(targetData);

        private bool GoToInternal(string nodeId)
        {
            if (!managementActive) return false;

            ScreenNodeSO targetData = currentNodeData.GetTransition(nodeId);
            if (targetData == null) return false;

            return GoToInternal(targetData);
        }

        private bool GoToInternal(ScreenNodeSO targetData)
        {
            if (!managementActive || targetData == null) return false;

            bool firstScreenPush = history.Count == minimumHistorySize;

            currentNode?.Hide();

            if (nodeInstances.TryGetValue(targetData.nodeId, out var existingNode))
                currentNode = existingNode;
            else
            {
                currentNode = targetData.GetNode(transform);
                nodeInstances[targetData.nodeId] = currentNode;
            }

            history.Push(targetData);
            currentNodeData = targetData;

            currentNode?.Open();
            currentNode?.SetSortOrder(history.Count - 1);

            if (firstScreenPush && targetData != startNode)
                OnFirstScreenPush();

            if (targetData == startNode)
                OnAllScreensPopped();

            return true;
        }

        public void GoBack()
        {
            if (history.Count == minimumHistorySize) return;

            currentNode?.Close();
            history.Pop();
            currentNodeData = history.Peek();
            currentNode = nodeInstances[currentNodeData.nodeId];
            currentNode?.Show();

            if (history.Count == minimumHistorySize)
                OnAllScreensPopped();
        }

        public void PopAll()
        {
            while (history.Count > minimumHistorySize)
            {
                currentNode?.Close();
                history.Pop();
                currentNodeData = history.Peek();
                currentNode = nodeInstances[currentNodeData.nodeId];
            }
            currentNode?.Show();
            OnAllScreensPopped();
        }

        public void PopWindow() => currentNode?.PopWindow();
        public void PushWindow(WindowBase newWindow) => currentNode?.PushWindow(newWindow);

        protected void OnFirstScreenPush()
        {
            //GameStateEvents.BroadcastStateChange(GameState.UI_SCENE);
        }
        protected virtual void OnAllScreensPopped()
        {
            //GameStateEvents.BroadcastStateChange(GameState.PLAYING);
        }

        private void OnRemovedLoop(List<ScreenNodeSO> nodes)
        {
            foreach (var data in nodes)
                if (nodeInstances.TryGetValue(data.nodeId, out var node))
                    node.Close();
        }

        [ContextMenu("Print history stack")]
        public void PrintStack() => history?.PrintStack();
    }
}
