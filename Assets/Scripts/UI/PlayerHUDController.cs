using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.Serialization;
using static GunStats;
using System;

public class PlayerHUDController : MonoBehaviour
{
    [SerializeField]
    private RectTransform hud;

    [Header("Health and ammo")]
    [SerializeField]
    private RectTransform hudParent;
    private float? hudParentLocalY;
    private int hudParentJumpTween;

    [SerializeField]
    private SpriteRenderer healthBar;
    [SerializeField]
    private TMP_Text healthText;
    [SerializeField]
    private Color healthMax;
    [SerializeField]
    private Color healthMin;
    private Vector3 healthTextPosition;
    private int healthTextTween;

    [SerializeField]
    private RectTransform ammoHud;

    [SerializeField]
    private SpriteRenderer ammoBar;

    [SerializeField]
    private Image crosshair;
    private Material crosshairMaterial;

    private Material ammoCapacityMaterial;

    [SerializeField]
    private float tweenDuration = .07f;

    private const float ammoSpinDegrees = 30;
    private const float availableDegrees = 270;

    private int ammoTween;

    [SerializeField]
    private RectTransform chipBox;

    [SerializeField]
    private TMP_Text chipAmount;
    [SerializeField]
    private Color maxChipColor;
    private int chipTween;

    private float originalChipY;
    private float originalChipX;

    [Header("Death")]

    [SerializeField]
    private GameObject deathScreen;

    [SerializeField]
    private TMP_Text deathText;

    [SerializeField]
    private TMP_Text spectateHintText;

    [SerializeField]
    private GameObject spectatorScreen;

    [SerializeField]
    private TMP_Text spectatorTargetText;

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
    // At which velocity speedlines should start fading out
    private const float lineDampeningVelocity = 11f;
    // Scale to what degree lines are removed from center with velocity
    private const float lineRemovalMultiplier = 0.8f;
    // Dampen how much horizontal velocity should influence center of speedlines
    private const float lineVelocityDampeningX = 0.25f;
    // Dampen how much vertical velocity should influence center of speedlines
    private const float lineVelocityDampeningY = 0.1f;
    private float crosshairCrossScale = 1.0f;

    [SerializeField]
    private RectTransform scopeZoom;
    private int scopeTween;
    private int hitTween;
    private int hitMarkTween;

    private void Awake()
    {
        crosshairMaterial = Instantiate(crosshair.material);
        crosshair.material = crosshairMaterial;
    }

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

        originalChipY = chipBox.anchoredPosition.y;
        if (!MatchController.Singleton || PlayerInputManagerController.Singleton.LocalPlayerInputs.Count() == 1)
        {
            // Anchor to top right if there's only one player.
            // This keeps the chip counter from conflicting with the timer
            chipBox.anchorMin = Vector2.one;
            chipBox.anchorMax = Vector2.one;
            originalChipX = -chipBox.sizeDelta.x / 2f - 40;
            // Also scale down the health bar stuff
            hudParent.localScale = .6f * Vector3.one;
        }
        else
        {
            originalChipX = 0;
        }
        chipBox.anchoredPosition = new Vector2(originalChipX, -originalChipY);

        healthBarScaleX = healthBar.transform.localScale.x;
        healthBar.color = healthMax;
        healthTextPosition = healthText.transform.localPosition;

