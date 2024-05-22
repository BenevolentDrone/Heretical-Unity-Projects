using HereticalSolutions.Entities;

using ILogger = HereticalSolutions.Logging.ILogger;

using DefaultEcs;

namespace HereticalSolutions.HorizonRun
{
	public class PositionPresenterInitializationSystem
		: IDefaultECSEntityInitializationSystem
	{
		private readonly HorizonRunEntityManager entityManager;

		private readonly ILogger logger;

		public PositionPresenterInitializationSystem(
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

			if (!entity.Has<PositionPresenterComponent>())
				return;

			ref var positionPresenterComponent = ref entity.Get<PositionPresenterComponent>();

			var guid = entity.Get<GUIDComponent>().GUID;

			var simulationEntity = entityManager.GetEntity(
				guid,
				WorldConstants.SIMULATION_WORLD_ID);

			if (!simulationEntity.IsAlive)
			{
				logger?.LogError<PositionPresenterInitializationSystem>(
					$"ENTITY {guid} HAS NO SIMULATION ENTITY");

				return;
			}

			positionPresenterComponent.TargetEntity = simulationEntity;
		}

		public void Dispose()
		{
		}
	}
}