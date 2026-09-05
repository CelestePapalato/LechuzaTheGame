using UnityEngine;

public class LightSource : MonoBehaviour, ILightSource
{
    [SerializeField]
    [Tooltip("Radio de la máscara de luz en espacio viewport (0–1). ")]
    private float radius = 0.1f;

    public Vector3 WorldPosition => transform.position;
    public float   Radius => radius;
    public bool    IsActive => isActiveAndEnabled;

    private void OnEnable() => LightShaderController.Register(this);
    private void OnDisable() => LightShaderController.Unregister(this);
}
