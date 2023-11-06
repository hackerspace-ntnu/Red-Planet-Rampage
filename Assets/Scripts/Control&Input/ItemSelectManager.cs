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
    
    public void SetItemSpawnPoints(InputManager inputManager,Transform spawnPoint){
        List<Item> bodyItems = inputManager.gameObject.GetComponent<PlayerIdentity>().Bodies;
        GameObject defaultBody = Instantiate(defaultItemPrefabs[0], Vector3.zero, Quaternion.Euler(new Vector3(0, 90, 0)));
        defaultBody.transform.localScale = new Vector3(1f,150f,150f);


        for(int i = 0; i < bodyItems.Count; i++){
            bodies.Add(Instantiate(bodyItems[i].augment, Vector3.zero, Quaternion.Euler(new Vector3(0, 90, 0))));
            bodies[i].transform.localScale = new Vector3(1f,150f,150f);
            Debug.Log("number" + i);
        }
        Debug.Log(bodies.Count);

        if(bodies.Count == 0){
            defaultBody.transform.SetParent(itemSpawnPoints[0]);
            defaultBody.transform.localScale = new Vector3(1f,150f,150f);

        }else{
            Debug.Log("parented");
            bodies[bodies.Count - 1].transform.SetParent(itemSpawnPoints[0]);
            bodies[bodies.Count - 1].transform.localPosition = Vector3.zero;
            bodies[bodies.Count - 1].transform.localScale = new Vector3(1f,150f,150f);


        }
    }
}
