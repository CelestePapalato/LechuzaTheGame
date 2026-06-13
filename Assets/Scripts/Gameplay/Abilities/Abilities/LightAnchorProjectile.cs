using System;
using UnityEngine;

public class LightAnchorProjectile : MonoBehaviour
{
    [SerializeField] private float speed    = 14f;
    [SerializeField] private float lifetime = 2.5f;

    public event Action OnExpired;

    private Vector2 direction;
    private float   timeAlive;

    public void Initialize(Vector2 launchDirection)
    {
        direction = launchDirection.normalized;
    }

    public Vector2 Pull()
    {
        Vector2 pos = (Vector2)transform.position;
        Destroy(gameObject);
        return pos;
    }

    private void Update()
    {
        // Luego modificar para que rebote y eso
        // Si no, el jugador puede quedar atrapado
        // Es decir, no usar transform
        transform.Translate(speed * Time.deltaTime * direction, Space.World);

        timeAlive += Time.deltaTime;
        if (timeAlive >= lifetime)
            Expire();
    }

    private void Expire()
    {
        OnExpired?.Invoke();
        Destroy(gameObject);
    }
}
