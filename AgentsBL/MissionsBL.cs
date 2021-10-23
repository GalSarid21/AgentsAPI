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

namespace AgentsBL
{
    public class MissionsBL
    {
        #region Consts
        const string SUCCESS_MSG = "SUCCEEDED";
        const string FAIL_MSG = "FAILED";
        const string APP_SETTINGS = "appsettings.json";
        const string GOOGLE_API_KEY = "GoogleApiKey";
        const string GOOGLE_API_ERROR_MSG = "GoogleApiErrorMsg";

        // set INFINITY to largest long possible
        const int ISOLATED_AGENT_MISSIONS_NUMBER = 1;
        const int ISOLATED_COUNTRY_INITIAL_DEGREE = 1;
        const int EMPTY = 0;
        const int EARTH_RADIUS_IN_KM = 6371;
        const int LAT_INDX = 0;
        const int LON_INDX = 1;
        #endregion

        private readonly AppDataConnector appDataConnector;
        private readonly string googleApiKey;
        private readonly string googleApiDirectionWasntFoundMsg;
        public MissionsBL()
        {
            appDataConnector = new AppDataConnector();

            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile(
                APP_SETTINGS, false, true).Build();

            googleApiKey = configuration.GetValue<string>(GOOGLE_API_KEY);
            googleApiDirectionWasntFoundMsg = configuration.GetValue<string>(GOOGLE_API_ERROR_MSG);
        }

        public BaseResponse AddMission(AddMissionRequest request)
        {
            BaseResponse response = new BaseResponse();
            request.mission.Country = MakeMissionNameStartWithCapitalLetter(request.mission);
            Mission newMission = CreateNewMission(request);

            try
            {
                appDataConnector.AddNewMission(newMission);
                response.UpdateStatusAndError(SUCCESS_MSG);
            }
            catch(Exception ex)
            {
                // possible - write error to log

                response.UpdateStatusAndError(FAIL_MSG, ex.Message);
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
        public IEnumerable<KeyValuePair<string,int>> GetCountriesByIsolation()
        {
            IEnumerable <KeyValuePair<string, int>> mostIsolatedCountries  = null;
            Dictionary<string, int> isolatedCountries = null;

            try
            {
                var isolatedAgentsMissions = appDataConnector.GetAllMissionsWithIsolatedAgents(ISOLATED_AGENT_MISSIONS_NUMBER);
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
        public ClosestMissionResponse FindClosestMission(ClosestMissionRequest missionRequest)
        {
            ClosestMissionResponse response = new ClosestMissionResponse();
            double minDistance = double.MaxValue;

            try
            {
                List<Mission> allMissions = appDataConnector.GetAllMissions();
                Coordinate missionRequestCoordinates = null;

                if (missionRequest.AddressOrCoordinates.Any(c => char.IsLetter(c)))
                {
                    missionRequestCoordinates = GetCoordinateFromGeocodeAPI(missionRequest.AddressOrCoordinates); 
                }
                else
                {
                    string[] coordinates = missionRequest.AddressOrCoordinates.Split(',');

                    missionRequestCoordinates = new Coordinate(Convert.ToDouble(coordinates[LAT_INDX]),
                        Convert.ToDouble(coordinates[LON_INDX]));
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

                response.UpdateStatusAndError(SUCCESS_MSG);
            }
            catch (Exception ex)
            {
                // possible - write error to log

                response.UpdateStatusAndError(FAIL_MSG, ex.Message);
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
                distance = GetDistanceFromLatLongInKM(missionCoordinates, coordinate); 
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
        private double GetDistanceFromLatLongInKM(Coordinate dstCoordination, Coordinate originCoordination)
        {
            double latDiffInRad = ConvertDegreesToRadians(dstCoordination.Latitude - originCoordination.Latitude);
            double lonDiffInRad = ConvertDegreesToRadians(dstCoordination.Longitude - originCoordination.Longitude);
            double haversine = Math.Sin(latDiffInRad / 2) * Math.Sin(latDiffInRad / 2) +
                               Math.Cos(ConvertDegreesToRadians(originCoordination.Latitude)) * 
                               Math.Cos(ConvertDegreesToRadians(dstCoordination.Latitude)) *
                               Math.Sin(lonDiffInRad / 2) * Math.Sin(lonDiffInRad / 2);

            return 2 *EARTH_RADIUS_IN_KM* Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));
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
        private double ConvertDegreesToRadians(double input)
        {
            return input * (Math.PI / 180);
        }
        #endregion
    }
}
