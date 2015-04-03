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
    public class UserCollection : MumbleModelCollection<User, UserState>
    {
    }
}
