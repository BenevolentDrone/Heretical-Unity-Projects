using System;
using System.Collections.Generic;

using HereticalSolutions.Repositories;

using HereticalSolutions.Entities;

using HereticalSolutions.Logging;

using World = DefaultEcs.World;

using Entity = DefaultEcs.Entity;

namespace HereticalSolutions.Templates.Universal
{
	public class UniversalTemplateEntityManager : DefaultECSEntityManager<Guid>
	{
		public UniversalTemplateEntityManager(
			Func<Guid> allocateIDDelegate,
			IRepository<Guid, Entity> registryEntitiesRepository,
			IReadOnlyEntityWorldsRepository<World, IDefaultECSEntityWorldController> entityWorldsRepository,
			IReadOnlyList<World> childEntityWorlds,
			ILogger logger = null)
			: base (
				allocateIDDelegate,
				registryEntitiesRepository,
				entityWorldsRepository,
				childEntityWorlds,
				logger)
		{}
	}
}