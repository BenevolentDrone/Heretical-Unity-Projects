using HereticalSolutions.Entities;

using ILogger = HereticalSolutions.Logging.ILogger;

using DefaultEcs;

namespace HereticalSolutions.HorizonRun
{
	public class UniformRotationPresenterInitializationSystem
		: IDefaultECSEntityInitializationSystem
	{
		private readonly HorizonRunEntityManager entityManager;

		private readonly ILogger logger;

		public UniformRotationPresenterInitializationSystem(
			HorizonRunEntityManager entityManager,
			ILogger logger = null)
		{
			this.entityManager = entityManager;

			this.logger = logger;
		}

		//Required by ISystem
		public bool IsEnabled { get; set; } = true;

		public void Update(Entity entity)
		{
			if (!IsEnabled)
				return;

			if (!entity.Has<UniformRotationPresenterComponent>())
				return;

			ref var rotationPresenterComponent = ref entity.Get<UniformRotationPresenterComponent>();

			var guid = entity.Get<GUIDComponent>().GUID;

			var simulationEntity = entityManager.GetEntity(
				guid,
				WorldConstants.SIMULATION_WORLD_ID);

			if (!simulationEntity.IsAlive)
			{
				logger?.LogError<UniformRotationPresenterInitializationSystem>(
					$"ENTITY {guid} HAS NO SIMULATION ENTITY");

				return;
			}

			rotationPresenterComponent.TargetEntity = simulationEntity;
		}

		public void Dispose()
		{
		}
	}
}