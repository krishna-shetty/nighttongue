using System.Collections.Generic;
using UnityEngine;

public class UpdateHealthUI : MonoBehaviour
{
    public PlayerHealth _health;

    [Tooltip("Components that disappear as health goes down. ORDER MATTERS (last disappears first)")]
    public List<GameObject> UIComponents;

    private int currentIndex = -1;

    void Awake()
    {
        if (_health) _health.OnDamage += UpdateUIOnDamage;
        currentIndex = UIComponents.Count - 1;
    }

    private void UpdateUIOnDamage(GameObject victim, GameObject source, bool knockback)
    {
        if (currentIndex >= 0)
        {
            UIComponents[currentIndex].SetActive(false);
            currentIndex--;
        }
    }
}
