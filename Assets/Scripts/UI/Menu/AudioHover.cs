using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class AudioHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject backPaper;
    [SerializeField]
    private float backPaperSpeed;
    public enum PostTypes
    {
        Sound,
        Display,
        Misc
    }
    [SerializeField]
    private PostTypes postType;

    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (postType)
        {
            case PostTypes.Sound:
                EnteredSound();
                break;
            case PostTypes.Display:
                EnteredDisplay();
                break;
            case PostTypes.Misc:
                EnteredMisc();
                break;
            default:
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void EnteredSound()
    {
        backPaper.transform.DOLocalMoveX(0f, backPaperSpeed).SetUpdate(true);
    }

    public void EnteredDisplay()
    {
        backPaper.transform.DOLocalMoveX(460f, backPaperSpeed).SetUpdate(true);
    }

    public void EnteredMisc()
    {
        backPaper.transform.DOLocalMoveX(905f, backPaperSpeed).SetUpdate(true);
    }
}
