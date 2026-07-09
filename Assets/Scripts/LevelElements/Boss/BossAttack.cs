using UnityEngine;

public class BossAttack : MonoBehaviour
{
    public UnityEngine.Events.UnityEvent OnSetSlam;
    public UnityEngine.Events.UnityEvent OnImpact;
    
    public static BossAttack Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [SerializeField]
    private float hoverAbovePlayer, moveSpeed, slamTimer, ascendSpeed;
    [SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    private bool _isSlamming = false;
    private float _timer = 0.0f;
    private GameObject _player;
    private RaycastHit _hit;
    private bool _hasTarget;
    private (float, float) _bossSize;
    [SerializeField]
    private bool _slammingDown = true;
    private Vector3 _slamUpPosition;
    public bool startAttack = false;
    private float _currentSpeed = 0f;
    [SerializeField]
    private float initialSlamSpeed, maxSlamSpeed, accelerationRate;
    [SerializeField]
    private GameObject attackIndicator;
    [SerializeField]
    private GameObject bossSprite;
    private Camera _playerCamera;
    private float _cameraHeight, _cameraWidth;
    private Vector3 _cameraCenter;
    private PlayerController _playerController;
    public enum PlayerMoveState { NotMoving, MovingLeft, MovingRight}
    private PlayerMoveState _moveState = PlayerMoveState.NotMoving;
    private Animator bossAnimation;
    private float bossSpriteSpeed = 3.7f;
    [SerializeField]
    private float bossMaxDraggableDistance;
    [SerializeField]
    private float slamAnimationTriggerDistance;
    private bool _slamAnimationTriggered = false;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GameObject.FindWithTag("Player");
        _playerController = _player.GetComponent<PlayerController>();
        _playerCamera = _player.transform.Find("Camera").Find("Main Camera").gameObject.GetComponent<Camera>();
        _cameraHeight = 2f * _playerCamera.orthographicSize;
        _cameraWidth = _cameraHeight * _playerCamera.aspect;
        RecalculateCameraCenter();
        bossSprite = Instantiate(bossSprite, _cameraCenter, Quaternion.identity);
        bossAnimation = bossSprite.GetComponent<Animator>();
        _bossSize = MeshSize.GetSize(gameObject);
        attackIndicator = Instantiate(attackIndicator, transform.position, Quaternion.identity);
        attackIndicator.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (startAttack)
        {
            if (!_isSlamming)
            {
                _timer += Time.fixedDeltaTime;
                HoverAbovePlayer();
                if (_timer > slamTimer)
                {
                    SetSlam();
                    //play animation that runs setSlam
                    OnSetSlam?.Invoke();
                }
            }
            else
            {
                if (_slammingDown)
                {
                    SlamDown();
                }
                else
                {
                    SlamUp();
                    
                }
            }
        }
        else
        {
            HoverAbovePlayer();
        }
        MoveSprite();
    }

    private void MoveSprite()
    {
        DetectPlayerMove();
        RecalculateCameraCenter();
        Vector3 initialBossTransform = bossSprite.transform.position;
        switch (_moveState)
        {
            case (PlayerMoveState.NotMoving):
                {
                    break;
                }

            case (PlayerMoveState.MovingLeft):
                {
                    initialBossTransform.x += bossSpriteSpeed * Time.fixedDeltaTime;
                    break;
                }

            case (PlayerMoveState.MovingRight):
                {
                    initialBossTransform.x -= bossSpriteSpeed * Time.fixedDeltaTime;
                    break;
                }

            default:
                {
                    break;
                }
        }
        initialBossTransform.x = Mathf.Clamp(initialBossTransform.x, _cameraCenter.x - bossMaxDraggableDistance, _cameraCenter.x + bossMaxDraggableDistance);
        bossSprite.transform.position = initialBossTransform;
    }

    private void HoverAbovePlayer()
    {
        transform.position = Vector3.MoveTowards(transform.position, _player.transform.position + new Vector3(0, hoverAbovePlayer, 0), moveSpeed * Time.fixedDeltaTime);
    }

    private void GetSlamTarget()
    {
        _hasTarget = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out _hit, 1000f, layerMask);
        Debug.Log("hit point: "+_hit.point);
        attackIndicator.SetActive(true);
        attackIndicator.transform.position = _hit.point;
        if (_hasTarget)
        {
            _hit.point = _hit.point + new Vector3(0, _bossSize.Item2 / 2, 0);
        }
    }

    private void SlamDown()
    {
        if (_hasTarget)
        {
            _currentSpeed += accelerationRate * Time.deltaTime;
            _currentSpeed = Mathf.Min(_currentSpeed, maxSlamSpeed);
            transform.position = Vector3.MoveTowards(transform.position, _hit.point, _currentSpeed * Time.fixedDeltaTime);
            if((slamAnimationTriggerDistance >= Vector3.Distance(_hit.point, transform.position)) && !_slamAnimationTriggered)
            {
                Debug.Log("Distance: "+ Vector3.Distance(_hit.point, transform.position));
                bossAnimation.SetBool("IsAttacking", true);
                _slamAnimationTriggered = true;
            }
            if (Vector3.Distance(transform.position, _hit.point) < 0.001f)
            {
                Debug.Log(bossAnimation);
                bossAnimation.SetBool("IsAttacking", false);
                _slammingDown = false;
                _currentSpeed = initialSlamSpeed;
                attackIndicator.SetActive(false);
                _slamAnimationTriggered = false;
                OnImpact?.Invoke();
            }
        }
        else
        {
            _isSlamming = false;
        }       
    }

    private void SlamUp()
    {
        transform.position = Vector3.MoveTowards(transform.position, _slamUpPosition, ascendSpeed * Time.fixedDeltaTime);
        if (Vector3.Distance(transform.position, _slamUpPosition) < 0.001f)
        {
            _isSlamming = false;
            _slammingDown = true;
            OnImpact?.Invoke();
        }
    }

    public void StartAttack()
    {
        startAttack = true;
    }
    public void StopAttack()
    {
        startAttack = false;
    }

    private void RecalculateCameraCenter()
    {
        Vector3 tempPosition = _playerCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, _playerCamera.nearClipPlane));
        tempPosition.z = 5f;
        _cameraCenter = tempPosition;
    }

    private void DetectPlayerMove()
    {
        if (_playerController.Velocity.x == 0)
        {
            _moveState = PlayerMoveState.NotMoving;
        }
        else if(_playerController.Velocity.x > 0)
        {
            _moveState = PlayerMoveState.MovingRight;
        }
        else
        {
            _moveState = PlayerMoveState.MovingLeft;
        }
    }

    public void SetSlam()
    {
        Debug.Log("setting Slam");
        _isSlamming = true;
        _timer = 0.0f;
        _slamUpPosition = transform.position;
        GetSlamTarget();
    }
}
