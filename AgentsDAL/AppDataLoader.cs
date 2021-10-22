using AgentsDM;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace AgentsDAL
{
    public sealed class AppDataLoader
    {
        private static readonly Lazy<ConcurrentBag<Mission>> lazy = new Lazy<ConcurrentBag<Mission>>(() => CreateDataFromExternalFile());
        public static ConcurrentBag<Mission> Missions { get { return lazy.Value; } }

        private static ConcurrentBag<Mission> CreateDataFromExternalFile()
        {
            string jsonPath = Path.Combine(Environment.CurrentDirectory, "ExternalData.json");
            List<Mission> missions = JsonConvert.DeserializeObject<List<Mission>>(File.ReadAllText(jsonPath));
            return new ConcurrentBag<Mission>(missions);
        }

    }
}
