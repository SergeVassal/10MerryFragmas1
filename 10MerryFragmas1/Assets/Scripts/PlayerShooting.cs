using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour {

	public ParticleSystem muzzleFlash;
	public GameObject impactPrefab;

	Animator anim;
	GameObject[] impacts;
	int currentImpact=0;
	int maxImpacts=5;
	bool shooting=false;
	float damage=10f;

	void Awake(){
		muzzleFlash.gameObject.SetActive (false);
		anim = GetComponentInChildren<Animator> ();
	}

	void Start () {
		impacts = new GameObject[maxImpacts];
		for (int i = 0; i < maxImpacts; i++) {
			impacts [i] = (GameObject)Instantiate (impactPrefab);
		}


	
	}
	
	void Update () {
		if (Input.GetButtonDown ("Fire1") && !Input.GetKey (KeyCode.LeftShift)) {			
			muzzleFlash.gameObject.SetActive (true);
			muzzleFlash.Play ();
			Debug.Log (muzzleFlash.isPlaying);
			anim.SetBool ("Fire", true);
			shooting = true;
		} else {
			anim.SetBool ("Fire",false);
		}
	}

	void FixedUpdate(){
		if (shooting) {
			shooting = false;
			RaycastHit hit;
			if (Physics.Raycast (transform.position, transform.forward, out hit, 50f)) {
				if (hit.transform.tag == "Player"&& !hit.transform.GetComponent<PhotonView>().isMine) {
					hit.transform.GetComponent<PhotonView> ().RPC ("GetShot", PhotonTargets.All, damage,PhotonNetwork.player.name);
				}
				impacts [currentImpact].transform.position = hit.point;
				impacts [currentImpact].GetComponent<ParticleSystem> ().Play ();

				if (++currentImpact >= maxImpacts) {
					currentImpact = 0;
				}
			}
		}
	}


}
