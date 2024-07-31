using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using System;
using static System.Collections.Specialized.BitVector32;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ASTAR_ALGORITHM_WITH_GRAPHDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ASearchController : ControllerBase
    {

        private readonly IDriver _driver;
       
        private string uri = "bolt://localhost:7687";
        private string user = "pramesh";
        private string password = "pramesh@123";
        public ASearchController(IDriver driver)
        {

            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        }

        [HttpGet("GetAllLocation")]

        public async Task<IActionResult> GetAllLocation()
        {
            string message = "hello pramesh";
            await using var session = _driver.AsyncSession();



            var result = await session.RunAsync("MATCH (location :Station) RETURN location");

            // Fetch all results into a list
            var locations = await result.ToListAsync(record => record["location"].As<dynamic>());

            // Print data in a list format
            //Console.WriteLine("Locations in List Format:");
            //foreach (var location in locations)
            //{
            //    Console.WriteLine(location.Id);
            //    var l = location.Properties[1];
            //    Console.WriteLine(location);
            //}


            return Ok(locations);
        }

        


        [HttpGet("GetPath")]
        public async Task<IActionResult> GetPath([FromQuery] string startLocation , string endLocation)
        {

            await using var session = _driver.AsyncSession();

            var result = await session.RunAsync("MATCH (source:Station {name: 'Kings Cross'}), (target:Station {name: 'Kentish Town'})\r\nCALL gds.shortestPath.astar.stream('myGraph', {\r\n    sourceNode: source,\r\n    targetNode: target,\r\n    latitudeProperty: 'latitude',\r\n    longitudeProperty: 'longitude',\r\n    relationshipWeightProperty: 'distance'\r\n})\r\nYIELD index, sourceNode, targetNode, totalCost, nodeIds, costs, path\r\nRETURN\r\n    index,\r\n    gds.util.asNode(sourceNode).name AS sourceNodeName,\r\n    gds.util.asNode(targetNode).name AS targetNodeName,\r\n    totalCost,\r\n    [nodeId IN nodeIds | gds.util.asNode(nodeId).name] AS nodeNames,\r\n    costs,\r\n    nodes(path) as path\r\nORDER BY index");


            var locations = await result.ToListAsync(record => record["nodeNames"].As<dynamic>());

            return Ok(locations);
        }

        
    }
}
