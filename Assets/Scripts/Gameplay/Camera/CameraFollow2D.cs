using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;

    public CameraVolume volume;

    public Vector3 offset = new Vector3(0f, 1f, -10f);

    public float smoothTime = 0.25f;

    private Vector3 smoothVelocity = Vector3.zero;
    private Vector3 frozenDesired;
    private bool hasBeenInRegion;
    private bool wasInRegionLastFrame;

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desired = target.position + offset;
        bool inRegion = volume == null || volume.ContainsWorldPoint(desired);

        if (inRegion)
        {
            if (!wasInRegionLastFrame)
                smoothVelocity = Vector3.zero;

            hasBeenInRegion = true;
            frozenDesired = desired;

            Vector3 lag = desired - transform.position;
            lag = Vector3.SmoothDamp(lag, Vector3.zero, ref smoothVelocity, smoothTime);
            transform.position = desired - lag;
            wasInRegionLastFrame = true;
            return;
        }

        wasInRegionLastFrame = false;

        if (!hasBeenInRegion)
            return;

        Vector3 stopLag = frozenDesired - transform.position;
        stopLag = Vector3.SmoothDamp(stopLag, Vector3.zero, ref smoothVelocity, smoothTime);
        transform.position = frozenDesired - stopLag;
    }
}
