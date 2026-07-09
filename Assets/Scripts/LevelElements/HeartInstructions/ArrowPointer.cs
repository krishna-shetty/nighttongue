using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.GPUSort;

public class ArrowPointer : MonoBehaviour
{
    public static ArrowPointer Instance { get; private set; }

    public GameObject LeftArrow;
    public GameObject RightArrow;

    [Header("Displayed for debug. Assign goalpoints in InstructionHolder, not here")]

    [Tooltip("Auto-populates via InstructionHolder")]
    public List<GameObject> GoalPoints = new();
    public GameObject CurrentGoal = null;
    public int CurrentGoalIndex = 0;

    private float ScreenEdgeMargin = 50f;
    private float SmoothTime = 0.2f;

    private GameObject _player;
    private GameObject _arrow;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Initialize()
    {
        _player = FindFirstObjectByType<PlayerController>().gameObject;
        _arrow = GetComponentInChildren<Image>().gameObject;

        if (!LeftArrow || !RightArrow) Debug.LogError("ArrowPointer :: Arrow images not assigned");
        else SetBothArrowsActive(false, false);

        if (CurrentGoalIndex >= 0 && CurrentGoalIndex < GoalPoints.Count) CurrentGoal = GoalPoints[CurrentGoalIndex];
    }

    private void Update()
    {
        // debug
        if (Input.GetKeyDown(KeyCode.Y)) UpdateToNextGoal();

        if (CurrentGoal)
        {
            if (IsOnScreen(CurrentGoal)) SetBothArrowsActive(false, false);
            else UpdateArrows();
        }
    }

    private void UpdateArrows()
    {
        Vector3 dir = (CurrentGoal.transform.position - _player.transform.position).normalized;
        if (dir.x < 0) SetBothArrowsActive(true, false);
        else SetBothArrowsActive(false, true);
    }

    public void SetGoal(GameObject obj)
    {
        if (this != Instance)
        {
            Instance.SetGoal(obj);
            return;
        }

        CurrentGoal = obj;
    }

    public void UpdateToNextGoal()
    {
        if (this != Instance)
        {
            Instance.UpdateToNextGoal();
            return;
        }

        CurrentGoalIndex++;
        Debug.LogWarning("Attempting to point arrow to next goal: " + CurrentGoalIndex);

        if (CurrentGoalIndex >= GoalPoints.Count)
        {
            CurrentGoal = null;
            CurrentGoalIndex = GoalPoints.Count;
            SetBothArrowsActive(false, false);
            Debug.LogWarning("ArrowPointer :: Attempted to set next goal, but reached the end of the list");
            return;
        }

        CurrentGoal = GoalPoints[CurrentGoalIndex];
    }

    private void SetBothArrowsActive(bool left, bool right)
    {
        LeftArrow.SetActive(left);
        RightArrow.SetActive(right);
    }

    private bool IsOnScreen(GameObject obj)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(obj.transform.position);
        return screenPos.z > 0 &&
            screenPos.x > 0 && screenPos.x < Screen.width &&
            screenPos.y > 0 && screenPos.y < Screen.height;
    }

    private void DynamicArrowCodeIWorkedHardOnThatGotDumped()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(CurrentGoal.transform.position);
        if (screenPos.z < 0) screenPos.z *= -1;

        screenPos.x = Mathf.Clamp(screenPos.x, ScreenEdgeMargin, Screen.width - ScreenEdgeMargin);
        screenPos.y = Mathf.Clamp(screenPos.y, ScreenEdgeMargin, Screen.height - ScreenEdgeMargin);

        Vector3 screenCenter = new(Screen.width / 2f, Screen.height / 2f, 0);
        Vector3 dir = (screenPos - screenCenter).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Vector3 velocity = Vector3.zero;
        _arrow.transform.position = Vector3.SmoothDamp(
            _arrow.transform.position,
            screenPos,
            ref velocity,
            SmoothTime
        );

        Quaternion targetRot = Quaternion.Euler(0, 0, angle);
        _arrow.transform.rotation = Quaternion.Slerp(
            _arrow.transform.rotation,
            targetRot,
            Time.deltaTime
        );
    }
}
