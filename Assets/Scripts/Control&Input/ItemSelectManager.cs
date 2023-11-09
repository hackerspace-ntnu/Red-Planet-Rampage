using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSelectManager : MonoBehaviour
{
    [SerializeField] 
    private GameObject[] defaultItemPrefabs;
    [SerializeField] 
    private Transform[] itemSpawnPoints;
    private GameObject currentBody;
    private GameObject currentBarrel;
    private GameObject currentExtension;
    private List<GameObject> bodies = new List<GameObject>();
    private List<GameObject> barrels = new List<GameObject>();
    private List<GameObject> extensions = new List<GameObject>();
    
    public void SetItemSpawnPoints(InputManager inputManager){
        List<Item> bodyItems = inputManager.gameObject.GetComponent<PlayerIdentity>().Bodies;
        List<Item> barrelItems = inputManager.gameObject.GetComponent<PlayerIdentity>().Barrels;
        List<Item> extensionItems = inputManager.gameObject.GetComponent<PlayerIdentity>().Extensions;

        SpawnItems(bodyItems,defaultItemPrefabs[0],itemSpawnPoints[0], bodies);
        SpawnItems(barrelItems,defaultItemPrefabs[1],itemSpawnPoints[1], barrels);

    }

    private void SpawnItems(List<Item> items, GameObject defaultItemPrefab, Transform itemSpawnPoint, List<GameObject> itemObjects){
        GameObject defaultItem = Instantiate(defaultItemPrefab, Vector3.zero, Quaternion.Euler(new Vector3(0, 90, 0)));
        defaultItem.transform.localScale = new Vector3(1f,150f,150f);

        for(int i = 0; i < items.Count; i++){
            itemObjects.Add(Instantiate(items[i].augment, Vector3.zero, Quaternion.Euler(new Vector3(0, 90, 0))));
        }
        Debug.Log(itemObjects.Count);

        if(itemObjects.Count == 0){
            SetSpawnPoint(defaultItem,itemSpawnPoint);
        }else{
            SetSpawnPoint(itemObjects[itemObjects.Count - 1],itemSpawnPoint);
        }
    }
    private void SetSpawnPoint(GameObject item,Transform itemSpawnPoint){
        Debug.Log("parented");
        item.transform.SetParent(itemSpawnPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = new Vector3(1f,150f,150f);
    }
    
}
