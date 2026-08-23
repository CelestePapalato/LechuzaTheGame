using System.Collections.Generic;
using UnityEngine;

public class Updater : MonoBehaviour
{
    private readonly List<IUpdatable> _updatables = new();
    private readonly List<IFixedUpdatable> _fixedUpdatables = new();
    private readonly List<ILateUpdatable> _lateUpdatables = new();

    private readonly List<IUpdatable> _pendingAddUpdatables = new();
    private readonly List<IFixedUpdatable> _pendingAddFixed = new();
    private readonly List<ILateUpdatable> _pendingAddLate = new();

    private readonly List<IUpdatable> _pendingRemoveUpdatables = new();
    private readonly List<IFixedUpdatable> _pendingRemoveFixed = new();
    private readonly List<ILateUpdatable> _pendingRemoveLate = new();

    private bool _isUpdating;
    private bool _isFixedUpdating;
    private bool _isLateUpdating;

    public void Register(object target)
    {
        if (target == null)
            return;

        if (target is IUpdatable updatable)
            Add(_updatables, _pendingAddUpdatables, _pendingRemoveUpdatables, updatable, _isUpdating);

        if (target is IFixedUpdatable fixedUpdatable)
            Add(_fixedUpdatables, _pendingAddFixed, _pendingRemoveFixed, fixedUpdatable, _isFixedUpdating);

        if (target is ILateUpdatable lateUpdatable)
            Add(_lateUpdatables, _pendingAddLate, _pendingRemoveLate, lateUpdatable, _isLateUpdating);
    }

    public void Unregister(object target)
    {
        if (target == null)
            return;

        if (target is IUpdatable updatable)
            Remove(_updatables, _pendingAddUpdatables, _pendingRemoveUpdatables, updatable, _isUpdating);

        if (target is IFixedUpdatable fixedUpdatable)
            Remove(_fixedUpdatables, _pendingAddFixed, _pendingRemoveFixed, fixedUpdatable, _isFixedUpdating);

        if (target is ILateUpdatable lateUpdatable)
            Remove(_lateUpdatables, _pendingAddLate, _pendingRemoveLate, lateUpdatable, _isLateUpdating);
    }

    private void Update()
    {
        ApplyPending(_updatables, _pendingAddUpdatables, _pendingRemoveUpdatables);
        _isUpdating = true;

        for (int i = 0; i < _updatables.Count; i++)
        {
            IUpdatable item = _updatables[i];
            if (item == null)
                continue;

            item.Update();
        }

        _isUpdating = false;
        ApplyPending(_updatables, _pendingAddUpdatables, _pendingRemoveUpdatables);
        PruneNulls(_updatables);
    }

    private void FixedUpdate()
    {
        ApplyPending(_fixedUpdatables, _pendingAddFixed, _pendingRemoveFixed);
        _isFixedUpdating = true;

        for (int i = 0; i < _fixedUpdatables.Count; i++)
        {
            IFixedUpdatable item = _fixedUpdatables[i];
            if (item == null)
                continue;

            item.FixedUpdate();
        }

        _isFixedUpdating = false;
        ApplyPending(_fixedUpdatables, _pendingAddFixed, _pendingRemoveFixed);
        PruneNulls(_fixedUpdatables);
    }

    private void LateUpdate()
    {
        ApplyPending(_lateUpdatables, _pendingAddLate, _pendingRemoveLate);
        _isLateUpdating = true;

        for (int i = 0; i < _lateUpdatables.Count; i++)
        {
            ILateUpdatable item = _lateUpdatables[i];
            if (item == null)
                continue;

            item.LateUpdate();
        }

        _isLateUpdating = false;
        ApplyPending(_lateUpdatables, _pendingAddLate, _pendingRemoveLate);
        PruneNulls(_lateUpdatables);
    }

    private static void Add<T>(List<T> list, List<T> pendingAdd, List<T> pendingRemove, T item, bool isIterating)
        where T : class
    {
        pendingRemove.Remove(item);

        if (list.Contains(item) || pendingAdd.Contains(item))
            return;

        if (isIterating)
            pendingAdd.Add(item);
        else
            list.Add(item);
    }

    private static void Remove<T>(List<T> list, List<T> pendingAdd, List<T> pendingRemove, T item, bool isIterating)
        where T : class
    {
        pendingAdd.Remove(item);

        if (isIterating)
        {
            if (!pendingRemove.Contains(item))
                pendingRemove.Add(item);
            return;
        }

        list.Remove(item);
    }

    private static void ApplyPending<T>(List<T> list, List<T> pendingAdd, List<T> pendingRemove)
        where T : class
    {
        if (pendingRemove.Count > 0)
        {
            for (int i = 0; i < pendingRemove.Count; i++)
                list.Remove(pendingRemove[i]);

            pendingRemove.Clear();
        }

        if (pendingAdd.Count > 0)
        {
            for (int i = 0; i < pendingAdd.Count; i++)
            {
                T item = pendingAdd[i];
                if (!list.Contains(item))
                    list.Add(item);
            }

            pendingAdd.Clear();
        }
    }

    private static void PruneNulls<T>(List<T> list) where T : class
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] == null)
                list.RemoveAt(i);
        }
    }
}
