using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CollectionExtensions;
using System.Linq;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Singleton { get; private set; }

    [Header("Related objects")]
    [SerializeField] private Image radialTimer;

    [SerializeField] private GameObject loadingBar;

    [SerializeField] private GameObject staticText;

    [SerializeField] private TMP_Text tipsText;

    [SerializeField] private GameObject keybinds;

    [SerializeField] private GameObject auctionInstructions;

    [TextArea][SerializeField] private List<string> tips;

    [SerializeField] private RawImage background;

    [SerializeField] private Canvas transitionScreen;

    [Header("Timing")]
    [SerializeField] private float mandatoryDuration = 4;
    public float MandatoryDuration => mandatoryDuration;

    [SerializeField] private float normalRotationSpeed = 60;
    [SerializeField] private float fastRotationSpeed = 120;

    private float rotationSpeed = 60;

    private static int loadingCounter = 0;

    private Vector2 backgroundVelocity;

    private void Awake()
    {
        #region Singleton boilerplate

        if (Singleton != null)
        {
            if (Singleton != this)
            {
                Debug.LogWarning($"There's more than one {Singleton.GetType()} in the scene!");
                Destroy(gameObject);
            }

            return;
        }

        Singleton = this;

        #endregion Singleton boilerplate

#if UNITY_EDITOR
        mandatoryDuration = 1;
#endif

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        radialTimer.material = Instantiate(radialTimer.material);
        Hide(Camera.main);
    }

    private IEnumerator UpdateTimer(float duration)
    {
        var secondsPerChamber = duration / 6f;
        var chamberCoverageAngle = 360f;
        rotationSpeed = normalRotationSpeed;

        radialTimer.material.SetFloat("_Arc2", chamberCoverageAngle);

        for (int i = 0; i < 6; i++)
        {
            yield return new WaitForSeconds(secondsPerChamber);
            chamberCoverageAngle -= 60f;
            radialTimer.material.SetFloat("_Arc2", chamberCoverageAngle);

            if (i == 4)
            {
                rotationSpeed = fastRotationSpeed;
            }
        }
    }

    public static void ResetCounter()
    {
        loadingCounter = 0;
    }

    public void Show(Camera transitionCamera)
    {
        if (enabled)
            return;
        enabled = true;

        SetTransition(transitionCamera, true);
    }

    private void SetTransition(Camera transitionCamera, bool isShow)
    {
        var transition = transform.GetChild(1).gameObject;
        var slideMaterial = transition.GetComponent<Image>().material;
        slideMaterial.SetFloat("_Coverage", -0.5f);
        transition.SetActive(true);
        if (!isShow)
            transform.GetChild(0).gameObject.SetActive(false);

        transitionScreen.worldCamera = transitionCamera;
        transitionScreen.planeDistance = 1;

        slideMaterial.SetFloat("_Direction", isShow ? 1f : 0f);

        if (isShow)
            LeanTween.value(gameObject, (value) => slideMaterial.SetFloat("_Coverage", value), -0.5f, 1.5f, 0.5f)
                .setEaseInOutQuad()
                .setOnComplete(ShowEntireScreen);
        // Avoids animating transitions when not transitioning from a loading screen
        else if (loadingCounter < 1)
            transition.SetActive(false);
        else
            LeanTween.value(gameObject, (value) => slideMaterial.SetFloat("_Coverage", value), -0.5f, 1.5f, 0.5f)
                .setEaseInOutQuad()
                .setOnComplete(() => transition.SetActive(false));
    }

    private void ShowEntireScreen()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        gameObject.transform.GetChild(1).gameObject.SetActive(false);
        // Random background funkiness
        var angle = Random.Range(-15f, 15f);
        background.transform.eulerAngles = angle * Vector3.forward;
        // Background moves along same angle as it is tilted, but horizontally
        backgroundVelocity = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        PlayerInputManagerController.Singleton.RemoveListeners();
        loadingCounter += 1;
        StartCoroutine(UpdateTimer(mandatoryDuration));

        tipsText.text = "";
        staticText.SetActive(false);

        var isFirstLoadingScreen = loadingCounter == 1;
        var isBeforeFirstAuction = MatchController.Singleton?.RoundCount == 1 && !MatchController.Singleton.IsAuction;

        keybinds.SetActive(false);
        auctionInstructions.SetActive(false);
        staticText.SetActive(false);

        if (isFirstLoadingScreen)
        {
            keybinds.SetActive(true);
        }
        else if (isBeforeFirstAuction)
        {
            auctionInstructions.SetActive(true);
        }
        else
        {
            staticText.SetActive(true);
            tipsText.text = tips.RandomElement();
        }
    }

    public void Hide(Camera transitionCamera)
    {
        if (!enabled)
            return;
        enabled = false;

        SetTransition(transitionCamera, false);
    }

    private void Update()
    {
        loadingBar.transform.Rotate(Vector3.forward, Time.deltaTime * rotationSpeed);

        var uv = background.uvRect;
        background.uvRect = new Rect(uv.x + backgroundVelocity.x * Time.deltaTime, uv.y + backgroundVelocity.y * Time.deltaTime, uv.width, uv.height);
    }
}
