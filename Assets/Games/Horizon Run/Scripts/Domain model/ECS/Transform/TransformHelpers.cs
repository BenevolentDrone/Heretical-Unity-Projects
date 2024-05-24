using HereticalSolutions.Entities;

using DefaultEcs;

using UnityEngine;

namespace HereticalSolutions.HorizonRun
{
	public static class TransformHelpers
	{
		#region World position

		#region World position 2D

		public static Vector2 GetWorldPosition2D(
			Entity entity)
		{
			Vector2 position = Vector2.zero;

			if (entity.Has<Position2DComponent>())
			{
				position = entity.Get<Position2DComponent>().Position;
			}

			GetParentWorldPositionRotation(
				entity,
				out Vector2 parentPosition,
				out float parentRotation);

			var worldPosition = MathHelpersUnity.GetWorldPosition(
				parentPosition,
				position,
				parentRotation);

			return worldPosition;
		}

		public static void GetParentWorldPositionRotation(
			Entity entity,
			out Vector2 worldPosition,
			out float worldRotation)
		{
			worldPosition = Vector2.zero;

			worldRotation = 0f;

			if (entity.Has<HierarchyComponent>())
			{
				var hierarchyComponent = entity.Get<HierarchyComponent>();

				if (hierarchyComponent.Parent.IsAlive
					&& hierarchyComponent.Parent.Has<Transform2DComponent>())
				{
					var parentTransformComponent = hierarchyComponent.Parent.Get<Transform2DComponent>();

					worldPosition = parentTransformComponent.WorldPosition;

					worldRotation = parentTransformComponent.WorldRotation;
				}
			}
		}

		#endregion

		#region World position 3D

		public static Vector3 GetWorldPosition3D(
			Matrix4x4 TRSMatrix)
		{
			//TODO: check
			Vector3 worldPosition = TRSMatrix.GetColumn(3);

			return worldPosition;
		}

		public static Vector3 GetWorldPosition3D(
			LocalTransform3D localTransform,
			Matrix4x4 parentTRSMatrix)
		{
			var localTRSMatrix = Matrix4x4.TRS(
				localTransform.LocalPosition,
				localTransform.LocalRotation,
				localTransform.LocalScale);

			var worldTransformMatrix = parentTRSMatrix * localTRSMatrix;

			//TODO: check
			Vector3 worldPosition = worldTransformMatrix.GetColumn(3);

			return worldPosition;
		}

		public static Vector3 GetWorldPosition3D(
			Vector3 localPosition,
			Quaternion localRotation,
			Vector3 localScale,
			Matrix4x4 parentTRSMatrix)
		{
			var localTRSMatrix = Matrix4x4.TRS(
				localPosition,
				localRotation,
				localScale);

			var worldTransformMatrix = parentTRSMatrix * localTRSMatrix;

			//TODO: check
			Vector3 worldPosition = worldTransformMatrix.GetColumn(3);

			return worldPosition;
		}

		public static Vector3 GetWorldPosition3D(
			Entity entity)
		{
			Vector3 localPosition = Vector3.zero;

			if (entity.Has<Position3DComponent>())
			{
				localPosition = entity.Get<Position3DComponent>().Position;
			}

			Quaternion localRotation = Quaternion.identity;

			if (entity.Has<QuaternionComponent>())
			{
				localRotation = entity.Get<QuaternionComponent>().Quaternion;
			}

			Vector3 localScale = Vector3.one;

			var localTRSMatrix = Matrix4x4.TRS(
				localPosition,
				localRotation,
				localScale);

			var parentTRSMatrix = GetParentTRSMatrix(entity);

			var worldTransformMatrix = parentTRSMatrix * localTRSMatrix;

			//TODO: check
			Vector3 worldPosition = worldTransformMatrix.GetColumn(3);

			return worldPosition;
		}

		public static Matrix4x4 GetParentTRSMatrix(
			Entity entity)
		{
			Matrix4x4 parentTRSMatrix = Matrix4x4.identity;

			if (entity.Has<HierarchyComponent>())
			{
				var hierarchyComponent = entity.Get<HierarchyComponent>();

				if (hierarchyComponent.Parent.IsAlive
					&& hierarchyComponent.Parent.Has<Transform3DComponent>())
				{
					var parentTransformComponent = hierarchyComponent.Parent.Get<Transform3DComponent>();

					parentTRSMatrix = parentTransformComponent.TRSMatrix;
				}
			}

			return parentTRSMatrix;
		}

		#endregion

		#endregion

		#region Transform

		#region Transform 2D

		public static void UpdateTransform2DRecursively(
			Entity entity,
			DefaultECSEntityListManager entityListManager)
		{
			Vector2 parentPosition = Vector2.zero;

			float parentRotation = 0f;

			if (entity.Has<HierarchyComponent>())
			{
				var hierarchyComponent = entity.Get<HierarchyComponent>();

				if (hierarchyComponent.Parent.IsAlive
					&& hierarchyComponent.Parent.Has<Transform2DComponent>())
				{
					var parentTransformComponent = hierarchyComponent.Parent.Get<Transform2DComponent>();

					if (parentTransformComponent.Dirty)
					{
						//Early return trick
						//The idea is that the transforms are updated recursively
						//So this particular transform will be updated later anyway when parent transform gets updated
						//So there's no need to do that twice
						return;
					}

					parentPosition = parentTransformComponent.WorldPosition;

					parentRotation = parentTransformComponent.WorldRotation;
				}
			}

			UpdateTransform2DRecursively(
				entity,
				entityListManager,
				parentPosition,
				parentRotation);
		}

