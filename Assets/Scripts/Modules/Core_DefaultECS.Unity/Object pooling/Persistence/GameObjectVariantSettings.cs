using System;

using HereticalSolutions.Allocations;

using UnityEngine;

namespace HereticalSolutions.Modules.Core_DefaultECS
{
	[Serializable]
	public class GameObjectVariantSettings
	{
		public float Chance;

		public GameObject Prefab;

		public AllocationCommandDescriptor Initial;

		public AllocationCommandDescriptor Additional;
	}
}