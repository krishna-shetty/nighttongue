using System;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class ToggleDialoguePresenter : DialoguePresenterBase
{
    [Serializable]
    public class DialogueBubble
    {
        public string CharacterName;
        public LinePresenter LinePresenter;
        public CanvasGroup CanvasGroup;
    }

    public List<DialogueBubble> DialogueCanvasGroups;
    private Dictionary<string, LinePresenter> _nearestPresenters = new(); // nearest linepresenter for each npc

    private string _lastCharacterName = "Tami";

    public override YarnTask OnDialogueStartedAsync() => YarnTask.CompletedTask;

    public override YarnTask OnDialogueCompleteAsync()
    {
        foreach (DialogueBubble db in DialogueCanvasGroups) db.CanvasGroup.alpha = 0;
        return YarnTask.CompletedTask;
    }

    public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        var speaker = line.CharacterName ?? _lastCharacterName;
        _lastCharacterName = speaker;
        LinePresenter presenter = _nearestPresenters.GetValueOrDefault(speaker);
        Debug.Log(speaker + " " + presenter);

        // toggle off all canvases except correct one
        foreach (DialogueBubble db in DialogueCanvasGroups)
        {
            if (db.CharacterName.Equals(speaker))
            {
                if (presenter && db.LinePresenter != presenter) continue;
                else if (!presenter) presenter = db.LinePresenter;
                db.CanvasGroup.alpha = 1;
            }
            else
            {
                db.CanvasGroup.alpha = 0;
            }
        }

        if (!presenter) Debug.LogError("ToggleDialoguePresenter :: Could not find LinePresenter for character " + speaker);
        await presenter.RunLineAsync(line, token);
    }

    public void SetNearestPresenter(string name, LinePresenter presenter)
    {
        _nearestPresenters[name] = presenter;
    }

    public void ClearNearestPresenter(string name)
    {
        _nearestPresenters.Remove(name);
    }
}
