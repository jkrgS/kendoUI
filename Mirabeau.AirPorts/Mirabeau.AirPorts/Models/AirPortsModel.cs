using System.Collections.Generic;

namespace Mirabeau.AirPorts.Models
{
    public class AirPortsModel
    {
        public string iata { get; set; }
        public decimal lon { get; set; }
        public string iso { get; set; } 
        public int status { get; set; } 
        public string name { get; set; } 
        public string continent { get; set; } 
        public string type { get; set; } 
        public decimal lat { get; set; } 
        public string size { get; set; } 

        //public class AirPortsList
        //{
        //    public IList<AirPortsModel> AirPorts;
        //} 

    }

    public class DistanceModel
    {
        public string distanceFrom { get; set; }
        public string distanceTo { get; set; }
        public string distance { get; set; }
    }
}