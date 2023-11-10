using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSelectManager : MonoBehaviour
{
    [SerializeField] 
    private GameObject[] defaultItemPrefabs;
    [SerializeField] 
    private Transform[] itemSpawnPoints;
    
    private List<GameObject> bodies = new List<GameObject>();
    private List<GameObject> barrels = new List<GameObject>();
    private List<GameObject> extensions = new List<GameObject>();
    [SerializeField] 
    private Timer timer;

    public void SetItemSpawnPoints(InputManager inputManager){

        timer.StartTimer(10f);
        timer.OnStopTimer += ChangeScene;
        List<Item> bodyItems = inputManager.gameObject.GetComponent<PlayerIdentity>().Bodies;
        List<Item> barrelItems = inputManager.gameObject.GetComponent<PlayerIdentity>().Barrels;
        List<Item> extensionItems = inputManager.gameObject.GetComponent<PlayerIdentity>().Extensions;

        Debug.Log("bodyItems: " + bodyItems.Count);
        Debug.Log("barrel list: " + barrelItems.Count);       

        SpawnItems(bodyItems,defaultItemPrefabs[0],itemSpawnPoints[0], bodies);
        SpawnItems(barrelItems,defaultItemPrefabs[1],itemSpawnPoints[1], barrels);

        inputManager.gameObject.GetComponent<PlayerIdentity>().SetLoadout(bodyItems[bodyItems.Count - 1]);
    }

    private void SpawnItems(List<Item> items, GameObject defaultItemPrefab, Transform itemSpawnPoint, List<GameObject> itemObjects){

        itemObjects.Add(Instantiate(defaultItemPrefab, Vector3.zero, Quaternion.Euler(new Vector3(0, 90, 0))));

        for(int i = 0; i < items.Count; i++){
            itemObjects.Add(Instantiate(items[i].augment, Vector3.zero, Quaternion.Euler(new Vector3(0, 90, 0))));
        }
        Debug.Log(items.Count);
        Debug.Log(itemObjects.Count);
        
        SetSpawnPoint(itemObjects[itemObjects.Count - 1],itemSpawnPoint);
            
        
    }
    private void SetSpawnPoint(GameObject item,Transform itemSpawnPoint){
        Debug.Log("parented");
        item.transform.SetParent(itemSpawnPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = new Vector3(1f,150f,150f);
    }
    void ChangeScene(){
        AuctionDriver.Singleton.ChangeScene();
    }
    
}
