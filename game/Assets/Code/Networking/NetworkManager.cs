using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour {

	public string playerName;
	public string matchName;
	public NetworkManager instance;
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
			networkView.RPC("getLevel", id, CurLevel.LoadName, MatchStarted);
			networkView.RPC ("Client_PlayerJoined", id, pl.PlayerName, pl.OnlinePlayer);
		}
	}

	void OnConnectedToServer()
	{
		networkView.RPC("Server_PlayerJoined", RPCMode.Server, PlayerName, Network.player);
	}

	void OnServerInitialized()
	{
		Server_PlayerJoined (PlayerName, Network.player);
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
		temp.PlayerName = Username;
		temp.OnlinePlayer = id;
		PlayerList.Add(temp);
		if(Network.player == id)
		{
			MyPlayer = temp;
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
			if(pl.OnlinePlayer == id)
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
}

[System.Serializable]
public class Player {
	public string playerName;
	public NetworkPlayer onlinePlayer;
	public float Health = 100;
	//public PlayerController manager;
	public bool isAlive;
	public int Team;
}

[System.Serializable]
public class Level
{
	public string LoadName;
	public string PlayName;
}
