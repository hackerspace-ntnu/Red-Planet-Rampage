using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
   
    public void SpawnItems(InputManager inputManager){

        timer.StartTimer(10f);
        List<Item> bodyItems = inputManager.gameObject.GetComponent<PlayerIdentity>().Bodies;
        List<Item> barrelItems = inputManager.gameObject.GetComponent<PlayerIdentity>().Barrels;
        List<Item> extensionItems = inputManager.gameObject.GetComponent<PlayerIdentity>().Extensions;

        Debug.Log("bodyItems: " + bodyItems.Count);
        Debug.Log("barrel list: " + barrelItems.Count);       

        SetSpawnPoints(bodyItems,defaultBodyItem,itemSpawnPoints[0], bodies);
        SetSpawnPoints(barrelItems,defaultBarrelItem,itemSpawnPoints[1], barrels);
        SetSpawnPoints(extensionItems,null,itemSpawnPoints[2],extensions);  

        inputManager.gameObject.GetComponent<PlayerIdentity>().SetLoadout(
            bodyItems[^1],
            barrelItems[^1],
            extensionItems.Count != 0 ? extensionItems[^1] : null);

        bodies.Clear();
        barrels.Clear();
        extensions.Clear(); 

        timer.OnStopTimer += ChangeScene;

    }
    
    private void SetSpawnPoints(List<Item> items, Item defaultItem, Transform itemSpawnPoint, List<GameObject> itemObjects){

        if(defaultItem != null) items.Insert(0,defaultItem);

        for(int i = 0; i < items.Count; i++){
            itemObjects.Add(Instantiate(items[i].augment, new Vector3(-1000,-1000,0), Quaternion.Euler(new Vector3(0, 90, 0))));
        }
        Debug.Log(items.Count);
        Debug.Log(itemObjects.Count);
        
        SetItemSpawnPoint(itemObjects.Count != 0 ? itemObjects[^1] : null,itemSpawnPoint);
            

    }
    private void SetItemSpawnPoint(GameObject item,Transform itemSpawnPoint){
        if(item != null){
            Debug.Log("parented");
            item.transform.SetParent(itemSpawnPoint);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = new Vector3(1f,150f,150f);
        }else{
            Debug.Log("Player has no extensions");
        }
    }
 
    void ChangeScene(){
        AuctionDriver.Singleton.ChangeScene();
    }
    
}
