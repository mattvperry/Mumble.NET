//-----------------------------------------------------------------------
// <copyright file="ServerInfo.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble
{
    using System;

    /// <summary>
    /// Class which holds basic Mumble server information
    /// </summary>
    public class ServerInfo
    {
        /// <summary>
        /// Gets server host name
        /// </summary>
        public string HostName { get; internal set; }

        /// <summary>
        /// Gets server listening port
        /// </summary>
        public int Port { get; internal set; }

        /// <summary>
        /// Gets OS on which the server is running
        /// </summary>
        public string OS { get; internal set; }

        /// <summary>
        /// Gets version of OS running the server
        /// </summary>
        public string OSVersion { get; internal set; }

        /// <summary>
        /// Gets flavor of the Mumble server
        /// </summary>
        public string Release { get; internal set; }

        /// <summary>
        /// Gets mumble server version
        /// </summary>
        public Version Version { get; internal set; }
    }
}
