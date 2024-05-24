using System;

using HereticalSolutions.Entities;

using HereticalSolutions.Time;

using HereticalSolutions.Delegates;
using HereticalSolutions.Delegates.Factories;

using HereticalSolutions.Logging;
using ILogger = HereticalSolutions.Logging.ILogger;

using DefaultEcs;

namespace HereticalSolutions.HorizonRun
{
	public class SensorWithTimerInitializationSystem
		: IDefaultECSEntityInitializationSystem
	{
		private readonly ITimerManager simulationTimerManager;

		private readonly IEventEntityBuilder<Entity, Guid> eventEntityBuilder;

		private readonly ILoggerResolver loggerResolver;

		private readonly ILogger logger;

		public SensorWithTimerInitializationSystem(
			ITimerManager simulationTimerManager,
			IEventEntityBuilder<Entity, Guid> eventEntityBuilder,
			ILoggerResolver loggerResolver = null,
			ILogger logger = null)
		{
			this.simulationTimerManager = simulationTimerManager;

			this.eventEntityBuilder = eventEntityBuilder;

			this.loggerResolver = loggerResolver;

			this.logger = logger;
		}

		//Required by ISystem
		public bool IsEnabled { get; set; } = true;

		public void Update(Entity entity)
		{
			if (!IsEnabled)
				return;

			if (!entity.Has<SensorWithTimerComponent>())
				return;

			if (!entity.Has<LoopedTimerComponent>())
				return;

			ref var loopedTimerComponent = ref entity.Get<LoopedTimerComponent>();


			//Create a timer
			if (!simulationTimerManager.CreateTimer(
				out var timerHandle,
				out var timer))
			{
				throw new Exception(
					logger.TryFormat<SensorWithTimerInitializationSystem>(
						$"ERROR CREATING TIMER FROM TIME MANAGER {simulationTimerManager.ID}"));
			}

			loopedTimerComponent.TimerHandle = timerHandle;

			timer.Reset();

			timer.Repeat = true;

			timer.FlushTimeElapsedOnRepeat = false;

			Entity entityClosure = entity;

			ISubscription timerTickSubscription = DelegatesFactory.BuildSubscriptionSingleArgGeneric<IRuntimeTimer>(
				(timerArg) => OnTick(entityClosure),
				loggerResolver);

			timer.OnFinish.Subscribe(
				(ISubscriptionHandler<
					INonAllocSubscribableSingleArgGeneric<IRuntimeTimer>,
					IInvokableSingleArgGeneric<IRuntimeTimer>>)
					timerTickSubscription);
		}

		private void OnTick(Entity passiveIncomeEntity)
		{
			eventEntityBuilder
				.NewEvent(out var eventEntity)
				.CausedByWorldLocalEntity(
					eventEntity,
					passiveIncomeEntity)
				.WithData<SensorScanPerformedEventComponent>(
					eventEntity,
					new SensorScanPerformedEventComponent());
		}

		public void Dispose()
		{
		}
	}
}