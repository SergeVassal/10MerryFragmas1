using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

public class PlayerNetWorkMover : Photon.MonoBehaviour{

	public delegate void Respawn (float time);
	public event Respawn RespawnMe;
	public delegate void SendMessage(string message);
	public event SendMessage SendNetworkMessage;

	Vector3 position;
	Quaternion rotation;
	float smoothing =10f;
	float health=100f;
	bool aim=false;
	bool sprint=false;
	Animator anim;
	bool initialLoad=true;
	Slider healthBar;

	void Start () {	

		anim = GetComponentInChildren<Animator> ();
		GameObject gun = transform.Find ("FirstPersonCharacter/GunCamera/CartoonSMGprefab").gameObject;

		
		if (photonView.isMine) {
			Rigidbody rBody = GetComponent<Rigidbody> ();
			healthBar = GameObject.FindWithTag ("HealthBar").GetComponent<Slider>();
			rBody.useGravity = true;
			GetComponent<FirstPersonController> ().enabled = true;												
			GetComponentInChildren<PlayerShooting> ().enabled = true;
			foreach (Camera cam in GetComponentsInChildren<Camera>()) {
				cam.enabled = true;
			}
			foreach (Transform trans in gun.GetComponentsInChildren<Transform>(true)) {
				trans.gameObject.layer = 11;
			}
		} else {
			StartCoroutine ("UpdateData");
		}
	}

	void Update(){
		if (photonView.isMine) {
			healthBar.value = health;
		}
	}

	IEnumerator UpdateData(){
		if (initialLoad) {
			initialLoad = false;
			transform.position = position;
			transform.rotation = rotation;
		}
		while (true) {
			transform.position = Vector3.Lerp (transform.position, position, Time.deltaTime * smoothing);
			transform.rotation = Quaternion.Lerp (transform.rotation, rotation, Time.deltaTime * smoothing);
			anim.SetBool ("Aim", aim);
			anim.SetBool ("Sprint", sprint);
			yield return null;
		}
	}

	void OnPhotonSerializeView(PhotonStream stream,PhotonMessageInfo info){
		if (stream.isWriting) {
			stream.SendNext (transform.position);
			stream.SendNext (transform.rotation);
			stream.SendNext (health);
			stream.SendNext(anim.GetBool("Aim"));
			stream.SendNext(anim.GetBool("Sprint"));				
		} else {
			position = (Vector3)stream.ReceiveNext ();
			rotation = (Quaternion)stream.ReceiveNext ();
			health = (float)stream.ReceiveNext ();
			aim=(bool)stream.ReceiveNext ();
			sprint=(bool)stream.ReceiveNext ();
		}
	}

	[PunRPC]
	public void GetShot(float damage,string enemyName){
		health -= damage;
		if (health <= 0&&photonView.isMine) {
			if (SendNetworkMessage != null) {
				SendNetworkMessage (PhotonNetwork.player.name + " was killed by "+enemyName);
			}
			if (RespawnMe != null) {
				RespawnMe (3f);
			}
			PhotonNetwork.Destroy (gameObject);
		}
	}

}
