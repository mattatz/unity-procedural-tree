using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using mattatz.GenerativeTree.Animator;
using mattatz.GenerativeTree.Utils;

namespace mattatz {

	namespace GenerativeTree {

		public class Branch {

			#region Accessor

			public Segment from {
				get {
					return _from;
				}
			}

			public Segment[] segments {
				get {
					return _segments;
				}
			}

			public Branch[] branches {
				get {
					return _branches;
				}
			}

			public int generation {
				get {
					return _generation;
				}
			}

			public int vertexOffset {
				get {
					return _vertexOffset;
				}
			}

			public Mesh mesh {
				get {
					return _mesh;
				}
			}

			#endregion

			#region Public Variables

			public int 
				segmentHeight = 14,
				segmentWidth = 10;

			public float 
				segmentNoise = 0.2f,
				radiusReductionRate = 0.6f,
				heightReductionRateMin = 0.8f, 
				heightReductionRateMax = 0.9f,
				childSegmentFromMin = 0.2f,
				childSegmentFromMax = 0.9f,
				bendingNoise = 0.3f,
				bendingScale = 1.1f;

			#endregion

			#region Private Variables

			private Mesh _mesh;
			private int _vertexOffset;

			private Vector3 _start;
			private Vector3 _end;
			private Vector3 _direction;
			private int _generation;
			private int _childCount;
			private float _height;
			private float _radius;

			private int _generationLength;

			private Segment _from;
			private Segment[] _segments;

			/*
			 * child objects for a Branch instance
			 */
			private Branch[] _branches;

			/*
			 * tangent vectors for a branch' curve 
			 */
			private Vector3 _tangent0;
			private Vector3 _tangent1;

			#endregion

			#region Public Functions

			public static Branch LoadPreset (Vector3 direction, Preset preset) {
				Branch branch = new Branch(Vector3.zero, direction, preset.height, preset.radius, preset.generation, preset.childCount);
				branch.segmentHeight = preset.segmentHeight ;
				branch.segmentWidth = preset.segmentWidth;
				branch.segmentNoise = preset.segmentNoise;
				branch.radiusReductionRate = preset.radiusReductionRate;
				branch.heightReductionRateMin = preset.heightReductionRateMin; 
				branch.heightReductionRateMax = preset.heightReductionRateMax;
				branch.bendingNoise = preset.bendingNoise;
				branch.bendingScale = preset.bendingScale;
				return branch;
			}

			public Branch (Vector3 start, Vector3 direction, float height, float radius, int generation, int childCount) {
				_start = start;
				_direction = direction;
				_height = height;
				_radius = radius;
				_generation = Mathf.Clamp(generation, 0, 15);
				_childCount = childCount;
			}

			public Mesh Build (Segment from = null, int fromSegmentIndex = -1) {
				_from = from;

				if(_from == null) { // root 
					_generationLength = _generation;
				}

				BuildSegments();

				if(_generation > 1) {
					BranchChildren();
				}

				// if this branch is root,
				// create a mesh.
				if(_from == null) {
					ComputeMesh(0);
					_mesh.RecalculateBounds();
					_mesh.RecalculateNormals();
				}

                return _mesh;
			}

			public Branch[] GetChildren (int generation) {
				if(generation == 0) return new Branch[1] { this };
				else {
					return 
						_branches.Select(
							b => b.GetChildren(generation - 1)
						).Aggregate(
							new Branch[0], 
							(acc, branches) => acc.Concat(branches).ToArray()
						).ToArray();
				}
			}

			// debug function
			public void DrawGizmos () {
				Gizmos.color = Color.yellow;

				if(_branches != null) {
					for(int i = 0, n = _branches.Length; i < n; i++) {
						_branches[i].DrawGizmos();
					}
				}

				/*
				Vector3[] vertices = _mesh.vertices;
				for(int i = 0, n = vertices.Length; i < n; i++) {
					Gizmos.DrawWireSphere(vertices[i], 0.04f);
				}
				*/

				// Gizmos.color = Color.red;
				// Gizmos.DrawWireSphere(_segments[0].position, 0.25f);


			}

			public void DrawGizmosSelected () {
				Gizmos.color = Color.yellow;

				if(_branches != null) {
					for(int i = 0, n = _branches.Length; i < n; i++) {
						_branches[i].DrawGizmosSelected();
					}
				}

				for(int i = 0, n = _segments.Length; i < n; i++) {
					Segment segment = _segments[i];
					Vector3 direction = segment.rotation * Vector3.up;
					DrawArrow.ForGizmo(segment.position, direction.normalized);
				}

			}

