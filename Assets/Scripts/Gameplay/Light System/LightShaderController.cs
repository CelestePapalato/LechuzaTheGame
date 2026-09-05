using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class LightShaderController : MonoBehaviour
{
    private const int MaxLights = 16;
    private const string LightCountProperty = "_LightCount";
    private const string LightPositionsProperty = "_LightPositions";

    [Header("Falloff")]
    [SerializeField] float lowestFalloffValue = 0.1f;
    [SerializeField] float highestFalloffValue = 0.5f;
    [SerializeField] float smoothSpeed = 5f;

    public string falloffProperty = "_Falloff";

    [Header("Gradient Origin (main player light)")]

    [SerializeField]private Image targetImage;
    [SerializeField] private Transform targetObject;
    [SerializeField] private Camera viewportCamera;
    [SerializeField] private Vector2 offset;

    [Header("Shader Properties")]
    [SerializeField] private string originXProperty = "_OriginX";
    [SerializeField] private string originYProperty = "_OriginY";

    [Header("Light Source Proximity")]
    [SerializeField] private float proximityMargin = 0.25f;

    Material radialLightshader;
    float currentFalloff;
    float targetFalloff;

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

    private void Awake()
    {
        EnsureRefs();
        if (Application.isPlaying && targetImage != null)
        {
            radialLightshader = new Material(targetImage.materialForRendering);
            targetImage.material = radialLightshader;
            currentFalloff = lowestFalloffValue;
            targetFalloff = currentFalloff;
        }
        SetupMaterial();
    }

    private void Start()
    {
        EnsureRefs();
        SetupMaterial();
    }

    private void OnEnable()
    {
        PlayerState.OnLightChange += OnLightChanged;
        SetupMaterial();
#if UNITY_EDITOR
        EditorApplication.update += EditorUpdate;
#endif
    }

    private void OnDisable()
    {
        PlayerState.OnLightChange -= OnLightChanged;
#if UNITY_EDITOR
        EditorApplication.update -= EditorUpdate;
#endif
    }

    private void Update()
    {
        if (Application.isPlaying && radialLightshader != null)
        {
            currentFalloff = Mathf.Lerp(
                currentFalloff,
                targetFalloff,
                Time.deltaTime * smoothSpeed
            );

         radialLightshader.SetFloat(falloffProperty, currentFalloff);
        }

        if (radialLightshader == null || viewportCamera == null) return;
        UpdateShaderOrigin();
        UpdateShaderLights();
    }

    private void OnLightChanged(int current, int max)
    {
        targetFalloff = Mathf.Lerp(
            lowestFalloffValue,
            highestFalloffValue,
            (float)current / max
        );
    }

#if UNITY_EDITOR
    private void EditorUpdate()
    {
        if (radialLightshader == null || viewportCamera == null) return;
        UpdateShaderOrigin();
        UpdateShaderLights();
    }
#endif

    private void EnsureRefs()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
        if (viewportCamera == null) viewportCamera = Camera.main;
        if (targetObject == null) targetObject = transform;
    }
    private void SetupMaterial()
    {
        if (radialLightshader != null) return;
        if (targetImage == null) return;
        radialLightshader = targetImage.material;
    }

    private void UpdateShaderOrigin()
    {
        if (targetObject == null) return;
        Vector2 vp = viewportCamera.WorldToViewportPoint(targetObject.position);
        vp += offset;
        if (radialLightshader.HasProperty(originXProperty))
            radialLightshader.SetFloat(originXProperty, vp.x);
        if (radialLightshader.HasProperty(originYProperty))
            radialLightshader.SetFloat(originYProperty, vp.y);
    }

    private void UpdateShaderLights()
    {
        int count = 0;
        foreach (ILightSource source in registeredSources)
        {
            if (count >= MaxLights) break;
            if (!source.IsActive) continue;
            if (!IsNearScreen(source)) continue;
            Vector3 vp = viewportCamera.WorldToViewportPoint(source.WorldPosition);
            lightPositionsBuffer[count] = new Vector4(vp.x, vp.y, source.Radius, 0f);
            count++;
        }
        for (int i = count; i < MaxLights; i++)
            lightPositionsBuffer[i] = Vector4.zero;
        radialLightshader.SetInt(LightCountProperty, count);
        radialLightshader.SetVectorArray(LightPositionsProperty, lightPositionsBuffer);
    }
    private bool IsNearScreen(ILightSource source)
    {
        Vector3 vp = viewportCamera.WorldToViewportPoint(source.WorldPosition);
        float lo = -proximityMargin;
        float hi = 1f + proximityMargin;
        return vp.x >= lo && vp.x <= hi
            && vp.y >= lo && vp.y <= hi;
    }
}
