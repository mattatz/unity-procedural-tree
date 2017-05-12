using UnityEngine;
using System.Collections;

namespace mattatz {

	namespace GenerativeTree {

		namespace Utils {

			public class BezierCurve {
				/*
				 * http://en.wikibooks.org/wiki/Cg_Programming/Unity/B%C3%A9zier_Curves
				 * bezier path algorithm
				 * p0 : from
				 * p1 : control
				 * p2 : to
				 * t is a lerp value (0.0 ~ 1.0)
				 */
				public static Vector3 GetPoint (Vector3 p0, Vector3 p1, Vector3 p2, float t) {
					return (1.0f - t) * (1.0f - t) * p0 + 2.0f * (1.0f - t) * t * p1 + t * t * p2;
				}
			}

			public class CubicHermiteCurve {
				/*
				 * http://en.wikibooks.org/wiki/Cg_Programming/Unity/Hermite_Curves
				 * hermite curve algorithm
				 */
				public static Vector3 GetPoint (Vector3 p0, Vector3 p1, Vector3 m0, Vector3 m1, float t) {
					return (2.0f * t * t * t - 3.0f * t * t + 1.0f) * p0 
						+ (t * t * t - 2.0f * t * t + t) * m0
						+ (- 2.0f * t * t * t + 3.0f * t * t) * p1
						+ (t * t * t - t * t) * m1;
				}
			}

		}

	}

}

