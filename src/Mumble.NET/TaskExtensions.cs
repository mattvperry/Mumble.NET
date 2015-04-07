//-----------------------------------------------------------------------
// <copyright file="TaskExtensions.cs" company="Matt Perry">
//     Copyright (c) Matt Perry. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mumble
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Class containing extensions for dealing with async tasks
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Create a new token source which combines the given token and a new timeout token
        /// </summary>
        /// <param name="cancellationToken">Original token</param>
        /// <param name="timeout">Timeout to add in seconds</param>
        /// <returns>New cancellation token source</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Caller is responsible for disposing the source")]
        public static CancellationTokenSource AddTimeout(this CancellationToken cancellationToken, int timeout)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (timeout != Timeout.Infinite)
            {
                cts.CancelAfter(TimeSpan.FromSeconds(timeout));
            }

            return cts;
        }

        /// <summary>
        /// Handles a task which can be cancelled because of a timeout
        /// </summary>
        /// <typeparam name="T">Type of task result</typeparam>
        /// <param name="task">Task to catch cancel exception from</param>
        /// <param name="cancellationToken">Timeout token</param>
        /// <param name="memberName">Name of calling method</param>
        /// <returns>The result of the given task</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed",
            Justification = "CallerMemberName needs a default parameter")]
        public static async Task<T> HandleTimeout<T>(this Task<T> task, CancellationToken cancellationToken, [CallerMemberName] string memberName = "")
        {
            var genericTask = (Task)task;
            await genericTask.HandleTimeout(cancellationToken, memberName).ConfigureAwait(false);
            return await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Handles a task which can be cancelled because of a timeout
        /// </summary>
        /// <param name="task">Task to catch cancel exception from</param>
        /// <param name="cancellationToken">Timeout token</param>
        /// <param name="memberName">Name of calling method</param>
        /// <returns>Empty task</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed",
            Justification = "CallerMemberName needs a default parameter")]
        public static async Task HandleTimeout(this Task task, CancellationToken cancellationToken, [CallerMemberName] string memberName = "")
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == cancellationToken)
                {
                    throw new TimeoutException(string.Format(CultureInfo.InvariantCulture, "{0} timed out.", memberName), ex);
                }

                throw;
            }
        }
    }
}
