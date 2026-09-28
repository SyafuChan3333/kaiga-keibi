using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public AudioSource footstepSource;
    public AudioClip footstepClip;
    public float footstepInterval = 0.45f;

    private CharacterController controller;
    private float footstepTimer = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (footstepSource != null)
        {
            footstepSource.loop = false;
            footstepSource.playOnAwake = false;
            footstepSource.Stop();
        }
    }

    void Update()
    {
        if (BriefingManager.IsOpen)
            return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 move =
            transform.right * horizontal +
            transform.forward * vertical;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        controller.Move(move * moveSpeed * Time.deltaTime);

        bool holdingMoveKey =
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.UpArrow) ||
            Input.GetKey(KeyCode.DownArrow) ||
            Input.GetKey(KeyCode.LeftArrow) ||
            Input.GetKey(KeyCode.RightArrow);

        if (holdingMoveKey && move.sqrMagnitude > 0.2f)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                PlayFootstep();
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        if (footstepSource == null || footstepClip == null)
            return;

        footstepSource.pitch = Random.Range(0.92f, 1.08f);
        footstepSource.PlayOneShot(footstepClip);
    }
}