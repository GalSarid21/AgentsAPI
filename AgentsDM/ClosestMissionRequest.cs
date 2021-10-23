namespace AgentsDM
{
    public class ClosestMissionRequest
    {
        /// <summary>
        /// Adress must contain character, coordinated are latitude-longitude pair seperated by comma.
        /// </summary>
        /// <example>Address: Shalem 12, Ramat Gan. Coordinates: 46.5610058, 26.9098054</example>
        public string AddressOrCoordinates { get; set; }
    }
}
