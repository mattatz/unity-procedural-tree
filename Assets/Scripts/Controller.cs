using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {

	#region Public Variables

	public GameObject prefab;
	public float radius = 10f;
	public bool auto;

	#endregion

	void Start () {
	}
	
	void Update () {
		if(Input.GetMouseButtonDown(0)) {
			Spawn();
		}

		if(auto && Random.value > 0.98f) {
			Spawn();
		}
	}

	void Spawn () {
		GameObject go = Instantiate(prefab) as GameObject;
		go.transform.position = Random.insideUnitSphere * radius + transform.position;
	}

	void OnDrawGizmos () {
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(transform.position, radius);
	}

}
