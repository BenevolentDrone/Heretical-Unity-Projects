using HereticalSolutions.Logging;

using DefaultEcs;
using DefaultEcs.System;

namespace HereticalSolutions.Entities
{
    /// <summary>
    /// Represents a system responsible for despawning entities.
    /// </summary>
    public class DespawnSystem : ISystem<float>
    {
        private readonly ILogger logger;
        
        /// <summary>
        /// Gets or sets a value indicating whether the system is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        private EntitySet entitiesToDespawn;

            //private EntitySet allEntities;

        /// <summary>
        /// Initializes a new instance of the <see cref="DespawnSystem"/> class.
        /// </summary>
        /// <param name="world">The world.</param>
        public DespawnSystem(
            World world,
            ILogger logger = null)
        {
            this.logger = logger;
            
            // Get the entities with a DespawnComponent and store them in a set
            entitiesToDespawn =
                world
                    .GetEntities()
                    .With<DespawnComponent>()
                    .AsSet();
        }

        /// <summary>
        /// Updates the system.
        /// </summary>
        /// <param name="state">The current state.</param>
        public void Update(float state)
        {
            // Iterate through the entities to despawn and dispose them
            foreach (Entity entity in entitiesToDespawn.GetEntities())
            {
                entity.Dispose();
                
                logger?.Log<DespawnSystem>(
                    $"ENTITY {entity} DESPAWNED");
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}