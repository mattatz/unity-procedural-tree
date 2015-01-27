using UnityEngine;

using System.Collections;

namespace Mattatz {

	namespace GenerativeTree {

		namespace Utils {

			[System.Serializable]
			public class Preset {

				#region Public Variables

				public string name;

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
					bendingNoise;

				public float
					bendingScale;

				#endregion

			}

		}

	}

}