			#endregion

			#region Private Functions

			private void BuildSegments () {
				Quaternion rotation = Quaternion.LookRotation(_direction) * Quaternion.AngleAxis(90f, Vector3.right);

				Vector3 noise = new Vector3(UnityEngine.Random.Range(- bendingNoise, bendingNoise), 0f, UnityEngine.Random.Range(- bendingNoise, bendingNoise));
				Vector3 bend = rotation * noise;

				_end = _start + (_direction.normalized + bend).normalized * _height;

				if(_tangent0 == Vector3.zero) {
					_tangent0 = _direction.normalized * _height;
				} 
				_tangent1 = (_end - _start + _direction.normalized * _height);

				_segments = new Segment[segmentHeight];

				for(int i = 0; i < segmentHeight; i++) {

					float t = (float)i / segmentHeight;
					Segment segment = new Segment(segmentWidth, _radius * Mathf.Lerp(1f, radiusReductionRate, t), segmentNoise);

					Vector3 current = CubicHermiteCurve.GetPoint(_start, _end, _tangent0, _tangent1, t);
					Vector3 next = CubicHermiteCurve.GetPoint(_start, _end, _tangent0, _tangent1, (float)(i + 1) / segmentHeight);
					segment.position = current; 

					if(i == 0 && _from != null) {
						segment.points = _from.points;
						segment.rotation = _from.rotation;
					} else if (i != 0 && _from != null) {
						Segment ps = _segments[i - 1];
						Quaternion to = Quaternion.FromToRotation(ps.rotation * Vector3.up, (next - current).normalized);
						segment.rotation = to * ps.rotation;
					} else {
						Quaternion rot = Quaternion.LookRotation((next - current).normalized);
						segment.rotation = Quaternion.FromToRotation(rot * Vector3.back, rot * Vector3.up) * rot;
					}

					_segments[i] = segment;
				}
			}

			private void BranchChildren () {
				int childCount = UnityEngine.Random.Range(2, _childCount);
				_branches = new Branch[childCount];

				for(int i = 0, n = childCount; i < n; i++) {
					int segmentIndex = (i == n - 1) ? segmentHeight - 1 : (int)(segmentHeight * UnityEngine.Random.Range(childSegmentFromMin, childSegmentFromMax));
					Segment segment = _segments[segmentIndex];

					float cheight = _height * UnityEngine.Random.Range(heightReductionRateMin, heightReductionRateMax);
					float cradius = (segmentIndex == segmentHeight - 1) ? segment.radius : segment.radius * UnityEngine.Random.Range(0.7f, 0.85f);

					Branch branch = new Branch(segment.position, segment.rotation * Vector3.up, cheight, cradius, _generation - 1, _childCount);
					branch._generationLength = _generationLength;
					if(segmentIndex == segmentHeight - 1) {
						// for smooth curve.
						branch._tangent0 = _tangent1;
					}

					CopyProperties(branch);
					branch.bendingNoise *= bendingScale;
					branch.Build(segment, segmentIndex);

					_branches[i] = branch;
				}
			}

			private void CopyProperties (Branch target) {
				target.segmentHeight = segmentHeight;
				target.segmentWidth = segmentWidth;
				target.segmentNoise = segmentNoise;

				target.childSegmentFromMin = childSegmentFromMin;
				target.childSegmentFromMax = childSegmentFromMax;

				target.radiusReductionRate = radiusReductionRate;
				target.heightReductionRateMin = heightReductionRateMin;
				target.heightReductionRateMax = heightReductionRateMax;

				target.bendingNoise = bendingNoise;
				target.bendingScale = bendingScale;
			}

			/*
			 * ComputeMesh() traverse children by depth-first.
			 * a root mesh vertices depend on this algorithm.
			 */
			private Mesh ComputeMesh (int offset) {
				Mesh mesh = new Mesh();

				_vertexOffset = offset;

				/*
				 * combine child meshes and a mesh of self
				 */
				if(_branches != null && _branches.Length > 0) {
					int branchLength = _branches.Length;
					CombineInstance[] combine = new CombineInstance[branchLength + 1];
					combine[0].mesh = CreateMesh();
					offset += combine[0].mesh.vertexCount;

					for(int i = 0, n = branchLength; i < n; i++) {
						Branch branch = _branches[i];
						combine[i + 1].mesh = branch.ComputeMesh(offset);
						offset += combine[i + 1].mesh.vertexCount;
					}

					mesh.CombineMeshes(combine, true, false);
				} else {
					mesh = CreateMesh();
				}

				if(from == null) {
					mesh.RecalculateBounds();
					mesh.RecalculateNormals();
				}

				return _mesh = mesh;
			}

