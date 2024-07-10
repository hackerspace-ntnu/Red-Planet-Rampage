using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CreditsMenu : MonoBehaviour
{
    [SerializeField] private MainMenuController mainMenuController;

    [SerializeField] private VerticalLayoutGroup content;

    [SerializeField] private float speed = 10;

    [SerializeField] private RectTransform lastElement;

    private float height = 5000;
    private int tween;
    private int initialTopPadding;

    private void OnEnable()
    {
        StartCoroutine(StartAnimation());
    }

    private IEnumerator StartAnimation()
    {
        // Wait until position of last element can be determined.
        yield return new WaitForEndOfFrame();
        initialTopPadding = content.padding.top;
        height = Mathf.Abs(lastElement.anchoredPosition.y + lastElement.sizeDelta.y * .5f);

        tween = LeanTween.sequence()
            .append(1)
            .append(
                LeanTween.value(content.gameObject, SetPosition, 0, 1, height / speed)
                .setOnComplete(() => SteamManager.Singleton.UnlockAchievement(AchievementType.SitThroughCredits)))
            .id;
    }

    private void StopAnimation()
    {
        if (LeanTween.isTweening(tween))
            LeanTween.cancel(tween);
        // Doubly cancel since the first doesn't seem to work (?)
        LeanTween.cancel(content.gameObject);
        content.padding.Remove(new Rect());
        content.padding.top = initialTopPadding;
    }

    private void SetPosition(float t)
    {
        var topPadding = Mathf.RoundToInt(initialTopPadding - height * t);
        content.padding = new RectOffset(content.padding.left, content.padding.right, topPadding,
            content.padding.bottom);
    }

    public void SetPlayerInput(InputManager inputManager)
    {
        inputManager.onCancel += Back;
        inputManager.onSelect += Back;
    }

    private void Back(InputAction.CallbackContext ctx)
    {
        if (!gameObject.activeInHierarchy)
            return;

        StopAnimation();
        mainMenuController.ReturnToMainMenu();
    }
}
