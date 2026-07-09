using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class MovingPlatform : MonoBehaviour
{
    public enum PointChoice {TriggerPoint, AnchorPoint};

    [SerializeField] private PointChoice _triggerOrAnchor = PointChoice.TriggerPoint;
    public PointChoice triggerOrAnchor => _triggerOrAnchor;

    // Trigger Point fields
    public bool verticalMovement = false;
    public float verticalMoveDistance;
    public float horizontalMoveDistance;
    public List<DirectionalGuide> triggerPoints;

    // Anchor Point fields
    public List<Transform> anchorPoints;

    public float moveSpeed = 1f;

    [SerializeField] private bool debugVisualsVisible = true;

    private Vector3 _currentVelocity;
    private Vector3 _previousPosition;

    public Vector3 GetCurrentVelocity() => _currentVelocity;

    private Vector3 _baseLocation;
    private bool _movingRightOrUp = true;
    private float _rightLimit;
    private float _leftLimit;
    private float _upLimit;
    private float _downLimit;

    private float _triggerCooldown = 0.25f;
    private float _lastTriggerTime = -999f;

    private string _currentMovement;
    private DirectionalGuide _activeTrigger;

    private Transform _activeAnchor;
    private Transform _nextAnchor;
    private int _activeAnchorIndex = -1;

    private const float arriveEpsilon = 0.02f;
    private const float rangeBorderThickness = 0.3f;
    private static readonly Color rangeBorderColor = new Color(1f, 1f, 0f, 0.6f); // yellow
    private GameObject[] _rangeBorders = new GameObject[4];
    private Material[] _rangeMats = new Material[4];
    
    [SerializeField] private AK.Wwise.Event changeDirectionSound;

    private void OnValidate()
    {
        SetPointsVisibility(debugVisualsVisible);
    }

    private void OnEnable()
    {
        CreateRangeVisual();
    }

    private void OnDisable()
    {
        DestroyRangeVisual();
    }

    private void SetPointsVisibility(bool visible)
    {
        if (_triggerOrAnchor == PointChoice.TriggerPoint && triggerPoints != null)
        {
            foreach (var point in triggerPoints)
            {
                if (point == null) continue;
                var mr = point.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = visible;
            }
        }
        else if (anchorPoints != null)
        {
            foreach (var point in anchorPoints)
            {
                if (point == null) continue;
                var mr = point.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = visible;
            }
        }

        SetRangeVisibility(visible);
    }

    private void CreateRangeVisual()
    {
        if (_rangeBorders[0] != null) return;

        for (int i = 0; i < 4; i++)
        {
            _rangeBorders[i] = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _rangeBorders[i].name = "_MoveRangeBorder_" + i;
            _rangeBorders[i].hideFlags = HideFlags.DontSave;

            DestroyImmediate(_rangeBorders[i].GetComponent<Collider>());

            var rend = _rangeBorders[i].GetComponent<MeshRenderer>();
            _rangeMats[i] = CameraTriggerRange.MakeTransparent(rend, rangeBorderColor);
            _rangeMats[i].renderQueue = 3001;
            rend.enabled = debugVisualsVisible && _triggerOrAnchor == PointChoice.TriggerPoint;
        }

        SyncRangeVisual();
    }

    private void DestroyRangeVisual()
    {
        for (int i = 0; i < 4; i++)
        {
            if (_rangeMats[i] != null)
            {
                if (Application.isPlaying) Destroy(_rangeMats[i]);
                else DestroyImmediate(_rangeMats[i]);
                _rangeMats[i] = null;
            }
            if (_rangeBorders[i] != null)
            {
                if (Application.isPlaying) Destroy(_rangeBorders[i]);
                else DestroyImmediate(_rangeBorders[i]);
                _rangeBorders[i] = null;
            }
        }
    }

    private void SyncRangeVisual()
    {
        if (_rangeBorders[0] == null) return;

        Vector3 center = Application.isPlaying ? _baseLocation : transform.position;
        float hw = horizontalMoveDistance;
        float hh = verticalMoveDistance;
        const float z = -5f;

        // top
        _rangeBorders[0].transform.position = new Vector3(center.x, center.y + hh, z);
        _rangeBorders[0].transform.localScale = new Vector3(horizontalMoveDistance * 2f, rangeBorderThickness, 1f);
        // bottom
        _rangeBorders[1].transform.position = new Vector3(center.x, center.y - hh, z);
        _rangeBorders[1].transform.localScale = new Vector3(horizontalMoveDistance * 2f, rangeBorderThickness, 1f);
        // left
        _rangeBorders[2].transform.position = new Vector3(center.x - hw, center.y, z);
        _rangeBorders[2].transform.localScale = new Vector3(rangeBorderThickness, verticalMoveDistance * 2f, 1f);
        // right
        _rangeBorders[3].transform.position = new Vector3(center.x + hw, center.y, z);
        _rangeBorders[3].transform.localScale = new Vector3(rangeBorderThickness, verticalMoveDistance * 2f, 1f);
    }

    private void SetRangeVisibility(bool visible)
    {
        bool show = visible && _triggerOrAnchor == PointChoice.TriggerPoint;
        for (int i = 0; i < 4; i++)
        {
            if (_rangeBorders[i] != null)
                _rangeBorders[i].GetComponent<MeshRenderer>().enabled = show;
        }
    }

    void Start()
    {
        if (!Application.isPlaying) return;
        _baseLocation = transform.position;
        _previousPosition = transform.position;

        float halfWidth = 0f;
        float halfHeight = 0f;
        var col = GetComponent<BoxCollider>();
        if (col != null)
        {
            halfWidth = col.size.x * transform.lossyScale.x * 0.5f;
            halfHeight = col.size.y * transform.lossyScale.y * 0.5f;
        }

        _rightLimit = _baseLocation.x + horizontalMoveDistance - halfWidth;
        _leftLimit = _baseLocation.x - horizontalMoveDistance + halfWidth;
        _upLimit = _baseLocation.y + verticalMoveDistance - halfHeight;
        _downLimit = _baseLocation.y - verticalMoveDistance + halfHeight;
    }

    void FixedUpdate()
    {
        if (!Application.isPlaying) return;
        _currentVelocity = (transform.position - _previousPosition) / Time.deltaTime;
        _previousPosition = transform.position;
        Move();
    }

    void Update()
    {
        SyncRangeVisual();
    }

    private bool IsHittingTriggerPoint()
    {
        foreach (DirectionalGuide trigger in triggerPoints)
        {
            if (trigger == null) continue;
            if (Vector3.Distance(trigger.transform.position, transform.position) < 0.1f)
            {
                _activeTrigger = trigger;
                return true;
            }
        }

        return false;
    }

    private bool ReverseRightOrUp()
    {

        if (
            _activeTrigger.GetComponent<DirectionalGuide>().MovingRight == DirectionalGuide.VerticalChoice.Down &&
            _currentMovement == "Right"
            ) // If right to down && currently moving right
        {
            return true;
        }
        else if (
            _activeTrigger.GetComponent<DirectionalGuide>().MovingLeft == DirectionalGuide.VerticalChoice.Up &&
            _currentMovement == "Left"
            ) // If left to up && currently moving left
        {
            return true;
        }
        else if (
            _activeTrigger.GetComponent<DirectionalGuide>().MovingUp == DirectionalGuide.HorizontalChoice.Left &&
            _currentMovement == "Up"
            ) // If up to left && currently moving up
        {
            return true;
        }
        else if (
            _activeTrigger.GetComponent<DirectionalGuide>().MovingDown == DirectionalGuide.HorizontalChoice.Right &&
            _currentMovement == "Down"
            ) // If down to right && currently moving down
        {
            return true;
        }

        return false;

    }

    private void MoveLeft()
    {
        transform.position = transform.position + Vector3.left * Time.deltaTime * moveSpeed;
    }

    private void MoveRight()
    {
        transform.position = transform.position + Vector3.right * Time.deltaTime * moveSpeed;
    }

    private void MoveUp()
    {
        transform.position = transform.position + Vector3.up * Time.deltaTime * moveSpeed;
    }

    private void MoveDown()
    {
        transform.position = transform.position + Vector3.down * Time.deltaTime * moveSpeed;
    }

    protected void TriggerMove()
    {
        if (IsHittingTriggerPoint() && Time.time > _lastTriggerTime + _triggerCooldown) // If hitting TriggerPoint
        {
            _lastTriggerTime = Time.time;
            verticalMovement = !verticalMovement;

            if (ReverseRightOrUp())
            {
                _movingRightOrUp = !_movingRightOrUp;
            }
            PlayChangeDirectionSound();
        }
        else if (
            (_currentMovement == "Up" && transform.position.y > _upLimit) ||
            (_currentMovement == "Right" && transform.position.x > _rightLimit) ||
            (_currentMovement == "Down" && transform.position.y < _downLimit) ||
            (_currentMovement == "Left" && transform.position.x < _leftLimit)
        ) // If hitting boundary
        {
            _movingRightOrUp = !_movingRightOrUp;
            PlayChangeDirectionSound();
        }
        
        if (verticalMovement)
        {
            if (_movingRightOrUp)
            {
                _currentMovement = "Up";
                MoveUp();
            }
            else
            {
                _currentMovement = "Down";
                MoveDown();
            }
        }
        else
        {
            if (_movingRightOrUp)
            {
                _currentMovement = "Right";
                MoveRight();
            }
            else
            {
                _currentMovement = "Left";
                MoveLeft();
            }
        }

    }

    private void MoveInNewDirection()
    {
        if (anchorPoints == null || anchorPoints.Count == 0)
            return;

        Vector3 targetPos;
        if (_activeAnchorIndex >= 0 && _nextAnchor != null)
            targetPos = _nextAnchor.position;
        else
            targetPos = anchorPoints[0].position;

        targetPos.z = transform.position.z;
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
    }

    protected void AnchorMove()
    {
        if (anchorPoints == null || anchorPoints.Count == 0)
            return;

        if (anchorPoints.Count == 1)
        {
            Vector3 singleTarget = anchorPoints[0].position;
            singleTarget.z = transform.position.z;
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, singleTarget, step);
            return;
        }

        if (_activeAnchorIndex < 0)
        {
            _activeAnchorIndex = 0;
            _activeAnchor = anchorPoints[_activeAnchorIndex];
            _nextAnchor = anchorPoints[(_activeAnchorIndex + 1) % anchorPoints.Count];
        }

        MoveInNewDirection();

        Vector3 nextPos = _nextAnchor != null ? new Vector3(_nextAnchor.position.x, _nextAnchor.position.y, transform.position.z) : transform.position;
        if (_nextAnchor != null && Vector3.SqrMagnitude(transform.position - nextPos) <= arriveEpsilon * arriveEpsilon)
        {
            transform.position = nextPos;

            _activeAnchorIndex = (_activeAnchorIndex + 1) % anchorPoints.Count;
            _activeAnchor = anchorPoints[_activeAnchorIndex];
            _nextAnchor = anchorPoints[(_activeAnchorIndex + 1) % anchorPoints.Count];
            
            PlayChangeDirectionSound();
        }
    }

    protected void Move()
    {
        if (triggerOrAnchor == PointChoice.TriggerPoint)
        {
            TriggerMove();
        }
        else
        {
            if (anchorPoints.Count == 0)
            {
               TriggerMove(); 
            }
            else
            {
                AnchorMove();
            }
        }
    }

    private void PlayChangeDirectionSound()
    {
        if (changeDirectionSound != null && changeDirectionSound.IsValid())
        {
            changeDirectionSound.Post(gameObject);
        }
    }

}
