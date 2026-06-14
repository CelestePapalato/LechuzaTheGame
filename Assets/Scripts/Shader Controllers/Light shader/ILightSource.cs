using UnityEngine;

public interface ILightSource
{
    Vector3 WorldPosition { get; }
    float Radius { get; } // valor entre 0 y 1
    bool IsActive { get; }
}
