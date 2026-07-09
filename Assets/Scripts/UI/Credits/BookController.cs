using UnityEngine;
using UnityEngine.InputSystem;

public class BookController : MonoBehaviour
{
    private Book _book;
    [SerializeField]
    private float _sensitivity = 500f;

    private Vector2 _virtualPos;
    private bool _isDragging;
    private bool _dragStartedRight;

    private void Awake()
    {
        _book = GetComponent<Book>();
    }

    void Start()
    {
        _virtualPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    void Update()
    {
        if (Gamepad.current == null) return;

        Vector2 stick = Gamepad.current.leftStick.ReadValue();

        if (Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            _virtualPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
            _isDragging = false;
            _dragStartedRight = false;
        }

        if (Gamepad.current.buttonSouth.isPressed && stick.magnitude > 0.1f)
        {
            _virtualPos += stick * _sensitivity * Time.deltaTime;
            _virtualPos.x = Mathf.Clamp(_virtualPos.x, 0, Screen.width);
            _virtualPos.y = Mathf.Clamp(_virtualPos.y, 0, Screen.height);

            if (!_isDragging)
            {
                if (stick.x > 0.3f)
                {
                    if (_book.currentPage <= 0) return;
                    _book.DragLeftPageToPoint(_book.transformPoint(_virtualPos));
                    _dragStartedRight = false;
                    _isDragging = true;
                }
                else if (stick.x < -0.3f)
                {
                    if (_book.currentPage >= _book.TotalPageCount) return;
                    _book.DragRightPageToPoint(_book.transformPoint(_virtualPos));
                    _dragStartedRight = true;
                    _isDragging = true;
                }
            }
            else
            {
                if (_dragStartedRight)
                    _book.UpdateBookRTLToPoint(_book.transformPoint(_virtualPos));
                else
                    _book.UpdateBookLTRToPoint(_book.transformPoint(_virtualPos));
            }
        }

        if (Gamepad.current.buttonSouth.wasReleasedThisFrame && _isDragging)
        {
            _book.ReleasePage();
            _isDragging = false;
        }
    }
}