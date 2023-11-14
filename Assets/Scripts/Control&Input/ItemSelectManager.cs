using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

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
    private Image bodySelect;
    [SerializeField] 
    private Image barrelSelect;
    [SerializeField] 
    private Image extensionSelect; 
    [SerializeField]
    private Color selectedColor;
    [SerializeField]
    private Color defaultColor;
    private int selectedIndex = 0;

   void Start()
    {
        bodySelect.color = selectedColor;
    }
    public IEnumerator SpawnItems(InputManager inputManager){

        //Without this line it is arbitrary whether an item from aucion is transferred
        yield return null;
        this.inputManager = inputManager;
        canvas.worldCamera = inputManager.GetComponent<Camera>();

        bodyItems = inputManager.gameObject.GetComponent<PlayerIdentity>().Bodies;
        barrelItems = inputManager.gameObject.GetComponent<PlayerIdentity>().Barrels;
        extensionItems = inputManager.gameObject.GetComponent<PlayerIdentity>().Extensions;   

        InstantiateItems(bodyItems,defaultBodyItem,itemSpawnPoints[0], bodies);
        InstantiateItems(barrelItems,defaultBarrelItem,itemSpawnPoints[1], barrels);
        InstantiateItems(extensionItems,null,itemSpawnPoints[2],extensions);

        Debug.Log("body items" + bodyItems.Count);
        Debug.Log("barrel items" + barrelItems.Count);
        Debug.Log("extension items" + extensionItems.Count);
        bodyIndex = bodies.Count - 1;
        barrelIndex = barrels.Count - 1;
        extensionIndex = extensions.Count - 1;
        Debug.Log("bodyIndexLast" + bodyIndex);
        Debug.Log("barrelIndexLast" + barrelIndex);
        Debug.Log("extensionIndexLast" + extensionIndex);


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

    private void ChangeItemDisplayed(GameObject previousItem,GameObject nextItem,Transform itemSpawnPoint){

        if(previousItem != null && nextItem != null){
            previousItem.transform.SetParent(null);
            previousItem.transform.localPosition = Vector3.zero;
            Debug.Log("Item displayed");
            nextItem.transform.SetParent(itemSpawnPoint);
            if(itemSpawnPoint == itemSpawnPoints[1]){
                nextItem.transform.localPosition = new Vector3(-35f,0,0);

            }else{
                nextItem.transform.localPosition = Vector3.zero;

            }
            nextItem.transform.localScale = new Vector3(300f,150f,150f);
        }else{
            Debug.Log("Player has no extensions");
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
        Debug.Log("moved");
        moveInput = ctx.ReadValue<Vector2>();
        Debug.Log(moveInput);

        if(moveInput.y > 1 - errorMarginInput && gamepadMoveReady){
            Debug.Log("up");
            MoveUpPerformed();
            gamepadMoveReady = false;
            StartCoroutine(gamepadMoveDelay());
        }else if (moveInput.y < -1 + errorMarginInput && gamepadMoveReady){
            MoveDownPerformed();
            gamepadMoveReady = false;
            StartCoroutine(gamepadMoveDelay());

        }else if (moveInput.x < -1 + errorMarginInput && gamepadMoveReady){
            selectedIndex--;
            selectedIndex = selectedIndex < 0 ? 2 : selectedIndex;
            gamepadMoveReady = false;
            StartCoroutine(gamepadMoveDelay());

        }else if (moveInput.x > 1 - errorMarginInput && gamepadMoveReady){
            selectedIndex++;
            selectedIndex = selectedIndex > 2 ? 0 : selectedIndex;
            gamepadMoveReady = false;
            StartCoroutine(gamepadMoveDelay());
        }
        bodySelect.color  = defaultColor;
        barrelSelect.color  = defaultColor;
        extensionSelect.color  = defaultColor;
        Debug.Log("SelectedIndex was " + selectedIndex);
        switch (selectedIndex){
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
        switch(selectedIndex)
        {
            case 0:
                Debug.Log("bodyIndexBefore" + bodyIndex);

                if(bodyIndex == bodies.Count - 1){
                    bodyIndex = 0;
                    ChangeItemDisplayed(bodies[^1],bodies[bodyIndex],itemSpawnPoints[0]);
                }else {
                    bodyIndex ++;
                    ChangeItemDisplayed(bodies[bodyIndex - 1],bodies[bodyIndex],itemSpawnPoints[0]);
                }
                Debug.Log("bodyIndexAfter" + bodyIndex);
                break;
            case 1:
                Debug.Log("barrelIndexBefore" + barrelIndex);
                if(barrelIndex == barrels.Count - 1){
                    barrelIndex = 0;
                    ChangeItemDisplayed(barrels[^1],barrels[barrelIndex],itemSpawnPoints[1]);

                }else{
                    barrelIndex ++;
                    ChangeItemDisplayed(barrels[barrelIndex - 1],barrels[barrelIndex],itemSpawnPoints[1]);
                }
                Debug.Log("barrelIndexAfter" + barrelIndex);

                break;
            case 2:
                Debug.Log("extensionIndexBefore" + extensionIndex);

                if(extensionIndex == extensions.Count - 1){
                    extensionIndex = 0;
                    ChangeItemDisplayed(extensions[^1],extensions[extensionIndex],itemSpawnPoints[2]);

                }else{
                    extensionIndex ++;
                    ChangeItemDisplayed(extensions[extensionIndex - 1],extensions[extensionIndex],itemSpawnPoints[2]);
                }
                Debug.Log("extensionIndexAfter" + extensionIndex);
                break;
        }

    }
    private void MoveDownPerformed(){
        switch(selectedIndex)
        {
            case 0:
                Debug.Log("bodyIndexBefore" + bodyIndex);

                if(bodyIndex == 0){
                    bodyIndex = bodies.Count - 1;
                    ChangeItemDisplayed(bodies[0],bodies[bodyIndex],itemSpawnPoints[0]);
                }else {
                    bodyIndex --;
                    ChangeItemDisplayed(bodies[bodyIndex + 1],bodies[bodyIndex],itemSpawnPoints[0]);
                }
                Debug.Log("bodyIndexAfter" + bodyIndex);

                break;
            case 1:
                Debug.Log("barrelIndexBefore" + barrelIndex);

                if(barrelIndex == 0){
                    barrelIndex = barrels.Count - 1;
                    ChangeItemDisplayed(barrels[0],barrels[barrelIndex],itemSpawnPoints[1]);

                }else{
                    barrelIndex --;
                    ChangeItemDisplayed(barrels[barrelIndex + 1],barrels[barrelIndex],itemSpawnPoints[1]);
                }
               Debug.Log("barrelIndexAfter" + barrelIndex);

                break;
            case 2:
                Debug.Log("extensionIndexBefore" + extensionIndex);

                if(extensions.Count != 0){
                    if(extensionIndex == 0){
                        extensionIndex = extensions.Count - 1;
                        ChangeItemDisplayed(extensions[0],extensions[extensionIndex],itemSpawnPoints[2]);

                    }else{
                        extensionIndex --;
                        ChangeItemDisplayed(extensions[extensionIndex + 1],extensions[extensionIndex],itemSpawnPoints[2]);
                    }
                }
                Debug.Log("extensionIndexAfter" + extensionIndex);
                break;
        }

    }
    public void SetLoadout(){
        inputManager.gameObject.GetComponent<PlayerIdentity>().SetLoadout(
            bodyItems[bodyIndex],
            barrelItems[barrelIndex],
            extensionItems.Count != 0 ? extensionItems[extensionIndex] : null);
    }
    private void OnDestroy() {
        //inputManager.onMovePerformed -= MoveInputPerformed;
        //inputManager.onMoveCanceled -= MoveInputCanceled;
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
