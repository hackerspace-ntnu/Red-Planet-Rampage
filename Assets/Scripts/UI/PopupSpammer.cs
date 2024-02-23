using System.Collections;
using UnityEngine;

public class PopupSpammer : MonoBehaviour
{
    [SerializeField] private Popup popup;

    [SerializeField] private RectTransform spamTarget;
    [SerializeField] private RectTransform hud;

    [SerializeField] private float minSpamDelay = .05f;
    [SerializeField] private float maxSpamDelay = .2f;

    [SerializeField]
    private AudioSource audioSource;

    private int spamRemaining = 0;

    private Coroutine spamRoutine = null;

    private void OpenPopup()
    {
        var halfWidth = hud.sizeDelta.x / 2;
        var halfHeight = hud.sizeDelta.y / 2;
        var position = new Vector2(
            Random.Range(-halfWidth, halfWidth),
            Random.Range(-halfHeight, halfHeight));

        var instance = Instantiate(popup, spamTarget.transform.position, spamTarget.transform.rotation, spamTarget);
        instance.GetComponent<RectTransform>().anchoredPosition = position;
    }

    private IEnumerator SpamPeriodically()
    {
        while (spamRemaining > 0)
        {
            OpenPopup();
            audioSource.Play();
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
