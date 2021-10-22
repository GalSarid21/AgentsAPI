using AgentsBL;
using AgentsDM;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AgentsAPI.Controllers
{
    public class MissionsController : ControllerBase
    {
        private readonly MissionsBL missionsBL;
        public MissionsController()
        {
            missionsBL = new MissionsBL();
        }

        [HttpPost]
        [Route("mission")]
        public AddMissionResponse AddMission(AddMissionRequest request)
        {
            AddMissionResponse response = new AddMissionResponse();
            response = missionsBL.AddMission(request);

            return response;
        }

        [HttpPost]
        [Route("find-closest")]
        public Mission FindClosestMission(ClosestMissionRequest request)
        {
            return missionsBL.FindClosestMission(request);
        }

        [HttpGet]
        [Route("countries-by-isolation")]
        public IEnumerable<KeyValuePair<string, int>> CountriesByIsolation()
        {
            return missionsBL.GetCountriesByIsolation();
        }

        [HttpGet]
        [Route("all-missions")]
        public ConcurrentBag<Mission> AllMissions()
        {
            return missionsBL.GetAllMissions();
        }
    }
}
