using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour {

	public static MenuManager instance;

	private string curMenu;
	public string name, matchName, ipToConnect;
	public int maxPlayers;

	public int selected = 1;

	void Start () {
		instance = this;
		curMenu = "Main";
		name = PlayerPrefs.GetString ("name");
		maxPlayers = 10;
		matchName = "Server " + Random.Range (0, 99);
	}

	void Update () {
	
	}

	void ToMenu (string menu) {
		curMenu = menu;

	}

	void OnGUI () {
		if(curMenu == "Main")
			Main ();
		if(curMenu == "Host")
			Host ();
		if(curMenu == "Lobby")
			Lobby ();
		if(curMenu == "MatchList")
			MatchList ();
		if(curMenu == "Levels")
			Levels ();
	}

	void Main () {
		if (GUI.Button (new Rect (0, 0, 128, 32), "Host Game")) {
			ToMenu ("Host");
		}
		if (GUI.Button (new Rect (0, 33, 128, 32), "Browse Games")) {
			ToMenu ("MatchList");
		}

		name = GUI.TextField (new Rect(130, 0, 128, 32), name);
		if(GUI.Button (new Rect(258, 0, 128, 32), "Set Name")){
			PlayerPrefs.SetString("name", name);
			NetworkManager.instance.playerName = name;
		}
		ipToConnect = GUI.TextField(new Rect(130, 33, 128, 32), ipToConnect);
		if(GUI.Button (new Rect(258, 33, 128, 32), "Connect")){
			Network.Connect(ipToConnect,5996);
		}
	}

	void Host () {
		if (GUI.Button(new Rect (0, 0, 128, 32), "Start Game")) {
			NetworkManager.instance.StartServer (matchName, maxPlayers);
			ToMenu ("Lobby");
		}

		if (GUI.Button(new Rect (0, 33, 128, 32), "Main Menu")) {
			ToMenu ("Main");
		}

		if (GUI.Button(new Rect (0, 66, 128, 32), "Choose Map")) {
			ToMenu ("Levels");
		}
	}

	void Lobby () {
		if (Network.isServer) 
		{
			if(GUI.Button (new Rect(Screen.width - 128, Screen.height - 64, 128, 32), "Start Match")){
				NetworkManager.instance.networkView.RPC ("LoadLevel", RPCMode.All, "mansion");
			}

		}

		if(GUI.Button(new Rect(Screen.width - 128, Screen.height - 32, 128, 32), "Disconnect")){
			Network.Disconnect();
			ToMenu ("Main");
		}

		GUILayout.BeginArea(new Rect(0, 0, 128, Screen.height), "Players");
		GUILayout.Space(20);
		foreach(Player pl in NetworkManager.instance.PlayerList){
			GUILayout.BeginHorizontal();
			GUI.color = Color.blue;
			GUILayout.Box(pl.playerName);
			GUILayout.EndHorizontal();
		}
		GUILayout.EndArea ();
	}

	void MatchList () {
		MasterServer.RequestHostList("RainbowSix");
		if (GUI.Button (new Rect (0, 0, 128, 32), "Refresh")) {
			MasterServer.RequestHostList("RainbowSix");
		}

		if (GUI.Button (new Rect (0, 33, 128, 32), "Main Menu")) {
			ToMenu("Main");
			}

		GUILayout.BeginArea (new Rect (Screen.width / 2, 0, Screen.width / 2, Screen.height), "Server List", "box");

		foreach(HostData hd in MasterServer.PollHostList ()) {
			GUILayout.BeginHorizontal();
			GUILayout.Label (hd.gameName);
			if(GUILayout.Button ("Connect")){
				Network.Connect (hd) ;
				ToMenu ("Lobby");
			}
			GUILayout.EndHorizontal();
		}

		GUILayout.EndArea ();
	}

	void Levels () {
		foreach (Level lvl in NetworkManager.instance.ListOfLevels) {
			if(GUILayout.Button (lvl.PlayName))
				NetworkManager.instance.curLevel = lvl;
		}

		if (GUILayout.Button ("Back"))
						ToMenu ("Host");

	}
}


