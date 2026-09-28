using UnityEngine;
using System.Collections;

public class LightOffReveal : MonoBehaviour
{
    [Header("手元ライト")]
    public FlashlightToggle flashlightToggle;

    [Header("プレイヤーカメラ")]
    public Transform playerCamera;

    [Header("表示対象")]
    public Renderer targetRenderer;
    public Collider targetCollider;

    [Header("撮影判定")]
    public PaintingAnomaly photoTargetAnomaly;

    [Header("出現ポイント")]
    public Transform[] appearPoints;

    [Header("ゆっくり出現")]
    public float fadeInTime = 1.2f;

    [Header("プレイヤーの方を向く")]
    public float rotationOffsetY = 180f;
    public float rotateSpeed = 10f;

    [Header("最後の追跡")]
    public float chaseSpeed = 2.0f;
    public float stopDistance = 1.8f;
    public bool stopAtWalls = true;

    private int currentPointIndex = 0;
    private bool wasLightOn = true;

    private bool chaseActive = false;
    private bool appearingAtLastPoint = false;

    // GameManagerから、このDAYだけ有効にする
    private bool activeForThisDay = false;

    // 撮影されたらtrue
    private bool completed = false;

    private Material runtimeMaterial;
    private Color normalColor;

    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer =
                GetComponent<Renderer>();
        }

        if (targetCollider == null)
        {
            targetCollider =
                GetComponent<Collider>();
        }

        if (targetRenderer != null)
        {
            runtimeMaterial =
                targetRenderer.material;

            if (runtimeMaterial.HasProperty("_BaseColor"))
            {
                normalColor =
                    runtimeMaterial.GetColor("_BaseColor");
            }
            else
            {
                normalColor = Color.white;
            }
        }
    }

    void Start()
    {
        HideFigure();

        wasLightOn =
            IsLightOn();
    }

    void Update()
    {
        // このDAYの異変として
        // 選ばれていなければ何もしない
        if (!activeForThisDay)
            return;

        // 撮影済みならもう出ない
        if (completed)
            return;

        // 追跡中だったのに
        // isAnomaly が false になった
        // ＝カメラで撮影された
        if (chaseActive &&
            photoTargetAnomaly != null &&
            !photoTargetAnomaly.isAnomaly)
        {
            CompleteAfterPhoto();
            return;
        }

        bool lightOn =
            IsLightOn();

        if (lightOn != wasLightOn)
        {
            if (lightOn)
            {
                HideFigure();
            }
            else
            {
                ShowAtNextPoint();
            }

            wasLightOn =
                lightOn;
        }

        if (!lightOn)
        {
            FacePlayer();

            if (chaseActive)
            {
                ChasePlayer();
            }
        }
    }

    bool IsLightOn()
    {
        if (flashlightToggle == null)
            return true;

        return flashlightToggle.IsOn;
    }

    void ShowAtNextPoint()
    {
        if (appearPoints == null ||
            appearPoints.Length == 0)
        {
            return;
        }

        chaseActive = false;

        int index =
            Mathf.Clamp(
                currentPointIndex,
                0,
                appearPoints.Length - 1
            );

        Transform point =
            appearPoints[index];

        if (point != null)
        {
            transform.position =
                point.position;
        }

        appearingAtLastPoint =
            index ==
            appearPoints.Length - 1;

        if (targetRenderer != null)
        {
            targetRenderer.enabled =
                true;
        }

        if (targetCollider != null)
        {
            targetCollider.enabled =
                true;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(
                fadeCoroutine
            );
        }

        fadeCoroutine =
            StartCoroutine(
                FadeIn()
            );

        if (currentPointIndex <
            appearPoints.Length - 1)
        {
            currentPointIndex++;
        }
    }

    IEnumerator FadeIn()
    {
        float timer = 0f;

        SetAlpha(0f);

        while (timer < fadeInTime)
        {
            timer +=
                Time.deltaTime;

            float alpha =
                Mathf.Clamp01(
                    timer / fadeInTime
                );

            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(1f);

        fadeCoroutine = null;

        // 最後のポイントで
        // 撮影可能＋追跡開始
        if (appearingAtLastPoint)
        {
            if (photoTargetAnomaly != null)
            {
                photoTargetAnomaly.SetAnomaly(true);
            }

            chaseActive = true;
        }
    }

    void ChasePlayer()
    {
        if (playerCamera == null)
            return;

        Vector3 currentPosition =
            transform.position;

        Vector3 targetPosition =
            playerCamera.position;

        Vector3 direction =
            targetPosition -
            currentPosition;

        direction.y = 0f;

        float distance =
            direction.magnitude;

        if (distance <= stopDistance)
            return;

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        if (stopAtWalls &&
            WallInFront(direction))
        {
            return;
        }

        float moveAmount =
            Mathf.Min(
                chaseSpeed * Time.deltaTime,
                distance - stopDistance
            );

        transform.position +=
            direction * moveAmount;
    }

    bool WallInFront(Vector3 direction)
    {
        float checkDistance = 0.6f;

        RaycastHit[] hits =
            Physics.RaycastAll(
                transform.position +
                Vector3.up * 0.1f,
                direction,
                checkDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore
            );

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == transform)
                continue;

            if (hit.transform.IsChildOf(transform))
                continue;

            if (hit.transform.CompareTag("Player"))
                continue;

            return true;
        }

        return false;
    }

    void FacePlayer()
    {
        if (playerCamera == null)
            return;

        Vector3 direction =
            playerCamera.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        targetRotation *=
            Quaternion.Euler(
                0f,
                rotationOffsetY,
                0f
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );
    }

    void HideFigure()
    {
        chaseActive = false;

        if (fadeCoroutine != null)
        {
            StopCoroutine(
                fadeCoroutine
            );

            fadeCoroutine = null;
        }

        SetAlpha(0f);

        if (targetRenderer != null)
        {
            targetRenderer.enabled =
                false;
        }

        if (targetCollider != null)
        {
            targetCollider.enabled =
                false;
        }

        if (photoTargetAnomaly != null)
        {
            photoTargetAnomaly.SetAnomaly(false);
        }
    }

    void CompleteAfterPhoto()
    {
        completed = true;
        chaseActive = false;

        if (fadeCoroutine != null)
        {
            StopCoroutine(
                fadeCoroutine
            );

            fadeCoroutine = null;
        }

        SetAlpha(0f);

        if (targetRenderer != null)
        {
            targetRenderer.enabled =
                false;
        }

        if (targetCollider != null)
        {
            targetCollider.enabled =
                false;
        }

        if (photoTargetAnomaly != null)
        {
            photoTargetAnomaly.SetAnomaly(false);
        }

        Debug.Log(
            "★ ライト異変を撮影して消去しました"
        );
    }

    void SetAlpha(float alpha)
    {
        if (runtimeMaterial == null)
            return;

        Color color =
            normalColor;

        color.a =
            alpha;

        if (runtimeMaterial.HasProperty(
            "_BaseColor"
        ))
        {
            runtimeMaterial.SetColor(
                "_BaseColor",
                color
            );
        }
    }

    // ==========================
    // GameManagerから呼ぶ
    // ==========================

    public void ActivateForDay()
    {
        activeForThisDay = true;

        currentPointIndex = 0;
        chaseActive = false;
        appearingAtLastPoint = false;
        completed = false;

        wasLightOn =
            IsLightOn();

        HideFigure();

        Debug.Log(
            "ライトOFF異変を有効化しました"
        );
    }

    public void DeactivateForDay()
    {
        activeForThisDay = false;

        currentPointIndex = 0;
        chaseActive = false;
        appearingAtLastPoint = false;
        completed = false;

        HideFigure();

        Debug.Log(
            "ライトOFF異変を無効化しました"
        );
    }
}