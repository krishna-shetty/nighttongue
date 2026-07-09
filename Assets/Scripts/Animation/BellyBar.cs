using UnityEngine;

public class BellyBar : MonoBehaviour
{
    private PlayerInputHandler _playerInputHandler;

    [SerializeField] private Animator animator;
    [SerializeField] private string idleStateName = "Turn_Sofa_idle";

    private void Awake()
    {
        _playerInputHandler = GetComponentInParent<PlayerInputHandler>();

        if (animator == null)
            animator = GetComponentInParent<Animator>();
    }

    private void Start()
    {
        if (_playerInputHandler != null)
            _playerInputHandler.SetFrozen(true);

        if (animator != null)
            animator.Play(idleStateName, 0, 0f);
    }

    private void OnDisable()
    {
        if (_playerInputHandler != null)
            _playerInputHandler.SetFrozen(false);
    }
}