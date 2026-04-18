using UnityEngine;

public class BindableIconBar : IconBar
{
    [SerializeField] private GameBindableKey sourceKey;

    private IntRangeBinding _source;

    private void OnEnable()
    {
        BindToSource();
    }

    private void OnDisable()
    {
        UnbindFromSource();
    }

    private void BindToSource()
    {
        if (!isActiveAndEnabled) return;
        UnbindFromSource();
        _source = GameBindableRegistry.Resolve<IntRangeBinding>(sourceKey);
        if (_source == null) return;
        _source.ValuesChanged += OnSourceValuesChanged;
        ApplyBarState(_source.Current, _source.Max);
    }

    private void UnbindFromSource()
    {
        if (_source == null) return;
        _source.ValuesChanged -= OnSourceValuesChanged;
        _source = null;
    }

    private void OnSourceValuesChanged(int current, int max)
    {
        ApplyBarState(current, max);
    }
}
