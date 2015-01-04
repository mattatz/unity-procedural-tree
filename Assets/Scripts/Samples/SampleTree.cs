using UnityEngine;

using System.Collections;
using Mattatz.GenerativeTree;
using Mattatz.GenerativeTree.Utils;

[ExecuteInEditMode]
[RequireComponent (typeof (MeshRenderer))]
[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (GenerativeTreePreset))]
public class SampleTree : MonoBehaviour {

	private Branch _branch;
	public bool useRandomSeed = false;
	public int randomSeed = 1;

	void Start () {
		Init();
	}
	
	void Update () {
	}

	void Init () {
		if(useRandomSeed) {
			UnityEngine.Random.seed = randomSeed;
		}

		GenerativeTreePreset preset = GetComponent<GenerativeTreePreset>();
		_branch = Branch.LoadPreset(Vector3.zero, Vector3.up, preset);
		_branch.Build();
		GetComponent<MeshFilter>().sharedMesh = _branch.mesh;
	}

	void OnValidate () {
		Init();
	}

	void OnPresetChanged () {
		Init();
	}

	void OnDrawGizmos () {
		_branch.DrawGizmos();
	}


}
