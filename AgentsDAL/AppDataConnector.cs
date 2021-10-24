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
        public IEnumerable<IGrouping<string,Mission>> GetMostIsolatedCountries(int isolatedAgentThreshold)
        {
            // The linq code simulate quering the DB
            var missions = AppDataLoader.Missions.GroupBy(m => m.Agent)
                                                 .Where(a => a.Count() == isolatedAgentThreshold)
                                                 .SelectMany(g => g)
                                                 .ToList()
                                                 .GroupBy(m => m.Country);

            int maxIsolationDegree = missions.Max(g => g.Count());
            var mostIsolatedCountries = missions.Where(g => g.Count() == maxIsolationDegree);

            return mostIsolatedCountries;
        }
        public void AddNewMission(Mission newMission)
        {
            AppDataLoader.Missions.Add(newMission);
        }
    }
}
