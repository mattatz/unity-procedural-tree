using UnityEngine;
using System.Collections;

using Mattatz.GenerativeTree;
using Mattatz.GenerativeTree.Utils;
using Mattatz.GenerativeTree.Animator;

[RequireComponent (typeof (MeshRenderer))]
[RequireComponent (typeof (MeshFilter))]
public class SampleAnimationTree : MonoBehaviour {

	private Branch _branch;
	private TreeAnimator _animator;
	private float _timer;

	void Start () {
		_branch = new Branch(Vector3.zero, Vector3.up, 8f, 1.5f, 7, 3);
		_branch.Build ();
		_animator = new TreeAnimator(_branch, 0.5f, true);

		GetComponent<MeshFilter>().sharedMesh = _animator.mesh;
	}
	
	void Update () {
		_animator.Animate();
	}

}
