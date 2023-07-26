using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUDController : MonoBehaviour
{
    [SerializeField]
    private RectTransform healthBar;

    [SerializeField]
    private RectTransform ammoHud;

    [SerializeField]
    private SpriteRenderer ammoBar;

    private Material ammoCapacityMaterial;

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

    private float healthBarScaleX;

    private Material damageBorder;

    void Start()
    {
        var image = GetComponent<RawImage>();
        // Prevent material properties from being handled globally
        damageBorder = Instantiate(image.material);
        image.material = damageBorder;
        damageBorder.SetFloat("_Intensity", 0);

        ammoCapacityMaterial = Instantiate(ammoBar.material);
        ammoBar.material = ammoCapacityMaterial;
        ammoCapacityMaterial.SetFloat("_Arc2", 0);

        healthBarScaleX = healthBar.localScale.x;
    }

    public void OnDamageTaken(float damage, float currentHealth, float maxHealth)
    {
        UpdateHealthBar(currentHealth, maxHealth);
        LeanTween.value(gameObject, UpdateDamageBorder, 0f, 1f, damageBorderFlashDuration);
        // Leave a persistent red border at low health, softly increasing as the player inches closer to death
        persistentDamageBorder = Mathf.Clamp01(Mathf.Log10(1.2f * maxHealth / (currentHealth + maxHealth * .01f)));
    }

    public void UpdateOnFire(float ammoPercent)
    {
        if (LeanTween.isTweening(ammoHud.gameObject))
        {
            LeanTween.cancel(ammoHud.gameObject);
            ammoBar.gameObject.transform.eulerAngles = new Vector3(ammoBar.gameObject.transform.eulerAngles.x, ammoBar.gameObject.transform.eulerAngles.y, 0);
        }
        
        ammoCapacityMaterial.SetFloat("_Arc2", (1-ammoPercent)*360);
        ammoHud.gameObject.LeanRotateAroundLocal(Vector3.forward, 30, 0.5f).setEaseSpring()
            .setOnStart(
            () => ammoBar.gameObject.transform.Rotate(new Vector3(0, 0, -30)));
    }

    public void UpdateOnReload(float ammoPercent)
    {
        ammoCapacityMaterial.SetFloat("_Arc2", (1 - ammoPercent) * 360);
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float width = (Mathf.Max(currentHealth, 0) / maxHealth) * healthBarScaleX;
        if (width > 0)
            width = Mathf.Max(width, 0.001f);

        LeanTween.value(healthBar.gameObject, SetHealthBar, healthBar.localScale.x, width, tweenDuration);
    }

    private void SetHealthBar(float width)
    {
        healthBar.localScale = new Vector3(width, healthBar.localScale.y, healthBar.localScale.z);
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
        ammoHud.parent.gameObject.SetActive(false);
    }
}
