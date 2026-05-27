using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.12f;

    private Vector3 _velocity;
    private float _shakeTimer;
    private float _shakeMagnitude;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, smoothTime);

        if (_shakeTimer > 0f)
        {
            _shakeTimer -= Time.unscaledDeltaTime;
            transform.position += (Vector3)(Random.insideUnitCircle * _shakeMagnitude);
        }
    }

    /// <summary>
    /// Short camera shake used when the player takes damage.
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        _shakeTimer = duration;
        _shakeMagnitude = magnitude;
    }
}
