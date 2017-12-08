using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace MyActorTest.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IMyActorTest : IActor
    {
        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <returns></returns>
        Task<int> GetCountAsync(CancellationToken cancellationToken);

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task SetCountAsync(int count, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the latest measurement time
        /// </summary>
        /// <param name="sensorId">Id of sensor</param>
        /// <param name="oldestTimestamp">Timestamp of the oldest measurement received from sensor. -1 if no data</param>
        /// <param name="newestTimestamp">Timestamp of the newest measurement receieved from sensor or sensorTransmission timestamp id no data reveiced.</param>
        /// <returns>true</returns>
        Task UpdateDetectionTimestamp(string sensorId, int oldestTimestamp, int newestTimestamp);
    }

    public static class UnixTimeHelper
    {
        private static readonly DateTime UnixEpoc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static int ToUnixTime(this DateTime time)
        {
            return (int)(time - UnixEpoc).TotalSeconds;
        }

        public static DateTime ToDateTime(this int time)
        {
            return UnixEpoc.AddSeconds(time);
        }
    }
}
