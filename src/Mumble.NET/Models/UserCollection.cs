//-----------------------------------------------------------------------
// <copyright file="UserCollection.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble.Models
{
    using Messages;

    /// <summary>
    /// Class representing the collection of users signed onto a Mumble server
    /// </summary>
    public sealed class UserCollection : MumbleModelCollection<User, UserState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserCollection"/> class.
        /// </summary>
        /// <param name="client">Client to which this user collection belongs</param>
        internal UserCollection(MumbleClient client)
            : base(client)
        {
            this.Client.UserStateReceived += (sender, args) => this.UpdateState(args.Message);
            this.Client.UserRemoveReceived += (sender, args) => this.Remove(args.Message.Session);
        }
    }
}
