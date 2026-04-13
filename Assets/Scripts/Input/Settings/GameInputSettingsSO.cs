using System;
using UnityEngine;

[System.Serializable]
public class GameInputSettings
{
    [Header("Camera Settings")]
    [SerializeField] private float cameraSensitivity = 1.0f;
    [SerializeField] private bool gyroEnabled = false;
    [SerializeField] private float gyroHorizontalSensitivity = 5f;
    [SerializeField] private float gyroVerticalSensitivity = 5f;

    public Action<bool> OnGyroEnabledUpdate;

    public float CameraSensitivity
    {
        get => cameraSensitivity;
        set => cameraSensitivity = Mathf.Clamp(value, 0.1f, 50f); // para evitar valores locos
    }
    public bool GyroEnabled
    {
        get => gyroEnabled;
        set
        {
            if(gyroEnabled != value)
            {
                gyroEnabled = value;
                OnGyroEnabledUpdate?.Invoke(gyroEnabled);
            }
        }
    }

    public float GyroHorizontalSensitivity
    {
        get => gyroHorizontalSensitivity;
        set => gyroHorizontalSensitivity = Mathf.Clamp(value, 0f, 30f);
    }

    public float GyroVerticalSensitivity
    {
        get => gyroVerticalSensitivity;
        set => gyroVerticalSensitivity = Mathf.Clamp(value, 0f, 30f);
    }
}

[CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/Player Settings", order = 1)]
public class GameInputSettingsSO : ScriptableObject
{
    public GameInputSettings Settings = new GameInputSettings();
}
