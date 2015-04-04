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
        public string Name
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
    }
}
