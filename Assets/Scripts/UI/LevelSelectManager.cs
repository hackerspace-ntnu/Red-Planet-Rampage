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
    private MainMenuController mainMenuController;
    [SerializeField]
    private UnityEngine.UI.Button backButton;
    [SerializeField]
    private UnityEngine.UI.Button startButton;
    [SerializeField]
    private GameObject parent;
    [SerializeField]
    private List<GameObject> levelCards;
    [SerializeField]
    private float totalDegrees = 130f;
    [SerializeField]
    private float radius = 0.5f;


    private List<GameObject> instantiatedCards = new List<GameObject>();
    private string sceneName;

    public void Start()
    {
        float angleBetween = totalDegrees / ((float)levelCards.Count + 1);
        float startAngle = 180 - totalDegrees / 2;
        
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

        if (startButton != null && mainMenuController != null)
        {
            startButton.onClick.AddListener(HandleStartClick);

            Navigation backNavigation = backButton.navigation;
            backNavigation.mode = Navigation.Mode.Explicit;
            UnityEngine.UI.Button firstButton = instantiatedCards[0].GetComponent<UnityEngine.UI.Button>();

            if (firstButton != null)
            {
                backNavigation.selectOnUp = firstButton;
            }

            backButton.navigation = backNavigation;
        }

        for (int i = 0; i < levelCards.Count; i++)
        {
            UnityEngine.UI.Button currentButton = instantiatedCards[i].GetComponent<UnityEngine.UI.Button>();
            if (currentButton != null)
            {
                sceneName = instantiatedCards[i].name;
                sceneName = sceneName.TrimEnd("(Clone)");
                Debug.Log(sceneName);
                SetupButtonNavigation(currentButton, i, levelCards.Count);
            }
        }
    }

    private void SetupButtonNavigation(UnityEngine.UI.Button button, int index, int totalCount)
    {
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.Explicit;

        if (index == 0 && totalCount > 1) //First levelcard button
        {
            navigation.selectOnRight = instantiatedCards[index + 1].GetComponent<UnityEngine.UI.Button>();
            navigation.selectOnDown = backButton;

            button.onClick.AddListener(HandleCardClick);
        }
        else if (index == totalCount - 1) //Second levelcard button
        {
            navigation.selectOnLeft = instantiatedCards[index - 1].GetComponent<UnityEngine.UI.Button>();
            navigation.selectOnDown = backButton;

            button.onClick.AddListener(HandleCardClick);
        }
        else //Middle levelcard buttons
        {
            navigation.selectOnRight = instantiatedCards[index + 1].GetComponent<UnityEngine.UI.Button>();
            navigation.selectOnLeft = instantiatedCards[index - 1].GetComponent<UnityEngine.UI.Button>();
            navigation.selectOnDown = backButton;

            button.onClick.AddListener(HandleCardClick);

        }

        button.navigation = navigation; 
    }

    private void HandleCardClick()
    {
        if (mainMenuController)
            mainMenuController.ChangeScene(sceneName);
    }

    private void HandleStartClick()
    {
        if (mainMenuController)
            mainMenuController.StartGameButton(instantiatedCards[0].GetComponent<UnityEngine.UI.Button>());
    }
}
