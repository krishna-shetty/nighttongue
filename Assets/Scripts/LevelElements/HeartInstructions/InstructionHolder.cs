using System;
using System.Collections.Generic;
using UnityEngine;

public class InstructionHolder : MonoBehaviour
{
    [Serializable]
    public class InstructionData
    {
        public GameObject Card;
        public GameObject GoalPoint;
        public bool HasAssociatedCheckpoint = false;
        public int AssociatedCheckpoint = -1;
    }

    [Tooltip("Only specify 'Associated Checkpoint' for instructions that should be the first displayed upon loading a specific checkpoint")]
    public List<InstructionData> Instructions;

    void Start()
    {
        var cardMover = InstructionCardMover.Instance;
        var arrowScript = ArrowPointer.Instance;
        var checkpoint = SaveManager.Instance.currCheckpoint;

        cardMover.Cards.Clear();
        arrowScript.GoalPoints.Clear();

        int minAssociatedCheckpoint = 0;
        foreach (InstructionData d in Instructions)
        {
            if (d.Card) cardMover.Cards.Add(d.Card);
            if (d.GoalPoint) arrowScript.GoalPoints.Add(d.GoalPoint);
            if (d.HasAssociatedCheckpoint
                && d.AssociatedCheckpoint <= checkpoint
                && d.AssociatedCheckpoint >= minAssociatedCheckpoint)
            {
                minAssociatedCheckpoint = d.AssociatedCheckpoint;
                cardMover.CurrentCardIndex = cardMover.Cards.Count - 1;
                arrowScript.CurrentGoalIndex = arrowScript.GoalPoints.Count - 1;
            }
        }

        cardMover.Initialize();
        arrowScript.Initialize();
    }
}
