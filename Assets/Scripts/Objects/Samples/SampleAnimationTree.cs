using UnityEngine;
using System.Collections;

using Mattatz.GenerativeTree;
using Mattatz.GenerativeTree.Utils;
using Mattatz.GenerativeTree.Animator;

[RequireComponent (typeof (MeshRenderer))]
[RequireComponent (typeof (MeshFilter))]
public class SampleAnimationTree : MonoBehaviour {

	public float speed = 0.5f;
	public Preset preset;

	private Branch _branch;
	private TreeAnimator _animator;
	private float _timer;

	void Start () {
		_branch = Branch.LoadPreset(Vector3.up, preset);
		_branch.Build ();
		_animator = new TreeAnimator(_branch, speed, true);

		GetComponent<MeshFilter>().sharedMesh = _animator.mesh;
	}
	
	void Update () {
		_animator.Animate();
	}

}
