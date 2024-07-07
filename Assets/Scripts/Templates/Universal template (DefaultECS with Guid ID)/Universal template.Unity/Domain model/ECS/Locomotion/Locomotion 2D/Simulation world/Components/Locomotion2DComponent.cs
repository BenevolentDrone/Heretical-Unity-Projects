using HereticalSolutions.Entities;

using UnityEngine;

namespace HereticalSolutions.Templates.Universal.Unity
{
	[Component("Simulation world/Locomotion")]
	public struct Locomotion2DComponent
	{
		public float LocomotionSpeedNormal;

		public float MaxLocomotionSpeed;

		public Vector2 LocomotionVectorNormalized;
	}
}