		public static void UpdateTransform2DRecursively(
			Entity entity,
			DefaultECSEntityListManager entityListManager,
			Vector2 parentPosition,
			float parentRotation)
		{
			ref var transformComponent = ref entity.Get<Transform2DComponent>();

			Vector2 position = Vector2.zero;

			if (entity.Has<Position2DComponent>())
			{
				position = entity.Get<Position2DComponent>().Position;
			}

			float rotation = 0f;

			if (entity.Has<UniformRotationComponent>())
			{
				rotation = entity.Get<UniformRotationComponent>().Angle;
			}

			UpdateTransform2D(
				ref transformComponent,
				position,
				rotation,
				parentPosition,
				parentRotation);


			if (!entity.Has<HierarchyComponent>())
			{
				return;
			}

			var hierarchyComponent = entity.Get<HierarchyComponent>();

			if (!entityListManager.TryGetList(
				hierarchyComponent.ChildrenListHandle,
				out var childrenList))
			{
				return;
			}

			foreach (var child in childrenList)
			{
				if (!child.IsAlive)
					continue;

				if (!child.Has<Transform2DComponent>())
					continue;

				UpdateTransform2DRecursively(
					child,
					entityListManager,
					transformComponent.WorldPosition,
					transformComponent.WorldRotation);
			}
		}

		public static void UpdateTransform2D(
			ref Transform2DComponent transformComponent,

			Vector2 position,
			float rotation,

			Vector2 parentPosition,
			float parentRotation)
		{
			transformComponent.WorldPosition = MathHelpersUnity.GetWorldPosition(
				parentPosition,
				position,
				parentRotation);

			transformComponent.WorldRotation = MathHelpers.SanitizeAngle(parentRotation + rotation);

			transformComponent.Dirty = false;
		}

		#endregion

		#region Transform 3D

		public static void UpdateTransform3DRecursively(
			Entity entity,
			DefaultECSEntityListManager entityListManager)
		{
			Matrix4x4 parentTRSMatrix = Matrix4x4.identity;

			if (entity.Has<HierarchyComponent>())
			{
				var hierarchyComponent = entity.Get<HierarchyComponent>();

				if (hierarchyComponent.Parent.IsAlive
					&& hierarchyComponent.Parent.Has<Transform3DComponent>())
				{
					var parentTransformComponent = hierarchyComponent.Parent.Get<Transform3DComponent>();

					if (parentTransformComponent.Dirty)
					{
						//Early return trick
						//The idea is that the transforms are updated recursively
						//So this particular transform will be updated later anyway when parent transform gets updated
						//So there's no need to do that twice
						return;
					}

					parentTRSMatrix = parentTransformComponent.TRSMatrix;
				}
			}

			UpdateTransform3DRecursively(
				entity,
				entityListManager,
				parentTRSMatrix);
		}

		public static void UpdateTransform3DRecursively(
			Entity entity,
			DefaultECSEntityListManager entityListManager,
			Matrix4x4 parentTRSMatrix)
		{
			ref var transformComponent = ref entity.Get<Transform3DComponent>();

			Vector3 position = Vector3.zero;

			if (entity.Has<Position3DComponent>())
			{
				position = entity.Get<Position3DComponent>().Position;
			}

			Quaternion rotation = Quaternion.identity;

			if (entity.Has<QuaternionComponent>())
			{
				rotation = entity.Get<QuaternionComponent>().Quaternion;
			}

			Vector3 scale = Vector3.one;

			UpdateTransform3D(
				ref transformComponent,
				position,
				rotation,
				scale,
				parentTRSMatrix);


			if (!entity.Has<HierarchyComponent>())
			{
				return;
			}

			var hierarchyComponent = entity.Get<HierarchyComponent>();

			if (!entityListManager.TryGetList(
				hierarchyComponent.ChildrenListHandle,
				out var childrenList))
			{
				return;
			}

			foreach (var child in childrenList)
			{
				if (!child.IsAlive)
					continue;

				if (!child.Has<Transform3DComponent>())
					continue;

				UpdateTransform3DRecursively(
					child,
					entityListManager,
					transformComponent.TRSMatrix);
			}
		}

		public static void UpdateTransform3D(
			ref Transform3DComponent transformComponent,

			Vector3 position,
			Quaternion rotation,
			Vector3 scale,

			Matrix4x4 parentTRSMatrix)
		{
			transformComponent.TRSMatrix = parentTRSMatrix * Matrix4x4.TRS(
				position,
				rotation,
				scale);

			transformComponent.Dirty = false;
		}

		#endregion

		#endregion
	}
}