using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [SerializeField]
    private RectTransform healthBar;

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float width = (Mathf.Max(currentHealth, 0f) / maxHealth) * 200f;
        healthBar.sizeDelta = new Vector2(width, healthBar.sizeDelta.y);
        healthBar.anchoredPosition = new Vector2(25 + width / 2f, healthBar.anchoredPosition.y);
    }
}
