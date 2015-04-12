//-----------------------------------------------------------------------
// <copyright file="IConnectionFactory.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble
{
    /// <summary>
    /// Interface for a factory of connection objects
    /// </summary>
    internal interface IConnectionFactory
    {
        /// <summary>
        /// Creates a connection object for a specific host and port
        /// </summary>
        /// <param name="host">Host name of server to connect to</param>
        /// <param name="port">Port on which the server is listening</param>
        /// <returns>New server connection</returns>
        IConnection CreateConnection(string host, int port);
    }
}
