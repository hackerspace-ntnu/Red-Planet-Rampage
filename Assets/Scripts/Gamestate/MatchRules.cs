using UnityEngine;

public class MatchRules : MonoBehaviour
{
    public static MatchRules Singleton { get; private set; }

    public Ruleset Rules;

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
}
