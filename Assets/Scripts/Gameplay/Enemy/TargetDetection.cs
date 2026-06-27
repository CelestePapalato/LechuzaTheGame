using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TargetDetection : MonoBehaviour
{
    public UnityEvent<Transform[]> TargetUpdate;
    public UnityEvent<Transform> TargetFound;
    public UnityEvent<Transform> TargetLost;

    List<Transform> targets = new List<Transform>();

    public Transform[] Targets { get => targets.ToArray(); }

    private void OnTriggerEnter(Collider other)
    {
        if(targets.Contains(other.transform)) { return; }
        targets.Add(other.transform);
        TargetFound?.Invoke(other.transform);
        TargetUpdate?.Invoke(targets.ToArray());
    }

    private void OnTriggerExit(Collider collision)
    {
        targets.Remove(collision.transform);
        TargetLost?.Invoke(collision.transform);
        TargetUpdate?.Invoke(targets.ToArray());
    }
}
