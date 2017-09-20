using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

namespace ProceduralModeling {

	public class PTGarden : MonoBehaviour {

		[SerializeField] Camera cam;
		[SerializeField] List<GameObject> prefabs;
		[SerializeField] Vector2 scaleRange = new Vector2(1f, 1.2f);

		const string SHADER_PATH = "Hidden/Internal-Colored";

		Material lineMaterial = null;
		MeshCollider col = null;
		Vector3[] vertices;
		int[] triangles;

		bool hit;
		Vector3 point;
		Vector3 normal;
		Quaternion rotation;

		void Update () {
			var mouse = Input.mousePosition;
			var ray = cam.ScreenPointToRay(mouse);
			RaycastHit info;
			hit = col.Raycast(ray, out info, float.MaxValue);
			if(hit) {
				point = info.point;
				var t = info.triangleIndex * 3;
				var a = triangles[t];
				var b = triangles[t + 1];
				var c = triangles[t + 2];
				var va = vertices[a];
				var vb = vertices[b];
				var vc = vertices[c];
				normal = transform.TransformDirection(Vector3.Cross(vb - va, vc - va));
				rotation = Quaternion.LookRotation(normal);
			}

			if(Input.GetMouseButtonUp(0) && hit) {
				var go = Instantiate(prefabs[Random.Range(0, prefabs.Count)]) as GameObject;
				go.transform.position = point;
				go.transform.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);
				go.transform.localRotation = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);

				var tree = go.GetComponent<ProceduralTree>();
				tree.Data.randomSeed = Random.Range(0, 300);
			}

		}

		const int resolution = 16;
		const float pi2 = Mathf.PI * 2f;
		const float radius = 0.5f;
		Color color = new Color(0.6f, 0.75f, 1f);

		void OnRenderObject () {
			if(!hit) return;

			CheckInit();

			lineMaterial.SetPass(0);
			lineMaterial.SetInt("_ZTest", (int)CompareFunction.Always);

			GL.PushMatrix();
			GL.Begin(GL.LINES);
			GL.Color(color);

			for(int i = 0; i < resolution; i++) {
				var cur = (float)i / resolution * pi2;
				var next = (float)(i + 1) / resolution * pi2;
				var p1 = rotation * new Vector3(Mathf.Cos(cur), Mathf.Sin(cur), 0f);
				var p2 = rotation * new Vector3(Mathf.Cos(next), Mathf.Sin(next), 0f);
				GL.Vertex(point + p1 * radius);
				GL.Vertex(point + p2 * radius);
			}

			GL.End();
			GL.PopMatrix();
		}

		void OnEnable () {
			col = GetComponent<MeshCollider>();
			var mesh = GetComponent<MeshFilter>().sharedMesh;
			vertices = mesh.vertices;
			triangles = mesh.triangles;
		}

		void CheckInit () {
			if(lineMaterial == null) {
				Shader shader = Shader.Find(SHADER_PATH);
				if (shader == null) return;
				lineMaterial = new Material(shader);
				lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
		}

	}
	
}

