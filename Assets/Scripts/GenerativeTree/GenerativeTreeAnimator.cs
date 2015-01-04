using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Mattatz {

	namespace GenerativeTree {

		namespace Animator {

			public class TreeAnimator {

				#region Accessor

				public Mesh mesh {
					get {
						return _mesh;
					}
				}

				#endregion

				#region Public Variables

				#endregion

				#region Private Variables

				private float _speed;
				private bool _smooth;

				private Mesh _mesh;

				private Vector3[] _vertices;

				private List<BranchAnimator> _animators;

				#endregion

				public TreeAnimator (Branch branch, float speed = 0.5f, bool smooth = false) {
					_speed = speed;
					_smooth = smooth;

					_mesh = new Mesh();
					_mesh.vertices = branch.mesh.vertices;
					_mesh.triangles = branch.mesh.triangles;
					_mesh.uv = branch.mesh.uv;
					_mesh.colors = branch.mesh.colors;
					_mesh.normals = branch.mesh.normals;
					_mesh.bounds = branch.mesh.bounds;
					_mesh.MarkDynamic();

					_mesh.vertices = _mesh.vertices.Select(v => Vector3.zero).ToArray();

					_vertices = _mesh.vertices;

					// add first branch
					_animators = new List<BranchAnimator>();
					_animators.Add (new BranchAnimator(branch, _vertices, _speed, _smooth));
				}

				public void Animate () {

					for(int i = 0, n = _animators.Count; i < n; i++) {
						if(_animators[i] != null) {
							_animators[i].Animate();

							if(_animators[i].HasChildren()) {
								if(_animators[i].IsEmitableChildren()) {
									Branch branch = _animators[i].EmitChild();
									BranchAnimator anim = new BranchAnimator(branch, _vertices, _speed, _smooth);
									anim.Animate();
									_animators.Add(anim);
								} 
							} else if(_animators[i].IsFinish()) {
								_animators.RemoveAt(i);
								n--;
							}
						
						}
					}

					// update mesh vertices
					_mesh.vertices = _vertices;
				}

				public bool IsFinish () {
					return _animators.Count == 0;
				}

				#region Private Functions

				private bool IsAnimation (Branch branch) {
					return _animators.Any(animator => animator.branch == branch);
				}

				#endregion

			}

			class BranchAnimator {

				#region Accessor

				public Branch branch {
					get {
						return _branch;
					}
				}

				public float frame {
					get {
						return _frame;
					}
				}

				#endregion

				#region Private Variables

				private Branch _branch;

				// common vertices
				private Vector3[] _vertices;

				private float _speed = 1f;
				private float _frame;

				private int _segmentCount;
				private float _segmentFrameLength;
				private int _previousSegmentIndex = -1;

				/*
				 * smooth animation
				 */
				private bool _smooth;

				/*
				 * _branch.branches indices for emitting a animator of a child branch
				 * int : _branch.branches index
				 * int : _branch.segments index
				 */
				private Dictionary<int, int> _childrenIndices;

				#endregion

				public BranchAnimator (Branch branch, Vector3[] vertices, float speed, bool smooth = false) {
					_branch = branch;
					_vertices = vertices;
					_speed = speed;
					_smooth = smooth;

					_segmentCount = _branch.segmentWidth * _branch.segmentHeight;
					_segmentFrameLength = 1.0f / _branch.segmentHeight;

					_childrenIndices = new Dictionary<int, int>();
					if(_branch.branches != null && _branch.branches.Length > 0) {
						int[] indices = _branch.branches.Select(b => Array.FindIndex(_branch.segments, s => s == b.from)).ToArray();
						for(int i = 0, n = indices.Length; i < n; i++) {
							_childrenIndices.Add(i, indices[i]);
						}
					}

					InitializeAnimation();
				}

				public void Animate () {

					int segmentIndex = (int)(_branch.segmentHeight * _frame);
					float t = _frame / _segmentFrameLength - Mathf.FloorToInt(_frame / _segmentFrameLength);

					if(_previousSegmentIndex > 0 && segmentIndex - _previousSegmentIndex > 1) {
						for(int i = _previousSegmentIndex; i <= segmentIndex && i < _branch.segmentHeight; i++) {
							Grow(i);
						}
					}

					if(segmentIndex < _branch.segmentHeight - 1) {
						if(!_smooth) {
							if(segmentIndex != _previousSegmentIndex) {
								Grow(_previousSegmentIndex + 1);
							}
						} else {
							SmoothGrow(segmentIndex, t);

						}

						// set past vertex positions to target ones.
						if(segmentIndex != _previousSegmentIndex) {
							Vector3 top = _branch.segments[segmentIndex].position;
							for(int i = segmentIndex + 2; i < _branch.segmentHeight; i++) {
								for(int j = 0; j < _branch.segmentWidth; j++) {
									_vertices[i * _branch.segmentWidth + j + _branch.vertexOffset] = top;
								}
							}
							_vertices[_segmentCount + 1 + _branch.vertexOffset] = top;
						}
					} else { 
						int topSegmentIndex = _branch.segmentHeight - 1;
						for(int j = 0; j < _branch.segmentWidth; j++) {
							int idx = topSegmentIndex * _branch.segmentWidth + j;
							_vertices[idx + _branch.vertexOffset] = _branch.mesh.vertices[idx];
						}

						// Vector3 from = _branch.segments[segmentIndex].position;
						// Vector3 to = _branch.mesh.vertices[_segmentCount + 1];
						// _vertices[_segmentCount + 1 + _branch.vertexOffset] = Vector3.Lerp(from, to, t);

						_vertices[_segmentCount + 1 + _branch.vertexOffset] = _branch.mesh.vertices[_segmentCount + 1];
					}

					_previousSegmentIndex = segmentIndex;

					_frame += _segmentFrameLength * _speed;
				}

				public bool HasChildren () {
					return _childrenIndices.Count > 0;
				}

				public bool IsEmitableChildren () {
					int[] keys = _childrenIndices.Keys.ToArray ();
					for(int i = 0, n = keys.Length; i < n; i++) {
						int sidx = _childrenIndices[keys[i]];
						if(sidx <= _previousSegmentIndex + 1) {
							return true;
						}
					}
					return false;
				}

				public Branch EmitChild () {
					// int segmentIndex = (int)(_branch.segmentHeight * _frame);
					int[] keys = _childrenIndices.Keys.ToArray ();

					for(int i = 0, n = keys.Length; i < n; i++) {
						int key = keys[i];
						int sidx = _childrenIndices[key];
						if(sidx <= _previousSegmentIndex + 1) {
							_childrenIndices.Remove(key);
							return _branch.branches[key];
						}
					}

					return _branch.branches[_childrenIndices[keys[0]]];
				}

				public bool IsFinish () {
					// return _frame > 1.0f;
					return _previousSegmentIndex >= _branch.segmentHeight;
				}

				#region Private Functions

				private void InitializeAnimation () {
					// at begin,
					// set all vertices position to first segment.

					Vector3 v = _branch.mesh.vertices[_segmentCount];
					for(int j = 0; j < _branch.segmentWidth; j++) {
						// Vector3 v = _branch.mesh.vertices[j];

						int offset = j + _branch.vertexOffset;
						_vertices[offset] = _branch.mesh.vertices[j];

						for(int i = 1; i < _branch.segmentHeight; i++) {
							_vertices[i * _branch.segmentWidth + offset] = v;
						}
					}

					// set branch bottom and top vertex position
					_vertices[_segmentCount + _branch.vertexOffset] = _branch.mesh.vertices[_segmentCount];
					_vertices[_segmentCount + 1 + _branch.vertexOffset] = _branch.mesh.vertices[_segmentCount];
				}

				/*
				 * set vertex positions to target points
				 */
				private void Grow (int segmentIndex) {
					int offset = segmentIndex * _branch.segmentWidth;
					for(int j = 0; j < _branch.segmentWidth; j++) {
						int idx = offset + j;
						_vertices[idx + _branch.vertexOffset] = _branch.mesh.vertices[idx];
					}
				}

				/*
				 * t: 0.0 ~ 1.0 value.
				 */
				private void SmoothGrow (int segmentIndex, float t) {
					for(int j = 0; j < _branch.segmentWidth; j++) {
						int idx = segmentIndex * _branch.segmentWidth + j;
						int nidx = (segmentIndex + 1) * _branch.segmentWidth + j;
						_vertices[nidx + _branch.vertexOffset] = Vector3.Lerp(_branch.mesh.vertices[idx], _branch.mesh.vertices[nidx], t);
					}
				}
				
				#endregion

			}

		}

	}

}
