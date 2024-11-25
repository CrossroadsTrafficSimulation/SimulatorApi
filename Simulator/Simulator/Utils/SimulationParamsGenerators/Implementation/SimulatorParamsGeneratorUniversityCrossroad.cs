using Simulator.Model.Dtos.Request;
using Simulator.Model.Enums;
using Simulator.Utils.SimulationParamsGenerators.Interface;
using System.Text.Json;

using RouteRequestTo = System.Collections.Generic.List<string>;

namespace Simulator.Utils.SimulationParamsGenerators.Implementation;

public class SimulationParamsGeneratorUniversityCrossroad : ISimulationParamsGenerator
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

        // Top
        // <=
        var topLeftPoint = new PointRequestTo("Top Left");
        points.Add(topLeftPoint);
        // =>
        var topRightPoint = new PointRequestTo("Top Right");
        points.Add(topRightPoint);

        // Bottom
        // |
        var bottomLeftPoint = new PointRequestTo("Bottom Left");
        points.Add(bottomLeftPoint);
        // ^
        var bottomRightPoint = new PointRequestTo("Bottom Right");
        points.Add(bottomRightPoint);

        var leftFlowSource = new PointRequestTo("Left Source");
        var rightFlowSource = new PointRequestTo("Right Source");
        var topFlowSource = new PointRequestTo("Top Source");
        var bottomFlowSource = new PointRequestTo("Bottom Source");

        var leftFlowDest = new PointRequestTo("Left Dest");
        var rightFlowDest = new PointRequestTo("Right Dest");
        var topFlowDest = new PointRequestTo("Top Dest");
        var bottomFlowDest = new PointRequestTo("Bottom Dest");

        points.AddRange([leftFlowSource, rightFlowSource, topFlowSource, topFlowDest, bottomFlowSource, leftFlowDest, rightFlowDest,
            bottomFlowDest]);
        #endregion

        #region Pedestrian flows
        List<PedestrianFlowRequestTo> pedestrianFlows = [];

        var sourceDict = new Dictionary<TimeOnly, double>
        {
            // morning
            { new TimeOnly(hour: 0, minute: 0, second: 0), 0.1 },
            { new TimeOnly(hour: 6, minute: 0, second: 0), 1.5 },
            // pair 1 9:00
            { new TimeOnly(hour: 8, minute: 40, second: 0), 20 },
            { new TimeOnly(hour: 9, minute: 5, second: 0), 1 },
            // pair 2 10:20 - 10:35
            { new TimeOnly(hour: 10, minute: 15, second: 0), 25 },
            { new TimeOnly(hour: 10, minute: 40, second: 0), 1 },
            // pair 3 11:55 - 12:25
            { new TimeOnly(hour: 11, minute: 50, second: 0), 30 },
            { new TimeOnly(hour: 12, minute: 30, second: 0), 1 },
            // pair 4 13:45 - 14:00
            { new TimeOnly(hour: 13, minute: 40, second: 0), 35 },
            { new TimeOnly(hour: 14, minute: 5, second: 0), 1 },
            // pair 5 15:20 - 15:50
            { new TimeOnly(hour: 15, minute: 15, second: 0), 40 },
            { new TimeOnly(hour: 15, minute: 55, second: 0), 1 },                    
            // pair 6 17:10 - 17:25
            { new TimeOnly(hour: 17, minute: 5, second: 0), 35 },
            { new TimeOnly(hour: 17, minute: 30, second: 0), 1 },                    
            // pair 7 18:45 - 19:00
            { new TimeOnly(hour: 18, minute: 40, second: 0), 25 },
            { new TimeOnly(hour: 19, minute: 5, second: 0), 1 },                    
            // pair 8 20:20 - 20:40
            { new TimeOnly(hour: 20, minute: 15, second: 0), 15 },
            { new TimeOnly(hour: 20, minute: 45, second: 0), 1 },
            // night
            { new TimeOnly(hour: 22, minute: 0, second: 0), 10 },
            { new TimeOnly(hour: 22, minute: 5, second: 0), 0.1 },
        };

        var leftBottomPedestriansSource = new PedestrianFlowRequestTo([leftBottomPoint.Id, leftTopPoint.Id], sourceDict);
        var leftTopPedestriansSource = new PedestrianFlowRequestTo([leftTopPoint.Id, leftBottomPoint.Id], sourceDict);
        var bottomLeftPedestrianSource = new PedestrianFlowRequestTo([bottomLeftPoint.Id, bottomRightPoint.Id], sourceDict);
        var bottomRightPedestrianSource = new PedestrianFlowRequestTo([bottomRightPoint.Id, bottomLeftPoint.Id], sourceDict);
        var rightBottomPedestrianSource = new PedestrianFlowRequestTo([rightBottomPoint.Id, rightTopPoint.Id], sourceDict);
        var rightTopPedestrianSource = new PedestrianFlowRequestTo([rightTopPoint.Id, rightBottomPoint.Id], sourceDict);
        var topLeftPedestrianSource = new PedestrianFlowRequestTo([topLeftPoint.Id, topRightPoint.Id], sourceDict);
        var topRightPedestrianSource = new PedestrianFlowRequestTo([topRightPoint.Id, topLeftPoint.Id], sourceDict);

        pedestrianFlows.AddRange([bottomLeftPedestrianSource, topLeftPedestrianSource, rightBottomPedestrianSource, leftBottomPedestriansSource]);
        pedestrianFlows.AddRange([leftTopPedestriansSource, bottomRightPedestrianSource, rightTopPedestrianSource, topRightPedestrianSource]);
        #endregion

        #region Edges
        List<EdgeRequestTo> edges = [];

        const double speed = 10.0;
        const double distance = 100.0;

        var leftSourceToLeftBottom = new EdgeRequestTo($"{leftFlowSource.Id} => {leftBottomPoint.Id}", speed, distance, leftFlowSource.Id,
            leftBottomPoint.Id);

        var leftTopToLeftDest = new EdgeRequestTo($"{leftTopPoint.Id} => {leftFlowDest.Id}", speed, distance, leftTopPoint.Id, leftFlowDest.Id);

        var bottomLeftToBottomDest = new EdgeRequestTo($"{bottomLeftPoint.Id} => {bottomFlowDest.Id}", speed, distance, bottomLeftPoint.Id,
            bottomFlowDest.Id);

        var bottomSourceToBottomRight = new EdgeRequestTo($"{bottomFlowSource.Id} => {bottomRightPoint.Id}", speed, distance, bottomFlowSource.Id,
            bottomRightPoint.Id);

        var rightBottomToRightDest = new EdgeRequestTo($"{rightBottomPoint.Id} => {rightFlowDest.Id}", speed, distance, rightBottomPoint.Id,
            rightFlowDest.Id);

        var rightSourceToRightTop = new EdgeRequestTo($"{rightFlowSource.Id} => {rightTopPoint.Id}", speed, distance, rightFlowSource.Id,
            rightTopPoint.Id);

        var topSourceToTopLeft = new EdgeRequestTo($"{topFlowSource.Id} => {topLeftPoint.Id}", speed, distance, topFlowSource.Id,
            topLeftPoint.Id);

        var topRightToTopDest = new EdgeRequestTo($"{topRightPoint.Id} => {topFlowDest.Id}", speed, distance, topRightPoint.Id,
            topFlowDest.Id);
        #endregion

        #region Traffic lights
        // Green
        var leftBottomToBottomLeft = new EdgeRequestTo($"{leftBottomPoint.Id} => {bottomLeftPoint.Id}", speed, distance, leftBottomPoint.Id,
            bottomLeftPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 14, YellowSeconds: 2, GreenSeconds: 72, TrafficLightState.Red));

        var leftBottomToRightBottom = new EdgeRequestTo($"{leftBottomPoint.Id} => {rightBottomPoint.Id}", speed, distance, leftBottomPoint.Id,
            rightBottomPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 14, YellowSeconds: 2, GreenSeconds: 72, TrafficLightState.Red));

        var leftBottomToTopRight = new EdgeRequestTo($"{leftBottomPoint.Id} => {topRightPoint.Id}", speed, distance, leftBottomPoint.Id,
            topRightPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 14, YellowSeconds: 2, GreenSeconds: 72, TrafficLightState.Red));

        var topLeftToLeftTop = new EdgeRequestTo($"{topLeftPoint.Id} => {leftTopPoint.Id}", speed, distance, topLeftPoint.Id, leftTopPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 14, YellowSeconds: 2, GreenSeconds: 72, TrafficLightState.Red));

        var bottomRightToRightBottom = new EdgeRequestTo($"{bottomRightPoint.Id} => {rightBottomPoint.Id}", speed, distance, bottomRightPoint.Id,
            rightBottomPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 14, YellowSeconds: 2, GreenSeconds: 72, TrafficLightState.Red));

        var rightTopToTopRight = new EdgeRequestTo($"{rightTopPoint.Id} => {topRightPoint.Id}", speed, distance, rightTopPoint.Id, topRightPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 14, YellowSeconds: 2, GreenSeconds: 72, TrafficLightState.Red));

        // Red
        var rightTopToLeftTop = new EdgeRequestTo($"{rightTopPoint.Id} => {leftTopPoint.Id}", speed, distance, rightTopPoint.Id, leftTopPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 14, YellowSeconds: 2, GreenSeconds: 72, TrafficLightState.Red));

        var rightTopToBottomLeft = new EdgeRequestTo($"{rightTopPoint.Id} => {bottomLeftPoint.Id}", speed, distance, rightTopPoint.Id,
            bottomLeftPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 14, YellowSeconds: 2, GreenSeconds: 72, TrafficLightState.Red));

        var bottomRightToTopRight = new EdgeRequestTo($"{bottomRightPoint.Id} => {topRightPoint.Id}", speed, distance, bottomRightPoint.Id,
            topRightPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 14, YellowSeconds: 2, GreenSeconds: 72, TrafficLightState.Red));

        var bottomRightToLeftTop = new EdgeRequestTo($"{bottomRightPoint.Id} => {leftTopPoint.Id}", speed, distance, bottomRightPoint.Id,
            leftTopPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 14, YellowSeconds: 2, GreenSeconds: 72, TrafficLightState.Red));

        var topLeftToRightBottom = new EdgeRequestTo($"{topLeftPoint.Id} => {rightBottomPoint.Id}", speed, distance, topRightPoint.Id,
            rightBottomPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 14, YellowSeconds: 2, GreenSeconds: 72, TrafficLightState.Red));

        var topLeftToBottomLeft = new EdgeRequestTo($"{topLeftPoint.Id} => {bottomLeftPoint.Id}", speed, distance, topLeftPoint.Id,
            bottomLeftPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 14, YellowSeconds: 2, GreenSeconds: 72, TrafficLightState.Red));

        edges.AddRange([leftSourceToLeftBottom, leftTopToLeftDest, bottomLeftToBottomDest, bottomSourceToBottomRight, rightBottomToRightDest,
            rightSourceToRightTop, topSourceToTopLeft, topRightToTopDest, leftBottomToBottomLeft, leftBottomToRightBottom,
            bottomRightToRightBottom, bottomRightToLeftTop, rightTopToLeftTop, rightTopToBottomLeft, topLeftToRightBottom, topLeftToLeftTop,
            topLeftToBottomLeft, rightTopToTopRight, bottomRightToTopRight, leftBottomToTopRight]);
        #endregion

        #region Routes
        List<RouteRequestTo> routes = [];

        routes.AddRange(
            [
                [leftFlowSource.Id, leftBottomPoint.Id, bottomLeftPoint.Id, bottomFlowDest.Id],
                [leftFlowSource.Id, leftBottomPoint.Id, rightBottomPoint.Id, rightFlowDest.Id],
                [leftFlowSource.Id, leftBottomPoint.Id, topRightPoint.Id, topFlowDest.Id],
                [bottomFlowSource.Id, bottomRightPoint.Id, rightBottomPoint.Id, rightFlowDest.Id],
                [bottomFlowSource.Id, bottomRightPoint.Id, leftTopPoint.Id, leftFlowDest.Id],
                [bottomFlowSource.Id, bottomRightPoint.Id, topRightPoint.Id, topFlowDest.Id],
                [rightFlowSource.Id, rightTopPoint.Id, bottomLeftPoint.Id, bottomFlowDest.Id],
                [rightFlowSource.Id, rightTopPoint.Id, leftTopPoint.Id, leftFlowDest.Id],
                [rightFlowSource.Id, rightTopPoint.Id, topRightPoint.Id, topFlowDest.Id],
                [topFlowSource.Id, topLeftPoint.Id, rightBottomPoint.Id, rightFlowDest.Id],
                [topFlowSource.Id, topLeftPoint.Id, bottomLeftPoint.Id, bottomFlowDest.Id],
                [topFlowSource.Id, topLeftPoint.Id, leftTopPoint.Id, leftFlowDest.Id]
            ]);
        #endregion

        #region Vehicle flows
        List<FlowRequestTo> flows = [];

        flows.AddRange(
            [
                new FlowRequestTo(leftFlowSource.Id, Density: new Dictionary<TimeOnly, double>
                {
                    // morning
                    { new TimeOnly(hour: 0, minute: 0, second: 0), 0.1 },
                    { new TimeOnly(hour: 6, minute: 0, second: 0), 1.5 },
                    // pair 1 9:00
                    { new TimeOnly(hour: 8, minute: 55, second: 0), 2 },
                    { new TimeOnly(hour: 9, minute: 5, second: 0), 1 },
                    // pair 2 10:20 - 10:35
                    { new TimeOnly(hour: 10, minute: 15, second: 0), 2 },
                    { new TimeOnly(hour: 10, minute: 40, second: 0), 1 },
                    // pair 3 11:55 - 12:25
                    { new TimeOnly(hour: 11, minute: 50, second: 0), 2 },
                    { new TimeOnly(hour: 12, minute: 30, second: 0), 1 },
                    // pair 4 13:45 - 14:00
                    { new TimeOnly(hour: 13, minute: 40, second: 0), 2 },
                    { new TimeOnly(hour: 14, minute: 5, second: 0), 1 },
                    // pair 5 15:20 - 15:50
                    { new TimeOnly(hour: 15, minute: 15, second: 0), 2 },
                    { new TimeOnly(hour: 15, minute: 55, second: 0), 1 },                    
                    // pair 6 17:10 - 17:25
                    { new TimeOnly(hour: 17, minute: 5, second: 0), 2 },
                    { new TimeOnly(hour: 17, minute: 30, second: 0), 1 },                    
                    // pair 7 18:45 - 19:00
                    { new TimeOnly(hour: 18, minute: 40, second: 0), 2 },
                    { new TimeOnly(hour: 19, minute: 5, second: 0), 1 },                    
                    // pair 8 20:20 - 20:40
                    { new TimeOnly(hour: 20, minute: 15, second: 0), 2 },
                    { new TimeOnly(hour: 20, minute: 45, second: 0), 1 },
                    // night
                    { new TimeOnly(hour: 21, minute: 55, second: 0), 2 },
                    { new TimeOnly(hour: 22, minute: 5, second: 0), 0.1 },
                }),
                new FlowRequestTo(bottomFlowSource.Id, Density: new Dictionary<TimeOnly, double>
                {
                    { new TimeOnly(hour: 0, minute: 0, second: 0), 1 }
                }),
                new FlowRequestTo(rightFlowSource.Id, Density: new Dictionary<TimeOnly, double>
                {
                    { new TimeOnly(hour: 0, minute: 0, second: 0), 1 }
                }),
                new FlowRequestTo(topFlowSource.Id, Density: new Dictionary<TimeOnly, double>
                {
                    // morning
                    { new TimeOnly(hour: 0, minute: 0, second: 0), 0.1 },
                    { new TimeOnly(hour: 6, minute: 0, second: 0), 1.5 },
                    // pair 1 9:00
                    { new TimeOnly(hour: 8, minute: 55, second: 0), 2 },
                    { new TimeOnly(hour: 9, minute: 5, second: 0), 1 },
                    // pair 2 10:20 - 10:35
                    { new TimeOnly(hour: 10, minute: 15, second: 0), 2 },
                    { new TimeOnly(hour: 10, minute: 40, second: 0), 1 },
                    // pair 3 11:55 - 12:25
                    { new TimeOnly(hour: 11, minute: 50, second: 0), 2 },
                    { new TimeOnly(hour: 12, minute: 30, second: 0), 1 },
                    // pair 4 13:45 - 14:00
                    { new TimeOnly(hour: 13, minute: 40, second: 0), 2 },
                    { new TimeOnly(hour: 14, minute: 5, second: 0), 1 },
                    // pair 5 15:20 - 15:50
                    { new TimeOnly(hour: 15, minute: 15, second: 0), 2 },
                    { new TimeOnly(hour: 15, minute: 55, second: 0), 1 },                    
                    // pair 6 17:10 - 17:25
                    { new TimeOnly(hour: 17, minute: 5, second: 0), 2 },
                    { new TimeOnly(hour: 17, minute: 30, second: 0), 1 },                    
                    // pair 7 18:45 - 19:00
                    { new TimeOnly(hour: 18, minute: 40, second: 0), 2 },
                    { new TimeOnly(hour: 19, minute: 5, second: 0), 1 },                    
                    // pair 8 20:20 - 20:40
                    { new TimeOnly(hour: 20, minute: 15, second: 0), 2 },
                    { new TimeOnly(hour: 20, minute: 45, second: 0), 1 },
                    // night
                    { new TimeOnly(hour: 22, minute: 0, second: 0), 2 },
                    { new TimeOnly(hour: 22, minute: 5, second: 0), 0.1 },
                }),
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
