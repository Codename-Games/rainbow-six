using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour {
	
	public static LevelManager instance;
	
	//public List<Transform> SpawnPoints = new List<Transform>();
	public GameObject[] SpawnPoints;
	
	void Start () {
		instance = this;
		SpawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
	}
	
	void Update () {
		
	}
}
