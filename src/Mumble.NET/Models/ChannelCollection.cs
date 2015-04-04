//-----------------------------------------------------------------------
// <copyright file="ChannelCollection.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble.Models
{
    using Messages;

    /// <summary>
    /// Class representing the collection of channels in a Mumble server
    /// </summary>
    public sealed class ChannelCollection : MumbleModelCollection<Channel, ChannelState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelCollection"/> class.
        /// </summary>
        /// <param name="client">Client to which this channel collection belongs</param>
        internal ChannelCollection(MumbleClient client)
            : base(client)
        {
            this.Client.ChannelStateReceived += (sender, args) => this.UpdateState(args.Message);
            this.Client.ChannelRemoveReceived += (sender, args) => this.Remove(args.Message.ChannelId);
        }
    }
}
