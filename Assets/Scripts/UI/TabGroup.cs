using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    public List<TabsButton> tabButtons;
    public Color tabIdle;
    public Color tabActive;

    public TabsButton selectedTab;

    public void Subscribe(TabsButton button)
    {
        if(tabButtons == null)
        {
            tabButtons = new List<TabsButton>();
        }

        tabButtons.Add(button);
    }

    public void OnTabSelected(TabsButton tab)
    {
        // Change the contents
        selectedTab.tab.SetActive(false);
        tab.tab.SetActive(true);

        // Change the tab UI
        selectedTab = tab;
        ResetTabs();
        tab.background.color = tabActive;
        
    }

    public void ResetTabs()
    {
        foreach(TabsButton button in tabButtons)
        {
            if(selectedTab != null && button == selectedTab) 
                continue;
            
            button.background.color = tabIdle;
        }
    }
}
