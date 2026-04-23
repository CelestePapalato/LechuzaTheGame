using UnityEngine;
using UnityEngine.UI;

public class LightShaderController : MonoBehaviour
{
    [SerializeField]
    float lowestFalloffValue = 0.1f;
    [SerializeField]
    float highestFalloffValue = 0.5f;
    [SerializeField] float smoothSpeed = 5f;

    public string falloffProperty = "_Falloff";

    Image lighTimageComponent;
    Material radialLightshader;

    float currentFalloff;
    float targetFalloff;

    private void Awake()
    {
        lighTimageComponent = GetComponent<Image>();
        radialLightshader = new Material(lighTimageComponent.materialForRendering);
        lighTimageComponent.material = radialLightshader;
        currentFalloff = lowestFalloffValue;
        targetFalloff = currentFalloff;
    }

    private void OnEnable()
    {
        PlayerState.OnLightChange += OnLightChanged;
    }

    private void OnDisable()
    {
        PlayerState.OnLightChange -= OnLightChanged;
    }

    private void Update()
    {
        currentFalloff = Mathf.Lerp(
            currentFalloff,
            targetFalloff,
            Time.deltaTime * smoothSpeed
        );

        radialLightshader.SetFloat(falloffProperty, currentFalloff);
    }

    private void OnLightChanged(int current, int max)
    {
        targetFalloff = Mathf.Lerp(
            lowestFalloffValue,
            highestFalloffValue,
            (float)current / max
        );
    }
}
