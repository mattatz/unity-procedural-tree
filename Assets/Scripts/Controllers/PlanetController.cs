using UnityEngine;
using System.Collections;

public class PlanetController : MonoBehaviour {

	public GameObject prefab;
	public Planet planet;

	void Start () {
	}
	
	void Update () {
		if(Time.frameCount % 20 == 0 && planet.faces.Count > 0) {
			Spawn();
		}
	}

	void Spawn () {
		GameObject go = Instantiate(prefab) as GameObject;
		go.transform.parent = planet.transform;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale *= UnityEngine.Random.Range(0.01f, 0.2f);

		Planet.Face f = planet.RandomFace();
		Vector3 point = f.center;
		go.transform.localPosition = point;

		Vector3 dir = f.normal;
		go.transform.localRotation = Quaternion.LookRotation(dir) * Quaternion.FromToRotation(Vector3.back, Vector3.up);
	}

}
