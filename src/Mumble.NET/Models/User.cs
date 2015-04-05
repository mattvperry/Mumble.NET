//-----------------------------------------------------------------------
// <copyright file="User.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble.Models
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Messages;

    /// <summary>
    /// Class representing a Mumble user
    /// </summary>
    public sealed class User : MumbleModel<User, UserState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="state">Initial State of the user</param>
        /// <param name="client">Client to which this user belongs</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Only called with reflection")]
        internal User(UserState state, MumbleClient client)
            : base(state, client)
        {
        }

        /// <summary>
        /// Gets the id of the user
        /// </summary>
        public override uint Id
        {
            get
            {
                return this.State.Session;
            }
        }

        /// <summary>
        /// Gets the name of the user
        /// </summary>
        public override string Name
        {
            get
            {
                return this.State.Name;
            }
        }

        /// <summary>
        /// Gets the registered user id of the user
        /// </summary>
        public uint RegisteredId
        {
            get
            {
                return this.State.UserId;
            }
        }

        /// <summary>
        /// Gets the current channel that this user is in
        /// </summary>
        public Channel CurrentChannel
        {
            get
            {
                return this.Client.Channels[this.State.ChannelId];
            }
        }

        /// <summary>
        /// Gets the user's comment
        /// </summary>
        public string Comment
        {
            get
            {
                return this.State.Comment;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the admin has muted the user
        /// </summary>
        public bool AdminMute
        {
            get
            {
                return this.State.Mute;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the admin has deafened the user
        /// </summary>
        public bool AdminDeafen
        {
            get
            {
                return this.State.Deaf;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user has muted themselves
        /// </summary>
        public bool SelfMute
        {
            get
            {
                return this.State.SelfMute;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user has deafen themselves
        /// </summary>
        public bool SelfDeafen
        {
            get
            {
                return this.State.SelfDeaf;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user has priority speaker
        /// </summary>
        public bool PrioritySpeaker
        {
            get
            {
                return this.State.PrioritySpeaker;
            }
        }

        /// <summary>
        /// Gets a value indicating whether is currently recording
        /// </summary>
        public bool Recording
        {
            get
            {
                return this.State.Recording;
            }
        }

        /// <summary>
        /// Make this user join a channel
        /// </summary>
        /// <param name="id">Id of channel to join</param>
        /// <returns>Empty task</returns>
        public async Task JoinChannelAsync(uint id)
        {
            await this.Client.SendMessage(new UserState.Builder
            {
                Session = this.Id,
                Actor = this.Client.ClientUser.Id,
                ChannelId = id,
            });
        }

        /// <summary>
        /// Make this user join a channel
        /// </summary>
        /// <param name="name">Name of channel to join</param>
        /// <returns>Empty task</returns>
        public async Task JoinChannelAsync(string name)
        {
            await this.JoinChannelAsync(this.Client.Channels[name]);
        }

        /// <summary>
        /// Make this user join a channel
        /// </summary>
        /// <param name="channel">Channel to join</param>
        /// <returns>Empty task</returns>
        public async Task JoinChannelAsync(Channel channel)
        {
            await this.JoinChannelAsync(channel.Id);
        }
    }
}
