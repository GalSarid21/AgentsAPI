using AgentsDM;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AgentsDAL
{
    public class AppDataConnector
    {
        public List<Mission> GetAllMissions()
        {
            // AppDataLoader.Missions = DB table
            // Here suppose to be DB connection part

            // Use ConcurrencyBag for thread safe assurance
            return AppDataLoader.Missions.ToList();
        }
        public List<Mission> GetAllMissionsWithIsolatedAgents(int isolatedAgentThreshold)
        {
            // The linq code simulate quering the DB
            List<Mission> missions = AppDataLoader.Missions.GroupBy(m => m.Agent)
                                                            .Where(a => a.Count() == isolatedAgentThreshold)
                                                            .SelectMany(g => g).ToList();

            return missions;
        }
        public void AddNewMission(Mission newMission)
        {
            AppDataLoader.Missions.Add(newMission);
        }
    }
}
