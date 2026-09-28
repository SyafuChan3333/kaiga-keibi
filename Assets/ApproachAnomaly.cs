using UnityEngine;

public class ApproachAnomaly : MonoBehaviour
{
    [Header("対象の絵")]
    public PaintingAnomaly painting;

    [Header("プレイヤー")]
    public Transform player;

    [Header("発動距離")]
    public float triggerDistance = 3f;

    private bool activeForThisDay = false;
    private bool anomalyTriggered = false;

    void Update()
    {
        // このDAYの異変に選ばれていなければ何もしない
        if (!activeForThisDay)
            return;

        if (painting == null || player == null)
            return;

        // プレイヤーと絵の距離
        float distance = Vector3.Distance(
            player.position,
            painting.transform.position
        );

        // 一定距離まで近づいたら異変発生
        if (!anomalyTriggered &&
            distance <= triggerDistance)
        {
            anomalyTriggered = true;

            painting.SetAnomaly(true);

            Debug.Log("絵に近づいたことで異変が発生しました");
        }
    }

    // GameManagerから呼ばれる
    public void ActivateForDay()
    {
        activeForThisDay = true;
        anomalyTriggered = false;

        if (painting != null)
        {
            painting.SetAnomaly(false);
        }
    }

    // DAY終了・別の異変が選ばれたとき
    public void DeactivateForDay()
    {
        activeForThisDay = false;
        anomalyTriggered = false;

        if (painting != null)
        {
            painting.SetAnomaly(false);
        }
    }
}