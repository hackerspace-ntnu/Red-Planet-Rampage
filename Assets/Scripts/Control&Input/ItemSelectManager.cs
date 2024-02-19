using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemSelectManager : MonoBehaviour
{
    [SerializeField] private float graceTime = 1;

    private List<ItemSelectMenu> itemSelectMenus;

    private Coroutine waitRoutine;
    
    [SerializeField]
    private GameObject loadingScreen;

    private int delayDuration = 2;

    public void StartTrackingMenus()
    {
        itemSelectMenus = new List<ItemSelectMenu>();
        foreach (var menu in FindObjectsOfType<ItemSelectMenu>())
        {
            itemSelectMenus.Add(menu);
            menu.OnReady += OnReady;
            menu.OnNotReady += OnNotReady;
        }
    }

    private void Finish()
    {
        StartCoroutine(LoadAndChangeScene());
    }

    private IEnumerator LoadAndChangeScene(){

        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(delayDuration);
        AuctionDriver.Singleton.ChangeScene();
    }

    private IEnumerator WaitAndFinish()
    {
        yield return new WaitForSeconds(graceTime);
        Finish();
    }

    private void OnNotReady(ItemSelectMenu menu)
    {
        if (waitRoutine is not null)
        {
            StopCoroutine(waitRoutine);
        }
    }

    private void OnReady(ItemSelectMenu menu)
    {
        var allPlayersAreReady = itemSelectMenus.All(m => m.IsReady);
        if (allPlayersAreReady)
        {
            waitRoutine = StartCoroutine(WaitAndFinish());
        }
    }
}
