using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CreditsMenu : MonoBehaviour
{
    [SerializeField] private MainMenuController mainMenuController;

    [SerializeField] private RectTransform content;

    [SerializeField] private RectTransform textContent;

    [SerializeField] private RectTransform lastElement;

    [SerializeField] private float speed = 10;

    private int tween;
    private float height;
    private float initialInset;

    private InputManager inputManager;

    private void OnEnable()
    {
        if (inputManager)
        {
            inputManager.onCancel += Back;
            inputManager.onSelect += Back;
            inputManager.onClick += Back;
        }

        StartCoroutine(StartAnimation());
    }

    private IEnumerator StartAnimation()
    {
        // Wait until position of last element can be determined.
        yield return new WaitForEndOfFrame();
        initialInset = content.anchorMin.y;
        var diffBetweenContentAndText = Mathf.Abs(content.anchoredPosition.y - textContent.anchoredPosition.y);
        height = diffBetweenContentAndText + Mathf.Abs(lastElement.anchoredPosition.y + lastElement.sizeDelta.y * .5f);

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
        content.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, initialInset, content.rect.height);
    }

    private void SetPosition(float t)
    {
        var inset = Mathf.RoundToInt(initialInset - height * t);
        content.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, inset, content.rect.height);
    }

    public void SetPlayerInput(InputManager inputManager)
    {
        this.inputManager = inputManager;
    }

    private void Back(InputAction.CallbackContext ctx)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (inputManager)
        {
            inputManager.onCancel -= Back;
            inputManager.onSelect -= Back;
            inputManager.onClick -= Back;
        }

        StopAnimation();
        mainMenuController.ReturnToMainMenu();
    }
}