        crosshair.rectTransform.sizeDelta *= SettingsDataManager.Singleton.SettingsDataInstance.CrosshairSize;
    }

    public void SetSpeedLines(Vector3 velocity)
    {
        // TODO replace this by actually not having any UI stuff whatsoever for the AI/bidding players
        if (!speedLinesMaterial)
            return;

        var magnitude = velocity.magnitude;
        if (magnitude < lineDampeningVelocity)
        {
            if (oldLargeVelocity < lineDampeningVelocity)
            {
                speedLinesMaterial.SetFloat("_LineRemovalRadius", 1f);
                return;
            }

            // Dampen larger speeds faster w/log factor
            var dampenedMagnitude = Mathf.Lerp(oldLargeVelocity, 1f, Time.fixedDeltaTime * Mathf.Log(oldLargeVelocity) * .5f);
            speedLinesMaterial.SetFloat("_LineRemovalRadius", speedLineEase.Evaluate(1 / dampenedMagnitude) * lineRemovalMultiplier);
            speedLinesMaterial.SetVector("_Center", new Vector4(0.5f, 0.5f));
            oldLargeVelocity = dampenedMagnitude;
            return;
        }

        var direction = velocity.normalized;
        speedLinesMaterial.SetVector("_Center", new Vector4(0.5f + Vector3.Dot(transform.parent.right, direction) * lineVelocityDampeningX, 0.5f + Vector3.Dot(transform.parent.up, direction) * lineVelocityDampeningY));
        speedLinesMaterial.SetFloat("_LineRemovalRadius", speedLineEase.Evaluate(1 / magnitude) * lineRemovalMultiplier);
        oldLargeVelocity = magnitude;
    }

    public void AnimateHudJump()
    {
        if (!hudParentLocalY.HasValue)
            hudParentLocalY = hudParent.localPosition.y;
        if (LeanTween.isTweening(hudParentJumpTween))
        {
            LeanTween.cancel(hudParentJumpTween);
            hudParent.localPosition = new Vector3(hudParent.localPosition.x, hudParentLocalY.Value, hudParent.localPosition.z);
        }
        hudParentJumpTween = hudParent.LeanMoveLocalY(hudParent.localPosition.y - 10f, 1.3f)
            .setEasePunch()
            .id;
    }

    public void OnDamageTaken(float damage, float currentHealth, float maxHealth)
    {
        UpdateHealthBar(currentHealth, maxHealth);
        LeanTween.value(gameObject, UpdateDamageBorder, 0f, 1f, damageBorderFlashDuration);
        // Leave a persistent red border at low health, softly increasing as the player inches closer to death
        persistentDamageBorder = Mathf.Clamp01(Mathf.Log10(1.2f * maxHealth / (currentHealth + maxHealth * .01f)));
    }

    public void OnChipChange(int amount)
    {
        // TODO replace by not requiring UI stuff on the bidding players
        if (!chipAmount)
            return;
        bool isMax = amount == MatchRules.Current.MaxChips;
        chipAmount.text = isMax ? "MAX" : amount.ToString();
        chipAmount.color = isMax ? maxChipColor : Color.white;
        if (LeanTween.isTweening(chipTween))
        {
            LeanTween.cancel(chipTween);
        }
        chipTween = LeanTween.sequence()
            .append(LeanTween.value(chipBox.gameObject, SetChipBoxPosition, 1f, 0f, .15f).setEaseInBounce())
            .append(2)
            .append(LeanTween.value(chipBox.gameObject, SetChipBoxPosition, 0f, 1f, .15f).setEaseOutBounce()).id;
    }

    private void SetChipBoxPosition(float height)
    {
        chipBox.anchoredPosition = new Vector2(originalChipX, originalChipY - height * 2 * originalChipY);
    }

    public void UpdateOnFire(float ammoPercent)
    {
        if (LeanTween.isTweening(ammoTween))
        {
            LeanTween.cancel(ammoTween);
            ammoBar.gameObject.transform.eulerAngles = new Vector3(ammoBar.gameObject.transform.eulerAngles.x, ammoBar.gameObject.transform.eulerAngles.y, 0);
        }

        ammoCapacityMaterial.SetFloat("_Arc2", (1 - ammoPercent) * availableDegrees);
        ammoTween = ammoHud.gameObject.LeanRotateAroundLocal(Vector3.forward, ammoSpinDegrees, 0.5f).setEaseSpring()
            .setOnStart(
            () => ammoBar.gameObject.transform.Rotate(new Vector3(0, 0, -ammoSpinDegrees))).id;
    }

    public void UpdateOnReload(float ammoPercent)
    {
        ammoCapacityMaterial.SetFloat("_Arc2", (1 - ammoPercent) * availableDegrees);
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        healthText.text = $"{Mathf.Clamp(Mathf.CeilToInt(currentHealth), 0, 100)}%";
        if (LeanTween.isTweening(healthTextTween))
        {
            LeanTween.cancel(healthTextTween);
            healthText.transform.localPosition = healthTextPosition;
        }

        healthTextTween = healthText.gameObject.LeanMoveLocal(healthTextPosition * 2f, 0.5f).setEasePunch().id;
        float width = (Mathf.Max(currentHealth, 0) / maxHealth) * healthBarScaleX;
        if (width > 0)
            width = Mathf.Max(width, 0.001f);

        LeanTween.value(healthBar.gameObject, SetHealthBar, healthBar.transform.localScale.x, width, tweenDuration);
        healthBar.color = Color.Lerp(healthMin, healthMax, (Mathf.Max(currentHealth, 0) / maxHealth));
    }

    private void SetHealthBar(float width)
    {
        healthBar.transform.localScale = new Vector3(width, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
    }

    public void TweenScope(float alpha, float seconds)
    {
        if (LeanTween.isTweening(scopeTween))
            LeanTween.cancel(scopeTween);
        scopeTween = scopeZoom.LeanAlpha(alpha, seconds).setEaseInOutCubic().id;
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
        spectateHintText.gameObject.SetActive(false);
        ammoHud.parent.gameObject.SetActive(false);
        speedLines.gameObject.SetActive(false);
    }

    public void DisplaySpectateHint()
    {
        spectateHintText.gameObject.SetActive(true);
    }

    public void DisplaySpectatorScreen(PlayerIdentity target)
    {
        deathScreen.SetActive(false);
        spectatorScreen.SetActive(true);
        spectatorTargetText.text = target.playerName;
        spectatorTargetText.color = target.color;
    }

    // x and y expected to be in range [-1, 1]
    public void MoveCrosshair(float x, float y)
    {
        var halfWidth = hud.sizeDelta.x / 2;
        var halfHeight = hud.sizeDelta.y / 2;

        crosshair.rectTransform.anchoredPosition = (new Vector2(halfWidth * x, halfHeight * y));
    }

    public void HitAnimation(HitboxController other, ref ProjectileState state)
    {
        if (LeanTween.isTweening(hitTween))
        {
            LeanTween.cancel(hitTween);
            SetCrosshairScale(crosshairCrossScale);
        }
        hitTween = LeanTween.value(crosshair.gameObject, SetCrosshairScale, crosshairCrossScale, 2.5f, 0.4f).setEasePunch().id;
    }

    // TODO better name for this method?
    public void DamageAnimation(DamageInfo damage)
    {
        if (LeanTween.isTweening(hitMarkTween))
        {
            LeanTween.cancel(hitMarkTween);
            SetHitmarkScale(0f);
            SetCrosshairCrit(false);
        }
        SetCrosshairCrit(damage.isCritical);
        hitMarkTween = LeanTween
            .value(crosshair.gameObject, SetHitmarkScale, 0f, 1.5f, 0.4f)
            .setEasePunch()
            .setOnComplete(() => SetCrosshairCrit(false)).id;
    }

    private void SetCrosshairCrit(bool isCritical)
    {
        crosshairMaterial.SetFloat("_IsCritical", isCritical ? 1 : 0);
    }

    private void SetCrosshairScale(float scale)
    {
        crosshairMaterial.SetFloat("_CrossSize", scale);
    }

    private void SetHitmarkScale(float scale)
    {
        crosshairMaterial.SetFloat("_HitMarkerRadius", scale);
    }

    public void UpdateOnInitialize(GunStats stats)
    {
        crosshairMaterial.SetFloat("_Radius", stats.CrosshairRadius.Value() == 0f ? 0f : 1f / stats.CrosshairRadius.Value());

        // Has to be done this way as enum keywords in reality are a set of boolean keywords...
        foreach (CrossHairModes mode in Enum.GetValues(typeof(CrossHairModes)))
            if (mode != stats.CrossHairMode)
                crosshairMaterial.DisableKeyword("_MODE_" + mode.ToString().ToUpper());
            else
                crosshairMaterial.EnableKeyword("_MODE_" + mode.ToString().ToUpper());
    }
}
