using OperatorExtensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    private List<LevelCard> levelCards;
    [SerializeField]
    private float totalDegrees = 130f;
    [SerializeField]
    private float radius = 0.5f;


    private List<GameObject> instantiatedCards = new List<GameObject>();
    private InputManager input;
    private EventSystem eventSystem;

    public void Start()
    {
        eventSystem = EventSystem.current;
        
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
            GameObject levelCard = Instantiate(levelCards[i].LevelCardPrefab, newParent.transform.position, Quaternion.Euler(-90f, 180f, 180f), newParent.transform);
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
                backNavigation.selectOnDown = firstButton;
            }

            backButton.navigation = backNavigation;
        }

        for (int i = 0; i < levelCards.Count; i++)
        {
            UnityEngine.UI.Button currentButton = instantiatedCards[i].GetComponent<UnityEngine.UI.Button>();
            if (currentButton != null)
            {
                SetupButtonNavigation(currentButton, i, levelCards.Count);
            }
        }
    }

    public void SetPlayerInput(InputManager input)
    {
        this.input = input;
        input.onSelect += HandleCardClick;
    }

    private void SetupButtonNavigation(UnityEngine.UI.Button button, int index, int totalCount)
    {
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.Explicit;

        navigation.selectOnRight = instantiatedCards[(index + 1).Mod(totalCount)].GetComponent<UnityEngine.UI.Button>();
        navigation.selectOnLeft = instantiatedCards[(index - 1).Mod(totalCount)].GetComponent<UnityEngine.UI.Button>();
        navigation.selectOnDown = backButton;
        navigation.selectOnUp = backButton;

        button.navigation = navigation; 
    }

    private void HandleCardClick(InputAction.CallbackContext ctx)
    {
        GameObject selected = eventSystem.currentSelectedGameObject;

        if (selected != null && instantiatedCards.Contains(eventSystem.currentSelectedGameObject))
        {
            string levelName = selected.GetComponent<LevelCard>().CardName;
            mainMenuController.ChangeScene(levelName);
        }
    }

    private void HandleStartClick()
    {
        if (mainMenuController)
            mainMenuController.StartGameButton(instantiatedCards[0].GetComponent<UnityEngine.UI.Button>());
    }

    private void OnDestroy()
    {
        if (input)
            input.onSelect -= HandleCardClick;
    }
}
