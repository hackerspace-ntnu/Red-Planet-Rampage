using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using SecretName;

public class ItemSelectMenu : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private Timer timer;

    [SerializeField]
    private TMP_Text timerText;

    [SerializeField]
    private Transform cameraPosition;
    public Transform CameraPosition => cameraPosition;

    [SerializeField]
    private ItemSelectSlot bodySlot;

    [SerializeField]
    private ItemSelectSlot barrelSlot;

    [SerializeField]
    private ItemSelectSlot extensionSlot;

    [SerializeField]
    private PlayerStatUI playerStatUI;

    [SerializeField]
    private TMP_Text secretName;

    [SerializeField]
    private RectTransform readyIndicator;

    private InputManager inputManager;

    private Vector2 moveInput;

    private const float errorMarginInput = 0.1f;

    private bool gamepadMoveReady = true;

    private PlayerIdentity player;

    private AugmentType selectedType = AugmentType.Body;

    private ItemSelectSlot selectedSlot => (selectedType) switch
    {
        AugmentType.Body => bodySlot,
        AugmentType.Barrel => barrelSlot,
        AugmentType.Extension => extensionSlot,
        _ => bodySlot,
    };

    public delegate void SelectionEvent(ItemSelectMenu menu);

    public SelectionEvent OnReady;

    private bool isReady = false;
    public bool IsReady => isReady;

    private void Start()
    {
        readyIndicator.gameObject.SetActive(false);
    }

    public IEnumerator SpawnItems(InputManager inputManager)
    {

        // Without this line it is arbitrary whether an item from auction is transferred
        yield return null;

        this.inputManager = inputManager;
        canvas.worldCamera = inputManager.PlayerCamera;

        player = inputManager.GetComponent<PlayerIdentity>();
        playerStatUI.PlayerIdentity = player;

        var bodyItems = new List<Item>(player.Bodies);
        bodyItems.Insert(0, StaticInfo.Singleton.StartingBody);
        bodySlot.SetItems(bodyItems, player.Body);

        var barrelItems = new List<Item>(player.Barrels);
        barrelItems.Insert(0, StaticInfo.Singleton.StartingBarrel);
        barrelSlot.SetItems(barrelItems, player.Barrel);

        var extensionItems = new List<Item>(player.Extensions);
        if (StaticInfo.Singleton.StartingExtension)
            extensionItems.Insert(0, StaticInfo.Singleton.StartingExtension);
        extensionSlot.SetItems(extensionItems, player.Extension);

        SetLoadout();

        bodySlot.Select();
        var selectedItem = bodySlot.SelectedItem;
        playerStatUI.SetDescription(selectedItem == null ? "" : selectedItem.displayDescription);

        yield return null;
        yield return null;

        inputManager.onMovePerformed += MoveInputPerformed;
        inputManager.onMoveCanceled += MoveInputCanceled;

        inputManager.onSelect += SelectPerformed;

        timer.StartTimer(20f);
        timer.OnTimerUpdate += OnTimerUpdate;
        timer.OnTimerRunCompleted += OnTimerRunCompleted;
    }


    private void InstantiateItems(List<Item> items, Item defaultItem, Transform itemSpawnPoint, List<GameObject> itemObjects)
    {

        if (defaultItem != null && !items.Contains(defaultItem)) items.Insert(0, defaultItem);

        for (int i = 0; i < items.Count; i++)
        {
            itemObjects.Add(Instantiate(items[i].augment, Vector3.zero, Quaternion.Euler(new Vector3(0, 90, -20))));
        }
        Debug.Log(items.Count);
        Debug.Log(itemObjects.Count);
    }

    private void MoveInputPerformed(InputAction.CallbackContext ctx)
    {
        if (isReady)
            ToggleReadiness();

        moveInput = ctx.ReadValue<Vector2>();

        if (moveInput.y > 1 - errorMarginInput && gamepadMoveReady)
        {
            MoveUpPerformed();
            gamepadMoveReady = false;
            StartCoroutine(GamepadMoveDelay());
        }
        else if (moveInput.y < -1 + errorMarginInput && gamepadMoveReady)
        {
            MoveDownPerformed();
            gamepadMoveReady = false;
            StartCoroutine(GamepadMoveDelay());

        }
        else if (moveInput.x < -1 + errorMarginInput && gamepadMoveReady)
        {
            MoveLeftPerformed();
            gamepadMoveReady = false;
            StartCoroutine(GamepadMoveDelay());
        }
        else if (moveInput.x > 1 - errorMarginInput && gamepadMoveReady)
        {
            MoveRightPerformed();
            gamepadMoveReady = false;
            StartCoroutine(GamepadMoveDelay());
        }
    }

    private IEnumerator GamepadMoveDelay()
    {
        yield return new WaitForSeconds(0.2f);
        gamepadMoveReady = true;
    }

    private void MoveInputCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    private void MoveLeftPerformed()
    {
        selectedType = selectedType == AugmentType.Body ? AugmentType.Extension : selectedType - 1;
        ChangeSelectedSlot();
    }

    private void MoveRightPerformed()
    {
        selectedType = selectedType == AugmentType.Extension ? AugmentType.Body : selectedType + 1;
        ChangeSelectedSlot();
    }

    private void ChangeSelectedSlot()
    {
        bodySlot.Deselect();
        barrelSlot.Deselect();
        extensionSlot.Deselect();
        selectedSlot.Select();
        var selectedItem = selectedSlot.SelectedItem;
        playerStatUI.SetDescription(selectedItem == null ? "" : selectedItem.displayDescription);
    }

    private void MoveUpPerformed()
    {
        selectedSlot.Previous();
        var selectedItem = selectedSlot.SelectedItem;
        playerStatUI.SetDescription(selectedItem == null ? "" : selectedItem.displayDescription);
        SetLoadout();
    }

    private void MoveDownPerformed()
    {
        selectedSlot.Next();
        var selectedItem = selectedSlot.SelectedItem;
        playerStatUI.SetDescription(selectedItem == null ? "" : selectedItem.displayDescription);
        SetLoadout();
    }

    public void SetLoadout()
    {
        player.SetLoadout(
            bodySlot.SelectedItem,
            barrelSlot.SelectedItem,
            extensionSlot.SelectedItem);
        playerStatUI.UpdateStats();
        secretName.text = player.GetGunName();
    }

    private void SelectPerformed(InputAction.CallbackContext ctx)
    {
        ToggleReadiness();
    }

    private void ToggleReadiness()
    {
        isReady = !isReady;

        if (isReady)
        {
            readyIndicator.gameObject.SetActive(true);
            LeanTween.scale(readyIndicator.gameObject, 1.4f * Vector3.one, .3f).setEasePunch();
            OnReady?.Invoke(this);
        }
        else
        {
            readyIndicator.gameObject.SetActive(false);
        }
    }

    private void OnTimerRunCompleted()
    {
        isReady = true;
        OnReady?.Invoke(this);
    }

    // TODO Move timer up to manager
    private void OnTimerUpdate()
    {
        timerText.text = Mathf.Round(timer.WaitTime - timer.ElapsedTime).ToString();
    }
}

