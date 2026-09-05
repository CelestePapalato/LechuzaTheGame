using UnityEngine;

public class BirdMovement : MonoBehaviour, IEnemyMovement
{
    [Header("Smoothing")]
    [SerializeField]
    private float smoothTime = 0.4f;

    [Header("Vertical Bob")]
    [SerializeField]
    private float verticalAmplitude = 0.4f;
    [SerializeField]
    private float verticalFrequency = 1f;

    private Vector2 destination;
    private Vector2 dampVelocity;
    private float maxSpeed;
    private bool isActive;

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
            destination + GetVerticalOffset(),
            ref dampVelocity,
            smoothTime,
            maxSpeed);
    }

    private Vector2 GetVerticalOffset()
    {
        return new Vector2(0f, Mathf.Sin(Time.time * verticalFrequency) * verticalAmplitude);
    }
}
