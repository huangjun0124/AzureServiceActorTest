using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using MyStatefulService.Interface;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using MyActorTest.Interfaces;

namespace MyWebAPIFrontEnd.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<string>> GetAsync()
        {
            ICounter counter =
                ServiceProxy.Create<ICounter>(new Uri("fabric:/MyApplication/MyStatefulService"), new ServicePartitionKey(0));

            long count = await counter.GetCountAsync();
            CounterClass cc = await counter.GetCounterClassAsync();
            return new string[] { $"count: {count.ToString()}  Jun's example : Name:{cc.Name} Date:{cc.CreateDate.ToString("MMdd")} Count:{cc.Count}" };
        }

        // GET api/values/5   http://localhost:8878/api/values/id
        [HttpGet("{id}")]
        public string Get(int id)
        {
            TestActor();
            return $"id={id}";
        }

        private async void TestActor()
        {
            var actorId = new ActorId("testsite");
            var siteActor = ActorProxy.Create<IMyActorTest>(actorId, new Uri("fabric:/MyApplication/MyActorTestActorService"));
            await siteActor.UpdateDetectionTimestamp("testsensor", new DateTime(2017, 7, 9).ToUnixTime(),
                new DateTime(2017, 8, 8).ToUnixTime());
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
