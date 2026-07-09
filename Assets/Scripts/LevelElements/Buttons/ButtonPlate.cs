using System.Collections;
using UnityEngine;

public class ButtonPlate : MonoBehaviour
{
    public float TimeToPress = 1f;

    [SerializeField]
    private float _travelDistance = 0.5f;

    private Vector3 _unpressedPosition;
    private Vector3 _pressedPosition;

    private float _progress = 0f;
    private bool _isPressed = false;
    private bool _correctlyPressed = false;
    private ButtonPuzzle _puzzle;

    void Start()
    {
        _unpressedPosition = transform.localPosition;
        _pressedPosition = _unpressedPosition + _travelDistance * Vector3.down;

        if (!_puzzle) Debug.LogError("ButtonPlate :: No puzzle assigned");
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_isPressed && other.CompareTag("Player")) PressButton();
    }

    private void OnTriggerExit(Collider other)
    {
        if (_isPressed && other.CompareTag("Player") && !_correctlyPressed)
        {
            UnpressButton();
            _puzzle.UnpressAllPrevious();
        }
    }

    public void PressButton()
    {
        _progress = Mathf.Clamp01(_progress + Time.deltaTime / TimeToPress);
        transform.localPosition = Vector3.Lerp(_unpressedPosition, _pressedPosition, _progress);

        if (_progress >= 1f)
        {
            _isPressed = true;
            transform.localPosition = _pressedPosition;

            _correctlyPressed = _puzzle.CheckButton(this);
        }
    }

    public void UnpressButton()
    {
        _progress = 0f;
        _isPressed = false;
        _correctlyPressed = false;
        transform.localPosition = _unpressedPosition;
    }

    public void SetPuzzle(ButtonPuzzle p) { _puzzle = p; }
}
