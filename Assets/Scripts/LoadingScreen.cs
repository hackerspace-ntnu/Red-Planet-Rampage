using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CollectionExtensions;

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

    [SerializeField] private List<string> tips;

    [SerializeField] private RawImage background;

    [Header("Timing")]
    [SerializeField] private float mandatoryDuration = 4;
    public float MandatoryDuration => mandatoryDuration;

    [SerializeField] private float normalRotationSpeed = 60;
    [SerializeField] private float fastRotationSpeed = 120;

    private float rotationSpeed = 60;

    private static int loadingCounter = 0;

    private Vector2 backgroundVelocity;

    // TODO turn into dontdestroyonload singleton actually
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
        Hide();
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

    public void Show()
    {
        enabled = true;
        gameObject.transform.GetChild(0).gameObject.SetActive(true);

        // Old stuff, move if necessary
        backgroundVelocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * Random.Range(.5f, 1.5f);
        if (backgroundVelocity.magnitude < .1) backgroundVelocity = new Vector2(.5f, .8f);
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

    public void Hide()
    {
        enabled = false;
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }

    private void Update()
    {
        loadingBar.transform.Rotate(Vector3.forward, Time.deltaTime * rotationSpeed);

        var uv = background.uvRect;
        background.uvRect = new Rect(uv.x + backgroundVelocity.x * Time.deltaTime, uv.y + backgroundVelocity.y * Time.deltaTime, uv.width, uv.height);
    }
}
