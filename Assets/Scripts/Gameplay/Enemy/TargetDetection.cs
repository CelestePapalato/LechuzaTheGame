using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TargetDetection : MonoBehaviour
{
    public UnityEvent<Transform[]> TargetUpdate;
    public UnityEvent<Transform> TargetFound;
    public UnityEvent<Transform> TargetLost;

    [SerializeField]
    private CircleCollider2D detectionCollider;

    List<Transform> targets = new List<Transform>();

    public Transform[] Targets => targets.ToArray();

    private void Awake()
    {
        if (detectionCollider == null)
            detectionCollider = GetComponent<CircleCollider2D>();
    }

    public void SetDetectionRadius(float radius)
    {
        if (detectionCollider == null) return;
        detectionCollider.radius = radius;
    }

    public void SetDetectionActive(bool active)
    {
        if (detectionCollider == null) return;

        if (!active)
            ClearTargets();

        detectionCollider.enabled = active;
    }

    public void ClearTargets()
    {
        if (targets.Count == 0) return;

        foreach (Transform t in targets.ToArray())
            TargetLost?.Invoke(t);

        targets.Clear();
        TargetUpdate?.Invoke(targets.ToArray());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (targets.Contains(other.transform)) return;

        targets.Add(other.transform);
        TargetFound?.Invoke(other.transform);
        TargetUpdate?.Invoke(targets.ToArray());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!targets.Remove(other.transform)) return;

        TargetLost?.Invoke(other.transform);
        TargetUpdate?.Invoke(targets.ToArray());
    }
}
