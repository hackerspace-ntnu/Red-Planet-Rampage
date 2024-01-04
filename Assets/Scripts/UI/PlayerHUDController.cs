using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;

public class PlayerHUDController : MonoBehaviour
{
    [SerializeField] 
    private RectTransform hud;

    [Header("Health and ammo")]

    [SerializeField]
    private RectTransform healthBar;

    [SerializeField]
    private RectTransform ammoHud;

    [SerializeField]
    private SpriteRenderer ammoBar;

    [SerializeField]
    private RectTransform crosshair;

    private Material ammoCapacityMaterial;

    [SerializeField]
    private float tweenDuration = .07f;

    private const float ammoSpinDegrees = 30;
    private const float availableDegrees = 270;


    [Header("Death")]

    [SerializeField]
    private GameObject deathScreen;

    [SerializeField]
    private TMP_Text deathText;

    [SerializeField]
    private float damageBorderFlashDuration = .2f;

    private float damageBorderTop = .2f;

    private float persistentDamageBorder = 0;

    private float healthBarScaleX;

    private Material damageBorder;


    [Header("Effects")]

    [SerializeField]
    private PopupSpammer popupSpammer;
    public PopupSpammer PopupSpammer => popupSpammer;
    [SerializeField]
    private Image speedLines;
    [SerializeField]
    private AnimationCurve speedLineEase;
    private Material speedLinesMaterial;
    private float oldLargeVelocity = 0f;
    private const float lineDampeningVelocity = 11f;
    private const float lineRemovalMultiplier = 0.8f;
    private const float lineVelocityDampeningX = 0.25f;
    private const float lineVelocityDampeningY = 0.1f;

    [SerializeField]
    private RectTransform scopeZoom;


    void Start()
    {
        speedLines.material = Instantiate(speedLines.material);
        speedLinesMaterial = speedLines.material;
        speedLines.gameObject.SetActive(true);
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

    public void SetSpeedLines(Vector3 velocity)
    {
        var magnitude = velocity.magnitude;
        if (magnitude < lineDampeningVelocity)
        {
            if (oldLargeVelocity < lineDampeningVelocity)
            {
                speedLinesMaterial.SetFloat("_LineRemovalRadius", 1f);
                return;
            }

            var lerpedMagnitude = Mathf.Lerp(oldLargeVelocity, 1f, Time.fixedDeltaTime);
            speedLinesMaterial.SetFloat("_LineRemovalRadius", speedLineEase.Evaluate(1 / lerpedMagnitude) * lineRemovalMultiplier);
            speedLinesMaterial.SetVector("_Center", new Vector4(0.5f, 0.5f));
            oldLargeVelocity = lerpedMagnitude;
            return;
        }

        var direction = velocity.normalized;
        speedLinesMaterial.SetVector("_Center", new Vector4(0.5f + Vector3.Dot(transform.parent.right, direction) * lineVelocityDampeningX, 0.5f + Vector3.Dot(transform.parent.up, direction) * lineVelocityDampeningY));
        speedLinesMaterial.SetFloat("_LineRemovalRadius", speedLineEase.Evaluate(1 / magnitude) * lineRemovalMultiplier);
        oldLargeVelocity = magnitude;
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
        
        ammoCapacityMaterial.SetFloat("_Arc2", (1-ammoPercent)* availableDegrees);
        ammoHud.gameObject.LeanRotateAroundLocal(Vector3.forward, ammoSpinDegrees, 0.5f).setEaseSpring()
            .setOnStart(
            () => ammoBar.gameObject.transform.Rotate(new Vector3(0, 0, -ammoSpinDegrees)));
    }

    public void UpdateOnReload(float ammoPercent)
    {
        ammoCapacityMaterial.SetFloat("_Arc2", (1 - ammoPercent) * availableDegrees);
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

    public void TweenScope(float alpha, float seconds)
    {
        scopeZoom.LeanAlpha(alpha, seconds).setEaseInOutCubic();
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
        speedLines.gameObject.SetActive(false);
    }

    // x and y expected to be in range [-1, 1]
    public void MoveCrosshair(float x, float y)
    {
        var halfWidth = hud.sizeDelta.x / 2;
        var halfHeight = hud.sizeDelta.y / 2;

        crosshair.anchoredPosition = (new Vector2(halfWidth * x, halfHeight * y));
    }
}
