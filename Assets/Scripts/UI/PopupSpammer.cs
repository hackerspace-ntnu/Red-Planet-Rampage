using System.Collections;
using CollectionExtensions;
using UnityEngine;

public class PopupSpammer : MonoBehaviour
{
    [SerializeField] private Popup[] popups;

    [SerializeField] private RectTransform spamTarget;
    [SerializeField] private RectTransform hud;

    [SerializeField] private float minSpamDelay = .1f;
    [SerializeField] private float maxSpamDelay = .3f;

    private int spamRemaining = 0;

    private Coroutine spamRoutine = null;

    private void OpenPopup()
    {
        var halfWidth = hud.sizeDelta.x / 2;
        var halfHeight = hud.sizeDelta.y / 2;
        var position = new Vector2(
            Random.Range(-halfWidth, halfWidth),
            Random.Range(-halfHeight, halfHeight));
  
        var popup = popups.RandomElement();
        var instance = Instantiate(popup, spamTarget.transform.position, spamTarget.transform.rotation, spamTarget);
        instance.GetComponent<RectTransform>().anchoredPosition = position;
    }

    private IEnumerator SpamPeriodically()
    {
        while (spamRemaining > 0)
        {
            OpenPopup();
            spamRemaining--;
            yield return new WaitForSeconds(Random.Range(minSpamDelay, maxSpamDelay));
        }

        spamRoutine = null;
    }

    public void Spam(int amount)
    {
        spamRemaining += amount;

        if (spamRoutine == null)
        {
            spamRoutine = StartCoroutine(SpamPeriodically());
        }
    }
}
