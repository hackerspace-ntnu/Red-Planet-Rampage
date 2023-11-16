using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using SecretName;

public class ItemSelectManager : MonoBehaviour
{
    [SerializeField] 
    private Item defaultBodyItem;
    [SerializeField] 
    private Item defaultBarrelItem;
    [SerializeField] 
    private Transform[] itemSpawnPoints;

    [SerializeField]
    private Canvas canvas;
    
    private List<GameObject> bodies = new List<GameObject>();
    private List<GameObject> barrels = new List<GameObject>();
    private List<GameObject> extensions = new List<GameObject>();
    [SerializeField] 
    private Timer timer;
    private List<Item> bodyItems;
    private List<Item> barrelItems;
    private List<Item> extensionItems;
    private InputManager inputManager;
    [SerializeField] 
    private Selectable defaultSelector;
    private Vector2 moveInput;
    private int bodyIndex;
    private int barrelIndex;
    private int extensionIndex;
    [SerializeField]
    private TMP_Text timerText;
    private const float errorMarginInput = 0.1f;
    private bool gamepadMoveReady = true;
    public Transform cameraPosition;
    [SerializeField]
    private PlayerStatUI playerStatUI;
    [SerializeField]
    private TMP_Text secretName;

    [SerializeField] 
    private Image bodySelect;
    [SerializeField] 
    private Image barrelSelect;
    [SerializeField] 
    private Image extensionSelect; 
    [SerializeField]
    private Color selectedColor;
    [SerializeField]
    private Color defaultColor;
    private int selectedTypeIndex = 0;

   void Start()
    {
        bodySelect.color = selectedColor;
    }
    public IEnumerator SpawnItems(InputManager inputManager){

        //Without this line it is arbitrary whether an item from aucion is transferred
        yield return null;
        this.inputManager = inputManager;
        canvas.worldCamera = inputManager.GetComponent<Camera>();
        playerStatUI.PlayerIdentity = inputManager.GetComponent<PlayerIdentity>();
        bodyItems = inputManager.GetComponent<PlayerIdentity>().Bodies;
        barrelItems = inputManager.GetComponent<PlayerIdentity>().Barrels;
        extensionItems = inputManager.GetComponent<PlayerIdentity>().Extensions;   

        InstantiateItems(bodyItems,defaultBodyItem,itemSpawnPoints[0], bodies);
        InstantiateItems(barrelItems,defaultBarrelItem,itemSpawnPoints[1], barrels);
        InstantiateItems(extensionItems,null,itemSpawnPoints[2],extensions);

       
        bodyIndex = bodies.Count - 1;
        barrelIndex = barrels.Count - 1;
        extensionIndex = extensions.Count - 1;
        


        ChangeItemDisplayed(bodies[bodyIndex], bodies[bodyIndex], itemSpawnPoints[0]);
        ChangeItemDisplayed(barrels[barrelIndex],barrels[barrelIndex],itemSpawnPoints[1]);
        ChangeItemDisplayed(
            extensions.Count != 0 ? extensions[extensionIndex] : null, 
            extensions.Count != 0 ? extensions[extensionIndex] : null, 
            itemSpawnPoints[2]);
        
        yield return null;
        yield return null;

        inputManager.onMovePerformed += MoveInputPerformed;
        inputManager.onMoveCanceled += MoveInputCanceled;

        timer.StartTimer(20f);
        timer.OnTimerUpdate += UpdateTimer;
        timer.OnTimerRunCompleted += ChangeScene;
        timer.OnTimerRunCompleted += SetLoadout;
        timer.OnTimerRunCompleted -= UpdateTimer;


    }

    private void ChangeItemDisplayed(GameObject previousItem, GameObject nextItem,Transform itemSpawnPoint){

        if(previousItem == null && nextItem == null){
            Debug.Log("Player has no extensions");
            return;
        }

            SetLoadout();
            previousItem.transform.SetParent(null);
            previousItem.transform.localPosition = Vector3.zero;
           
            nextItem.transform.SetParent(itemSpawnPoint);
            if(itemSpawnPoint == itemSpawnPoints[1]){
                nextItem.transform.localPosition = new Vector3(-40f,0,-60);

                if(nextItem.TryGetComponent<MeshProjectileController>(out MeshProjectileController meshProjectile)){
                    nextItem.LeanScale(new Vector3(100f, 100f, 100f), 0.5f);
                }else{
                    nextItem.LeanScale(new Vector3(150f, 150f, 150f), 0.5f);
                }
            }else{
                nextItem.transform.localPosition = Vector3.zero;
                nextItem.LeanScale(new Vector3(150f, 150f, 150f), 0.5f);

            }

    
    }
    
    private void InstantiateItems(List<Item> items, Item defaultItem, Transform itemSpawnPoint, List<GameObject> itemObjects){

        if(defaultItem != null && !items.Contains(defaultItem)) items.Insert(0,defaultItem);

        for(int i = 0; i < items.Count; i++){
            itemObjects.Add(Instantiate(items[i].augment, Vector3.zero, Quaternion.Euler(new Vector3(0, 90, -20))));
        }
        Debug.Log(items.Count);
        Debug.Log(itemObjects.Count);
        
    }
     private void MoveInputPerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
        Debug.Log(moveInput);

