using UnityEngine;

public class IconBar : MonoBehaviour
{
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private Transform poolParent;
    [SerializeField, Min(0)] private int poolCapacity = 5;

    private GameObject[] _instances;
    private IIcon[] _icons;

    protected virtual void Awake()
    {
        BuildPool();
    }

    protected void BuildPool()
    {
        int count = Mathf.Max(0, poolCapacity);
        _instances = new GameObject[count];
        _icons = new IIcon[count];

        if (count == 0 || !iconPrefab)
            return;

        Transform parent = poolParent ? poolParent : transform;

        for (int i = 0; i < count; i++)
        {
            GameObject instance = Instantiate(iconPrefab, parent);
            instance.name = $"{iconPrefab.name}_{i}";
            _instances[i] = instance;
            _icons[i] = FindIcon(instance);
        }
    }

    private static IIcon FindIcon(GameObject root)
    {
        foreach (MonoBehaviour mb in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb is IIcon icon)
                return icon;
        }
        return null;
    }

    public void ApplyBarState(int current, int max)
    {
        if (_instances == null || _icons == null) return;

        int effectiveMax = Mathf.Min(Mathf.Max(0, max), _instances.Length);
        int effectiveCurrent = Mathf.Clamp(current, 0, effectiveMax);

        for (int i = 0; i < _instances.Length; i++)
        {
            GameObject go = _instances[i];
            if (!go) continue;

            bool slotUsed = i < effectiveMax;
            go.SetActive(slotUsed);
            if (!slotUsed) continue;

            if (_icons[i] != null)
                _icons[i].IsOn = i < effectiveCurrent;
        }
    }
}
