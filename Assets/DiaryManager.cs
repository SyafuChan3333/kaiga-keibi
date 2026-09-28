using UnityEngine;
using TMPro;

public class DiaryManager : MonoBehaviour
{
    public GameObject diaryPanel;
    public TMP_Text diaryText;
    public GameManager gameManager;

    [Header("異変がなかった日")]
    [TextArea(3, 8)]
    public string[] noAnomalyTexts = new string[10];

    [Header("異変があった日")]
    [TextArea(3, 8)]
    public string[] anomalyTexts = new string[10];

    public void ShowDiary(int day, bool anomalyWasPresent)
    {
        if (diaryPanel != null)
            diaryPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        int index = day - 1;

        string[] source =
            anomalyWasPresent ? anomalyTexts : noAnomalyTexts;

        string body = "";

        if (source != null &&
            index >= 0 &&
            index < source.Length)
        {
            body = source[index];
        }

        if (diaryText != null)
            diaryText.text = "DAY " + day + "\n\n" + body;
    }

    public void NextDay()
    {
        if (diaryPanel != null)
            diaryPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (gameManager != null)
            gameManager.GoToNextDay();
    }
}