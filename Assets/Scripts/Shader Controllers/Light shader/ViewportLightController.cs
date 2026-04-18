using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ViewportLightController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Transform targetObject;
    [SerializeField] private Camera viewportCamera;
    [SerializeField] private Vector2 offset;
    
    [Header("Shader Properties")]
    [SerializeField] private string originXProperty = "_OriginX";
    [SerializeField] private string originYProperty = "_OriginY";
    
    private Material materialInstance;
    
    void Start()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
            
        if (viewportCamera == null)
            viewportCamera = Camera.main;
            
        if (targetObject == null)
            targetObject = transform;

        SetupMaterial();
    }
    
    void OnEnable()
    {
        SetupMaterial();
#if UNITY_EDITOR
        EditorApplication.update += UpdateShaderOrigin;
#endif
    }
    
    void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= UpdateShaderOrigin;
#endif
    }
    
    void Update()
    {
        if (targetImage != null && materialInstance != null && targetObject != null && viewportCamera != null)
        {
            UpdateShaderOrigin();
        }
    }
    
    private void SetupMaterial()
    {
        if (targetImage == null) return;
        materialInstance = targetImage.material;
    }
    
    private void UpdateShaderOrigin()
    {
        if (targetImage == null || materialInstance == null || targetObject == null || viewportCamera == null)
            return;
            
        Vector2 viewportPos = viewportCamera.WorldToViewportPoint(targetObject.position);

        viewportPos += offset;
        viewportPos = new Vector2(viewportPos.x, viewportPos.y);
        
        if (materialInstance.HasProperty(originXProperty))
            materialInstance.SetFloat(originXProperty, viewportPos.x);

        if (materialInstance.HasProperty(originYProperty))
            materialInstance.SetFloat(originYProperty, viewportPos.y);
    }
}
