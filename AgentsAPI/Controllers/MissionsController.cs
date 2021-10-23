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
        public BaseResponse AddMission(AddMissionRequest request)
        {
            return missionsBL.AddMission(request);
        }

        [HttpPost]
        [Route("find-closest")]
        public ClosestMissionResponse FindClosestMission(ClosestMissionRequest request)
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
        public List<Mission> AllMissions()
        {
            return missionsBL.GetAllMissions();
        }
    }
}
