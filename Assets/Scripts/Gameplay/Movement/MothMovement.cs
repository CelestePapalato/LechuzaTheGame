using UnityEngine;

public class MothMovement : MonoBehaviour
{
    [Header("Smoothing")]
    [SerializeField]
    private float smoothTime = 0.4f;

    [Header("Noise")]
    [SerializeField]
    private float noiseAmplitude = 0.3f;
    [SerializeField]
    private float noiseFrequency = 1.5f;
    [SerializeField]
    private float noiseSeed;

    private Vector2 destination;
    private Vector2 dampVelocity;
    private float maxSpeed;
    private bool isActive;

    private void Awake()
    {
        if (noiseSeed == 0f)
            noiseSeed = Random.Range(0f, 1000f);
    }

    public void SetDestination(Vector2 destination, float speed)
    {
        isActive = true;
        this.destination = destination;
        maxSpeed = speed;
    }

    public void Stop()
    {
        isActive = false;
        dampVelocity = Vector2.zero;
    }

    private void Update()
    {
        if (!isActive) return;

        transform.position = Vector2.SmoothDamp(
            transform.position,
            destination + GetNoiseOffset(),
            ref dampVelocity,
            smoothTime,
            maxSpeed);
    }

    private Vector2 GetNoiseOffset()
    {
        float t = Time.time * noiseFrequency;
        // Perlin Noise genera un valor en [0, 1]
        // multiplicamos por 2 y restamos 1 para obtener un valor en [-1, 1].
        float offsetX = (Mathf.PerlinNoise(t, noiseSeed) * 2f - 1f) * noiseAmplitude;
        float offsetY = (Mathf.PerlinNoise(t, noiseSeed + 17.3f) * 2f - 1f) * noiseAmplitude;
        return new Vector2(offsetX, offsetY);
    }
}
