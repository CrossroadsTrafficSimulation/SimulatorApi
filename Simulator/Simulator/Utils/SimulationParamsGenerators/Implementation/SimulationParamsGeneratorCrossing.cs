using Simulator.Model.Dtos.Request;
using Simulator.Model.Enums;
using Simulator.Utils.SimulationParamsGenerators.Interface;
using System.Text.Json;
using RouteRequestTo = System.Collections.Generic.List<string>;

namespace Simulator.Utils.SimulationParamsGenerators.Implementation;

public class SimulationParamsGenertorCrossing : ISimulationParamsGenerator
{
    public SimulationParamsRequestTo GetSimulationParams()
    {
        #region Points
        List<PointRequestTo> points = [];
        // Left

        // =>
        var leftBottomPoint = new PointRequestTo("Left Bottom");
        points.Add(leftBottomPoint);
        // <=
        var leftTopPoint = new PointRequestTo("Left Top");
        points.Add(leftTopPoint);

        // Right
        // =>
        var rightBottomPoint = new PointRequestTo("Right Bottom");
        points.Add(rightBottomPoint);
        // <=
        var rightTopPoint = new PointRequestTo("Right Top");
        points.Add(rightTopPoint);

        var leftFlowSource = new PointRequestTo("Left Source");
        var rightFlowSource = new PointRequestTo("Right Source");
        var leftFlowDest = new PointRequestTo("Left Dest");
        var rightFlowDest = new PointRequestTo("Right Dest");

        points.AddRange([leftFlowSource, rightFlowSource, leftFlowDest, rightFlowDest]);
        #endregion

        #region Pedestrian flows
        List<PedestrianFlowRequestTo> pedestrianFlows = [];

        var leftPedestriansSource = new PedestrianFlowRequestTo([leftBottomPoint.Id, leftTopPoint.Id], new Dictionary<TimeOnly, double>
        {
            { new TimeOnly(hour: 0, minute: 0, second: 0), 2.0 },
            { new TimeOnly(hour: 1, minute: 0, second: 0), 0.0 }
        });

        var rightPedestrianSource = new PedestrianFlowRequestTo([rightBottomPoint.Id, rightTopPoint.Id], new Dictionary<TimeOnly, double>
        {
            { new TimeOnly(hour: 0, minute: 0, second: 0), 20.0 },
            { new TimeOnly(hour: 1, minute: 0, second: 0), 0.0 }
        });

        pedestrianFlows.AddRange([rightPedestrianSource, leftPedestriansSource]);
        #endregion

        #region Edges
        List<EdgeRequestTo> edges = [];

        const double speed = 10.0;
        const double distance = 100.0;

        var leftSourceToLeftBottom = new EdgeRequestTo($"{leftFlowSource.Id} => {leftBottomPoint.Id}", speed, distance, leftFlowSource.Id,
            leftBottomPoint.Id);

        var leftTopToLeftDest = new EdgeRequestTo($"{leftTopPoint.Id} => {leftFlowDest.Id}", speed, distance, leftTopPoint.Id, leftFlowDest.Id);

        var rightBottomToRightDest = new EdgeRequestTo($"{rightBottomPoint.Id} => {rightFlowDest.Id}", speed, distance, rightBottomPoint.Id,
            rightFlowDest.Id);

        var rightSourceToRightTop = new EdgeRequestTo($"{rightFlowSource.Id} => {rightTopPoint.Id}", speed, distance, rightFlowSource.Id,
            rightTopPoint.Id);

        // Change taffic lights
        var leftBottomToRightBottom = new EdgeRequestTo($"{leftBottomPoint.Id} => {rightBottomPoint.Id}", speed, distance, leftBottomPoint.Id,
            rightBottomPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 10, YellowSeconds: 2, GreenSeconds: 10, TrafficLightState.Red));

        var rightTopToLeftTop = new EdgeRequestTo($"{rightTopPoint.Id} => {leftTopPoint.Id}", speed, distance, rightTopPoint.Id, leftTopPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 10, YellowSeconds: 2, GreenSeconds: 10, TrafficLightState.Red));

        edges.AddRange([leftSourceToLeftBottom, leftTopToLeftDest, rightBottomToRightDest, rightSourceToRightTop, leftBottomToRightBottom, 
            rightTopToLeftTop]);
        #endregion

        #region Routes
        List<RouteRequestTo> routes = [];

        routes.AddRange(
            [
                [leftFlowSource.Id, leftBottomPoint.Id, rightBottomPoint.Id, rightFlowDest.Id],
                [rightFlowSource.Id, rightTopPoint.Id, leftTopPoint.Id, leftFlowDest.Id],
             ]);
        #endregion

        #region Vehicle flows
        List<FlowRequestTo> flows = [];

        flows.AddRange(
            [
                new FlowRequestTo(leftFlowSource.Id, Density: new Dictionary<TimeOnly, double>
                {
                    { new TimeOnly(hour: 0, minute: 0, second: 0), 1.0 },
                    { new TimeOnly(hour: 1, minute: 0, second: 0), 2.0 }
                }),
                new FlowRequestTo(rightFlowSource.Id, Density: new Dictionary<TimeOnly, double>
                {
                    { new TimeOnly(hour: 0, minute: 0, second: 0), 1.0 },
                    { new TimeOnly(hour: 1, minute: 0, second: 0), 2.0 }
                })
            ]);
        #endregion

        return new SimulationParamsRequestTo(points, edges, flows, routes, pedestrianFlows);
    }

    public string GetSimulationParamsJson()
    {
        var simulationParams = GetSimulationParams();

        return JsonSerializer.Serialize(simulationParams);
    }
}
