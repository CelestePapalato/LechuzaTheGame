using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Image))]
public class GlowShaderController : MonoBehaviour
{
    [SerializeField] private Image targetImage;

    public string glowRadiusProperty = "_GlowRadius";
    public string glowStrengthProperty = "_GlowStrength";
    public string glowColorProperty = "_GlowColor";


    public float glowRadius = 1.35f;
    public float glowStrength = 1.78f;
    public Color glowColor = new Color(1f, 1f, 0f, 1f);

    private Material materialInstance;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        SetupMaterial();
    }

    private void OnValidate()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (materialInstance == null && targetImage != null)
            SetupMaterial();

        UpdateShaderParameters();
    }

    private void Update()
    {
        UpdateShaderParameters();
    }

    private void SetupMaterial()
    {
        if (targetImage == null) return;

        if (targetImage.material != null)
        {
            materialInstance = new Material(targetImage.material);
            targetImage.material = materialInstance;
        }
    }

    private void UpdateShaderParameters()
    {
        if (materialInstance == null) return;

        if (!string.IsNullOrEmpty(glowRadiusProperty) && materialInstance.HasProperty(glowRadiusProperty))
            materialInstance.SetFloat(glowRadiusProperty, glowRadius);

        if (!string.IsNullOrEmpty(glowStrengthProperty) && materialInstance.HasProperty(glowStrengthProperty))
            materialInstance.SetFloat(glowStrengthProperty, glowStrength);

        if (!string.IsNullOrEmpty(glowColorProperty) && materialInstance.HasProperty(glowColorProperty))
            materialInstance.SetColor(glowColorProperty, glowColor);
    }

    public void SetGlowRadius(float value)
    {
        glowRadius = value;
        UpdateShaderParameters();
    }

    public void SetGlowStrength(float value)
    {
        glowStrength = value;
        UpdateShaderParameters();
    }

    public void SetGlowColor(Color value)
    {
        glowColor = value;
        UpdateShaderParameters();
    }

    public float GetGlowRadius() => glowRadius;
    public float GetGlowStrength() => glowStrength;
    public Color GetGlowColor() => glowColor;
}
