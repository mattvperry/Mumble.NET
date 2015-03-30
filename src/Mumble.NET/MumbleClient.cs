//-----------------------------------------------------------------------
// <copyright file="MumbleClient.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Threading.Tasks;
    using Google.ProtocolBuffers;
    using Messages;

    /// <summary>
    /// Class representing a mumble client. Main entry point to the library.
    /// </summary>
    public sealed class MumbleClient : IDisposable
    {
        /// <summary>
        /// Constant for the default mumble port
        /// </summary>
        public const int DefaultPort = 64738;

        /// <summary>
        /// Username of Mumble client
        /// </summary>
        private readonly string userName;

        /// <summary>
        /// Password for authenticating with server
        /// </summary>
        private readonly string password;

        /// <summary>
        /// Network connection to the server
        /// </summary>
        private readonly IConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="MumbleClient"/> class.
        /// </summary>
        /// <param name="host">Hostname of Mumble server</param>
        /// <param name="userName">Username of Mumble client</param>
        public MumbleClient(string host, string userName)
            : this(host, userName, string.Empty, DefaultPort)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MumbleClient"/> class.
        /// </summary>
        /// <param name="host">Hostname of Mumble server</param>
        /// <param name="userName">Username of Mumble client</param>
        /// <param name="password">Password for authenticating with server</param>
        /// <param name="port">Port on which the server is listening</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Connection will be disposed")]
        public MumbleClient(string host, string userName, string password, int port)
            : this(userName, password, new Connection(host, port))
        {
            this.ServerInfo = new ServerInfo { HostName = host, Port = port };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MumbleClient"/> class.
        /// </summary>
        /// <param name="username">Username of Mumble client</param>
        /// <param name="password">Password for authenticating with server</param>
        /// <param name="connection">Network connection to the server</param>
        internal MumbleClient(string username, string password, IConnection connection)
        {
            this.userName = username;
            this.password = password;
            this.connection = connection;
        }

        /// <summary>
        /// Gets basic Mumble server information
        /// </summary>
        public ServerInfo ServerInfo { get; private set; }

        /// <summary>
        /// Establish a connection with the Mumble server
        /// </summary>
        /// <returns>Empty task</returns>
        public async Task ConnectAsync()
        {
            await this.connection.ConnectAsync();

            var version = await this.connection.ReadMessage<Messages.Version>();
            this.ServerInfo.OS = version.Os;
            this.ServerInfo.OSVersion = version.OsVersion;
            this.ServerInfo.Release = version.Release;
            this.ServerInfo.Version = DecodeVersion(version.Version_);

            await this.BuildAndSend<Messages.Version.Builder>((builder) =>
            {
                builder.Version_ = EncodeVersion(new System.Version(1, 2, 8));
                builder.Release = string.Format(CultureInfo.InvariantCulture, "Mumble.NET {0}", Assembly.GetExecutingAssembly().GetName().Version);
                builder.Os = Environment.OSVersion.Platform.ToString();
                builder.OsVersion = Environment.OSVersion.VersionString;
            });

            await this.BuildAndSend<Authenticate.Builder>((builder) =>
            {
                builder.Username = this.userName;
                builder.Password = this.password;
                builder.Opus = true;
            });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.connection.Dispose();
        }

        /// <summary>
        /// Encode a version into mumble wire protocol version
        /// </summary>
        /// <param name="version">Version to encode</param>
        /// <returns>Unsigned integer representing the version</returns>
        private static uint EncodeVersion(System.Version version)
        {
            return (uint)((version.Major << 16) | (version.Minor << 8) | (version.Build & 0xFF));
        }

        /// <summary>
        /// Decode a version number
        /// </summary>
        /// <param name="version">Encoding version</param>
        /// <returns>Decoded version object</returns>
        private static System.Version DecodeVersion(uint version)
        {
            return new System.Version(
                (int)(version >> 16) & 0xFF,
                (int)(version >> 8) & 0xFF,
                (int)version & 0xFF);
        }

        /// <summary>
        /// Build a protobuf message and send it
        /// </summary>
        /// <typeparam name="T">Type of message to build</typeparam>
        /// <param name="build">Callback for the actual building</param>
        /// <returns>Empty task</returns>
        private async Task BuildAndSend<T>(Action<T> build) where T : IBuilder, new()
        {
            var builder = new T();
            build(builder);
            await this.connection.SendMessage(builder.WeakBuild());
        }
    }
}
