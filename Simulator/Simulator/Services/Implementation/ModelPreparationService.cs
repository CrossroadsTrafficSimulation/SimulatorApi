using AutoMapper;
using Simulator.Model.Dtos;
using Simulator.Model.Dtos.Request;
using Simulator.Model.Entites;
using Simulator.Services.Interface;

using EdgeWithTrafficLight = (
    System.Collections.Generic.List<Simulator.Model.Entites.Edge> Edges,
    System.Collections.Generic.List<Simulator.Model.Entites.TrafficLight> TrafficLights
);
using Route = System.Collections.Generic.List<Simulator.Model.Entites.Point>;
using RouteRequestTo = System.Collections.Generic.List<string>;

namespace Simulator.Services.Implementation;

public class ModelPreparationService(ILogger<ModelPreparationService> logger, IMapper mapper) : IModelPreparationService
{
    private readonly Dictionary<string, Point> _points = [];

    public SimulationModel GetSimulationModel(SimulationParamsRequestTo simulationParams)
    {
        logger.LogInformation("Received: {param}", simulationParams);

        logger.LogInformation("Points processing");
        FillPoints(simulationParams.Points);
        logger.LogInformation("Points processed");

        logger.LogInformation("Pedestrian flows processing");
        CreatePedestrianFlows(simulationParams.PedestrianFlows);
        logger.LogInformation("Pedestrian flows processed");

        logger.LogInformation("Flows processing");
        var flows = CreateVehicleFlows(simulationParams.Flows);
        logger.LogInformation("Flows processed");

        logger.LogInformation("Edges & traffic lights processing");
        var (edges, trafficLights) = CreateEdgesWithTrafficLights(simulationParams.Edges);
        logger.LogInformation("Edges & traffic lights processed");

        logger.LogInformation("Routes processing");
        var routes = CreateRoutes(simulationParams.Routes);
        logger.LogInformation("Routes processed");

        return new SimulationModel(_points, edges, flows, trafficLights, routes);
    }

    private void FillPoints(List<PointRequestTo> points)
    {
        foreach (var pointRequest in points)
        {
            var point = mapper.Map<Point>(pointRequest);

            _points.Add(point.Id, point);
        }
    }

    private void CreatePedestrianFlows(List<PedestrianFlowRequestTo> pedestrianFlows)
    {
        foreach (var pedestrianFlow in pedestrianFlows)
        {
            pedestrianFlow.PointIds.ForEach(id => _points[id].PedestriansFlow = pedestrianFlow.Density);
        }
    }

    private List<Flow> CreateVehicleFlows(List<FlowRequestTo> flows)
    {
        List<Flow> res = [];

        foreach (var flowRequest in flows)
        {
            var flow = mapper.Map<Flow>(flowRequest);

            flow.Point = _points[flowRequest.PointId];
            flow.Point.Flow = flow;

            res.Add(flow);
        }

        return res;
    }

    private List<Route> CreateRoutes(List<RouteRequestTo> routes)
    {
        List<Route> res = [];

        foreach (var routeRequest in routes)
        {
            res.Add([]);
            foreach (var routePoint in routeRequest)
            {
                res[^1].Add(_points[routePoint]);
            }
        }

        return res;
    }

    private EdgeWithTrafficLight CreateEdgesWithTrafficLights(List<EdgeRequestTo> edges)
    {
        List<Edge> resEdges = [];
        List<TrafficLight> resTrafficLights = [];

        foreach (var edgeRequest in edges)
        {
            var edge = mapper.Map<Edge>(edgeRequest);
            if (edge.TrafficLight is not null)
            {
                edge.TrafficLight.EdgeId = edge.Id;
                edge.TrafficLight.Edge = edge;
                resTrafficLights.Add(edge.TrafficLight);
            }
            resEdges.Add(edge);
            edge.StartPoint = _points[edgeRequest.StartPointId];
            edge.StartPoint.Edges.Add(edge);
            edge.EndPoint = _points[edgeRequest.EndPointId];
            _points[edgeRequest.EndPointId].Edges.Add(edge);
        }

        return new EdgeWithTrafficLight(resEdges, resTrafficLights);
    }
}
