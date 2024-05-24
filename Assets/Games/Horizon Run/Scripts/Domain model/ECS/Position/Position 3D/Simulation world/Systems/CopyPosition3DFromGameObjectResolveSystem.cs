using HereticalSolutions.Entities;

using UnityEngine;

using DefaultEcs;

namespace HereticalSolutions.HorizonRun
{
    public class CopyPosition3DFromGameObjectResolveSystem<TSceneEntity>
        : IDefaultECSEntityInitializationSystem
          where TSceneEntity : MonoBehaviour
    {
        //Required by ISystem
        public bool IsEnabled { get; set; } = true;

        public void Update(Entity entity)
        {
            if (!IsEnabled)
                return;

            if (!entity.Has<ResolveSimulationComponent>())
                return;

            if (!entity.Has<Position3DComponent>())
                return;


            ref ResolveSimulationComponent resolveSimulationComponent = ref entity.Get<ResolveSimulationComponent>();

            ref Position3DComponent positionComponent = ref entity.Get<Position3DComponent>();


            GameObject source = resolveSimulationComponent.Source as GameObject;

            if (source == null)
                return;


            var worldPosition = source.transform.position;


            TransformPosition3DViewComponent positionViewComponent = source.GetComponentInChildren<TransformPosition3DViewComponent>();

            if (positionViewComponent != null)
            {
                worldPosition = positionViewComponent.PositionTransform.position;
            
            }

            positionComponent.Position = worldPosition;

            if (entity.Has<Transform3DComponent>())
            {
                ref Transform3DComponent transformComponent = ref entity.Get<Transform3DComponent>();

                transformComponent.Dirty = true;
            }
        }

        /// <summary>
        /// Disposes the system.
        /// </summary>
        public void Dispose()
        {
        }
    }
}