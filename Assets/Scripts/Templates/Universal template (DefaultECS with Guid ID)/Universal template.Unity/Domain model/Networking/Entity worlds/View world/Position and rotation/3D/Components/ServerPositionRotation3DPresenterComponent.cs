using HereticalSolutions.Entities;

using DefaultEcs;

namespace HereticalSolutions.Templates.Universal.Unity.Networking
{
    [Component("View world/Presenters")]
    public struct ServerPositionRotation3DPresenterComponent
    {
        public Entity TargetEntity;
    }
}