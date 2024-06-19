using UnityEngine;

public class MatchRules : MonoBehaviour
{
    public static MatchRules Singleton { get; private set; }

    public Ruleset Rules;
    public static Ruleset Current => Singleton.Rules;

    public void Initialize() => Rules.Initialize();

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
        Initialize();
    }
}
