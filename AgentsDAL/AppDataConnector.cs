using AgentsDM;
using System.Collections.Concurrent;

namespace AgentsDAL
{
    public class AppDataConnector
    {
        public ConcurrentBag<Mission> GetAllMissions()
        {
            // AppDataLoader.Missions = DB table
            // Here suppose to be DB connection part

            // Use ConcurrencyBag for thread safe assurance
            return AppDataLoader.Missions;
        }
        public void AddNewMission(Mission newMission)
        {
            AppDataLoader.Missions.Add(newMission);
        }
    }
}
