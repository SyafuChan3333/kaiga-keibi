using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("カメラUI")]
    public GameObject viewfinder;
    public GameObject photoFlash;
    public GameObject photoResultText;

    [Header("プレイヤーカメラ")]
    public Camera playerCamera;

    [Header("GameManager")]
    public GameManager gameManager;

    [Header("撮影写真表示")]
    public GameObject photoFrame;
    public RawImage photoPreview;

    [Header("ズーム設定")]
    public float normalFOV = 60f;
    public float zoomFOV = 40f;
    public float zoomSpeed = 8f;

    [Header("サウンド")]
    public AudioSource cameraAudioSource;
    public AudioClip shutterClip;

    private bool isAiming = false;
    private Texture2D capturedTexture;

    void Start()
    {
        if (viewfinder != null)
            viewfinder.SetActive(false);

        if (photoFlash != null)
            photoFlash.SetActive(false);

        if (photoResultText != null)
            photoResultText.SetActive(false);

        if (photoFrame != null)
            photoFrame.SetActive(false);

        if (playerCamera != null)
            playerCamera.fieldOfView = normalFOV;
    }

    void Update()
    {
        isAiming = Input.GetMouseButton(1);

        if (viewfinder != null)
            viewfinder.SetActive(isAiming);

        if (playerCamera != null)
        {
            float targetFOV =
                isAiming ? zoomFOV : normalFOV;

            playerCamera.fieldOfView =
                Mathf.Lerp(
                    playerCamera.fieldOfView,
                    targetFOV,
                    zoomSpeed * Time.deltaTime
                );
        }

        if (isAiming &&
            Input.GetMouseButtonDown(0))
        {
            TakePhoto();
        }
    }

    void TakePhoto()
    {
        if (playerCamera == null)
            return;

        PlayShutter();

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 20f))
        {
            Debug.Log(
                "撮影したもの：" +
                hit.collider.gameObject.name
            );

            PaintingAnomaly painting =
                FindPaintingAnomaly(hit.collider);

            if (painting != null)
            {
                Debug.Log(
                    "撮影判定：" +
                    painting.gameObject.name
                );

                if (painting.isAnomaly)
                {
                    Debug.Log(
                        "★ 異変を記録しました!"
                    );

                    StartCoroutine(
                        CaptureAnomalyPhoto(painting)
                    );
                }
                else
                {
                    StartCoroutine(Flash());

                    Debug.Log(
                        "この対象に異変はありません"
                    );
                }
            }
            else
            {
                StartCoroutine(Flash());

                Debug.Log(
                    "撮影対象に異変判定がありません"
                );
            }
        }
        else
        {
            StartCoroutine(Flash());

            Debug.Log(
                "何も写っていません"
            );
        }
    }

    PaintingAnomaly FindPaintingAnomaly(
        Collider hitCollider
    )
    {
        if (hitCollider == null)
            return null;

        // ① 当たったオブジェクト自身
        PaintingAnomaly painting =
            hitCollider.GetComponent<PaintingAnomaly>();

        if (painting != null)
            return painting;

        // ② 親側を探す
        painting =
            hitCollider.GetComponentInParent<PaintingAnomaly>();

        if (painting != null)
            return painting;

        // ③ 子側を探す
        painting =
            hitCollider.GetComponentInChildren<PaintingAnomaly>(
                true
            );

        return painting;
    }

    IEnumerator CaptureAnomalyPhoto(
        PaintingAnomaly painting
    )
    {
        if (photoFlash != null)
            photoFlash.SetActive(true);

        yield return new WaitForSeconds(0.08f);

        if (photoFlash != null)
            photoFlash.SetActive(false);

        bool viewfinderWasActive = false;

        if (viewfinder != null)
        {
            viewfinderWasActive =
                viewfinder.activeSelf;

            viewfinder.SetActive(false);
        }

        yield return new WaitForEndOfFrame();

        CaptureScreen();

        if (viewfinder != null &&
            viewfinderWasActive)
        {
            viewfinder.SetActive(true);
        }

        StartCoroutine(ShowResult());

        if (gameManager != null)
            gameManager.AnomalyPhotographed();

        if (painting != null)
            painting.SetAnomaly(false);
    }

    void CaptureScreen()
    {
        int width = Screen.width;
        int height = Screen.height;

        if (capturedTexture != null)
            Destroy(capturedTexture);

        capturedTexture =
            new Texture2D(
                width,
                height,
                TextureFormat.RGB24,
                false
            );

        capturedTexture.ReadPixels(
            new Rect(
                0,
                0,
                width,
                height
            ),
            0,
            0
        );

        capturedTexture.Apply();

        if (photoPreview != null)
            photoPreview.texture = capturedTexture;

        if (photoFrame != null)
            photoFrame.SetActive(true);
    }

    IEnumerator Flash()
    {
        if (photoFlash == null)
            yield break;

        photoFlash.SetActive(true);

        yield return new WaitForSeconds(0.08f);

        photoFlash.SetActive(false);
    }

    IEnumerator ShowResult()
    {
        if (photoResultText == null)
            yield break;

        photoResultText.SetActive(true);

        yield return new WaitForSeconds(2f);

        photoResultText.SetActive(false);
    }

    public void HidePhotoPreview()
    {
        if (photoFrame != null)
            photoFrame.SetActive(false);
    }

    void PlayShutter()
    {
        if (cameraAudioSource == null ||
            shutterClip == null)
        {
            return;
        }

        cameraAudioSource.PlayOneShot(
            shutterClip
        );
    }
}