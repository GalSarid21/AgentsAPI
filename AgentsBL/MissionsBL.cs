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

namespace AgentsBL
{
    public class MissionsBL
    {
        #region Consts
        const string SUCCESS_MSG = "SUCCEEDED";
        const string FAIL_MSG = "FAILED";

        // set INFINITY to largest int possible
        const int INFINITY = 2147483647;
        #endregion

        private readonly AppDataConnector appDataConnector;
        private readonly string googleApiKey;
        public MissionsBL()
        {
            appDataConnector = new AppDataConnector();

            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile(
                "appsettings.json", false, true).Build();

            googleApiKey = configuration.GetValue<string>("GoogleAliKey");
        }

        public AddMissionResponse AddMission(AddMissionRequest request)
        {
            AddMissionResponse response = new AddMissionResponse();
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
        public IsolatedCounties GetCountriesByIsolation()
        {
            IsolatedCounties mostIsolatedCountries = null;
            List<string> mostIsolatedCountryNames = null;
            int mostIsolatedAgentsSeen = 0;

            try
            {
                ConcurrentBag<Mission> allMissions = appDataConnector.GetAllMissions();
                var isolatedAgents = allMissions.GroupBy(m => m.Agent)
                                                .Where(a => a.Count() == 1);

                var missionsByCountries = allMissions.GroupBy(m => m.Country.ToLower());

                foreach (var country in missionsByCountries)
                {
                    int numOfIsolatedAgents = CountIsolatedAgents(country, isolatedAgents);
                    if (numOfIsolatedAgents > mostIsolatedAgentsSeen)
                    {
                        mostIsolatedAgentsSeen = numOfIsolatedAgents;
                        mostIsolatedCountryNames = new List<string>()
                        {
                            country.FirstOrDefault().Country.ToUpper()
                        };
                    }

                    else if(numOfIsolatedAgents == mostIsolatedAgentsSeen)
                    {
                        mostIsolatedCountryNames?.Add(country.FirstOrDefault().Country.ToUpper());
                    }
                }

                mostIsolatedCountries = CreateNewIsolatedCountries(mostIsolatedCountryNames, mostIsolatedAgentsSeen);
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
                    int tmpDistance = GetAdressesDistance(mission.Address, missionRequest.Address).Value;

                    if (tmpDistance < minDistance.Value)
                    {
                        minDistance.Value = tmpDistance;
                        closestMission = mission;
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
        private int CountIsolatedAgents(IGrouping<string,Mission> country, IEnumerable<IGrouping<string,Mission>> IsolatedAgents)
        {
            int numOfIsolatedAgent = 0;

            foreach (var mission in country)
            {
                if(IsolatedAgents.Any(x => x.Any(y => y.Agent == mission.Agent)))
                {
                    numOfIsolatedAgent++;
                }
            }

            return numOfIsolatedAgent;
        }
        private IsolatedCounties CreateNewIsolatedCountries(List<string> countries, int isolationDegree)
        {
            IsolatedCounties isolatedCountries = new IsolatedCounties()
            {
                Countries = countries,
                IsolationDegree = isolationDegree
            };

            return isolatedCountries;
        }
        private Distance GetAdressesDistance(string missionAddress, string requestAddress)
        {
            DirectionsRequest directionRequest = new DirectionsRequest();

            directionRequest.Key = googleApiKey;
            directionRequest.Origin = new LocationEx(new Address(missionAddress));
            directionRequest.Destination = new LocationEx(new Address(requestAddress));

            var response = GoogleMaps.Directions.Query(directionRequest);

            return response.Routes.First().Legs.First().Distance;
        }
        #endregion
    }
}
