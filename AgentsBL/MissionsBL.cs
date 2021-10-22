using AgentsDAL;
using AgentsDM;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GoogleApi.Entities.Maps.Directions.Request;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Common;
using GoogleApi;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace AgentsBL
{
    public class MissionsBL
    {
        #region Consts
        const string SUCCESS_MSG = "SUCCEEDED";
        const string FAIL_MSG = "FAILED";
        const string APP_SETTINGS = "appsettings.json";
        const string GOOGLE_API_KEY = "GoogleAliKey";

        // set INFINITY to largest int possible
        const int INFINITY = 2147483647;
        const int ISOLATED_AGENT_MISSIONS_NUMBER = 1;
        const int ISOLATED_COUNTRY_INITIAL_DEGREE = 1;
        const int EMPTY = 0;
        #endregion

        private readonly AppDataConnector appDataConnector;
        private readonly string googleApiKey;
        public MissionsBL()
        {
            appDataConnector = new AppDataConnector();

            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile(
                APP_SETTINGS, false, true).Build();

            googleApiKey = configuration.GetValue<string>(GOOGLE_API_KEY);
        }

        public AddMissionResponse AddMission(AddMissionRequest request)
        {
            AddMissionResponse response = new AddMissionResponse();
            request.mission.Country = MakeMissionNameStartWithCapitalLetter(request.mission);
            Mission newMission = CreateNewMission(request);

            try
            {
                appDataConnector.AddNewMission(newMission);
                response.Status = SUCCESS_MSG;
            }
            catch(Exception ex)
            {
                // possible - write error to log

                response.Status = FAIL_MSG;
                response.Error = ex.Message;
            }

            return response;
        }
        public ConcurrentBag<Mission> GetAllMissions()
        {
            ConcurrentBag<Mission> allMissions = null;

            try
            {
                allMissions = appDataConnector.GetAllMissions();
            }
            catch (Exception ex)
            {
                // possible - write error to log
            }

            return allMissions;
        }
        public IEnumerable<KeyValuePair<string,int>> GetCountriesByIsolation()
        {
            IEnumerable <KeyValuePair<string, int>> mostIsolatedCountries  = null;
            Dictionary<string, int> isolatedCountries = null;

            try
            {
                ConcurrentBag<Mission> allMissions = appDataConnector.GetAllMissions();
                var isolatedAgentsMissions = allMissions.GroupBy(m => m.Agent)
                                                        .Where(a => a.Count() == ISOLATED_AGENT_MISSIONS_NUMBER);

                isolatedCountries = new Dictionary<string, int>();

                foreach (var mission in isolatedAgentsMissions)
                {
                    if (isolatedCountries.Keys.Contains(mission.FirstOrDefault().Country))
                    {
                        isolatedCountries[mission.FirstOrDefault().Country]++;
                    }
                    else
                    {
                        isolatedCountries.Add(mission.FirstOrDefault().Country, ISOLATED_COUNTRY_INITIAL_DEGREE);
                    }
                }

                int maxIsolationDegree = isolatedCountries.Max(c => c.Value);
                mostIsolatedCountries = isolatedCountries.Where(c => c.Value == maxIsolationDegree);
            }
            catch (Exception ex)
            {
                // possible - write error to log
            }

            return mostIsolatedCountries;
        }
        public Mission FindClosestMission(ClosestMissionRequest missionRequest)
        {
            Mission closestMission = null;
            Distance minDistance = new Distance()
            {
                Value = INFINITY
            };

            try
            {
                ConcurrentBag<Mission> allMissions = appDataConnector.GetAllMissions();

                foreach (var mission in allMissions)
                {
                    Distance tmpDistance = GetAdressesDistance(mission.Address, missionRequest.Address);

                    if (tmpDistance != null)
                    {
                        if (tmpDistance.Value < minDistance.Value)
                        {
                            minDistance.Value = tmpDistance.Value;
                            closestMission = mission;
                        } 
                    }
                }

            }
            catch (Exception ex)
            {
                // possible - write error to log
            }

            return closestMission;
        }

        #region Private Methods
        private Mission CreateNewMission(AddMissionRequest request)
        {
            Mission newMission = new Mission()
            {
                Agent = request.mission.Agent,
                Country = request.mission.Country,
                Address = request.mission.Address,
                Date = request.mission.Date
            };

            return newMission;
        }
        private Distance GetAdressesDistance(string missionAddress, string requestAddress)
        {
            Distance distance = null;

            try
            {
                DirectionsRequest directionRequest = new DirectionsRequest();

                directionRequest.Key = googleApiKey;
                directionRequest.Origin = new LocationEx(new Address(missionAddress));
                directionRequest.Destination = new LocationEx(new Address(requestAddress));

                var response = GoogleMaps.Directions.Query(directionRequest);
                distance = response.Routes.First().Legs.First().Distance;
            }
            catch (Exception ex)
            {
                // possible - write error to log
            }

            return distance;
        }
        private string MakeMissionNameStartWithCapitalLetter(Mission mission)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var letter in mission.Country)
            {
                if(sb.Length.Equals(EMPTY))
                {
                    sb.Append(char.ToUpper(letter));
                }
                else
                {
                    sb.Append(char.ToLower(letter));
                }
            }

            return sb.ToString();
        }
        #endregion
    }
}
