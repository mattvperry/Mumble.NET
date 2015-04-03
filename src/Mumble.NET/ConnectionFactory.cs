//-----------------------------------------------------------------------
// <copyright file="ConnectionFactory.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble
{
    /// <summary>
    /// Class factory which creates connection objects
    /// </summary>
    internal class ConnectionFactory : IConnectionFactory
    {
        /// <inheritdoc />
        public IConnection CreateConnection(string host, int port)
        {
            return new Connection(host, port);
        }
    }
}
