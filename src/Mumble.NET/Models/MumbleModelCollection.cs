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
    using System.Globalization;
    using System.Linq;
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
        /// Client to which this collection belongs
        /// </summary>
        private readonly MumbleClient client;

        /// <summary>
        /// Internal thread-safe store of Mumble objects
        /// </summary>
        private ConcurrentDictionary<uint, TModel> containedDictionary = new ConcurrentDictionary<uint, TModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MumbleModelCollection{TModel,TState}"/> class.
        /// </summary>
        /// <param name="client">Client to which this collection belongs</param>
        protected MumbleModelCollection(MumbleClient client)
        {
            this.client = client;
        }

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

        /// <summary>
        /// Gets the client to which this collection belongs
        /// </summary>
        protected MumbleClient Client
        {
            get
            {
                return this.client;
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

        /// <summary>
        /// Indexer which finds a model by name
        /// </summary>
        /// <param name="name">Name to search</param>
        /// <returns>Model with the given name</returns>
        public TModel this[string name]
        {
            get
            {
                var model = this.containedDictionary.Values.SingleOrDefault(m => m.Name == name);
                if (model == null)
                {
                    throw new KeyNotFoundException();
                }

                return model;
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
            var newModel = MumbleModel<TModel, TState>.Create(state, this.Client);
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
