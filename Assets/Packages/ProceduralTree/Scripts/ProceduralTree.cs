using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace ProceduralModeling {

	public class ProceduralTree : ProceduralModelingBase {

		public TreeData Data { get { return data; } } 

		[SerializeField] TreeData data;
		[SerializeField, Range(2, 8)] protected int generations = 5;
		[SerializeField, Range(0.5f, 5f)] protected float length = 1f;
		[SerializeField, Range(0.1f, 2f)] protected float radius = 0.15f;

		const float PI2 = Mathf.PI * 2f;

		public static Mesh Build(TreeData data, int generations, float length, float radius) {
			data.Setup();

			var root = new TreeBranch(
				generations, 
				length, 
				radius, 
				data
			);

			var vertices = new List<Vector3>();
			var normals = new List<Vector3>();
			var tangents = new List<Vector4>();
			var uvs = new List<Vector2>();
			var triangles = new List<int>();

			float maxLength = TraverseMaxLength(root);

			Traverse(root, (branch) => {
				var offset = vertices.Count;

				var vOffset = branch.Offset / maxLength;
				var vLength = branch.Length / maxLength;

				for(int i = 0, n = branch.Segments.Count; i < n; i++) {
					var t = 1f * i / (n - 1);
					var v = vOffset + vLength * t;

					var segment = branch.Segments[i];
					var N = segment.Frame.Normal;
					var B = segment.Frame.Binormal;
					for(int j = 0; j <= data.radialSegments; j++) {
						// 0.0 ~ 2π
						var u = 1f * j / data.radialSegments;
						float rad = u * PI2;

						float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
						var normal = (cos * N + sin * B).normalized;
						vertices.Add(segment.Position + segment.Radius * normal);
						normals.Add(normal);

						var tangent = segment.Frame.Tangent;
						tangents.Add(new Vector4(tangent.x, tangent.y, tangent.z, 0f));

						uvs.Add(new Vector2(u, v));
					}
				}

				for (int j = 1; j <= data.heightSegments; j++) {
					for (int i = 1; i <= data.radialSegments; i++) {
						int a = (data.radialSegments + 1) * (j - 1) + (i - 1);
						int b = (data.radialSegments + 1) * j + (i - 1);
						int c = (data.radialSegments + 1) * j + i;
						int d = (data.radialSegments + 1) * (j - 1) + i;

						a += offset;
						b += offset;
						c += offset;
						d += offset;

						triangles.Add(a); triangles.Add(d); triangles.Add(b);
						triangles.Add(b); triangles.Add(d); triangles.Add(c);
					}
				}
			});

			var mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.normals = normals.ToArray();
			mesh.tangents = tangents.ToArray();
			mesh.uv = uvs.ToArray();
			mesh.triangles = triangles.ToArray();
			return mesh;
		}

		protected override Mesh Build ()
		{
			return Build(data, generations, length, radius);
		}

		static float TraverseMaxLength(TreeBranch branch) {
			float max = 0f;
			branch.Children.ForEach(c => {
				max = Mathf.Max(max, TraverseMaxLength(c));
			});
			return branch.Length + max;
		}

		static void Traverse(TreeBranch from, Action<TreeBranch> action) {
			if(from.Children.Count > 0) {
				from.Children.ForEach(child => {
					Traverse(child, action);
				});
			}
			action(from);
		}

	}

	[System.Serializable]
	public class TreeData {
		public int randomSeed = 0;
		[Range(0.25f, 0.95f)] public float lengthAttenuation = 0.8f, radiusAttenuation = 0.5f;
		[Range(1, 3)] public int branchesMin = 1, branchesMax = 3;
        [Range(-45f, 0f)] public float growthAngleMin = -15f;
        [Range(0f, 45f)] public float growthAngleMax = 15f;
        [Range(1f, 10f)] public float growthAngleScale = 4f;
		[Range(4, 20)] public int heightSegments = 10, radialSegments = 8;
		[Range(0.0f, 0.35f)] public float bendDegree = 0.1f;

		Rand rnd;

		public void Setup() {
			rnd = new Rand(randomSeed);
		}

		public int Range(int a, int b) {
			return rnd.Range(a, b);
		}

		public float Range(float a, float b) {
			return rnd.Range(a, b);
		}

		public int GetRandomBranches() {
			return rnd.Range(branchesMin, branchesMax + 1);
		}

		public float GetRandomGrowthAngle() {
			return rnd.Range(growthAngleMin, growthAngleMax);
		}

		public float GetRandomBendDegree() {
			return rnd.Range(-bendDegree, bendDegree);
		}
	}

	public class TreeBranch {
		public int Generation { get { return generation; } }
		public List<TreeSegment> Segments { get { return segments; } }
		public List<TreeBranch> Children { get { return children; } }

		public Vector3 From { get { return from; } }
		public Vector3 To { get { return to; } }
		public float Length { get { return length; } } 
		public float Offset { get { return offset; } }

		int generation;

		List<TreeSegment> segments;
		List<TreeBranch> children;

		Vector3 from, to;
		float fromRadius, toRadius;
		float length;
		float offset;

		// for Root branch constructor
		public TreeBranch(int generation, float length, float radius, TreeData data) : this(new List<TreeBranch>(), generation, generation, Vector3.zero, Vector3.up, Vector3.right, Vector3.back, length, radius, 0f, data) {
		}

		protected TreeBranch(List<TreeBranch> branches, int generation, int generations, Vector3 from, Vector3 tangent, Vector3 normal, Vector3 binormal, float length, float radius, float offset, TreeData data) {
			this.generation = generation;

			this.fromRadius = radius;
			this.toRadius = (generation == 0) ? 0f : radius * data.radiusAttenuation;

			this.from = from;

            var scale = Mathf.Lerp(1f, data.growthAngleScale, 1f - 1f * generation / generations);
            var rotation = Quaternion.AngleAxis(scale * data.GetRandomGrowthAngle(), normal) * Quaternion.AngleAxis(scale * data.GetRandomGrowthAngle(), binormal);
            this.to = from + rotation * tangent * length;

			this.length = length;
			this.offset = offset;

			segments = BuildSegments(data, fromRadius, toRadius, normal, binormal);

            branches.Add(this);

			children = new List<TreeBranch>();
			if(generation > 0) {
				int count = data.GetRandomBranches();
				for(int i = 0; i < count; i++) {
                    float ratio;
                    if(count == 1)
                    {
                        // for zero division
                        ratio = 1f;
                    } else
                    {
                        ratio = Mathf.Lerp(0.5f, 1f, (1f * i) / (count - 1));
                    }

                    var index = Mathf.FloorToInt(ratio * (segments.Count - 1));
					var segment = segments[index];

                    Vector3 nt, nn, nb;
                    if(ratio >= 1f)
                    {
                        // sequence branch
                        nt = segment.Frame.Tangent;
                        nn = segment.Frame.Normal;
                        nb = segment.Frame.Binormal;
                    } else
                    {
                        var rot = Quaternion.AngleAxis(i * 90f, tangent);
                        nt = rot * tangent;
                        nn = rot * normal;
                        nb = rot * binormal;
                    }

					var child = new TreeBranch(
                        branches,
						this.generation - 1, 
                        generations,
						segment.Position, 
						nt, 
						nn, 
						nb, 
						length * Mathf.Lerp(1f, data.lengthAttenuation, ratio), 
						radius * Mathf.Lerp(1f, data.radiusAttenuation, ratio),
						offset + length,
						data
					);

					children.Add(child);
				}
			}
		}

		List<TreeSegment> BuildSegments (TreeData data, float fromRadius, float toRadius, Vector3 normal, Vector3 binormal) {
			var segments = new List<TreeSegment>();

			var points = new List<Vector3>();

			var length = (to - from).magnitude;
			var bend = length * (normal * data.GetRandomBendDegree() + binormal * data.GetRandomBendDegree());
			points.Add(from);
			points.Add(Vector3.Lerp(from, to, 0.25f) + bend);
			points.Add(Vector3.Lerp(from, to, 0.75f) + bend);
			points.Add(to);

			var curve = new CatmullRomCurve(points);

			var frames = curve.ComputeFrenetFrames(data.heightSegments, normal, binormal, false);
			for(int i = 0, n = frames.Count; i < n; i++) {
				var u = 1f * i / (n - 1);
                var radius = Mathf.Lerp(fromRadius, toRadius, u);

				var position = curve.GetPointAt(u);
				var segment = new TreeSegment(frames[i], position, radius);
				segments.Add(segment);
			}
			return segments;
		}

	}

	public class TreeSegment {
		public FrenetFrame Frame { get { return frame; } }
		public Vector3 Position { get { return position; } }
        public float Radius { get { return radius; } }

		FrenetFrame frame;
		Vector3 position;
        float radius;

		public TreeSegment(FrenetFrame frame, Vector3 position, float radius) {
			this.frame = frame;
			this.position = position;
            this.radius = radius;
		}
	}

	public class Rand {
		System.Random rnd;

		public float value {
			get {
				return (float)rnd.NextDouble();
			}
		}

		public Rand(int seed) {
			rnd = new System.Random(seed);
		}

		public int Range(int a, int b) {
			var v = value;
			return Mathf.FloorToInt(Mathf.Lerp(a, b, v));
		}

		public float Range(float a, float b) {
			var v = value;
			return Mathf.Lerp(a, b, v);
		}
	}

}

