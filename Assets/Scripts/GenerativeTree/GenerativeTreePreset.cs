using UnityEngine;

using System.Collections;

namespace Mattatz {

	namespace GenerativeTree {

		namespace Utils {

			/*
			 * Branch preset
			 */
			public class GenerativeTreePreset : MonoBehaviour {

				#region Public Variables

				public string branchName;

				public float
					height,
					radius;

				public int
					generation,
					childCount;

				public int
					segmentHeight,
					segmentWidth;

				[Range (0.0f, 1.0f)]
				public float
					segmentNoise,
					radiusReductionRate,
					heightReductionRateMin,
					heightReductionRateMax,
					spread,
					bendingNoise;

				public float
					bendingScale;

				#endregion

				public void Init (
					string branchName,
					float height,
					float radius,
					int generation,
					int childCount,
					int segmentHeight,
					int segmentWidth,
					float segmentNoise,
					float radiusReductionRate,
					float heightReductionRateMin,
					float heightReductionRateMax,
					float spread,
					float bendingNoise,
					float bendingScale
					) {
					this.branchName = branchName;
					this.height = height;
					this.radius = radius;
					this.generation = generation;
					this.childCount = childCount;
					this.segmentHeight = segmentHeight;
					this.segmentWidth = segmentWidth;
					this.segmentNoise = segmentNoise;
					this.radiusReductionRate = radiusReductionRate;
					this.heightReductionRateMax = heightReductionRateMax;
					this.heightReductionRateMin = heightReductionRateMin;
					this.spread = spread;
					this.bendingNoise = bendingNoise;
					this.bendingScale = bendingScale;
				}

				void OnValidate () {
					SendMessage("OnPresetChanged", SendMessageOptions.DontRequireReceiver);
				}

			}

		}

	}

}
