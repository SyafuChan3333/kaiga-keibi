using UnityEngine;

public class FlashlightToggle : MonoBehaviour
{
    public Light flashlight;
    public KeyCode toggleKey = KeyCode.F;
    public float aimIntensity = 8f;

    private float normalIntensity;
    private bool isOn = true;

    // 他のスクリプトから
    // ライトのON/OFF状態を確認するため
    public bool IsOn
    {
        get { return isOn; }
    }

    void Awake()
    {
        if (flashlight == null)
            flashlight = GetComponent<Light>();

        if (flashlight != null)
            normalIntensity = flashlight.intensity;
    }

    void Update()
    {
        if (flashlight == null)
            return;

        if (Input.GetKeyDown(toggleKey))
        {
            isOn = !isOn;
            flashlight.enabled = isOn;
        }

        if (!isOn)
            return;

        bool aiming = Input.GetMouseButton(1);

        flashlight.intensity =
            aiming ? aimIntensity : normalIntensity;
    }
}