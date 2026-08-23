using System.Collections.Generic;
using UnityEngine;

namespace Lechuza.UI
{
    public class ScreenManager : MonoBehaviour
    {
        public string CurrentNodeId => currentNodeData?.nodeId;

        [SerializeField] ScreenNodeSO startNode;

        private Dictionary<string, ScreenNode> nodeInstances = new();
        private ScreenNodeSO currentNodeData;
        private ScreenNode currentNode;
        private FlowStack<ScreenNodeSO> history;

        private bool managementActive = true;
        private int minimumHistorySize = 1;
        private bool handlingNodeClosed;

        void Awake()
        {
            Init(startNode);
        }

        private void OnDestroy()
        {
            Unsubscribe(currentNode);
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
            Subscribe(currentNode);
            currentNode?.Open();
        }

        public bool GoTo(string nodeId)
        {
            if (!managementActive || currentNodeData == null) return false;

            ScreenNodeSO targetData = currentNodeData.GetTransition(nodeId);
            if (targetData == null) return false;

            return GoToInternal(targetData);
        }

        public bool GoTo(ScreenNodeSO targetData) => GoToInternal(targetData);

        private bool GoToInternal(ScreenNodeSO targetData)
        {
            if (!managementActive || targetData == null) return false;

            bool firstScreenPush = history.Count == minimumHistorySize;

            Unsubscribe(currentNode);
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

            Subscribe(currentNode);
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
        }

        public void PopAll()
        {
            Unsubscribe(currentNode);

            while (history.Count > minimumHistorySize)
            {
                handlingNodeClosed = true;
                currentNode?.Close();
                handlingNodeClosed = false;
                history.Pop();
                currentNodeData = history.Peek();
                currentNode = nodeInstances[currentNodeData.nodeId];
            }

            Subscribe(currentNode);
            currentNode?.Show();
            OnAllScreensPopped();
        }

        public void PopWindow() => currentNode?.PopWindow();
        public void PushWindow(WindowBase newWindow) => currentNode?.PushWindow(newWindow);

        private void Subscribe(ScreenNode node)
        {
            if (node == null) return;
            node.OpenRequested += HandleOpenRequested;
            node.OnClose.AddListener(HandleNodeClosed);
        }

        private void Unsubscribe(ScreenNode node)
        {
            if (node == null) return;
            node.OpenRequested -= HandleOpenRequested;
            node.OnClose.RemoveListener(HandleNodeClosed);
        }

        private void HandleOpenRequested(ScreenNodeSO target) => GoToInternal(target);

        private void HandleNodeClosed()
        {
            if (handlingNodeClosed) return;

            if (history.Count <= minimumHistorySize)
            {
                currentNode?.Open();
                return;
            }

            Unsubscribe(currentNode);
            history.Pop();
            currentNodeData = history.Peek();
            currentNode = nodeInstances[currentNodeData.nodeId];
            Subscribe(currentNode);
            currentNode?.Show();

            if (history.Count == minimumHistorySize)
                OnAllScreensPopped();
        }

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
            {
                if (!nodeInstances.TryGetValue(data.nodeId, out var node))
                    continue;

                handlingNodeClosed = true;
                node.Close();
                handlingNodeClosed = false;
            }
        }

        [ContextMenu("Print history stack")]
        public void PrintStack() => history?.PrintStack();
    }
}
