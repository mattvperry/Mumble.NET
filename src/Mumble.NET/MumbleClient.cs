//-----------------------------------------------------------------------
// <copyright file="MumbleClient.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Google.ProtocolBuffers;
    using Messages;
    using Models;

    /// <summary>
    /// Class representing a mumble client. Main entry point to the library.
    /// </summary>
    public sealed partial class MumbleClient : IDisposable
    {
        /// <summary>
        /// Constant for the default mumble port
        /// </summary>
        public const int DefaultPort = 64738;

        /// <summary>
        /// Client's Mumble version
        /// </summary>
        public static readonly System.Version ClientMumbleVersion = new System.Version(1, 2, 8);

        /// <summary>
        /// Factory to create server connection objects
        /// </summary>
        private readonly IConnectionFactory connectionFactory;

        /// <summary>
        /// Network connection to the server
        /// </summary>
        private IConnection connection;

        /// <summary>
        /// Long running task which continually reads messages
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields",
            Justification = "Long running task. Cleaned up by cancellation token. Need to hold a reference.")]
        private Task readTask;

        /// <summary>
        /// Long running task which pings the server every 20 seconds
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields",
            Justification = "Long running task. Cleaned up by cancellation token. Need to hold a reference.")]
        private Task pingTask;

        /// <summary>
        /// Cancellation token source for controlling the long running threads
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="MumbleClient"/> class.
        /// </summary>
        /// <param name="host">Hostname of Mumble server</param>
        public MumbleClient(string host)
            : this(host, DefaultPort)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MumbleClient"/> class.
        /// </summary>
        /// <param name="host">Hostname of Mumble server</param>
        /// <param name="port">Port on which the server is listening</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", 
            Justification = "Connection will be disposed")]
        public MumbleClient(string host, int port)
            : this(new ConnectionFactory())
        {
            this.ServerInfo = new ServerInfo { HostName = host, Port = port };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MumbleClient"/> class.
        /// </summary>
        /// <param name="connectionFactory">Factory to create server connection objects</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", 
            Justification = "There are a lot of protobuf message classes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", 
            Justification = "Code generation and event handling make the event system a bit complex")]
        internal MumbleClient(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
            this.Channels = new ChannelCollection();
            this.Users = new UserCollection();
            this.SetupEvents();
        }

        /// <summary>
        /// Gets a value indicating whether we are connected
        /// </summary>
        public bool Connected { get; private set; }

        /// <summary>
        /// Gets basic Mumble server information
        /// </summary>
        public ServerInfo ServerInfo { get; private set; }

        /// <summary>
        /// Gets the channel list
        /// </summary>
        public ChannelCollection Channels { get; private set; }

        /// <summary>
        /// Gets the user list
        /// </summary>
        public UserCollection Users { get; private set; }

        /// <summary>
        /// Establish a connection with the Mumble server
        /// </summary>
        /// <param name="userName">Username of Mumble client</param>
        /// <param name="password">Password for authenticating with server</param>
        /// <returns>Empty task</returns>
        public async Task ConnectAsync(string userName, string password)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.connection = this.connectionFactory.CreateConnection(this.ServerInfo.HostName, this.ServerInfo.Port);
            await this.connection.ConnectAsync();

            await this.SendMessage<Messages.Version.Builder>((builder) =>
            {
                builder.Version_ = EncodeVersion(ClientMumbleVersion);
                builder.Release = string.Format(CultureInfo.InvariantCulture, "Mumble.NET {0}", Assembly.GetExecutingAssembly().GetName().Version);
                builder.Os = Environment.OSVersion.Platform.ToString();
                builder.OsVersion = Environment.OSVersion.VersionString;
            });

            await this.SendMessage<Authenticate.Builder>((builder) =>
            {
                builder.Username = userName;
                builder.Password = password;
                builder.Opus = true;
            });

            await this.StartLoopingTask(() => !this.Connected, this.ReadMessage);
            this.readTask = this.StartLoopingTask(() => true, this.ReadMessage);
            this.pingTask = this.StartLoopingTask(() => true, this.SendPing);
        }

        /// <summary>
        /// Disconnect from the Mumble server
        /// </summary>
        public void Disconnect()
        {
            this.Connected = false;
            this.cancellationTokenSource.Cancel();
            this.connection.Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Disconnect();
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
        private async Task SendMessage<T>(Action<T> build) where T : IBuilder, new()
        {
            var builder = new T();
            build(builder);
            await this.connection.SendMessageAsync(builder.WeakBuild(), this.cancellationTokenSource.Token);
        }

        /// <summary>
        /// Read a single task from the server and handle it
        /// </summary>
        /// <returns>Empty task</returns>
        private async Task ReadMessage()
        {
            var message = await this.connection.ReadMessageAsync(this.cancellationTokenSource.Token);
            this.messageEventHandlers[message.GetType()](this, message);
        }

        /// <summary>
        /// Send a ping message with the current timestamp and sleep for 20 seconds
        /// </summary>
        /// <returns>Empty task</returns>
        private async Task SendPing()
        {
            await this.SendMessage<Ping.Builder>(builder =>
            {
                builder.Timestamp = (uint)DateTime.UtcNow.Ticks;
            });
            await Task.Delay(TimeSpan.FromSeconds(20), this.cancellationTokenSource.Token);
        }

        /// <summary>
        /// Start a long running task which loops on a condition and executes an action
        /// </summary>
        /// <param name="condition">Condition to control loop</param>
        /// <param name="action">Action to execute</param>
        /// <returns>Task running loop</returns>
        private Task StartLoopingTask(Func<bool> condition, Func<Task> action)
        {
            return Task.Run(
                async () =>
                {
                    while (condition())
                    {
                        await action();
                    }
                }, 
                this.cancellationTokenSource.Token);
        }

        /// <summary>
        /// Wire up the self subscribing events for updating basic state
        /// </summary>
        private void SetupEvents()
        {
            this.VersionReceived += this.HandleVersionReceived;
            this.CodecVersionReceived += this.HandleCodecVersionReceived;
            this.ChannelStateReceived += (sender, args) => this.Channels.UpdateState(args.Message);
            this.ChannelRemoveReceived += (sender, args) => this.Channels.Remove(args.Message.ChannelId);
            this.UserStateReceived += (sender, args) => this.Users.UpdateState(args.Message);
            this.UserRemoveReceived += (sender, args) => this.Users.Remove(args.Message.Session);
            this.ServerSyncReceived += this.HandleServerSyncReceived;
        }

        /// <summary>
        /// Handle a Server Sync message
        /// </summary>
        /// <param name="sender">Object which sent the event</param>
        /// <param name="e">Event argument containing message</param>
        private void HandleServerSyncReceived(object sender, MessageReceivedEventArgs<ServerSync> e)
        {
            this.Connected = true;
        }

        /// <summary>
        /// Handle the codec negotiation
        /// </summary>
        /// <param name="sender">Object which sent the event</param>
        /// <param name="e">Event argument containing message</param>
        private void HandleCodecVersionReceived(object sender, MessageReceivedEventArgs<CodecVersion> e)
        {
        }

        /// <summary>
        /// Handle a Version message
        /// </summary>
        /// <param name="sender">Object which sent the event</param>
        /// <param name="e">Event argument containing message</param>
        private void HandleVersionReceived(object sender, MessageReceivedEventArgs<Messages.Version> e)
        {
            this.ServerInfo.OS = e.Message.Os;
            this.ServerInfo.OSVersion = e.Message.OsVersion;
            this.ServerInfo.Release = e.Message.Release;
            this.ServerInfo.Version = DecodeVersion(e.Message.Version_);
        }
    }
}
