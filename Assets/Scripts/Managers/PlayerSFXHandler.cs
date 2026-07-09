using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSFXHandler : MonoBehaviour
{
    public AudioClip jumpClip;
    public AudioClip walkClip;
    public AudioClip grappleClip;
    public AudioClip swingClip;

    private AudioSource _audio;
    private PlayerController _player;

    [SerializeField] private float walkInterval = 0.4f;
    private float walkTimer;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _player = GetComponent<PlayerController>();
    }

    void OnEnable()
    {
        _player.OnJump += PlayJump;
        _player.OnGrappleStart += PlayGrapple;
        _player.OnSwingStart += PlaySwing;
    }

    void OnDisable()
    {
        _player.OnJump -= PlayJump;
        _player.OnGrappleStart -= PlayGrapple;
        _player.OnSwingStart -= PlaySwing;
    }

    void Update()
    {
        HandleWalkingSFX();
    }

    private void HandleWalkingSFX()
    {
        if (_player.GroundDetector.IsGrounded &&
            Mathf.Abs(_player.HorizontalInput) > 0.1f)
        {
            walkTimer += Time.deltaTime;
            if (walkTimer >= walkInterval)
            {
                PlayWalk();
                walkTimer = 0f;
            }
        }
        else
        {
            walkTimer = 0f;
        }
    }

    private void PlayJump() => _audio.PlayOneShot(jumpClip);
    private void PlayWalk() => _audio.PlayOneShot(walkClip);
    private void PlayGrapple() => _audio.PlayOneShot(grappleClip);
    private void PlaySwing() => _audio.PlayOneShot(swingClip);
}
