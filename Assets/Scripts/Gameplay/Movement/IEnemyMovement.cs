using UnityEngine;

public interface IEnemyMovement
{
    void SetDestination(Vector2 destination, float speed);
    void Stop();
}
