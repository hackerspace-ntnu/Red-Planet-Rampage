using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUDController : MonoBehaviour
{
    [SerializeField]
    private RectTransform healthBar;

    [SerializeField]
    private GameObject deathScreen;

    [SerializeField]
    private TMP_Text deathText;

    [SerializeField]
    private float tweenDuration = .07f;

    [SerializeField]
    private float damageBorderFlashDuration = .2f;

    private float damageBorderTop = .2f;

    private float persistentDamageBorder = 0;

    private Material damageBorder;

    void Start()
    {
        var image = GetComponent<RawImage>();
        // Prevent material properties from being handled globally
        damageBorder = Instantiate(image.material);
        image.material = damageBorder;
        damageBorder.SetFloat("_Intensity", 0);
    }

    public void OnDamageTaken(float damage, float currentHealth, float maxHealth)
    {
        UpdateHealthBar(currentHealth, maxHealth);
        LeanTween.value(gameObject, UpdateDamageBorder, 0f, 1f, damageBorderFlashDuration);
        // Leave a persistent red border at low health, softly increasing as the player inches closer to death
        persistentDamageBorder = Mathf.Clamp01(Mathf.Log10(1.2f * maxHealth / (currentHealth + maxHealth * .01f)));
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float width = (Mathf.Max(currentHealth, 0f) / maxHealth) * 200f;
        LeanTween.size(healthBar, new Vector2(width, healthBar.sizeDelta.y), tweenDuration);
        LeanTween.move(healthBar, new Vector2(25 + width / 2f, healthBar.anchoredPosition.y), tweenDuration);
    }

    public void UpdateDamageBorder(float intensity)
    {
        if (intensity < damageBorderTop)
        {
            // Start of curve, reach the top quickly
            // Always start at 0 so we get proper damage indication each time we get hit
            intensity = intensity * (1 / damageBorderTop);
        }
        else
        {
            // End of the curve, fade out more slowly to persistent border
            intensity = Mathf.Lerp(persistentDamageBorder, 1, 1 - (intensity - damageBorderTop) * (1 / (1 - damageBorderTop)));
        }
        damageBorder.SetFloat("_Intensity", intensity);
    }

    public void DisplayDeathScreen(PlayerIdentity killer)
    {
        deathText.text = killer.playerName;
        deathText.color = killer.color;
        deathScreen.SetActive(true);
    }
}
