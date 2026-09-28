using UnityEngine;

public class PaintingChaseAnomaly : MonoBehaviour
{
    [Header("異変本体")]
    public PaintingAnomaly paintingAnomaly;

    [Header("追いかける対象")]
    public Transform player;
    public Transform playerCamera;

    [Header("追跡中の見た目")]
    public Material chaseMaterial;

    [Header("追跡設定")]
    public float moveSpeed = 2.2f;
    public float stopDistance = 1.8f;

    [Header("向き設定")]
    public float rotationOffsetY = 180f;
    public float rotateSpeed = 6f;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    private Renderer paintingRenderer;
    private Material originalMaterial;

    private bool chasing = false;

    void Start()
    {
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;

        paintingRenderer = GetComponent<Renderer>();

        if (paintingRenderer != null)
        {
            originalMaterial =
                paintingRenderer.sharedMaterial;
        }
    }

    void Update()
    {
        if (paintingAnomaly == null ||
            player == null ||
            playerCamera == null)
        {
            return;
        }

        // 異変になった瞬間
        if (paintingAnomaly.isAnomaly &&
            !chasing)
        {
            StartChase();
        }

        // 撮影後・DAYリセット後
        if (!paintingAnomaly.isAnomaly)
        {
            if (chasing)
            {
                ResetPainting();
            }

            return;
        }

        if (chasing)
        {
            ChasePlayer();
        }
    }

    void StartChase()
    {
        chasing = true;

        // 追跡中だけ背景透過Materialへ変更
        if (paintingRenderer != null &&
            chaseMaterial != null)
        {
            paintingRenderer.sharedMaterial =
                chaseMaterial;
        }
    }

    void ChasePlayer()
    {
        Vector3 targetPosition =
            playerCamera.position;

        float distance =
            Vector3.Distance(
                transform.position,
                targetPosition
            );

        if (distance > stopDistance)
        {
            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    moveSpeed * Time.deltaTime
                );
        }

        Vector3 direction =
            playerCamera.position -
            transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
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
    }

    void ResetPainting()
    {
        chasing = false;

        transform.localPosition =
            originalLocalPosition;

        transform.localRotation =
            originalLocalRotation;

        // 元の絵に戻す
        if (paintingRenderer != null &&
            originalMaterial != null)
        {
            paintingRenderer.sharedMaterial =
                originalMaterial;
        }
    }
}