using UnityEngine;

public class CheckPointTrigger : MonoBehaviour
{
    [Header("報告画面")]
    public GameObject reportPanel;

    [Header("チェックポイント表示")]
    public GameObject checkPointText;

    private bool playerInside = false;

    void Start()
    {
        // 報告画面は最初は非表示
        if (reportPanel != null)
        {
            reportPanel.SetActive(false);
        }

        // 「[E] 巡回報告」も最初は非表示
        if (checkPointText != null)
        {
            checkPointText.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInside &&
            reportPanel != null &&
            !reportPanel.activeSelf &&
            Input.GetKeyDown(KeyCode.E))
        {
            OpenReport();
        }
    }

    void OpenReport()
    {
        // Eキー表示を消す
        if (checkPointText != null)
        {
            checkPointText.SetActive(false);
        }

        // 報告画面を表示
        reportPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            // チェックポイントに入ったら表示
            if (checkPointText != null)
            {
                checkPointText.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            // チェックポイントから離れたら消す
            if (checkPointText != null)
            {
                checkPointText.SetActive(false);
            }
        }
    }
}