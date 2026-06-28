using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventOnTrigger : MonoBehaviour
{
    public UnityEvent TriggerEnter;
    public UnityEvent TriggerExit;
    public UnityEvent SomeoneInside;
    public UnityEvent NoOneInside;

    List<Collider2D> inside = new List<Collider2D>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool noOne = inside.Count == 0;
        TriggerEnter?.Invoke();
        inside.Add(other);
        if(noOne) { SomeoneInside?.Invoke(); }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        TriggerExit?.Invoke();
        inside.Remove(other);
        if(inside.Count == 0) { NoOneInside?.Invoke(); }
    }

    private void OnDisable()
    {
        inside.Clear();
    }
}
