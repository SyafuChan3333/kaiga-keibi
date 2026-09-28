using UnityEngine;

public class PaintingAnomaly : MonoBehaviour
{
    public enum AnomalyType
    {
        ImageChange,
        Tilt,
        Disappear,
        Move,
        ScaleUp,
        Fall,
        Appear
    }

    public AnomalyType anomalyType;

    [Header("Image Change")]
    public Material normalMaterial;
    public Material anomalyMaterial;

    [Header("Tilt")]
    public float tiltAngle = 8f;

    [Header("Move")]
    public float moveX = 0.5f;
    public float moveY = 0f;

    [Header("Scale Up")]
    public float scaleMultiplier = 1.3f;

    [Header("Fall")]
    public float fallForward = 1.2f;
    public float fallHeight = 0.15f;

    [Header("出現・笑い声")]
    public AudioSource audioSource;
    public AudioClip appearClip;

    [HideInInspector]
    public bool isAnomaly = false;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private MeshRenderer meshRenderer;
    private bool originalRendererEnabled;
    private bool stored;

    void Awake()
    {
        StoreOriginal();
    }

    void StoreOriginal()
    {
        if (stored)
            return;

        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;

        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            originalRendererEnabled = meshRenderer.enabled;

            if (normalMaterial == null)
                normalMaterial = meshRenderer.sharedMaterial;
        }

        stored = true;
    }

    public void SetAnomaly(bool on)
    {
        StoreOriginal();

        isAnomaly = on;

        if (on)
            ApplyAnomaly();
        else
            Restore();
    }

    void ApplyAnomaly()
    {
        Restore();

        switch (anomalyType)
        {
            case AnomalyType.ImageChange:
                if (meshRenderer != null &&
                    anomalyMaterial != null)
                {
                    meshRenderer.material = anomalyMaterial;
                }
                break;

            case AnomalyType.Tilt:
                transform.rotation =
                    originalRotation *
                    Quaternion.Euler(0f, 0f, tiltAngle);
                break;

            case AnomalyType.Disappear:
                if (meshRenderer != null)
                    meshRenderer.enabled = false;
                break;

            case AnomalyType.Move:
                transform.position =
                    originalPosition +
                    new Vector3(moveX, moveY, 0f);
                break;

            case AnomalyType.ScaleUp:
                transform.localScale =
                    originalScale * scaleMultiplier;
                break;

            case AnomalyType.Fall:
                ApplyFall();
                break;

            case AnomalyType.Appear:
                if (meshRenderer != null)
                    meshRenderer.enabled = true;
                PlayAppearSound();
                break;
        }
    }

    void ApplyFall()
    {
        Vector3 inward = new Vector3(
            -originalPosition.x,
            0f,
            -originalPosition.z
        );

        if (inward.sqrMagnitude < 0.01f)
            inward = Vector3.back;

        inward.Normalize();

        Vector3 pos = originalPosition;
        pos += inward * fallForward;
        pos.y = fallHeight;

        transform.position = pos;
        transform.rotation =
            originalRotation * Quaternion.Euler(90f, 0f, 0f);
        transform.localScale = originalScale * 0.8f;
    }

    void PlayAppearSound()
    {
        if (audioSource == null || appearClip == null)
            return;

        audioSource.PlayOneShot(appearClip);
    }

    void Restore()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;

        if (meshRenderer != null)
        {
            meshRenderer.enabled = originalRendererEnabled;

            if (normalMaterial != null)
                meshRenderer.material = normalMaterial;
        }
    }
}