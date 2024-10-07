using System;
using System.Collections.Generic;

using HereticalSolutions.Allocations;
using HereticalSolutions.Allocations.Factories;
using HereticalSolutions.Collections;
using HereticalSolutions.Metadata.Allocations;

using HereticalSolutions.Logging;

namespace HereticalSolutions.Pools.Factories
{
    public static class LinkedListPoolFactory
    {
        #region LinkedListManagedPool

        public static LinkedListManagedPool<T> BuildLinkedListManagedPool<T>(
            AllocationCommand<T> initialAllocationCommand,
            AllocationCommand<T> additionalAllocationCommand,
            MetadataAllocationDescriptor[] metadataAllocationDescriptors = null,
            IAllocationCallback<IPoolElementFacade<T>> facadeAllocationCallback = null,
            ILoggerResolver loggerResolver = null)
        {
            ILogger logger =
                loggerResolver?.GetLogger<LinkedListManagedPool<T>>()
                ?? null;
            
            Func<IPoolElementFacade<T> > facadeAllocationDelegate = 
                () => ObjectPoolsAllocationFactory.BuildPoolElementFacadeWithLinkedList<T>(
                    metadataAllocationDescriptors);
            
            AllocationCommand<IPoolElementFacade<T>> initialFacadeAllocationCommand =
                ObjectPoolsAllocationCommandFactory.BuildPoolElementFacadeAllocationCommand(
                    initialAllocationCommand.Descriptor,
                    facadeAllocationDelegate,
                    facadeAllocationCallback);
            
            AllocationCommand<IPoolElementFacade<T>> additionalFacadeAllocationCommand =
                ObjectPoolsAllocationCommandFactory.BuildPoolElementFacadeAllocationCommand(
                    additionalAllocationCommand.Descriptor,
                    facadeAllocationDelegate,
                    facadeAllocationCallback);

            var firstElement = PerformInitialAllocation<T>(
                initialFacadeAllocationCommand,
                initialAllocationCommand,
                out var capacity,
                logger);

            return new LinkedListManagedPool<T>(
                additionalFacadeAllocationCommand,
                additionalAllocationCommand,
                firstElement,
                capacity,
                logger);
        }
        
        private static ILinkedListLink<T> PerformInitialAllocation<T>(
            AllocationCommand<IPoolElementFacade<T>> facadeAllocationCommand,
            AllocationCommand<T> valueAllocationCommand,
            out int capacity,
            ILogger logger = null)
        {
            int initialAmount = -1;

            switch (facadeAllocationCommand.Descriptor.Rule)
            {
                case EAllocationAmountRule.ZERO:
                    initialAmount = 0;
                    break;

                case EAllocationAmountRule.ADD_ONE:
                    initialAmount = 1;
                    break;

                case EAllocationAmountRule.ADD_PREDEFINED_AMOUNT:
                    initialAmount = facadeAllocationCommand.Descriptor.Amount;
                    break;

                default:
                    throw new Exception(
                        logger.TryFormatException(
                            $"[LinkedListPoolFactory] INVALID INITIAL ALLOCATION COMMAND RULE: {facadeAllocationCommand.Descriptor.Rule.ToString()}"));
            }

            ILinkedListLink<T> firstElement = null;
            
            for (int i = 0; i < initialAmount; i++)
            {
                var newElementFacade = facadeAllocationCommand.AllocationDelegate(); 
                
                //MOVING IT AFTER THE VALUE ALLOCATION BECAUSE SOME WRAPPER PUSH LOGIC MAY DEPEND ON THE VALUE
                //facadeAllocationCommand.AllocationCallback?.OnAllocated(newElementFacade);
                
                var newElementValue = valueAllocationCommand.AllocationDelegate();
                    
                valueAllocationCommand.AllocationCallback?.OnAllocated(
                    newElementValue);

                newElementFacade.Value = newElementValue;

                //THIS SHOULD BE SET BEFORE ALLOCATION CALLBACK TO ENSURE THAT ELEMENTS ALREADY PRESENT ARE NOT PUSHED TWICE
                newElementFacade.Status = EPoolElementStatus.PUSHED;
                
                facadeAllocationCommand.AllocationCallback?.OnAllocated(newElementFacade);
                
                
                var newElementAsLink = newElementFacade as ILinkedListLink<T>;
            
                newElementAsLink.Previous = null;
            
                newElementAsLink.Next = firstElement;
            
                if (firstElement != null)
                    firstElement.Previous = newElementAsLink;
                
                firstElement = newElementAsLink;
            }
            
            capacity = initialAmount;

            return firstElement;
        }
        
        #endregion

        #region AppendableLinkedListManagedPool
        
