using UnityEngine;

public class DirectionalGuide : MonoBehaviour
{

    public enum VerticalChoice {Up, Down};
    public enum HorizontalChoice {Left, Right};
    [Header("If the platform has been moving left/right/top/bottom, choose its next direction:")]
    [SerializeField] private VerticalChoice movingLeft = VerticalChoice.Up;
    [SerializeField] private VerticalChoice movingRight = VerticalChoice.Up;
    [SerializeField] private HorizontalChoice movingUp = HorizontalChoice.Left;
    [SerializeField] private HorizontalChoice movingDown = HorizontalChoice.Left;
    public VerticalChoice MovingLeft => movingLeft;
    public VerticalChoice MovingRight => movingRight;
    public HorizontalChoice MovingUp => movingUp;
    public HorizontalChoice MovingDown => movingDown;

}