        if(moveInput.y > 1 - errorMarginInput && gamepadMoveReady){
            MoveUpPerformed();
            gamepadMoveReady = false;
            StartCoroutine(gamepadMoveDelay());
        }else if (moveInput.y < -1 + errorMarginInput && gamepadMoveReady){
            MoveDownPerformed();
            gamepadMoveReady = false;
            StartCoroutine(gamepadMoveDelay());

        }else if (moveInput.x < -1 + errorMarginInput && gamepadMoveReady){
            selectedTypeIndex--;
            selectedTypeIndex = selectedTypeIndex < 0 ? 2 : selectedTypeIndex;
            gamepadMoveReady = false;
            StartCoroutine(gamepadMoveDelay());

        }else if (moveInput.x > 1 - errorMarginInput && gamepadMoveReady){
            selectedTypeIndex++;
            selectedTypeIndex = selectedTypeIndex > 2 ? 0 : selectedTypeIndex;
            gamepadMoveReady = false;
            StartCoroutine(gamepadMoveDelay());
        }
        bodySelect.color  = defaultColor;
        barrelSelect.color  = defaultColor;
        extensionSelect.color  = defaultColor;
        switch (selectedTypeIndex){
            case 0:
                bodySelect.color = selectedColor;
                break;
            case 1:
                barrelSelect.color = selectedColor;
                break;
            case 2:
                extensionSelect.color = selectedColor;
                break;
        }
    }
    private IEnumerator gamepadMoveDelay(){
        yield return new WaitForSeconds(0.2f);
        gamepadMoveReady = true;
    }
     private void MoveInputCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }
     private void MoveUpPerformed(){
        switch(selectedTypeIndex)
        {
            case 0:

                if(bodyIndex == bodies.Count - 1){
                    bodyIndex = 0;
                    ChangeItemDisplayed(bodies[^1],bodies[bodyIndex],itemSpawnPoints[0]);
                }else {
                    bodyIndex ++;
                    ChangeItemDisplayed(bodies[bodyIndex - 1],bodies[bodyIndex],itemSpawnPoints[0]);
                }
                break;
            case 1:
                if(barrelIndex == barrels.Count - 1){
                    barrelIndex = 0;
                    ChangeItemDisplayed(barrels[^1],barrels[barrelIndex],itemSpawnPoints[1]);

                }else{
                    barrelIndex ++;
                    ChangeItemDisplayed(barrels[barrelIndex - 1],barrels[barrelIndex],itemSpawnPoints[1]);
                }

                break;
            case 2:

                if(extensionIndex == extensions.Count - 1){
                    extensionIndex = 0;
                    ChangeItemDisplayed(extensions[^1],extensions[extensionIndex],itemSpawnPoints[2]);

                }else{
                    extensionIndex ++;
                    ChangeItemDisplayed(extensions[extensionIndex - 1],extensions[extensionIndex],itemSpawnPoints[2]);
                }
                break;
        }

    }
    private void MoveDownPerformed(){
        switch(selectedTypeIndex)
        {
            case 0:

                if(bodyIndex == 0){
                    bodyIndex = bodies.Count - 1;
                    ChangeItemDisplayed(bodies[0],bodies[bodyIndex],itemSpawnPoints[0]);
                }else {
                    bodyIndex --;
                    ChangeItemDisplayed(bodies[bodyIndex + 1],bodies[bodyIndex],itemSpawnPoints[0]);
                }

                break;
            case 1:

                if(barrelIndex == 0){
                    barrelIndex = barrels.Count - 1;
                    ChangeItemDisplayed(barrels[0],barrels[barrelIndex],itemSpawnPoints[1]);

                }else{
                    barrelIndex --;
                    ChangeItemDisplayed(barrels[barrelIndex + 1],barrels[barrelIndex],itemSpawnPoints[1]);
                }

                break;
            case 2:

                if(extensions.Count != 0){
                    if(extensionIndex == 0){
                        extensionIndex = extensions.Count - 1;
                        ChangeItemDisplayed(extensions[0],extensions[extensionIndex],itemSpawnPoints[2]);

                    }else{
                        extensionIndex --;
                        ChangeItemDisplayed(extensions[extensionIndex + 1],extensions[extensionIndex],itemSpawnPoints[2]);
                    }
                }
                break;
        }

    }
    public void SetLoadout(){
        var player = inputManager.GetComponent<PlayerIdentity>();
        player.SetLoadout(
            bodyItems[bodyIndex],
            barrelItems[barrelIndex],
            extensionItems.Count != 0 ? extensionItems[extensionIndex] : null);
        playerStatUI.UpdateStats();
        secretName.text = player.GetGunName();
    }
    
    private void ChangeScene(){
        AuctionDriver.Singleton.ChangeScene();
    }
     public void SelectControl(Selectable target)
    {
        StartCoroutine(WaitSelect(target));
    }
    private IEnumerator WaitSelect(Selectable target)
    {
        yield return null;
    }
     private void UpdateTimer()
    {
        timerText.text = Mathf.Round(timer.WaitTime - timer.ElapsedTime).ToString();
    }
}

