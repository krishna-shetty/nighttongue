using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

public class TalkWhenCloseToTami : MonoBehaviour
{
    [Tooltip("The name of the yarn node containing this NPC's sequence of dialogue")]
    public string YarnNode;

    [Tooltip("This NPC's name in the dialogue")]
    public string CharacterName;

    [Tooltip("The LinePresenter attached to this object's dialogue canvas")]
    public LinePresenter LinePresenter;

    [Tooltip("Leave empty if dialogue is triggered by proximity")]
    public InputActionReference DialogueInputAction = null;

    private bool _canTalk = false;
    private DialogueRunner _dialogueRunner;
    private ToggleDialoguePresenter _toggleDialoguePresenter;

    private void Start()
    {
        _toggleDialoguePresenter = FindFirstObjectByType<ToggleDialoguePresenter>();

        _dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        if (!_dialogueRunner)
        {
            //Debug.LogError("TalkWhenCloseToTami :: Could not find DialogueRunner. Disabling script.");
            this.enabled = false;
        }

        //if (YarnNode == null || YarnNode.Length == 0) Debug.LogError("TalkWhenCloseToTami :: No yarn node specified.");

        if (DialogueInputAction != null)
            DialogueInputAction.action.performed += func => StartDialogue(YarnNode, true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerModel"))
        {
            _canTalk = true;
            _toggleDialoguePresenter?.SetNearestPresenter(CharacterName, LinePresenter);
            if (DialogueInputAction == null) StartDialogue(YarnNode, false);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerModel"))
        {
            _canTalk = false;
            _dialogueRunner.Stop();
            _toggleDialoguePresenter.ClearNearestPresenter(CharacterName);
        }
    }

    private void StartDialogue(string node, bool usedButton)
    {
        // prevent button press triggering dialogue when not in range
        if (!_canTalk) return;

        // prevent restarting dialogue with button
        if (_dialogueRunner.IsDialogueRunning && usedButton) return;

        //_dialogueRunner.Stop();
        _dialogueRunner.StartDialogue(node);
    }
}
