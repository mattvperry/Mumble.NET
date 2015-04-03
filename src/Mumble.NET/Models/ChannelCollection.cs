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
    public class ChannelCollection : MumbleModelCollection<Channel, ChannelState>
    {
    }
}
