using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class OptionsScript : MonoBehaviour
{

    [SerializeField]
    private Image selectedMaster, selectedMusic, selectedSFX;
    [SerializeField]
    private Sprite selectedSlider, freeSlider;
    [SerializeField]
    private GameObject backPaper;
    [SerializeField]
    private float backPaperSpeed;

    public void SelectedMasterSlider()
    {
        selectedMaster.sprite = selectedSlider;
    }

    public void FreeMasterSlider()
    {
        selectedMaster.sprite = freeSlider;
    }

    public void SelectedMusicSlider()
    {
        selectedMusic.sprite = selectedSlider;
    }

    public void FreeMusicSlider()
    {
        selectedMusic.sprite = freeSlider;
    }

    public void SelectedSFXSlider()
    {
        selectedSFX.sprite = selectedSlider;
    }

    public void FreeSFXSlider()
    {
        selectedSFX.sprite = freeSlider;
    }

    public void BackButton()
    {
        SettingMenu.Instance.OptionsCloseButton();
    }
}
