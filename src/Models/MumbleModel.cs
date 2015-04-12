//-----------------------------------------------------------------------
// <copyright file="MumbleModel.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble.Models
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using Google.ProtocolBuffers;

    /// <summary>
    /// Abstract class for any Mumble object
    /// </summary>
    /// <typeparam name="TModel">Type of Mumble object</typeparam>
    /// <typeparam name="TState">Type of message containing object state</typeparam>
    public abstract class MumbleModel<TModel, TState> 
        where TModel : MumbleModel<TModel, TState>
        where TState : IMessage<TState> 
    {
        /// <summary>
        /// Client to which this object belongs
        /// </summary>
        private readonly MumbleClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="MumbleModel{TModel,TState}"/> class.
        /// </summary>
        /// <param name="state">Initial State</param>
        /// <param name="client">Client to which this object belongs</param>
        protected MumbleModel(TState state, MumbleClient client)
        {
            this.State = state;
            this.client = client;
        }

        /// <summary>
        /// Gets the id of the object
        /// </summary>
        public abstract uint Id { get; }

        /// <summary>
        /// Gets the name of the object
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets or sets the current State of the object
        /// </summary>
        protected TState State { get; set; }

        /// <summary>
        /// Gets the Client to which this object belongs
        /// </summary>
        protected MumbleClient Client
        { 
            get
            {
                return this.client;
            }
        }

        /// <summary>
        /// Initializes a new concrete instance of the TModel class.
        /// </summary>
        /// <param name="state">Initial State</param>
        /// <param name="client">Client to which this object belongs</param>
        /// <returns>New instance</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes",
            Justification = "Required for generic construction of type TModel")]
        public static TModel Create(TState state, MumbleClient client)
        {
            return (TModel)Activator.CreateInstance(
                typeof(TModel), 
                BindingFlags.NonPublic | BindingFlags.Instance, 
                null, 
                new object[] { state, client }, 
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Update the State of a Mumble object
        /// </summary>
        /// <param name="stateUpdate">State message containing updated information</param>
        /// <returns>This Mumble object with updated state</returns>
        internal virtual TModel Update(TState stateUpdate)
        {
            this.State = (TState)this.State.WeakToBuilder().WeakMergeFrom(stateUpdate).WeakBuild();
            return (TModel)this;
        }
    }
}
