using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ItemSelectManager : MonoBehaviour
{
    [SerializeField] 
    private Item defaultBodyItem;
    [SerializeField] 
    private Item defaultBarrelItem;
    [SerializeField] 
    private Transform[] itemSpawnPoints;
    
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
    private GameObject currentSelectorObject;
    private Vector2 moveInput;
    private int bodyIndex;
    private int barrelIndex;
    private int extensionIndex;
    
   void Start()
    {

        inputManager.onMovePerformed += MoveInputPerformed;
        SelectControl(defaultSelector);
    }
    public void SpawnItems(InputManager inputManager){
        this.inputManager = inputManager;
        bodyItems = inputManager.gameObject.GetComponent<PlayerIdentity>().Bodies;
        barrelItems = inputManager.gameObject.GetComponent<PlayerIdentity>().Barrels;
        extensionItems = inputManager.gameObject.GetComponent<PlayerIdentity>().Extensions;   

        InstantiateItems(bodyItems,defaultBodyItem,itemSpawnPoints[0], bodies);
        InstantiateItems(barrelItems,defaultBarrelItem,itemSpawnPoints[1], barrels);
        InstantiateItems(extensionItems,null,itemSpawnPoints[2],extensions);

        bodyIndex = bodies.Count - 1;
        barrelIndex = barrels.Count - 1;
        extensionIndex = extensions.Count - 1;
        Debug.Log("bodyIndex" + bodyIndex);
        Debug.Log("barrelIndex" + barrelIndex);
        Debug.Log("extensionIndex" + extensionIndex);


        ChangeItemDisplayed(bodies[^1], bodies[^1], itemSpawnPoints[0]);
        ChangeItemDisplayed(barrels[^1],barrels[^1],itemSpawnPoints[1]);
        ChangeItemDisplayed(
            extensions.Count != 0 ? extensions[^1] : null, 
            extensions.Count != 0 ? extensions[^1] : null, 
            itemSpawnPoints[2]);

        timer.StartTimer(10f);
        
        timer.OnStopTimer += ChangeScene;
        timer.OnStopTimer += SetLoadout;
        timer.OnStopTimer += ClearItems;
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
            nextItem.transform.localScale = new Vector3(1f,150f,150f);
        }else{
            Debug.Log("Player has no extensions");
        }
    
    }
    private void InstantiateItems(List<Item> items, Item defaultItem, Transform itemSpawnPoint, List<GameObject> itemObjects){

        if(defaultItem != null) items.Insert(0,defaultItem);

        for(int i = 0; i < items.Count; i++){
            itemObjects.Add(Instantiate(items[i].augment, Vector3.zero, Quaternion.Euler(new Vector3(0, 90, 0))));
        }
        Debug.Log(items.Count);
        Debug.Log(itemObjects.Count);
        
    }
     private void MoveInputPerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
        Debug.Log(moveInput);
        currentSelectorObject = EventSystem.current.currentSelectedGameObject;
        Debug.Log(currentSelectorObject.name);

        if(moveInput == Vector2.up){
            Debug.Log("up");
            MoveUpPerformed();
        }else if (moveInput == Vector2.down){
            MoveDownPerformed();
        }
    }
     private void MoveUpPerformed(){
        switch(currentSelectorObject.name)
        {
            case "BodySelector":
                if(bodyIndex == bodies.Count - 1){
                    bodyIndex = 0;
                    ChangeItemDisplayed(bodies[^1],bodies[bodyIndex],itemSpawnPoints[0]);
                }else {
                    bodyIndex ++;
                    ChangeItemDisplayed(bodies[bodyIndex - 1],bodies[bodyIndex],itemSpawnPoints[0]);
                }
                break;
            case "BarrelSelector":
                if(barrelIndex == barrels.Count - 1){
                    barrelIndex = 0;
                    ChangeItemDisplayed(barrels[^1],barrels[barrelIndex],itemSpawnPoints[1]);

                }else{
                    barrelIndex ++;
                    ChangeItemDisplayed(barrels[barrelIndex - 1],barrels[barrelIndex],itemSpawnPoints[1]);
                }
                break;
            case "ExtensionSelector":
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
        switch(currentSelectorObject.name)
        {
            case "BodySelector":
                if(bodyIndex == 0){
                    bodyIndex = bodies.Count - 1;
                    ChangeItemDisplayed(bodies[0],bodies[bodyIndex],itemSpawnPoints[0]);
                }else {
                    bodyIndex --;
                    ChangeItemDisplayed(bodies[bodyIndex + 1],bodies[bodyIndex],itemSpawnPoints[0]);
                }
                break;
            case "BarrelSelector":
                if(barrelIndex == 0){
                    barrelIndex = barrels.Count - 1;
                    ChangeItemDisplayed(barrels[0],barrels[barrelIndex],itemSpawnPoints[1]);

                }else{
                    barrelIndex --;
                    ChangeItemDisplayed(barrels[barrelIndex + 1],barrels[barrelIndex],itemSpawnPoints[1]);
                }
                break;
            case "ExtensionSelector":
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
        inputManager.gameObject.GetComponent<PlayerIdentity>().SetLoadout(
            bodyItems[bodyIndex],
            barrelItems[barrelIndex],
            extensionItems.Count != 0 ? extensionItems[extensionIndex] : null);
    }
    public void ClearItems(){
        /*for(int i = 0; i < bodies.Count; i++){
            Destroy(bodies[i]);
        }
        for(int i = 0; i < barrels.Count; i++){
            Destroy(barrels[i]);
        }
        for(int i = 0; i < extensions.Count; i++){
            Destroy(extensions[i]);
        }*/
        bodies.Clear();
        barrels.Clear();
        extensions.Clear(); 

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
        target.Select();
    }
}
