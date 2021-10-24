using AgentsDAL;
using AgentsDM;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Text;
using GoogleApi.Entities.Common;
using GoogleApi;
using GoogleApi.Entities.Maps.Geocoding.Address.Request;
using GoogleApi.Entities.Maps.Geocoding;
using AgentsUtils.MathUtils;
using AgentsUtils.Consts;

namespace AgentsBL
{
    public class MissionsBL
    {
        private readonly AppDataConnector appDataConnector;
        private readonly string googleApiKey;
        private readonly string googleApiDirectionWasntFoundMsg;
        public MissionsBL()
        {
            appDataConnector = new AppDataConnector();

            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile(
                MissionConsts.APP_SETTINGS, false, true).Build();

            googleApiKey = configuration.GetValue<string>(MissionConsts.GOOGLE_API_KEY);
            googleApiDirectionWasntFoundMsg = configuration.GetValue<string>(MissionConsts.GOOGLE_API_ERROR_MSG);
        }

        public BaseResponse AddMission(AddMissionRequest request)
        {
            BaseResponse response = new BaseResponse();
            request.mission.Country = MakeMissionNameStartWithCapitalLetter(request.mission);
            Mission newMission = CreateNewMission(request);

            try
            {
                appDataConnector.AddNewMission(newMission);
                response.UpdateStatusAndError(MissionConsts.SUCCESS_MSG);
            }
            catch(Exception ex)
            {
                // possible - write error to log

                response.UpdateStatusAndError(MissionConsts.FAIL_MSG, ex.Message);
            }

            return response;
        }
        public List<Mission> GetAllMissions()
        {
            List<Mission> allMissions = null;

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
        public List<string> GetCountriesByIsolation()
        {
            List<string> isolatedCountries = null;

            try
            {
                List<Mission> isolatedAgentsMissions = appDataConnector
                    .GetAllMissionsWithIsolatedAgents(MissionConsts.ISOLATED_AGENT_MISSIONS_NUMBER);

                var isolatedAgentsMissionsGroupByCountry = isolatedAgentsMissions.GroupBy(m => m.Country);
                int maxIsolationDegree = isolatedAgentsMissionsGroupByCountry.Max(g => g.Count());
                var mostIsolatedCountries = isolatedAgentsMissionsGroupByCountry.Where(g => g.Count() == maxIsolationDegree);

                isolatedCountries = new List<string>();

                foreach (var country in mostIsolatedCountries)
                {
                    isolatedCountries.Add(country.FirstOrDefault().Country);
                }
            }
            catch (Exception ex)
            {
                // possible - write error to log
            }

            return isolatedCountries;
        }
        public ClosestMissionResponse FindClosestMission(ClosestMissionRequest missionRequest)
        {
            ClosestMissionResponse response = new ClosestMissionResponse();
            double minDistance = double.MaxValue;

            try
            {
                var allMissions = appDataConnector.GetAllMissions();
                Coordinate missionRequestCoordinates = null;

                if (missionRequest.AddressOrCoordinates.Any(c => char.IsLetter(c)))
                {
                    missionRequestCoordinates = GetCoordinateFromGeocodeAPI(missionRequest.AddressOrCoordinates); 
                }
                else
                {
                    string[] coordinates = missionRequest.AddressOrCoordinates.Split(',');

                    missionRequestCoordinates = new Coordinate(Convert.ToDouble(coordinates[MissionConsts.LAT_INDX]),
                        Convert.ToDouble(coordinates[MissionConsts.LON_INDX]));
                }

                foreach (var mission in allMissions)
                {
                    double tmpDistance = GetAdressesDistanceInMeters(mission.Address, missionRequestCoordinates);

                    if (tmpDistance < minDistance)
                    {
                        minDistance = tmpDistance;
                        response.Mission = mission;
                    }
                }

                if(response.Mission == null)
                {
                    throw new Exception(googleApiDirectionWasntFoundMsg);
                }

                response.UpdateStatusAndError(MissionConsts.SUCCESS_MSG);
            }
            catch (Exception ex)
            {
                // possible - write error to log

                response.UpdateStatusAndError(MissionConsts.FAIL_MSG, ex.Message);
                response.Mission = null;
            }

            return response;
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
        private double GetAdressesDistanceInMeters(string missionAddress, Coordinate coordinate)
        {
            double distance = double.MaxValue;
            Coordinate missionCoordinates = GetCoordinateFromGeocodeAPI(missionAddress);

            if (missionCoordinates != null)
            {
                distance = Calculations.GetDistanceFromLatLongInKM(missionCoordinates, coordinate); 
            }

            return distance;
        }
        private Coordinate GetCoordinateFromGeocodeAPI(string address)
        {
            Coordinate coordinate = null;

            try
            {
                AddressGeocodeRequest geocodeRequest = new AddressGeocodeRequest()
                {
                    Address = address,
                    Key = googleApiKey
                };

                GeocodeResponse geocodeResponse = GoogleMaps.AddressGeocode.Query(geocodeRequest);
                coordinate = geocodeResponse.Results.FirstOrDefault().Geometry.Location;
            }
            catch (Exception ex)
            {
                // possible - write error to log
            }

            return coordinate;
        }
        private string MakeMissionNameStartWithCapitalLetter(Mission mission)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var letter in mission.Country)
            {
                if(sb.Length == MissionConsts.EMPTY)
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
