using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using MyActorTest.Interfaces;

namespace MyActorTest
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class MyActorTest : Actor, IMyActorTest
    {
        /// <summary>
        /// Initializes a new instance of MyActorTest
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public MyActorTest(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        private int _processibleUntil = 0;
        private const int SensorReportTimeout = 2 * 60;

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            StateManager.TryAddStateAsync("latestUpdateTime", 0);
            StateManager.TryAddStateAsync("sensorUpdateTimes", new Dictionary<string, int>());
            StateManager.TryAddStateAsync("processibleUntil", _processibleUntil);

            return this.StateManager.TryAddStateAsync("count", 0);
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <returns></returns>
        Task<int> IMyActorTest.GetCountAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<int>("count", cancellationToken);
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task IMyActorTest.SetCountAsync(int count, CancellationToken cancellationToken)
        {
            // Requests are not guaranteed to be processed in order nor at most once.
            // The update function here verifies that the incoming count is greater than the current count to preserve order.
            return this.StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value, cancellationToken);
        }

        /// <summary>
        /// Updates the latest measurement time
        /// </summary>
        /// <param name="sensorId">Id of sensor</param>
        /// <param name="oldestTimestamp">Timestamp of the oldest measurement received from sensor. -1 if no data</param>
        /// <param name="newestTimestamp">Timestamp of the newest measurement receieved from sensor or sensorTransmission timestamp id no data reveiced.</param>
        /// <returns>true</returns>
        public async Task UpdateDetectionTimestamp(string sensorId, int oldestTimestamp, int newestTimestamp)
        {
            try
            {
                //int firstTime = await StateManager.GetStateAsync<int>("firstUpdateTime");
                int firstTime = await StateManager.GetStateAsync<int>("latestUpdateTime");

                await StateManager.AddOrUpdateStateAsync("latestUpdateTime", newestTimestamp, (key, value) => newestTimestamp > value ? newestTimestamp : value);
                await StateManager.AddOrUpdateStateAsync("firstUpdateTime", newestTimestamp.ToDateTime(), (key, value) => value);
                var firstUpdateTime = StateManager.TryGetStateAsync<DateTime>("firstUpdateTime").Result;
                firstTime = await StateManager.GetStateAsync<int>("latestUpdateTime");

                var sensorUpdateTimes = await StateManager.GetStateAsync<IDictionary<string, int>>("sensorUpdateTimes");
                if (sensorUpdateTimes.ContainsKey(sensorId))
                {
                    if (sensorUpdateTimes[sensorId] < newestTimestamp)
                        sensorUpdateTimes[sensorId] = newestTimestamp;
                }
                else
                {
                    sensorUpdateTimes[sensorId] = newestTimestamp;
                }

                // We give SensorReportTimeout s to receive all messages in order. TODO: remove this
                foreach (var kv in sensorUpdateTimes.ToList())
                {
                    if (kv.Value + SensorReportTimeout < oldestTimestamp)
                        sensorUpdateTimes.Remove(kv);
                }

                StateManager.AddOrUpdateStateAsync("sensorUpdateTimes", sensorUpdateTimes,
                    (key, value) => sensorUpdateTimes);

                var latestCommonUpdateTime = sensorUpdateTimes.Values.Min();
                if (latestCommonUpdateTime > _processibleUntil)
                    _processibleUntil = latestCommonUpdateTime;

                StateManager.SetStateAsync("processibleUntil", _processibleUntil);


            }
            catch (Exception err)
            {
                throw new Exception("Error while updating latest detection timestamp", err);
            }

        }
    }
}
