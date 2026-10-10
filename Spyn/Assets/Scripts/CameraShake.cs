using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float shakeSpeed = 25f;

    private Vector3 originalLocalPosition;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        originalLocalPosition = transform.localPosition;
    }

    public void Shake(float intensity, float duration)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        transform.localPosition = originalLocalPosition;
        shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, duration));
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float fade = 1f - Mathf.Clamp01(elapsed / duration);

            Vector2 offset = Random.insideUnitCircle * intensity * fade;

            transform.localPosition = originalLocalPosition + new Vector3(offset.x, offset.y, 0f);

            yield return null;
        }

        transform.localPosition = originalLocalPosition;
        shakeCoroutine = null;
    }
}
