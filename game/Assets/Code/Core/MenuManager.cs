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
		curMenu = "Host";
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
			ToMenu ("Host")
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
			ToMenu ("Lobby");
		}

		if (GUI.Button(new Rect (0, 33, 128, 32), "Main Menu")) {
			ToMenu ("Main")
		}

		if (GUI.Button(new Rect (0, 66, 128, 32), "Choose Map")) {
			ToMenu ("Levels")
		}
	}

	void Lobby () {

	}

	void MatchList () {

	}

	void Levels () {

	}
}


