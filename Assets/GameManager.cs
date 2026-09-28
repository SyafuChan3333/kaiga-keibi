using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public enum DayAnomalyType
    {
        Random,
        None,
        Normal,
        LookAway,
        Approach,
        LightOff
    }

    [Header("DAYごとの異変設定")]
    public DayAnomalyType[] daySettings = new DayAnomalyType[10];

    [Header("通常の絵の異変")]
    public PaintingAnomaly[] paintings;

    [Header("目を離したら変わる異変")]
    public LookAwayAnomaly[] lookAwayAnomalies;

    [Header("近づいたら変わる異変")]
    public ApproachAnomaly[] approachAnomalies;

    [Header("ライトOFFで現れる異変")]
    public LightOffReveal[] lightOffAnomalies;

    [Header("DAY表示")]
    public TMP_Text dayText;

    [Range(0f, 1f)]
    public float anomalyChance = 0.7f;

    public float dayDisplayTime = 1.5f;
    public float fadeTime = 1.5f;

    [Header("エンディング")]
    [TextArea(3, 8)]
    public string goalText =
        "10日間の巡回を終えた。\n記録は以上である。";

    [Header("日記")]
    public DiaryManager diaryManager;

    [Header("報告画面")]
    public GameObject reportPanel;

    [Header("記録済み表示")]
    public TMP_Text recordedText;

    [Header("撮影")]
    public CameraController cameraController;

    [Header("失敗演出")]
    public GameObject failurePanel;
    public Image failurePanelImage;
    public TMP_Text failureText;

    public float failureFadeTime = 1.5f;
    public float failureTextWait = 2.5f;
    public float failureFadeOutTime = 1.5f;

    private int currentDay = 1;

    private bool anomalyWasPresent = false;
    private bool anomalyWasPhotographed = false;

    private bool gameFinished = false;
    private bool failurePlaying = false;

    void Start()
    {
        currentDay = 1;

        if (failurePanel != null)
            failurePanel.SetActive(false);

        if (reportPanel != null)
            reportPanel.SetActive(false);

        if (recordedText != null)
            recordedText.gameObject.SetActive(false);

        if (BriefingManager.IsOpen)
            return;

        StartNewDay();
    }

    public void BeginFromBriefing()
    {
        StartNewDay();
    }

    void StartNewDay()
    {
        anomalyWasPresent = false;
        anomalyWasPhotographed = false;
        failurePlaying = false;

        if (recordedText != null)
            recordedText.gameObject.SetActive(false);

        if (cameraController != null)
            cameraController.HidePhotoPreview();

        StartCoroutine(ShowDayText());

        ResetAllAnomalies();

        int dayIndex = currentDay - 1;

        DayAnomalyType selectedType =
            DayAnomalyType.Random;

        if (daySettings != null &&
            dayIndex >= 0 &&
            dayIndex < daySettings.Length)
        {
            selectedType =
                daySettings[dayIndex];
        }

        switch (selectedType)
        {
            case DayAnomalyType.None:
                Debug.Log(
                    "DAY " +
                    currentDay +
                    " 異変なし"
                );
                break;

            case DayAnomalyType.Normal:
                StartNormalAnomaly();
                break;

            case DayAnomalyType.LookAway:
                StartLookAwayAnomaly();
                break;

            case DayAnomalyType.Approach:
                StartApproachAnomaly();
                break;

            case DayAnomalyType.LightOff:
                StartLightOffAnomaly();
                break;

            case DayAnomalyType.Random:
                StartRandomAnomaly();
                break;
        }
    }

    void ResetAllAnomalies()
    {
        if (paintings != null)
        {
            foreach (PaintingAnomaly painting in paintings)
            {
                if (painting != null)
                {
                    painting.SetAnomaly(false);
                }
            }
        }

        if (lookAwayAnomalies != null)
        {
            foreach (LookAwayAnomaly lookAway in lookAwayAnomalies)
            {
                if (lookAway != null)
                {
                    lookAway.DeactivateForDay();
                }
            }
        }

        if (approachAnomalies != null)
        {
            foreach (ApproachAnomaly approach in approachAnomalies)
            {
                if (approach != null)
                {
                    approach.DeactivateForDay();
                }
            }
        }

        if (lightOffAnomalies != null)
        {
            foreach (LightOffReveal lightOff in lightOffAnomalies)
            {
                if (lightOff != null)
                {
                    lightOff.DeactivateForDay();
                }
            }
        }
    }

    // ==============================
    // ランダム異変
    // ==============================

    void StartRandomAnomaly()
    {
        // まず「異変が起こるか」を決める
        bool anomalyOccurs =
            Random.value < anomalyChance;

        if (!anomalyOccurs)
        {
            Debug.Log(
                "DAY " +
                currentDay +
                " ランダム結果：異変なし"
            );

            return;
        }

        int availableTypes = 0;

        // 通常異変
        if (paintings != null &&
            paintings.Length > 0)
        {
            availableTypes++;
        }

        // 目を離したら異変
        if (lookAwayAnomalies != null &&
            lookAwayAnomalies.Length > 0)
        {
            availableTypes++;
        }

        // 接近異変
        if (approachAnomalies != null &&
            approachAnomalies.Length > 0)
        {
            availableTypes++;
        }

        // ライトOFF異変
        if (lightOffAnomalies != null &&
            lightOffAnomalies.Length > 0)
        {
            availableTypes++;
        }

        if (availableTypes == 0)
        {
            Debug.Log(
                "異変を設定できる対象がありません"
            );

            return;
        }

        int randomType =
            Random.Range(
                0,
                availableTypes
            );

        // --------------------------
        // 通常異変
        // --------------------------

        if (paintings != null &&
            paintings.Length > 0)
        {
            if (randomType == 0)
            {
                StartNormalAnomaly();
                return;
            }

            randomType--;
        }

        // --------------------------
        // LookAway
        // --------------------------

        if (lookAwayAnomalies != null &&
            lookAwayAnomalies.Length > 0)
        {
            if (randomType == 0)
            {
                StartLookAwayAnomaly();
                return;
            }

            randomType--;
        }

        // --------------------------
        // Approach
        // --------------------------

        if (approachAnomalies != null &&
            approachAnomalies.Length > 0)
        {
            if (randomType == 0)
            {
                StartApproachAnomaly();
                return;
            }

            randomType--;
        }

        // --------------------------
        // LightOff
        // --------------------------

        if (lightOffAnomalies != null &&
            lightOffAnomalies.Length > 0)
        {
            StartLightOffAnomaly();
        }
    }

    // ==============================
    // 通常異変
    // ==============================

    void StartNormalAnomaly()
    {
        if (paintings == null ||
            paintings.Length == 0)
        {
            Debug.Log(
                "通常異変の対象がありません"
            );

            return;
        }

        int index =
            Random.Range(
                0,
                paintings.Length
            );

        PaintingAnomaly selected =
            paintings[index];

        if (selected == null)
            return;

        selected.SetAnomaly(true);

        anomalyWasPresent = true;

        Debug.Log(
            "DAY " +
            currentDay +
            " 通常異変：" +
            selected.gameObject.name
        );
    }

    // ==============================
    // LookAway異変
    // ==============================

    void StartLookAwayAnomaly()
    {
        if (lookAwayAnomalies == null ||
            lookAwayAnomalies.Length == 0)
        {
            Debug.Log(
                "LookAway異変の対象がありません"
            );

            return;
        }

        int index =
            Random.Range(
                0,
                lookAwayAnomalies.Length
            );

        LookAwayAnomaly selected =
            lookAwayAnomalies[index];

        if (selected == null)
            return;

        selected.ActivateForDay();

        anomalyWasPresent = true;

        Debug.Log(
            "DAY " +
            currentDay +
            " LookAway異変：" +
            selected.gameObject.name
        );
    }

    // ==============================
    // Approach異変
    // ==============================

    void StartApproachAnomaly()
    {
        if (approachAnomalies == null ||
            approachAnomalies.Length == 0)
        {
            Debug.Log(
                "接近時異変の対象がありません"
            );

            return;
        }

        int index =
            Random.Range(
                0,
                approachAnomalies.Length
            );

        ApproachAnomaly selected =
            approachAnomalies[index];

        if (selected == null)
            return;

        selected.ActivateForDay();

        anomalyWasPresent = true;

        Debug.Log(
            "DAY " +
            currentDay +
            " 接近時異変：" +
            selected.gameObject.name
        );
    }

    // ==============================
    // ライトOFF異変
    // ==============================

    void StartLightOffAnomaly()
    {
        if (lightOffAnomalies == null ||
            lightOffAnomalies.Length == 0)
        {
            Debug.Log(
                "ライトOFF異変の対象がありません"
            );

            return;
        }

        int index =
            Random.Range(
                0,
                lightOffAnomalies.Length
            );

        LightOffReveal selected =
            lightOffAnomalies[index];

        if (selected == null)
            return;

        selected.ActivateForDay();

        anomalyWasPresent = true;

        Debug.Log(
            "DAY " +
            currentDay +
            " ライトOFF異変：" +
            selected.gameObject.name
        );
    }

    // ==============================
    // DAY表示
    // ==============================

    IEnumerator ShowDayText()
    {
        if (dayText == null)
            yield break;

        dayText.gameObject.SetActive(true);

        dayText.text =
            "DAY " + currentDay;

        Color color =
            dayText.color;

        color.a = 1f;

        dayText.color =
            color;

        yield return new WaitForSeconds(
            dayDisplayTime
        );

        float timer = 0f;

        while (timer < fadeTime)
        {
            timer +=
                Time.deltaTime;

            float alpha =
                1f -
                (timer / fadeTime);

            color.a = alpha;

            dayText.color =
                color;

            yield return null;
        }

        color.a = 0f;

        dayText.color =
            color;

        dayText.gameObject.SetActive(false);
    }

    // ==============================
    // 撮影済み
    // ==============================

    public void AnomalyPhotographed()
    {
        anomalyWasPhotographed = true;

        if (recordedText != null)
        {
            recordedText.gameObject.SetActive(
                true
            );
        }
    }

    // ==============================
    // 「異変を発見した」
    // ==============================

    public void ReportFoundAnomaly()
    {
        if (gameFinished ||
            failurePlaying)
        {
            return;
        }

        CloseReportPanel();

        if (anomalyWasPresent &&
            anomalyWasPhotographed)
        {
            Success();
        }
        else
        {
            StartCoroutine(
                FailureSequence()
            );
        }
    }

    // ==============================
    // 「異変はなかった」
    // ==============================

    public void ReportNoAnomaly()
    {
        if (gameFinished ||
            failurePlaying)
        {
            return;
        }

        CloseReportPanel();

        if (!anomalyWasPresent)
        {
            Success();
        }
        else
        {
            StartCoroutine(
                FailureSequence()
            );
        }
    }

    void Success()
    {
        if (recordedText != null)
        {
            recordedText.gameObject.SetActive(
                false
            );
        }

        if (cameraController != null)
        {
            cameraController.HidePhotoPreview();
        }

        if (diaryManager != null)
        {
            diaryManager.ShowDiary(
                currentDay,
                anomalyWasPresent
            );
        }
    }

    void CloseReportPanel()
    {
        if (reportPanel != null)
        {
            reportPanel.SetActive(false);
        }

        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible =
            false;
    }

    // ==============================
    // 失敗
    // ==============================

    IEnumerator FailureSequence()
    {
        failurePlaying = true;

        if (recordedText != null)
        {
            recordedText.gameObject.SetActive(
                false
            );
        }

        if (cameraController != null)
        {
            cameraController.HidePhotoPreview();
        }

        if (failurePanel == null)
        {
            currentDay = 1;

            StartNewDay();

            yield break;
        }

        failurePanel.SetActive(true);

        if (failurePanelImage != null)
        {
            Color panelColor =
                failurePanelImage.color;

            panelColor.a = 0f;

            failurePanelImage.color =
                panelColor;
        }

        if (failureText != null)
        {
            Color textColor =
                failureText.color;

            textColor.a = 0f;

            failureText.color =
                textColor;
        }

        float timer = 0f;

        while (timer <
            failureFadeTime)
        {
            timer +=
                Time.deltaTime;

            float alpha =
                timer /
                failureFadeTime;

            if (failurePanelImage != null)
            {
                Color panelColor =
                    failurePanelImage.color;

                panelColor.a =
                    alpha;

                failurePanelImage.color =
                    panelColor;
            }

            yield return null;
        }

        if (failureText != null)
        {
            Color textColor =
                failureText.color;

            textColor.a = 1f;

            failureText.color =
                textColor;
        }

        yield return new WaitForSeconds(
            failureTextWait
        );

        if (failureText != null)
        {
            Color textColor =
                failureText.color;

            textColor.a = 0f;

            failureText.color =
                textColor;
        }

        currentDay = 1;

        StartNewDay();

        timer = 0f;

        while (timer <
            failureFadeOutTime)
        {
            timer +=
                Time.deltaTime;

            float alpha =
                1f -
                (timer /
                failureFadeOutTime);

            if (failurePanelImage != null)
            {
                Color panelColor =
                    failurePanelImage.color;

                panelColor.a =
                    alpha;

                failurePanelImage.color =
                    panelColor;
            }

            yield return null;
        }

        failurePanel.SetActive(false);

        failurePlaying = false;
    }

    // ==============================
    // 次のDAY
    // ==============================

    public void GoToNextDay()
    {
        currentDay++;

        if (currentDay > 10)
        {
            gameFinished = true;

            StartCoroutine(
                ShowGoal()
            );

            return;
        }

        StartNewDay();
    }

    IEnumerator ShowGoal()
    {
        if (dayText == null)
            yield break;

        dayText.gameObject.SetActive(true);

        if (!string.IsNullOrEmpty(
            goalText
        ))
        {
            dayText.text =
                goalText;
        }
        else
        {
            dayText.text =
                "GOAL";
        }

        Color color =
            dayText.color;

        color.a = 1f;

        dayText.color =
            color;

        yield break;
    }
}