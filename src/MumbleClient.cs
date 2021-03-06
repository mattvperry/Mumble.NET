﻿//-----------------------------------------------------------------------
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
        /// Session id associated with the client user
        /// </summary>
        private uint session;

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
            : this(host, port, new ConnectionFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MumbleClient"/> class.
        /// </summary>
        /// <param name="host">Hostname of Mumble server</param>
        /// <param name="port">Port on which the server is listening</param>
        /// <param name="connectionFactory">Factory to create server connection objects</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", 
            Justification = "There are a lot of protobuf message classes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", 
            Justification = "Code generation and event handling make the event system a bit complex")]
        internal MumbleClient(string host, int port, IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
            this.ServerInfo = new ServerInfo { HostName = host, Port = port };
            this.Channels = new ChannelCollection(this);
            this.Users = new UserCollection(this);
            this.SetupEvents();
        }

        /// <summary>
        /// Event fired when any message is received
        /// </summary>
        internal event EventHandler<MessageReceivedEventArgs<IMessage>> MessageReceived;

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
        /// Gets the user associated with this client
        /// </summary>
        public User ClientUser 
        { 
            get
            {
                return this.Users[this.session];
            }
        }

        /// <summary>
        /// Establish a connection with the Mumble server
        /// </summary>
        /// <param name="userName">Username of Mumble client</param>
        /// <param name="password">Password for authenticating with server</param>
        /// <returns>Empty task</returns>
        public async Task ConnectAsync(string userName, string password)
        {
            if (this.Connected)
            {
                return;
            }

            this.cancellationTokenSource = new CancellationTokenSource();
            this.connection = this.connectionFactory.CreateConnection(this.ServerInfo.HostName, this.ServerInfo.Port);
            await this.connection.ConnectAsync().ConfigureAwait(false);

            await this.SendMessageAsync(new Messages.Version.Builder
            {
                Version_ = ClientMumbleVersion.EncodeVersion(),
                Release = string.Format(CultureInfo.InvariantCulture, "Mumble.NET {0}", Assembly.GetExecutingAssembly().GetName().Version),
                Os = Environment.OSVersion.Platform.ToString(),
                OsVersion = Environment.OSVersion.VersionString,
            }).ConfigureAwait(false);

            await this.SendMessageAsync(new Authenticate.Builder
            {
                Username = userName,
                Password = password,
                Opus = true,
            }).ConfigureAwait(false);

            await this.StartLoopingTaskAsync(() => !this.Connected, this.ReadMessageAsync).ConfigureAwait(false);
            this.readTask = this.StartLoopingTaskAsync(() => true, this.ReadMessageAsync);
            this.pingTask = this.StartLoopingTaskAsync(() => true, this.SendPingAsync);
        }

        /// <summary>
        /// Disconnect from the Mumble server
        /// </summary>
        public void Disconnect()
        {
            if (!this.Connected)
            {
                return;
            }

            this.Connected = false;
            this.cancellationTokenSource.Cancel();
            this.connection.Dispose();
            this.Users.Clear();
            this.Channels.Clear();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Disconnect();
        }

        /// <summary>
        /// Sends a text message to a user
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="target">Target user</param>
        /// <returns>Empty task</returns>
        public async Task SendTextMessageAsync(string message, User target)
        {
            await this.SendTextMessageAsync(message, builder => builder.AddSession(target.Id)).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a text message to a channel
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="target">Target channel</param>
        /// <returns>Empty task</returns>
        public async Task SendTextMessageAsync(string message, Channel target)
        {
            await this.SendTextMessageAsync(message, builder => builder.AddChannelId(target.Id)).ConfigureAwait(false);
        }

        /// <summary>
        /// Build a protobuf message and send it
        /// </summary>
        /// <param name="builder">Builder to craft message</param>
        /// <returns>Empty task</returns>
        internal async Task SendMessageAsync(IBuilder builder)
        {
            await this.connection.SendMessageAsync(builder.WeakBuild(), this.cancellationTokenSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a message and wait for a response
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="builder">Builder to craft message</param>
        /// <param name="filter">Filter for response</param>
        /// <returns>The response or null</returns>
        internal async Task<T> SendMessageWithResponseAsync<T>(IBuilder builder, Predicate<T> filter) where T : class, IMessage
        {
            using (var cts = this.cancellationTokenSource.Token.AddTimeout(5))
            {
                var success = this.WaitForMessageAsync<T>(filter, cts.Token);
                var denied = this.WaitForMessageAsync<PermissionDenied>(cts.Token);

                await this.SendMessageAsync(builder).ConfigureAwait(false);

                var result = await Task.WhenAny(success, denied).ConfigureAwait(false);
                cts.Cancel();

                if (result.IsCanceled)
                {
                    throw new TimeoutException();
                }
                else if (result == success)
                {
                    return await success;
                }
                else if (result == denied)
                {
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        /// <summary>
        /// Waits for a message to be received
        /// </summary>
        /// <typeparam name="T">Type of message to wait for</typeparam>
        /// <param name="token">Cancellation token</param>
        /// <returns>The message or null</returns>
        private Task<T> WaitForMessageAsync<T>(CancellationToken token) where T : class, IMessage
        {
            return this.WaitForMessageAsync<T>(m => true, token);
        }

        /// <summary>
        /// Waits for a message to be received
        /// </summary>
        /// <typeparam name="T">Type of message to wait for</typeparam>
        /// <param name="filter">Filter for the message</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The message or null</returns>
        private Task<T> WaitForMessageAsync<T>(Predicate<T> filter, CancellationToken token) where T : class, IMessage
        {
            var tcs = new TaskCompletionSource<T>();

            var handler = new EventHandler<MessageReceivedEventArgs<IMessage>>((sender, args) =>
            {
                if (args.Message is T && filter((T)args.Message))
                {
                    tcs.TrySetResult((T)args.Message);
                }
            });

            this.MessageReceived += handler;
            token.Register(
                () => 
                {
                    this.MessageReceived -= handler;
                    tcs.TrySetCanceled();
                }, 
                useSynchronizationContext: false);

            return tcs.Task;
        }

        /// <summary>
        /// Sends a text message to a target
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="addTarget">Action which adds a target to the message</param>
        /// <returns>Empty task</returns>
        private async Task SendTextMessageAsync(string message, Func<TextMessage.Builder, TextMessage.Builder> addTarget)
        {
            await this.SendMessageAsync(addTarget(new TextMessage.Builder { Actor = this.ClientUser.Id, Message = message })).ConfigureAwait(false);
        }

        /// <summary>
        /// Read a single task from the server and handle it
        /// </summary>
        /// <returns>Empty task</returns>
        private async Task ReadMessageAsync()
        {
            var message = await this.connection.ReadMessageAsync(this.cancellationTokenSource.Token).ConfigureAwait(false);
            this.messageEventHandlers[message.GetType()](this, message);
            this.MessageReceived.RaiseEvent(this, message);
        }

        /// <summary>
        /// Send a ping message with the current timestamp and sleep for 20 seconds
        /// </summary>
        /// <returns>Empty task</returns>
        private async Task SendPingAsync()
        {
            await this.SendMessageAsync(new Ping.Builder
            {
                Timestamp = (uint)DateTime.UtcNow.Ticks,
            }).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(20), this.cancellationTokenSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Start a long running task which loops on a condition and executes an action
        /// </summary>
        /// <param name="condition">Condition to control loop</param>
        /// <param name="action">Action to execute</param>
        /// <returns>Task running loop</returns>
        private Task StartLoopingTaskAsync(Func<bool> condition, Func<Task> action)
        {
            return Task.Run(
                async () =>
                {
                    while (condition())
                    {
                        await action().ConfigureAwait(false);
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
            this.ServerSyncReceived += this.HandleServerSyncReceived;
        }

        /// <summary>
        /// Handle a Server Sync message
        /// </summary>
        /// <param name="sender">Object which sent the event</param>
        /// <param name="e">Event argument containing message</param>
        private void HandleServerSyncReceived(object sender, MessageReceivedEventArgs<ServerSync> e)
        {
            this.session = e.Message.Session;
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
            this.ServerInfo.Version = e.Message.Version_.DecodeVersion();
        }
    }
}
