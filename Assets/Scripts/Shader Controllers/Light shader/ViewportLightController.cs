using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ViewportLightController : MonoBehaviour
{
    private const int MaxLights= 16;
    private const string LightCountProperty = "_LightCount";
    private const string LightPositionsProperty = "_LightPositions";

    [Header("Gradient Origin (main player light)")]
    [SerializeField] private Image     targetImage;
    [SerializeField] private Transform targetObject;
    [SerializeField] private Camera    viewportCamera;
    [SerializeField] private Vector2   offset;

    [Header("Shader Properties")]
    [SerializeField] private string originXProperty = "_OriginX";
    [SerializeField] private string originYProperty = "_OriginY";

    [Header("Light Source Proximity")]
    [SerializeField] private float proximityMargin = 0.25f;

    private Material materialInstance;
    private Vector4[] lightPositionsBuffer = new Vector4[MaxLights];

    private static readonly List<ILightSource> registeredSources = new();

    public static void Register(ILightSource source)
    {
        if (!registeredSources.Contains(source))
            registeredSources.Add(source);
    }

    public static void Unregister(ILightSource source)
    {
        registeredSources.Remove(source);
    }

    private void Start()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
        if (viewportCamera == null) viewportCamera = Camera.main;
        if (targetObject == null) targetObject = transform;

        SetupMaterial();
    }

    private void OnEnable()
    {
        SetupMaterial();
#if UNITY_EDITOR
        EditorApplication.update += EditorUpdate;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= EditorUpdate;
#endif
    }

    private void Update()
    {
        if (materialInstance == null || viewportCamera == null) return;
        UpdateShaderOrigin();
        UpdateShaderLights();
    }

#if UNITY_EDITOR
    private void EditorUpdate()
    {
        if (materialInstance == null || viewportCamera == null) return;
        UpdateShaderOrigin();
        UpdateShaderLights();
    }
#endif

    private void SetupMaterial()
    {
        if (targetImage == null) return;
        materialInstance = targetImage.material;
    }

    private void UpdateShaderOrigin()
    {
        if (targetObject == null) return;

        Vector2 vp = viewportCamera.WorldToViewportPoint(targetObject.position);
        vp += offset;

        if (materialInstance.HasProperty(originXProperty))
            materialInstance.SetFloat(originXProperty, vp.x);

        if (materialInstance.HasProperty(originYProperty))
            materialInstance.SetFloat(originYProperty, vp.y);
    }

    private void UpdateShaderLights()
    {
        int count = 0;

        foreach (ILightSource source in registeredSources)
        {
            if (count >= MaxLights) break;
            if (!source.IsActive)  continue;
            if (!IsNearScreen(source)) continue;

            Vector3 vp = viewportCamera.WorldToViewportPoint(source.WorldPosition);
            lightPositionsBuffer[count] = new Vector4(vp.x, vp.y, source.Radius, 0f);
            count++;
        }

        // Limpiar entradas sobrantes del frame anterior
        for (int i = count; i < MaxLights; i++)
            lightPositionsBuffer[i] = Vector4.zero;

        materialInstance.SetInt(LightCountProperty, count);
        materialInstance.SetVectorArray(LightPositionsProperty, lightPositionsBuffer);
    }

    // <summary>
    // Una fuente es "cercana a la pantalla" si su proyección en viewport cae dentro
    // de los bordes extendidos por "proximityMargin"
    private bool IsNearScreen(ILightSource source)
    {
        Vector3 vp = viewportCamera.WorldToViewportPoint(source.WorldPosition);

        float lo = -proximityMargin;
        float hi =  1f + proximityMargin;

        return vp.x >= lo && vp.x <= hi
            && vp.y >= lo && vp.y <= hi;
    }
}
