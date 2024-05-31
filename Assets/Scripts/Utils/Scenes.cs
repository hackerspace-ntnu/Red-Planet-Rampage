public class Scenes
{
    public const string Menu = "Menu";
    public const string ClientLobby = "ClientLobby";
    public const string TrainingMode = "TrainingMode";
    public const string Bidding = "Bidding";

    public static readonly string[] MenuScenes = new[] { Menu, ClientLobby };
    public static readonly string[] NotArenaScenes = new[] { Menu, ClientLobby, Bidding };
}
