using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RaycastCollision : MonoBehaviour
{
    [SerializeField] private Vector2 rayOriginOffset;

    [SerializeField] private float horizontalRayLength = 0.5f;
    [SerializeField] private float verticalRayLength = 0.5f;
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private int maxResolutionIterations = 6;

    private Rigidbody2D rb;

    private static readonly Vector2[] CardinalDirections =
    {
        Vector2.left,
        Vector2.right,
        Vector2.up,
        Vector2.down
    };

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 pos = rb.position;

        for (int iter = 0; iter < maxResolutionIterations; iter++)
        {
            Vector2 correction = Vector2.zero;
            Vector2 castOrigin = pos + rayOriginOffset;

            foreach (Vector2 dir in CardinalDirections)
            {
                float len = dir.x != 0f ? horizontalRayLength : verticalRayLength;
                RaycastHit2D hit = Physics2D.Raycast(castOrigin, dir, len, collisionMask);
                if (!hit.collider)
                    continue;
                if (hit.collider.attachedRigidbody == rb)
                    continue;
                if (hit.collider.isTrigger)
                    continue;

                float push = len - hit.distance;
                if (push > 0f)
                    correction += hit.normal * push;
            }

            if (correction.sqrMagnitude < 1e-10f)
                break;

            rb.MovePosition(pos + correction);
            pos = rb.position;
        }
    }
}
