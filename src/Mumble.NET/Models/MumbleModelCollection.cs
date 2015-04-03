//-----------------------------------------------------------------------
// <copyright file="MumbleModelCollection.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble.Models
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Google.ProtocolBuffers;

    /// <summary>
    /// Abstract class for any collection of Mumble objects
    /// </summary>
    /// <typeparam name="TModel">Type of contained Mumble object</typeparam>
    /// <typeparam name="TState">Type of message containing object state</typeparam>
    public abstract class MumbleModelCollection<TModel, TState> : IReadOnlyDictionary<uint, TModel>
        where TModel : MumbleModel<TModel, TState>
        where TState : IMessage<TState> 
    {
        /// <summary>
        /// Internal thread-safe store of Mumble objects
        /// </summary>
        private ConcurrentDictionary<uint, TModel> containedDictionary = new ConcurrentDictionary<uint, TModel>();

        /// <inheritdoc />
        public IEnumerable<uint> Keys
        {
            get 
            {
                return this.containedDictionary.Keys;
            }
        }

        /// <inheritdoc />
        public IEnumerable<TModel> Values
        {
            get 
            {
                return this.containedDictionary.Values;
            }
        }

        /// <inheritdoc />
        public int Count
        {
            get 
            {
                return this.containedDictionary.Count;
            }
        }

        /// <inheritdoc />
        public TModel this[uint key]
        {
            get 
            {
                return this.containedDictionary[key];
            }
        }

        /// <inheritdoc />
        public bool ContainsKey(uint key)
        {
            return this.containedDictionary.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(uint key, out TModel value)
        {
            return this.containedDictionary.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<uint, TModel>> GetEnumerator()
        {
            return this.containedDictionary.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.containedDictionary.GetEnumerator();
        }

        /// <summary>
        /// Update the internal collection based on a new state message
        /// </summary>
        /// <param name="state">State message</param>
        internal void UpdateState(TState state)
        {
            var newModel = MumbleModel<TModel, TState>.Create(state, this);
            this.containedDictionary.AddOrUpdate(newModel.Id, newModel, (_, model) => model.Update(state));
        }

        /// <summary>
        /// Remove an object with the given id
        /// </summary>
        /// <param name="id">Id of object to remove</param>
        internal void Remove(uint id)
        {
            TModel removedItem;
            this.containedDictionary.TryRemove(id, out removedItem);
        }
    }
}
