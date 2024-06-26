using UnityEngine;

public class MatchRules : MonoBehaviour
{
    public static MatchRules Singleton { get; private set; }
    public static Ruleset Current => Singleton.Rules;

    [SerializeField]
    private Ruleset rules;
    public Ruleset Rules => rules;

    [SerializeField]
    private Ruleset[] gamemodes;
    public Ruleset[] Gamemodes => gamemodes;

    [SerializeField]
    private Ruleset customRulesTemplate;
    public Ruleset CustomRulesTemplate => customRulesTemplate;

    private void Awake()
    {
        #region Singleton boilerplate

        if (Singleton != null)
        {
            if (Singleton != this)
            {
                Debug.LogWarning($"There's more than one {Singleton.GetType()} in the scene!");
                Destroy(gameObject);
            }

            return;
        }

        Singleton = this;

        #endregion Singleton boilerplate

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        rules.InitializeRulesAfterCreation();
    }

    public void SetReceivedRuleset(NetworkRuleset ruleset)
    {
        rules = ruleset.ToRuleset();
    }

    public void SetCreatedRuleset(Ruleset ruleset)
    {
        rules = ruleset;
        rules.InitializeRulesAfterCreation();
    }
}
