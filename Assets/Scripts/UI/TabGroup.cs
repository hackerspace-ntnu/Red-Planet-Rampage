using System.Collections.Generic;
using System.Linq;
using OperatorExtensions;
using UnityEngine;
using UnityEngine.InputSystem;


public class TabGroup : MonoBehaviour
{
    public List<TabsButton> tabButtons;
    public Color tabIdle;
    public Color tabActive;

    public TabsButton selectedTab;

    private MainMenuController mainMenuController;

    public void SetPlayerInput(InputManager inputManager)
    {
        inputManager.onLeftTab += OnLeftButton;
        inputManager.onRightTab += OnRightButton;
    }

    private void Awake()
    {
        tabButtons = GetComponentsInChildren<TabsButton>().ToList();
        mainMenuController = GetComponentInParent<MainMenuController>();
        ResetTabs();
    }

    private void OnEnable()
    {
        ResetTabs();
    }

    public void Subscribe(TabsButton button)
    {
        tabButtons ??= new List<TabsButton>();

        tabButtons.Add(button);
    }

    public void SelectTab(TabsButton tab)
    {
        // Change the contents
        selectedTab.tabContent.SetActive(false);
        tab.tabContent.SetActive(true);

        // Change the tab UI
        selectedTab = tab;

        // Reset the tab menu
        ResetTabs();

        // Select the first element
        mainMenuController.SelectControl(selectedTab.firstItem);
    }

    private void OnLeftButton(InputAction.CallbackContext ctx)
    {
        if (!isActiveAndEnabled)
            return;

        // Move to the previous tab
        SwitchTab(-1);
    }

    private void OnRightButton(InputAction.CallbackContext ctx)
    {
        if (!isActiveAndEnabled)
            return;

        // Move to the next tab
        SwitchTab(+1);
    }

    private void SwitchTab(int direction)
    {
        int i = tabButtons.FindIndex(x => x == selectedTab);
        SelectTab(tabButtons[(i + direction).Mod(tabButtons.Count)]);
    }

    public void ResetTabs()
    {
        foreach (TabsButton button in tabButtons)
        {
            button.background.color = tabIdle;
        }

        selectedTab.background.color = tabActive;
    }
}
