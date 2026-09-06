public class Scenes
{
    public const string Menu = "Menu";
    public const string ClientLobby = "ClientLobby";
    public const string TrainingMode = "TrainingMode";
    public const string Bidding = "Bidding";

    public const string CraterTown = "CraterTown";
    public const string ThePit = "ThePit";
    public const string GrandCanyon = "GrandCanyon";

    public static readonly string[] MenuScenes = new[] { Menu, ClientLobby };
    public static readonly string[] NotArenaScenes = new[] { Menu, ClientLobby, Bidding };

    public static string SceneToStatusString(string map) =>
        map switch
        {
            CraterTown => "Crater Town",
            ThePit => "The Pit",
            GrandCanyon => "Grand Canyon",
            _ => "[REDACTED]",
        };
}
