using System;

namespace Spellbound.Stats {
    /// <summary>
    /// Represents the abstract implementation of a modifier.
    /// </summary>
    /// <remarks>
    /// This class simply wraps both IModifier and IHasUniqueId. If a user has their own unique identifier system or
    /// needs to inherit from something else then they are free to implement IModifier and/or IHasUniqueId directly.
    /// </remarks>
    [Serializable]
    public abstract class SbModifier : IModifier, IHasUniqueId {
        public abstract void Apply(ICanBeModified target);

        public abstract void Remove(ICanBeModified target);

        public string UniqueId { get; } = Guid.NewGuid().ToString();

        #region Convenience Methods
        
        /// <summary>
        /// Attempts to get the StatContainer from the ICanBeModified target if an IHasStats exists on the target.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="stats"></param>
        /// <returns>
        /// bool - True if a stat container is found and false if it's not.
        /// </returns>
        protected bool TryGetStats(ICanBeModified target, out StatContainer stats) {
            stats = null;
            
            if (target is not IHasStats hs) 
                return false;
            
            stats = hs.Stats;
            return true;
        }
    
        /// <summary>
        /// Attempts to get the SbBehaviour from the ICanBeModified target if an IHasBehaviour exists on the target.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="behaviour"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        ///
        /// </returns>
        protected bool TryGetBehaviour<T>(ICanBeModified target, out T behaviour) where T : SbBehaviour {
            behaviour = null;
            return target is IHasBehaviours hb && hb.Behaviours.TryGetBehaviour(out behaviour);
        }
    
        /// <summary>
        /// Attempts to get the EventContainer from the ICanBeModified target if an IHasEvents exists on the target.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="events"></param>
        /// <returns>
        ///
        /// </returns>
        protected bool TryGetEvents(ICanBeModified target, out EventContainer events) {
            events = null;
            
            if (target is not IHasEvents he) 
                return false;
            
            events = he.Events;
            return true;
        }
        
        #endregion
    }
}