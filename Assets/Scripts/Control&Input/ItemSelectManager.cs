using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSelectManager : MonoBehaviour
{
    [SerializeField] 
    private GameObject[] defaultItemPrefabs;
    [SerializeField] 
    private Transform[] itemSpawnPoints;

    
    public void SetItemSpawnPoints(InputManager inputManager,Transform spawnPoint){
         //List<Item> bodies = inputManager.gameObject.GetComponent<PlayerIdentity>().Bodies;
        GameObject body = Instantiate(defaultItemPrefabs[0], Vector3.zero, Quaternion.Euler(new Vector3(0, 90, 0)));
        body.transform.SetParent(itemSpawnPoints[0]);
        body.transform.localScale = new Vector3(1f,150f,150f);
    

    }
    
}
