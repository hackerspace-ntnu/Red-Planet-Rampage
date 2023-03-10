using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


public class TabGroup : MonoBehaviour
{
    public List<TabsButton> tabButtons;
    public Color tabIdle;
    public Color tabActive;

    public TabsButton selectedTab;

    public void SetPlayerInput(InputManager inputManager)
    {
        inputManager.onLeftTab += OnLeftButton;
        inputManager.onRightTab += OnRightButton;
    }

    private void Awake()
    {
        tabButtons = GetComponentsInChildren<TabsButton>().ToList();
        ResetTabs();
    }

    private void OnEnable()
    {
        ResetTabs();
    }
    public void Subscribe(TabsButton button)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<TabsButton>();
        }

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
        MainMenuController mainMenuController = GetComponentInParent<MainMenuController>();
        mainMenuController.SelectControl(selectedTab.firstItem);
    }

    private void OnLeftButton(InputAction.CallbackContext ctx)
    {
        if (this.isActiveAndEnabled)
        {
            // Move to the previous tab
            int i = tabButtons.FindIndex(x => x == selectedTab);
            if (i > 0)
                SelectTab(tabButtons[i - 1]);
        }
    }

    private void OnRightButton(InputAction.CallbackContext ctx)
    {
        if (this.isActiveAndEnabled)
        {
            // Move to the next tab
            int i = tabButtons.FindIndex(x => x == selectedTab);
            if (i < tabButtons.Count - 1)
                SelectTab(tabButtons[i + 1]);
        }
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
