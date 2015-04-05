//-----------------------------------------------------------------------
// <copyright file="Channel.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Messages;
    using Validation;

    /// <summary>
    /// Immutable class representing a Mumble channel
    /// </summary>
    public sealed class Channel : MumbleModel<Channel, ChannelState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Channel"/> class.
        /// </summary>
        /// <param name="state">Initial State of the user</param>
        /// <param name="client">Client to which this channel belongs</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Only called with reflection")]
        internal Channel(ChannelState state, MumbleClient client)
            : base(state, client)
        {
        }

        /// <summary>
        /// Gets the id of the channel
        /// </summary>
        public override uint Id 
        { 
            get
            {
                return this.State.ChannelId;
            }
        }

        /// <summary>
        /// Gets the parent of the channel
        /// </summary>
        public Channel Parent
        { 
            get
            {
                if (!this.State.HasParent)
                {
                    return null;
                }

                return this.Client.Channels[this.State.Parent];
            }
        }

        /// <summary>
        /// Gets the list of users in this channel
        /// </summary>
        public IEnumerable<User> Users
        {
            get
            {
                return this.Client.Users.Values.Where(u => u.CurrentChannel.Id == this.Id);
            }
        }

        /// <summary>
        /// Gets the name of the channel
        /// </summary>
        public override string Name
        { 
            get
            {
                return this.State.Name;
            }
        }

        /// <summary>
        /// Gets the description of the channel
        /// </summary>
        public string Description
        { 
            get
            {
                return this.State.Description;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the channel is temporary
        /// </summary>
        public bool Temporary
        { 
            get
            {
                return this.State.Temporary;
            }
        }

        /// <summary>
        /// Gets the position of the channel in the tree
        /// </summary>
        public int Position
        { 
            get
            {
                return this.State.Position;
            }
        }

        /// <summary>
        /// Gets the list of the sub channels of this channel
        /// </summary>
        public IEnumerable<Channel> SubChannels
        {
            get
            {
                return this.Client.Channels.Values
                    .Where(c => c.Parent != null)
                    .Where(c => c.Parent.Id == this.Id);
            }
        }

        /// <summary>
        /// Gets a list of all channels linked to this one
        /// </summary>
        public IEnumerable<Channel> LinkedChannels
        {
            get
            {
                return this.State.LinksList.Select(linkId => this.Client.Channels[linkId]);
            }
        }

        /// <inheritdoc />
        internal override Channel Update(ChannelState state)
        {
            Requires.NotNull(state, "state");

            var baseLinks = state.LinksList.Any() ? state.LinksList : this.State.LinksList;

            // Default merge behavior for lists is to concatenate the new list with the current one.
            // We need the current list to be overriden with the incoming message's lists
            var builder = this.State.ToBuilder();
            builder.ClearLinks();
            builder.AddRangeLinks(baseLinks.Union(state.LinksAddList).Except(state.LinksRemoveList));

            var remainingState = state.ToBuilder();
            remainingState.ClearLinks();
            remainingState.ClearLinksAdd();
            remainingState.ClearLinksRemove();

            this.State = builder.MergeFrom(remainingState.Build()).Build();
            return this;
        }
    }
}
