using System;

namespace AgentsDM
{
    public class Mission
    {
        /// <summary>
        /// Agent's ID number.
        /// </summary>
        /// /// <example>007</example>
        public string Agent { get; set; }
        /// <summary>
        /// A certain country name.
        /// </summary>
        /// <example>Israel</example>
        public string Country { get; set; }
        /// <summary>
        /// An address constructed from street name, house number and city.
        /// </summary>
        /// <example>Rua Roberto Simonsen 122, Sao Paulo</example>
        public string Address { get; set; }
        /// <summary>
        /// A date in formate YYYY-MM-DDTHH:MM:SS
        /// </summary>
        /// <example>1996-11-29T17:32:41</example>
        public DateTime Date { get; set; }
    }
}
