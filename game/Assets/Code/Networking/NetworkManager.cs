using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour {

	public string playerName;
	public string matchName;
	public static NetworkManager instance;
	public List<Player> PlayerList = new List<Player>();
	public Player myPlayer;
	public GameObject SpawnPlayer;
	public bool matchStarted, matchLoaded;
	public Level curLevel;
	public List<Level> ListOfLevels = new List<Level>();
	private int TeamIndex;

	public int swatPlayers;
	public int terroristPlayers;

	void Awake()
	{
		instance = this;
		playerName = PlayerPrefs.GetString("name");
	}

	void Start () {
		DontDestroyOnLoad (gameObject);
	}

	void Update () {
	}

	public void StartServer(string serverName, int maxPlayers)
	{
		Network.InitializeSecurity();
		Network.InitializeServer(maxPlayers, 5996, true);
		MasterServer.RegisterHost("RainbowSix", serverName, serverName + "/Max Players: " + maxPlayers);
		Debug.Log("Server has started");
	}

	void OnPlayerConnected(NetworkPlayer id)
	{
		foreach(Player pl in PlayerList)
		{
			networkView.RPC("getLevel", id, curLevel.LoadName, matchStarted);
			networkView.RPC ("Client_PlayerJoined", id, pl.playerName, pl.onlinePlayer);
		}
	}

	void OnConnectedToServer()
	{
		networkView.RPC("Server_PlayerJoined", RPCMode.Server, playerName, Network.player);
	}

	void OnServerInitialized()
	{
		Server_PlayerJoined (playerName, Network.player);
	}

	void OnPlayerDisconnected(NetworkPlayer id)
	{
		networkView.RPC("RemovePlayer", RPCMode.All, id);
		Network.Destroy (getPlayer(id).manager.gameObject);
		Network.RemoveRPCs(id);
	}

	void OnDisconnectedFromServer(NetworkDisconnection info)
	{
		foreach(Player pl in PlayerList)
		{
			Network.Destroy(pl.manager.gameObject);
		}
		PlayerList.Clear ();
		Application.LoadLevel(0);
	}

	[RPC]
	public void Server_PlayerJoined(string Username, NetworkPlayer id) {
		networkView.RPC("Client_PlayerJoined", RPCMode.All, Username, id);
	}

	[RPC]
	public void Client_PlayerJoined(string Username, NetworkPlayer id) {
		Player temp = new Player();
		temp.playerName = Username;
		temp.onlinePlayer = id;
		PlayerList.Add(temp);
		if(Network.player == id)
		{
			myPlayer = temp;
			GameObject LastPlayer = Network.Instantiate(SpawnPlayer, Vector3.zero, Quaternion.identity, 0) as GameObject;
			LastPlayer.networkView.RPC("RequestPlayer", RPCMode.AllBuffered, Username);
		}
	}

	[RPC]
	public void RemovePlayer(NetworkPlayer id)
	{
		Player temp = new Player();
		foreach(Player pl in PlayerList)
		{
			if(pl.onlinePlayer == id)
			{
				temp = pl;
			}
		}
		if(temp != null)
		{
			PlayerList.Remove (temp);
		}
	}
	
	[RPC]
	public void RemoveAllPlayers()
	{
		Player temp = new Player();
		foreach(Player pl in PlayerList)
		{
			PlayerList.Remove (pl);
			print ("Player Removed");
			Network.Destroy(pl.manager.gameObject);
			if(Network.isServer)
				MasterServer.UnregisterHost();
			Application.LoadLevel(0);
		}
	}

	[RPC]
	public void LoadLevel(string loadName)
	{
		matchStarted = true;
		int checkIndex = 0;
		foreach(Player pl in instance.PlayerList)
		{
			if(checkIndex == 0)
			{
				pl.Team = 0;
				checkIndex = 1;
			}
			else
			{
				pl.Team = 1;
				checkIndex = 0;
			}
		}
		Application.LoadLevel(loadName);
	}
	
	[RPC]
	public Level getLevel(string LoadName, bool isStarted)
	{
		foreach(Level lvl in ListOfLevels)
		{
			if(LoadName == lvl.LoadName)
			{
				curLevel = lvl;
				return lvl;
			}
		}
		if(isStarted)
		{
			LoadLevel (LoadName);
		}
		
		return null;
	}
	
	public static Player getPlayer(NetworkPlayer id)
	{
		foreach(Player pl in instance.PlayerList)
		{
			if(pl.onlinePlayer == id)
			{
				return pl;
			}
		}
		return null;
	}
	
	public static bool HasPlayer(string n)
	{
		foreach(Player pl in instance.PlayerList)
		{
			if(pl.playerName == n)
			{
				return true;
			}
		}
		return false;
	}
	
	public static Player getPlayer(string id)
	{
		foreach(Player pl in instance.PlayerList)
		{
			if(pl.playerName == id)
			{
				return pl;
			}
		}
		return null;
	}
	
	void OnGUI()
	{
		if(matchStarted == true && !myPlayer.isAlive)
		{
			if(GUI.Button(new Rect(Screen.width - 50, 0, 50, 20), "Spawn"))
			{
				int SpawnIndex = Random.Range(0, LevelManager.instance.SpawnPoints.Length - 1);
				myPlayer.manager.FirstPerson.localPosition = LevelManager.instance.SpawnPoints[SpawnIndex].transform.position;
				myPlayer.manager.FirstPerson.localRotation = LevelManager.instance.SpawnPoints[SpawnIndex].transform.rotation;
				myPlayer.manager.networkView.RPC("Spawn", RPCMode.All);
			}
		}
	}
}

[System.Serializable]
public class Player {
	public string playerName;
	public NetworkPlayer onlinePlayer;
	public float Health = 100;
	public PlayerController manager;
	public bool isAlive;
	public int Team;
}

[System.Serializable]
public class Level
{
	public string LoadName;
	public string PlayName;
}
