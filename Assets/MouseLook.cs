using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 300f;
    public Transform playerBody;

    [Header("開始時のカメラ角度")]
    public float startLookX = 0f;

    [Header("開始時の視点固定時間")]
    public float startLockTime = 0.3f;

    private float xRotation = 0f;
    private float lockTimer = 0f;

    void Start()
    {
        xRotation = startLookX;

        transform.localRotation =
            Quaternion.Euler(xRotation, 0f, 0f);

        lockTimer = startLockTime;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (BriefingManager.IsOpen)
            return;

        if (lockTimer > 0f)
        {
            lockTimer -= Time.deltaTime;

            transform.localRotation =
                Quaternion.Euler(xRotation, 0f, 0f);

            return;
        }

        float mouseX =
            Input.GetAxis("Mouse X") *
            mouseSensitivity *
            Time.deltaTime;

        float mouseY =
            Input.GetAxis("Mouse Y") *
            mouseSensitivity *
            Time.deltaTime;

        xRotation -= mouseY;

        xRotation =
            Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation =
            Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
        {
            playerBody.Rotate(
                Vector3.up * mouseX
            );
        }
    }
}