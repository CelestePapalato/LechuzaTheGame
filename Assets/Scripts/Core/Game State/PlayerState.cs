using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public IntRangeBinding health = new IntRangeBinding();

    public void SetHealth(int current, int max)
    {
        health?.SetValues(current, max);
    }
}
