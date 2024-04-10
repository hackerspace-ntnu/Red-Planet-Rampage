using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class LevelSelectManager : MonoBehaviour
{
    [SerializeField]
    private GameObject parent;
    [SerializeField]
    private List<GameObject> levelCards;
    [SerializeField]
    private float totalDegrees = 130f;
    [SerializeField]
    private float radius = 0.5f;

    private List<GameObject> instantiatedCards = new List<GameObject>();
    


    public void Start()
    {
        float angleBetween = totalDegrees / ((float)levelCards.Count + 1);
        float startAngle = 180 - totalDegrees / 2;
        
        Debug.Log(startAngle);
        for (int i = 0; i < levelCards.Count; i++)
        {
            Vector3 offsetPosition = new Vector3(0f, 0f, 3f);
            Quaternion rotation = Quaternion.Euler(0f, startAngle + angleBetween * (i + 1), 0f);

            //Create new card parent to rotate independently of parent
            GameObject newParent = new GameObject(levelCards[i].name + "Parent");
            newParent.transform.SetParent(parent.transform);
            newParent.transform.position = parent.transform.position;
            newParent.transform.localScale = Vector3.one;
            
            //Instantiate levelcard and attatch to cardparent
            GameObject levelCard = Instantiate(levelCards[i], newParent.transform.position, Quaternion.Euler(-90f, 180f, 180f), newParent.transform);
            levelCard.transform.localPosition += new Vector3(newParent.transform.localPosition.x, newParent.transform.localPosition.y, newParent.transform.localPosition.z - 0.8f);
            newParent.transform.rotation = rotation;

            //Add levelcard to list for later use
            instantiatedCards.Add(levelCard);
        }

        for (int i = 0; i < levelCards.Count; i++)
        {
            UnityEngine.UI.Button currentButton = instantiatedCards[i].GetComponent<UnityEngine.UI.Button>();

            if (currentButton != null)
            {
                Navigation navigation = currentButton.navigation;
                navigation.mode = Navigation.Mode.Explicit;
                
                if (i == 0)
                {
                    UnityEngine.UI.Button nextButton = instantiatedCards[i + 1].GetComponent<UnityEngine.UI.Button>();
                    if (nextButton != null)
                    {
                        navigation.selectOnRight = nextButton;
                    }
                }
                else if (i == levelCards.Count - 1)
                {
                    UnityEngine.UI.Button prevButton = instantiatedCards[i - 1].GetComponent<UnityEngine.UI.Button>();
                    if (prevButton != null)
                    {
                        navigation.selectOnLeft = prevButton;
                    }
                }
                else
                {
                    UnityEngine.UI.Button nextButton = instantiatedCards[i + 1].GetComponent<UnityEngine.UI.Button>();
                    UnityEngine.UI.Button prevButton = instantiatedCards[i - 1].GetComponent<UnityEngine.UI.Button>();
                    navigation.selectOnRight = nextButton;
                    navigation.selectOnLeft = prevButton;
                }
            }
            
        }
    }
}
