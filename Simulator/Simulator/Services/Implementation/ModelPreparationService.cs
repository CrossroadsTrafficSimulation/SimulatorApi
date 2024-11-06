using AutoMapper;
using Simulator.Model.Dtos;
using Simulator.Model.Dtos.Request;
using Simulator.Model.Entites;
using Simulator.Services.Interface;

using Route = System.Collections.Generic.List<Simulator.Model.Entites.Point>;

namespace Simulator.Services.Implementation;

public class ModelPreparationService(ILogger<ModelPreparationService> logger, IMapper mapper) : IModelPreparationService
{
    public SimulationModel GetSimulationModel(SimulationParamsRequestTo simulationParams)
    {
        logger.LogInformation("Received: {param}", simulationParams);

        Dictionary<string, Point> points = [];
        List<Edge> edges = [];
        List<Flow> flows = [];
        List<TrafficLight> trafficLights = [];
        List<Route> routes = [];

        foreach (var pointRequest in simulationParams.Points)
        {
            var point = mapper.Map<Point>(pointRequest);

            points.Add(point.Id, point);
        }
        logger.LogInformation("Points processed");

        foreach (var flowRequest in simulationParams.Flows)
        {
            var flow = mapper.Map<Flow>(flowRequest);

            flow.Point = points[flowRequest.PointId];
            flow.Point.Flow = flow;

            flows.Add(flow);
        }
        logger.LogInformation("Flows processed");

        foreach (var edgeRequest in simulationParams.Edges)
        {
            var edge = mapper.Map<Edge>(edgeRequest);
            if (edge.TrafficLight is not null)
            {
                edge.TrafficLight.EdgeId = edge.Id;
                edge.TrafficLight.Edge = edge;
                trafficLights.Add(edge.TrafficLight);
            }
            edges.Add(edge);
            edge.StartPoint = points[edgeRequest.StartPointId];
            edge.StartPoint.Edges.Add(edge);
            edge.EndPoint = points[edgeRequest.EndPointId];
            points[edgeRequest.EndPointId].Edges.Add(edge);
        }
        logger.LogInformation("Edges & traffic lights processed");

        foreach (var routeRequest in simulationParams.Routes)
        {
            routes.Add([]);
            foreach (var routePoint in routeRequest)
            {
                routes[^1].Add(points[routePoint]);
            }
        }
        logger.LogInformation("Routes processed");

        return new SimulationModel(points, edges, flows, trafficLights, routes);
    }
}
