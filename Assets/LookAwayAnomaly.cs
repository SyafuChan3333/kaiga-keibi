using UnityEngine;

public class LookAwayAnomaly : MonoBehaviour
{
    [Header("対象の絵")]
    public PaintingAnomaly painting;

    [Header("プレイヤーのカメラ")]
    public Camera playerCamera;

    [Header("判定設定")]
    public float maxDistance = 15f;

    private bool hasSeenPainting = false;
    private bool anomalyTriggered = false;
    private bool activeForThisDay = false;

    private Renderer paintingRenderer;

    void Start()
    {
        if (painting != null)
        {
            paintingRenderer =
                painting.GetComponent<Renderer>();
        }
    }

    void Update()
    {
        if (!activeForThisDay)
            return;

        if (painting == null ||
            playerCamera == null ||
            paintingRenderer == null)
        {
            return;
        }

        float distance =
            Vector3.Distance(
                playerCamera.transform.position,
                paintingRenderer.bounds.center
            );

        if (distance > maxDistance)
            return;

        bool paintingVisible =
            IsPaintingReallyVisible();

        // 実際に絵が見えている時だけ
        // 「見た」と判定する
        if (!hasSeenPainting &&
            paintingVisible)
        {
            hasSeenPainting = true;

            Debug.Log(
                "絵を実際に確認しました：" +
                painting.gameObject.name
            );
        }

        // 一度見たあと、
        // 完全に見えなくなったら異変発動
        if (hasSeenPainting &&
            !paintingVisible &&
            !anomalyTriggered)
        {
            anomalyTriggered = true;

            painting.SetAnomaly(true);

            Debug.Log(
                "目を離した間に異変が発生しました：" +
                painting.gameObject.name
            );
        }
    }

    bool IsPaintingReallyVisible()
    {
        // ① まずカメラ画面内に入っているか
        Plane[] planes =
            GeometryUtility.CalculateFrustumPlanes(
                playerCamera
            );

        bool insideCamera =
            GeometryUtility.TestPlanesAABB(
                planes,
                paintingRenderer.bounds
            );

        if (!insideCamera)
            return false;

        // ② カメラから絵まで壁などがないか確認
        Vector3 cameraPosition =
            playerCamera.transform.position;

        Vector3 targetPosition =
            paintingRenderer.bounds.center;

        Vector3 direction =
            targetPosition - cameraPosition;

        float distance =
            direction.magnitude;

        if (distance <= 0.01f)
            return true;

        RaycastHit hit;

        if (Physics.Raycast(
            cameraPosition,
            direction.normalized,
            out hit,
            distance + 0.2f,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        ))
        {
            // 最初に当たったものが
            // この絵自身なら見えている
            if (hit.transform == painting.transform ||
                hit.transform.IsChildOf(
                    painting.transform
                ))
            {
                return true;
            }

            // 壁・柱・別オブジェクトなどに
            // 遮られている
            return false;
        }

        return false;
    }

    public void ActivateForDay()
    {
        activeForThisDay = true;
        hasSeenPainting = false;
        anomalyTriggered = false;

        if (painting != null)
        {
            painting.SetAnomaly(false);
        }
    }

    public void DeactivateForDay()
    {
        activeForThisDay = false;
        hasSeenPainting = false;
        anomalyTriggered = false;

        if (painting != null)
        {
            painting.SetAnomaly(false);
        }
    }
}