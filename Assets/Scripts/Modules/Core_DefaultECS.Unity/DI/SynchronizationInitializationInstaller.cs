using HereticalSolutions.Delegates;
using HereticalSolutions.Delegates.Factories;

using HereticalSolutions.Logging;

using UnityEngine;

using Zenject;

namespace HereticalSolutions.Modules.Core_DefaultECS.DI
{
    public class SynchronizationInitializationInstaller : MonoInstaller
    {
        [Inject]
        private ILoggerResolver loggerResolver;

        [SerializeField]
        private SynchronizationBehaviour synchronizationBehaviour;

        public override void InstallBindings()
        {
            #region Update

            var updatePinger = PingersFactory.BuildNonAllocPinger(loggerResolver);

            Container
                .Bind<IPublisherNoArgs>()
                .WithId("Update")
                .FromInstance(updatePinger)
                .AsCached();

            Container
                .Bind<INonAllocSubscribableNoArgs>()
                .WithId("Update")
                .FromInstance(updatePinger)
                .AsCached();

            #endregion

            #region Fixed update

            var fixedUpdatePinger = PingersFactory.BuildNonAllocPinger(loggerResolver);

            Container
                .Bind<IPublisherNoArgs>()
                .WithId("Fixed update")
                .FromInstance(fixedUpdatePinger)
                .AsCached();

            Container
                .Bind<INonAllocSubscribableNoArgs>()
                .WithId("Fixed update")
                .FromInstance(fixedUpdatePinger)
                .AsCached();

            #endregion

            #region Late update

            var lateUpdatePinger = PingersFactory.BuildNonAllocPinger(loggerResolver);

            Container
                .Bind<IPublisherNoArgs>()
                .WithId("Late update")
                .FromInstance(lateUpdatePinger)
                .AsCached();

            Container
                .Bind<INonAllocSubscribableNoArgs>()
                .WithId("Late update")
                .FromInstance(lateUpdatePinger)
                .AsCached();

            #endregion

            synchronizationBehaviour.Initialize(
                updatePinger,
                fixedUpdatePinger,
                lateUpdatePinger);

            Container
                .Bind<MonoBehaviour>()
                .FromInstance(synchronizationBehaviour)
                .AsCached();
        }
    }
}