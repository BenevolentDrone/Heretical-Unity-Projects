using System;

using HereticalSolutions.Messaging;

using HereticalSolutions.Entities;

using HereticalSolutions.Networking.ECS;

using HereticalSolutions.Networking.LiteNetLib;

using DefaultEcs;

using LiteNetLib;

namespace HereticalSolutions.Templates.Universal.Networking
{
    public class NetworkEntityDeinitializationSystem
        : IDefaultECSEntityInitializationSystem
    {
        private readonly UniversalTemplateEntityManager entityManager;
        
        private readonly INetworkEntityRepository<Guid> networkEntityRepository;
        
        private readonly INonAllocMessageSender networkBusAsSender;
        
        public NetworkEntityDeinitializationSystem(
            UniversalTemplateEntityManager entityManager,
            INetworkEntityRepository<Guid> networkEntityRepository,
            INonAllocMessageSender networkBusAsSender)
        {
            this.entityManager = entityManager;
            
            this.networkEntityRepository = networkEntityRepository;
            
            this.networkBusAsSender = networkBusAsSender;
        }

        //Required by ISystem
        public bool IsEnabled { get; set; } = true;

        public void Update(Entity entity)
        {
            if (!IsEnabled)
                return;

            if (!entity.Has<GUIDComponent>())
                return;
            
            var entityID = entity.Get<GUIDComponent>().GUID;


            var registryEntity = entityManager.GetRegistryEntity(
                entityID);

            if (!registryEntity.Has<NetworkEntityComponent>())
            {
                return;
            }

            var networkEntityComponent = registryEntity.Get<NetworkEntityComponent>();
            
            if (networkEntityComponent.NetworkID == ushort.MaxValue)
            {
                return;
            }
            
            networkBusAsSender
                .PopMessage<ServerSendPacketMessage>(
                    out var sendPacketMessage)
                .Write<ServerSendPacketMessage>(
                    sendPacketMessage,
                    new object[]
                    {
                        byte.MaxValue,
                        typeof(EntityDestroyedPacket),
                        new EntityDestroyedPacket
                        {
                            NetworkID = networkEntityComponent.NetworkID
                        },
                        null,
                        DeliveryMethod.ReliableUnordered
                    })
                .SendImmediately<ServerSendPacketMessage>(
                    sendPacketMessage);

            networkEntityRepository.TryRemoveNetworkEntity(
                networkEntityComponent.NetworkID);
        }

        public void Dispose()
        {
        }
    }
}