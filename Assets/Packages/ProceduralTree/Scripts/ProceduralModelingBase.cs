using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralModeling {

	[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]
	[ExecuteInEditMode]
	public abstract class ProceduralModelingBase : MonoBehaviour {

		public MeshFilter Filter {
			get {
				if(filter == null) {
					filter = GetComponent<MeshFilter>();
				}
				return filter;
			}
		}

		MeshFilter filter;

		protected virtual void Start () {
			Rebuild();
		}

		public void Rebuild() {
			if(Filter.sharedMesh != null) {
				if(Application.isPlaying) {
					Destroy(Filter.sharedMesh);
				} else {
					DestroyImmediate(Filter.sharedMesh);
				}
			} 
			Filter.sharedMesh = Build();
		}

		protected abstract Mesh Build();

	}
		
}

