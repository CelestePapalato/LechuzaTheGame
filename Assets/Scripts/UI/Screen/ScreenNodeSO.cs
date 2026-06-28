using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EcosDeLaMazmorra.UI
{
    [CreateAssetMenu(fileName = "Screen Node SO", menuName = "UI/Screen Node")]
    public class ScreenNodeSO : ScriptableObject
    {
        public string nodeId;
        public List<ScreenNodeSO> transitionsTo;
        public bool enableWindowPopping = true;
        public bool enableScenePopping  = true;
        public bool stopGame            = false;

        [Header("Prefab to instantiate if needed")]
        public GameObject nodePrefab;

        private ScreenNode instance;

        public ScreenNode GetNode(Transform parent = null)
        {
            if (instance != null) return instance;
            if (nodePrefab == null) return null;

            instance = GameObject.Instantiate(nodePrefab, parent).GetComponentInChildren<ScreenNode>(true);
            if (instance != null) instance.nodeData = this;
            return instance;
        }

        public ScreenNodeSO GetTransition(string targetId)
        {
            var target = transitionsTo?.FirstOrDefault(n => n != null && n.nodeId == targetId);
            if (target == null)
                Debug.LogWarning($"[ScreenNodeSO] Invalid transition from '{nodeId}' to '{targetId}'");
            return target;
        }
    }
}
