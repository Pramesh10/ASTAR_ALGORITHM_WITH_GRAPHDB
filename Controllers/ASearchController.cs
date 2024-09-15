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
            await using var session = _driver.AsyncSession();

            var result = await session.RunAsync(
                    "MATCH (location:Station) " +
                    "RETURN id(location) as locationId, location.name as locationName, location.latitude as latitude, location.longitude as longitude"
                );

            var records = await result.ToListAsync();
            var locations = new List<Location>();

            foreach (var record in records)
            {
                var location = new Location
                {
                    LocationId = record["locationId"].As<long>(),
                    LocationName = record["locationName"].As<string>(),
                    Latitude = record["latitude"].As<double>(),
                    Longitude = record["longitude"].As<double>()
                };
                locations.Add(location);
            }
            return Ok(locations);
        }



        [HttpGet("GetAllConnection")]
        public async Task<IActionResult> GetAllConnection()
        {            
            await using var session = _driver.AsyncSession();

            var resultConnection = await session.RunAsync("MATCH (:Station) -[S:CONNECTION]-> (:Station) RETURN S");

            var connectionRecords = await resultConnection.ToListAsync();

            return Ok(connectionRecords);
        }

        [HttpGet("GetPath")]
        public async Task<IActionResult> GetPath([FromQuery] string startLocation , string endLocation)
        {
            await using var session = _driver.AsyncSession();

           

            var result = await session.RunAsync(@"
        MATCH (source:Station {name: $startLocation}), (target:Station {name: $endLocation})
        CALL gds.shortestPath.astar.stream('myGraph', {
            sourceNode: source,
            targetNode: target,
            latitudeProperty: 'latitude',
            longitudeProperty: 'longitude',
            relationshipWeightProperty: 'distance'
        })
        YIELD index, sourceNode, targetNode, totalCost, nodeIds, costs, path
        RETURN
            index,
            gds.util.asNode(sourceNode).name AS sourceNodeName,
            gds.util.asNode(targetNode).name AS targetNodeName,
            totalCost,
            [nodeId IN nodeIds ] AS nodeNames,
            costs,
            nodes(path) as path
        ORDER BY index", new { startLocation, endLocation });

            var records = await result.ToListAsync();


            var locations = await result.ToListAsync(record => record["nodeNames"].As<dynamic>());

            

            return Ok(locations);
        }

        
    }
}

public class Location
{
    public long LocationId { get; set; }
    public string LocationName { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
