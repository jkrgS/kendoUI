using Mirabeau.AirPorts.Models;
using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Ajax.Utilities;

namespace Mirabeau.AirPorts.Controllers
{
    public class AirPortsController : Controller
    {
        //private IEnumerable airPortsDef;

        // GET: AirPorts
        //[AsyncTimeout(2000)]
        //[Route("Airports/List")]
        public ViewResult List(string sortOrder, string currentFilter, string searchString, string distanceFrom, string distanceTo, int? test, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CountryDescParm = String.IsNullOrEmpty(sortOrder) ? "country_desc" : "";
            ViewBag.CountryParm = sortOrder == "country" ? "country_desc" : "country";
            ViewBag.CurrentFilter = searchString;
            ViewBag.DistanceFrom = distanceFrom;
            ViewBag.DistanceTo = distanceTo;


            if (!String.IsNullOrEmpty(distanceFrom) & !String.IsNullOrEmpty(distanceTo))
            {
                return Index(distanceFrom, distanceTo);
            }

            // jSON Get
            using (HttpClient client = new HttpClient())
            {
                    string jsonUrl = WebConfigurationManager.AppSettings["jsonURL"];
                    var Url = new Uri(jsonUrl);

                    MediaTypeWithQualityHeaderValue contentType =
                        new MediaTypeWithQualityHeaderValue("application/json");
                    client.DefaultRequestHeaders.Accept.Add(contentType);

                    HttpResponseMessage response = client.GetAsync(Url).Result;

                    string data = response.Content.ReadAsStringAsync().Result;
                    var airPorts = JsonConvert.DeserializeObject<List<AirPortsModel>>(data);

                    var airPortsDef = from t in airPorts
                        where t.continent.Equals("EU") && t.type.Equals("airport")
                        select t;
                

                // Searching
                if (searchString != null)
                {
                    page = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                if (!String.IsNullOrEmpty(searchString))
                {
                    airPortsDef = airPortsDef.Where(s => s.iata.Contains(searchString));
                    
                    if (airPortsDef.Count() == 0)
                    {
                        airPortsDef = from t in airPorts
                                      where t.continent.Equals("EU") && t.type.Equals("airport")
                                      select t;
                    }
                }

                ViewBag.CurrentFilter = airPortsDef;
                // Country ordering
                switch (sortOrder)
                {
                    case "country":
                        airPortsDef = airPortsDef.OrderBy(s => s.iso);
                        break;
                    case "country_desc":
                        airPortsDef = airPortsDef.OrderByDescending(s => s.iso);
                        break;
                    default: 
                        airPortsDef = airPortsDef.OrderBy(s => s.iso);
                        break;
                }
                // Pages params
                //int pageSize = 10;
                //int pageNumber = (page ?? 1);

                //Return the view
                return View(airPortsDef);
            }
        }

        public ViewResult Index(string distanceFrom, string distanceTo)
        {
            //Url api bulding
            var apiURL = WebConfigurationManager.AppSettings["airportDistURL"];
            var token = "?user_key=d805e84363494ca03b9b52d5a505c4d1";
            apiURL = apiURL + distanceFrom + "/" + distanceTo + token;

            string distance = "";
            string unit = "";

            XmlDocument distancesApi = new XmlDocument();

            
            //Browsers headers for xml reading
            WebClient client = new WebClient();
            client.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1";
            client.Headers["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            string data = client.DownloadString(apiURL);

            //XML file loding
            distancesApi.LoadXml(data);
            
                //Distance value
                XmlNodeList nodeList = distancesApi.GetElementsByTagName("*");

                for (int i = 0; i < nodeList.Count; i++)
                {
                    if (nodeList[i].Attributes["success"].Value == "true")
                    {
                        distance = nodeList[i].Attributes["distance"].Value;
                        unit = nodeList[i].Attributes["units"].Value;
                    }
                }


                if (String.IsNullOrEmpty(distance))
                {
                    distanceFrom = "";
                    distanceTo = "";
                    distance = "";
                }


            DistanceModel distanceModel = new DistanceModel
            {
                distanceFrom = distanceFrom,
                distanceTo = distanceTo,
                distance = distance + " " + unit
            };

            return View("Distance", distanceModel);
        }
    }
}