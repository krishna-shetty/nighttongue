using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonPuzzle : MonoBehaviour
{
    public List<ButtonPlate> ButtonOrder = new List<ButtonPlate>();
    public UnityEvent OnPuzzleSolved;

    private int _index = 0;
    private bool _solved = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        foreach (ButtonPlate b in ButtonOrder)
        {
            if (b) b.SetPuzzle(this);
        }
    }

    public bool CheckButton(ButtonPlate b)
    {
        if (_solved) return false;

        if (ButtonOrder[_index] != b) return false;

        _index++;
        if (_index >= ButtonOrder.Count)
        {
            _solved = true;
            OnPuzzleSolved.Invoke();
            Debug.Log("invoked puzzle solve");
        }
        return true;
    }

    public void UnpressAllPrevious()
    {
        if (_solved) return;

        for (int i = 0; i < _index; i++)
        {
            ButtonOrder[i].UnpressButton();
        }
        _index = 0;
    }
}
