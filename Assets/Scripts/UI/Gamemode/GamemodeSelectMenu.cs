using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamemodeSelectMenu : MonoBehaviour
{
    [SerializeField]
    private TMP_Text title;

    [SerializeField]
    private TMP_Text description;

    public void ViewGamemode(Ruleset gamemode)
    {
        if (gamemode.DisplayName == "First to 30 Chips")
            title.text = $"<sprite name=chip> {gamemode.DisplayName} <sprite name=chip>";
        else
            title.text = gamemode.DisplayName;
        description.text = gamemode.Description;
    }

    public void SetGamemode(Ruleset gamemode)
    {
        MatchRules.Singleton.SetCreatedRuleset(gamemode);
    }
}
