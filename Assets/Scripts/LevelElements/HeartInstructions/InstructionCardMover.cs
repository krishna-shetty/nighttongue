using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ProBuilder.Shapes;

public class InstructionCardMover : MonoBehaviour
{
    public static InstructionCardMover Instance { get; private set; }

    [Tooltip("What the local y-position of a card should be when it's active")]
    public float YPosWhenActive = 0f;

    public float MoveDuration = 0.25f;

    [Header("Displayed for debug")]

    [Tooltip("Auto-populates via InstructionHolder")]
    public List<GameObject> Cards;
    public int CurrentCardIndex = -1;

    [Header("Door event")]
    public int CardIndexForDoor = -1;
    public UnityEvent MoveDoorOnCardMovedDown;

    private GameObject _activeCard = null;
    private Vector3 _initialPos;

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
        if (CurrentCardIndex > -1 && CurrentCardIndex < Cards.Count)
        {
            MoveCardDown(Cards[CurrentCardIndex]);
        }
    }

    private void Update()
    {
        // testing
        if (Input.GetKeyDown(KeyCode.Y))
        {
            MoveNextCardDown();
        }
    }

    public void MoveNextCardDown()
    {
        if (this != Instance)
        {
            Instance.MoveNextCardDown();
            return;
        }

        CurrentCardIndex++;

        if (CurrentCardIndex >= Cards.Count)
        {
            CurrentCardIndex = Cards.Count;
            MoveObjectLocally(_activeCard, _initialPos);
            Debug.LogWarning("InstructionCardMover :: Attempted to move next card down, but reached the end of the list");
            return;
        }

        MoveCardDown(Cards[CurrentCardIndex]);
    }

    public void MoveCardDown(GameObject card)
    {
        if (this != Instance)
        {
            Instance.MoveCardDown(card);
            return;
        }

        if (card == _activeCard) return;

        MoveObjectLocally(_activeCard, _initialPos); // move previous instruction back up

        _initialPos = card.transform.localPosition;
        _activeCard = card;
        CurrentCardIndex = Cards.IndexOf(card);

        MoveObjectLocally(card, new(_initialPos.x, YPosWhenActive, _initialPos.z));

        if (CurrentCardIndex == CardIndexForDoor) MoveDoorOnCardMovedDown?.Invoke();
    }

    private void MoveObjectLocally(GameObject obj, Vector3 target)
    {
        if (!obj) return;
        obj.transform.DOLocalMove(target, MoveDuration);
    }
}
