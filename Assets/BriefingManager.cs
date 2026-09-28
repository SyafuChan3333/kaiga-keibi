using UnityEngine;

public class BriefingManager : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    public GameObject briefingPanel;
    public GameManager gameManager;

    void Awake()
    {
        IsOpen = true;
    }

    void Start()
    {
        if (briefingPanel != null)
            briefingPanel.SetActive(true);

        UnlockCursor();
    }

    void Update()
    {
        if (!IsOpen)
            return;

        UnlockCursor();
    }

    public void CloseBriefingAndStart()
    {
        IsOpen = false;

        if (briefingPanel != null)
            briefingPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (gameManager != null)
            gameManager.BeginFromBriefing();
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}