			private Mesh CreateMesh () {
				Mesh mesh = new Mesh();

				Vector3[] vertices;
				int[] triangles;
				Vector2[] uvs;

				ComputeMeshProps(out vertices, out triangles, out uvs);

				mesh.vertices = vertices;
				mesh.triangles = triangles;
				mesh.uv = uvs;

				return mesh;
			}

			private void ComputeMeshProps (out Vector3[] vertices, out int[] triangles, out Vector2[] uvs) {

				// vertices count : segment points + terminal center point
				vertices = new Vector3[segmentWidth * segmentHeight + 2];
				uvs = new Vector2[vertices.Length];

				// number of triangles : terminal faces + between segments faces + terminal faces
				int trianglesCount = ((segmentWidth * 2) * (segmentHeight - 1)) * 3;

				// root branch
				if(_generation == _generationLength) {
					trianglesCount += segmentWidth * 3;
				}

				// terminal branch
				if(_generation == 1) {
					trianglesCount += segmentWidth * 3;
				}

				triangles = new int[trianglesCount];

				float uvYDelta = 1f / _generationLength;
				float uvYStart = uvYDelta * (_generationLength - _generation);

				for(int i = 0, n = _segments.Length; i < n; i++) {

					Segment segment = _segments[i];
					Vector3[] points = segment.points;
					for(int j = 0; j < segmentWidth; j++) {
						int idx = i * segmentWidth + j;
						vertices[idx] = segment.rotation * points[j % (segmentWidth - 1)] + segment.position;
						uvs[idx] = new Vector2((float)j / (segmentWidth - 1), (float)i / n * uvYDelta + uvYStart);
					}

					int p = i * segmentWidth;
					if(i == n - 1) {
						int lidx = (i + 1) * segmentWidth; 
						int uidx = (i + 1) * segmentWidth + 1; 
						vertices[lidx] = _segments[0].position;
						vertices[uidx] = _segments[i].position;

						// root 
						if(_generation == _generationLength) {
							for(int j = 0; j < segmentWidth; j++) {
								int idx = p * 6 + j * 3;

								// lower terminal faces
								triangles[idx] = lidx;
								triangles[idx + 1] = j;
								triangles[idx + 2] = (j + 1) % segmentWidth;
							}
						}

						if(_generation == 1) {
							int offset = (segmentHeight - 1) * segmentWidth;
							for(int j = 0; j < segmentWidth; j++) {
								int idx = p * 6 + j * 3;

								// upper terminal faces
								triangles[idx] = uidx;
								triangles[idx + 1] = (j + 1) % segmentWidth + offset;
								triangles[idx + 2] = offset + j;
							}
						}

					} else {
						for(int j = 0; j < segmentWidth; j++) {
							int idx = p + j;

							triangles[idx * 6] = idx;
							triangles[idx * 6 + 1] = idx + segmentWidth;
							triangles[idx * 6 + 2] = (j + 1) % segmentWidth + p;

							triangles[idx * 6 + 3] = (j + 1) % segmentWidth + p;
							triangles[idx * 6 + 4] = idx + segmentWidth;
							triangles[idx * 6 + 5] = (j + 1) % segmentWidth + p + segmentWidth;
						}
					}
				}
			}

			#endregion
		}

		public class Segment {

			#region Public Variables

			public Vector3[] points {
				get {
					return _points;
				}
				set {
					_points = value;
				}
			}

			public float radius {
				get {
					return _radius;
				}
			}

			public Vector3 position {
				get {
					return _position;
				}
				set {
					_position = value;
				}
			}

			public Quaternion rotation {
				get {
					return _rotation;
				}
				set {
					_rotation = value;
				}
			}

			#endregion

			#region Private Variables

			private Vector3[] _points;
			private float _radius;
			private Vector3 _position;
			private Quaternion _rotation;

			#endregion

			public Segment (int count, float radius, float noise = 0f) {

				_points = new Vector3[count - 1];
				_radius = radius;
				for(int i = 0; i < count - 1; i++) {
					float ratio = (float)i / (count - 1);
					float rad = Mathf.PI * 2 * ratio;
					float x = Mathf.Cos(rad) * radius + UnityEngine.Random.Range(-0.5f, 0.5f) * radius * noise;
					float z = Mathf.Sin(rad) * radius + UnityEngine.Random.Range(-0.5f, 0.5f) * radius * noise;
					_points[i] = new Vector3(x, 0f, z);
				}
			}

		}

	}

}
