using HereticalSolutions.Collections;

using HereticalSolutions.Repositories;

namespace HereticalSolutions.Pools
{
    public class PoolElementFacadeWithLinkedListLink<T>
        : PoolElementFacade<T>,
          ILinkedListLink<T>
    {
        public PoolElementFacadeWithLinkedListLink(
            IReadOnlyObjectRepository metadata)
            : base (metadata)
        {
        }

        #region ILinkedList

        public ILinkedListLink<T> Previous { get; set; }
        
        public ILinkedListLink<T> Next { get; set; }

        #endregion

        #region ICleanUppable

        public override void Cleanup()
        {
            Previous = null;
            
            Next = null;
            
            base.Cleanup();
        }

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            Previous = null;
            
            Next = null;
            
            base.Cleanup();
        }

        #endregion
    }
}