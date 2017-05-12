using UnityEngine;
using Random = UnityEngine.Random;

using System.Collections;

using mattatz.GenerativeTree;
using mattatz.GenerativeTree.Utils;

[ExecuteInEditMode]
[RequireComponent (typeof (MeshRenderer))]
[RequireComponent (typeof (MeshFilter))]
public class SampleTree : MonoBehaviour {

	public bool useRandomSeed = false;
	public int randomSeed = 1;

	public Preset preset;
	private Branch _branch;

	void Start () {
		Init();
	}
	
	void Update () {
	}

	void Init () {
		if(useRandomSeed) {
            Random.InitState(randomSeed);
		}

		// GenerativeTreePreset preset = GetComponent<GenerativeTreePreset>();
		_branch = Branch.LoadPreset(Vector3.up, preset);
		_branch.Build();
		GetComponent<MeshFilter>().sharedMesh = _branch.mesh;

		if(useRandomSeed) {
            Random.InitState(System.DateTime.Now.Second);
		}
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
