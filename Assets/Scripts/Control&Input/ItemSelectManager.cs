using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemSelectManager : MonoBehaviour
{
    private List<ItemSelectMenu> itemSelectMenus;

    public void StartTrackingMenus()
    {
        itemSelectMenus = new List<ItemSelectMenu>();
        foreach (var menu in FindObjectsOfType<ItemSelectMenu>())
        {
            itemSelectMenus.Add(menu);
            menu.OnReady += OnReady;
        }
    }

    private void Finish()
    {
        // TODO Fade to loading screen
        AuctionDriver.Singleton.ChangeScene();
    }

    private void OnReady(ItemSelectMenu menu)
    {
        if (itemSelectMenus.All(m => m.IsReady))
        {
            Finish();
        }
    }
}
