using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GamemodeButton : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [SerializeField]
    private TMP_Text label;

    [SerializeField]
    private Ruleset gamemode;

    [SerializeField]
    private GamemodeSelectMenu menu;

    private void Start()
    {
        label.text = gamemode.DisplayName;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowGamemode();
    }

    public void OnSelect(BaseEventData eventData)
    {
        ShowGamemode();
    }

    private void ShowGamemode()
    {
        menu.ViewGamemode(gamemode);
    }
}
