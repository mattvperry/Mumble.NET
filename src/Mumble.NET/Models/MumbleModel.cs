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
        /// State object representing the current state of the object
        /// </summary>
        private TState state;

        /// <summary>
        /// Initializes a new instance of the <see cref="MumbleModel{TModel,TState}"/> class.
        /// </summary>
        /// <param name="state">Initial state</param>
        /// <param name="containingCollection">Collection which houses this object</param>
        protected MumbleModel(TState state, IReadOnlyDictionary<uint, TModel> containingCollection)
        {
            this.state = state;
            this.ContainingCollection = containingCollection;
        }

        /// <summary>
        /// Gets the id of the object
        /// </summary>
        public abstract uint Id { get; }

        /// <summary>
        /// Gets the collection which houses this object
        /// </summary>
        protected IReadOnlyDictionary<uint, TModel> ContainingCollection { get; private set; }

        /// <summary>
        /// Gets the current state of the object
        /// </summary>
        protected TState State 
        { 
            get
            {
                return this.state;
            }
        }

        /// <summary>
        /// Initializes a new concrete instance of the TModel class.
        /// </summary>
        /// <param name="state">Initial state</param>
        /// <param name="containingCollection">Collection which houses this object</param>
        /// <returns>New instance</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes",
            Justification = "Required for generic construction of type TModel")]
        public static TModel Create(TState state, IReadOnlyDictionary<uint, TModel> containingCollection)
        {
            return (TModel)Activator.CreateInstance(
                typeof(TModel), 
                BindingFlags.NonPublic | BindingFlags.Instance, 
                null, 
                new object[] { state, containingCollection }, 
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Update the state of a Mumble object
        /// </summary>
        /// <param name="stateUpdate">State message containing updated information</param>
        /// <returns>New Mumble object with updated state</returns>
        public TModel Update(TState stateUpdate)
        {
            return Create((TState)this.state.WeakToBuilder().WeakMergeFrom(stateUpdate).WeakBuild(), this.ContainingCollection); 
        }
    }
}