        public static AppendableLinkedListManagedPool<T> BuildAppendableLinkedListManagedPool<T>(
            AllocationCommand<T> initialAllocationCommand,
            AllocationCommand<T> additionalAllocationCommand,
            MetadataAllocationDescriptor[] metadataAllocationDescriptors = null,
            IAllocationCallback<IPoolElementFacade<T>> facadeAllocationCallback = null,
            ILoggerResolver loggerResolver = null)
        {
            ILogger logger =
                loggerResolver?.GetLogger<StackManagedPool<T>>()
                ?? null;
            
            Func<IPoolElementFacade<T> > facadeAllocationDelegate = 
                () => ObjectPoolsAllocationFactory.BuildPoolElementFacadeWithLinkedList<T>(
                    metadataAllocationDescriptors);
            
            AllocationCommand<IPoolElementFacade<T>> initialFacadeAllocationCommand =
                ObjectPoolsAllocationCommandFactory.BuildPoolElementFacadeAllocationCommand(
                    initialAllocationCommand.Descriptor,
                    facadeAllocationDelegate,
                    facadeAllocationCallback);
            
            AllocationCommand<IPoolElementFacade<T>> additionalFacadeAllocationCommand =
                ObjectPoolsAllocationCommandFactory.BuildPoolElementFacadeAllocationCommand(
                    additionalAllocationCommand.Descriptor,
                    facadeAllocationDelegate,
                    facadeAllocationCallback);
            
            AllocationCommand<T> nullValueAllocationCommand =
                new AllocationCommand<T>
                {
                    Descriptor = new AllocationCommandDescriptor
                    {
                        Rule = EAllocationAmountRule.ADD_ONE,
                        
                        Amount = 1
                    },
                    
                    AllocationDelegate = AllocationsFactory.NullAllocationDelegate<T>
                };

            AllocationCommand<IPoolElementFacade<T>> appendFacadeAllocationCommand =
                ObjectPoolsAllocationCommandFactory.BuildPoolElementFacadeAllocationCommand(
                    nullValueAllocationCommand.Descriptor,
                    facadeAllocationDelegate,
                    facadeAllocationCallback);
            
            var firstElement = PerformInitialAllocation<T>(
                initialFacadeAllocationCommand,
                initialAllocationCommand,
                out var capacity,
                logger);

            return new AppendableLinkedListManagedPool<T>(
                additionalFacadeAllocationCommand,
                additionalAllocationCommand,
                
                appendFacadeAllocationCommand,
                nullValueAllocationCommand,
                
                firstElement,
                capacity,
                logger);
        }
        
        #endregion
        
        #region Resize

        public static void ResizeLinkedListManagedPool<T>(
            ref ILinkedListLink<T> firstElement,
            ref int currentCapacity,
            AllocationCommand<IPoolElementFacade<T>> facadeAllocationCommand,
            AllocationCommand<T> valueAllocationCommand,
            ILogger logger = null)
        {
            int addedCapacity = -1;

            switch (facadeAllocationCommand.Descriptor.Rule)
            {
                case EAllocationAmountRule.ADD_ONE:
                    addedCapacity = 1;
                    break;
                
                case EAllocationAmountRule.DOUBLE_AMOUNT:
                    addedCapacity = currentCapacity * 2;
                    break;

                case EAllocationAmountRule.ADD_PREDEFINED_AMOUNT:
                    addedCapacity = facadeAllocationCommand.Descriptor.Amount;
                    break;

                default:
                    throw new Exception(
                        logger.TryFormatException(
                            $"[LinkedListPoolFactory] INVALID RESIZE ALLOCATION COMMAND RULE FOR STACK: {facadeAllocationCommand.Descriptor.Rule.ToString()}"));
            }

            for (int i = 0; i < addedCapacity; i++)
            {
                var newElement = facadeAllocationCommand.AllocationDelegate(); 
                
                //MOVING IT AFTER THE VALUE ALLOCATION BECAUSE SOME WRAPPER PUSH LOGIC MAY DEPEND ON THE VALUE
                //facadeAllocationCommand.AllocationCallback?.OnAllocated(newElement);
                
                var newElementValue = valueAllocationCommand.AllocationDelegate();
                    
                valueAllocationCommand.AllocationCallback?.OnAllocated(
                    newElementValue);

                newElement.Value = newElementValue;
                
                //THIS SHOULD BE SET BEFORE ALLOCATION CALLBACK TO ENSURE THAT ELEMENTS ALREADY PRESENT ARE NOT PUSHED TWICE
                newElement.Status = EPoolElementStatus.PUSHED;
                
                facadeAllocationCommand.AllocationCallback?.OnAllocated(newElement);
                
                
                var newElementAsLink = newElement as ILinkedListLink<T>;
            
                newElementAsLink.Previous = null;
            
                newElementAsLink.Next = firstElement;
            
                if (firstElement != null)
                    firstElement.Previous = newElementAsLink;
                
                firstElement = newElementAsLink;
            }

            currentCapacity += addedCapacity;
        }
        
        #endregion
    }
}