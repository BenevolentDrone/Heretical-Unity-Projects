using HereticalSolutions.Entities;

using DefaultEcs;
using DefaultEcs.System;

namespace HereticalSolutions.Templates.Universal.Unity
{
	public class CreateEntityOnEntitySpawnedEventSystem : AEntitySetSystem<float>
	{
		private readonly UniversalTemplateEntityManager entityManager;

		private readonly EEntityAuthoringPresets authoringPreset;

		public CreateEntityOnEntitySpawnedEventSystem(
			World world,
			UniversalTemplateEntityManager entityManager,
			EEntityAuthoringPresets authoringPreset)
			: base(
				world
					.GetEntities()
					.With<EntitySpawnedEventComponent>()
					.Without<EventProcessedComponent>()
					.AsSet())
		{
			this.entityManager = entityManager;

			this.authoringPreset = authoringPreset;
		}

		protected override void Update(
			float deltaTime,
			in Entity entity)
		{
			var entitySpawnedEventComponent = entity.Get<EntitySpawnedEventComponent>();

			if (entitySpawnedEventComponent.Override.IsAlive)
			{
				entityManager
					.SpawnEntity(
						entitySpawnedEventComponent.GUID,
						entitySpawnedEventComponent.PrototypeID,
						new[]
						{
							new PrototypeOverride<Entity>
							{
								WorldID = WorldConstants.SIMULATION_WORLD_ID,

								OverrideEntity = entitySpawnedEventComponent.Override
							}
						},
						authoringPreset);
			}
			else
			{
				entityManager
					.SpawnEntity(
						entitySpawnedEventComponent.GUID,
						entitySpawnedEventComponent.PrototypeID,
						authoringPreset);
			}
		}
	}
}