using System;

using HereticalSolutions.Entities;

using HereticalSolutions.Allocations.Factories;

namespace HereticalSolutions.HorizonRun
{
    public class HorizonRunSceneEntity :
        ASceneEntityWithID<Guid>
    {
        public override Guid EntityID { get => Guid.Parse(entityID); }

#if UNITY_EDITOR
        protected override Guid AllocateID()
        {
            return IDAllocationsFactory.BuildGUID();
        }
#endif
    }
}