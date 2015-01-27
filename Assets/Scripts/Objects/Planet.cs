using UnityEngine;

using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (MeshFilter))]
public class Planet : MonoBehaviour {

	public List<Face> faces {
		get {
			return _faces;
		}
	}

	public Vector3 axis = new Vector3(1f, 1f, 0f);
	public float speed = 0.2f;

	private List<Face> _faces;

	void Start () {
		_faces = new List<Face>();

		Mesh mesh = GetComponent<MeshFilter>().mesh;
		int[] triangles = mesh.triangles;

		for(int i = 0, n = triangles.Length; i < n; i += 3) {
			Vector3 a = mesh.vertices[triangles[i]];
			Vector3 b = mesh.vertices[triangles[i + 1]];
			Vector3 c = mesh.vertices[triangles[i + 2]];
			_faces.Add(new Face(a, b, c));
		}
	}
	
	void Update () {
		transform.RotateAround(transform.position, axis, speed);
	}

	public Face RandomFace () {
		int i = UnityEngine.Random.Range(0, _faces.Count);
		Face f = _faces[i];
		_faces.RemoveAt(i);
		return f;
	}

	public class Face {

		public readonly Vector3 a;
		public readonly Vector3 b;
		public readonly Vector3 c;

		public Vector3 center {
			get {
				return (a + b + c) / 3f;
			}
		} 

		public Vector3 normal {
			get {
				Vector3 s1 = b - a;
				Vector3 s2 = c - a;
				return Vector3.Cross(s1, s2).normalized;
			}
		}

		public Face (Vector3 a, Vector3 b, Vector3 c) {
			this.a = a;
			this.b = b;
			this.c = c;
		}

	}

}
