using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class NewWorkManager : MonoBehaviour {

	[SerializeField] Text connectionText;
	[SerializeField] Transform[] spawnPoints;
	[SerializeField] Camera sceneCamera;

	[SerializeField] GameObject serverWindow;
	[SerializeField] InputField userName;
	[SerializeField] InputField roomName;
	[SerializeField] InputField roomList;
	[SerializeField] InputField messageWindow;

	GameObject player;
	Queue<string>messages;
	const int messageCount = 6;
	PhotonView photonView;
	Slider healthBar;

	void Start () {
		photonView = GetComponent<PhotonView> ();
		messages = new Queue<string> (messageCount);
		PhotonNetwork.logLevel = PhotonLogLevel.Full;
		PhotonNetwork.ConnectUsingSettings ("0.1");
		StartCoroutine ("UpdateConnectionString");
		healthBar = GameObject.FindWithTag ("HealthBar").GetComponent<Slider>();
		healthBar.gameObject.SetActive (false);
	
	}
	
	IEnumerator UpdateConnectionString () {
		while (true) {
			connectionText.text = PhotonNetwork.connectionStateDetailed.ToString ();
			yield return null;
		}
	}

	void OnJoinedLobby(){
		serverWindow.SetActive (true);
	}

	void OnReceivedRoomListUpdate(){
		roomList.text = "";
		RoomInfo[] rooms = PhotonNetwork.GetRoomList ();
		foreach (RoomInfo room in rooms) {
			roomList.text += room.name + "\n";
		}
	}

	public void JoinRoom(){

		PhotonNetwork.player.name = userName.text;
		RoomOptions ro = new RoomOptions () {
			IsVisible = true, MaxPlayers = 10
		};
		PhotonNetwork.JoinOrCreateRoom (roomName.text, ro, TypedLobby.Default);
	}

	void OnJoinedRoom(){
		serverWindow.SetActive (false);
		StopCoroutine ("UpdateConnectionString");
		connectionText.text = "";
		StartSpawnProcess (0f);
	}

	void StartSpawnProcess(float respawnTime){
		sceneCamera.enabled = true;
		StartCoroutine ("SpawnPlayer", respawnTime);
	}

	IEnumerator SpawnPlayer(float respawnTime){
		yield return new WaitForSeconds (respawnTime);
		int index = Random.Range (0, spawnPoints.Length);
		player = PhotonNetwork.Instantiate ("FPSPlayer", spawnPoints [index].position, spawnPoints [index].rotation, 0);
		player.GetComponent<PlayerNetWorkMover> ().RespawnMe += StartSpawnProcess;
		player.GetComponent<PlayerNetWorkMover> ().SendNetworkMessage += AddMessage;
		sceneCamera.enabled = false;
		healthBar.gameObject.SetActive (true);
		AddMessage ("Spawned player: " + PhotonNetwork.player.name);
	}

	void AddMessage(string message){
		photonView.RPC ("AddMessage_RPC", PhotonTargets.All, message);
	}

	[PunRPC]
	void AddMessage_RPC(string message){
		messages.Enqueue (message);
		if (messages.Count > messageCount) {
			messages.Dequeue ();
		}
		messageWindow.text = "";
		foreach (string m in messages) {
			messageWindow.text += m + "\n";
		}
	}
}
