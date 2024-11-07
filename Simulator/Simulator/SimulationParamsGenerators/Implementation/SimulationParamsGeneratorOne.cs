using Simulator.Model.Dtos.Request;
using Simulator.Model.Enums;
using System.Text.Json;
using TestBench.SimulationParamsGenerators.Interface;

using RouteRequestTo = System.Collections.Generic.List<string>;

namespace Simulator.SimulationParamsGenerators.Implementation;

public class SimulationParamsGeneratorOne : ISimulationParamsGenerator
{
    public SimulationParamsRequestTo GetSimulationParams()
    {
        // Points
        List<PointRequestTo> points = [];
        // Left
        var leftPedestrianSource = new Dictionary<TimeOnly, double>
        {
            { new TimeOnly(hour: 1, minute: 2, second: 3), 4.0 },
            { new(hour: 10, minute: 20, second: 30), 40.0 }
        };
        // =>
        var leftBottomPoint = new PointRequestTo("Left Bottom", leftPedestrianSource);
        points.Add(leftBottomPoint);
        // <=
        var leftTopPoint = new PointRequestTo("Left Top", leftPedestrianSource);
        points.Add(leftTopPoint);

        // Right
        var rightPedestrianSource = new Dictionary<TimeOnly, double>
        {
            { new(hour: 2, minute: 3, second: 4), 5.0 },
            { new(hour: 20, minute: 30, second: 40), 50.0 }
        };
        // =>
        var rightBottomPoint = new PointRequestTo("Right Bottom", rightPedestrianSource);
        points.Add(rightBottomPoint);
        // <=
        var rightTopPoint = new PointRequestTo("Right Top", rightPedestrianSource);
        points.Add(rightTopPoint);

        // Top
        var topPedestrainSource = new Dictionary<TimeOnly, double>
        {
            { new(hour: 3, minute: 4, second: 5), 6.0 },
            { new(hour: 6, minute: 20, second: 10), 60.0 }
        };
        // <=
        var topLeftPoint = new PointRequestTo("Top Left", topPedestrainSource);
        points.Add(topLeftPoint);
        // =>
        var topRightPoint = new PointRequestTo("Top Right", topPedestrainSource);
        points.Add(topRightPoint);

        // Bottom
        var bottomPedestriansSource = new Dictionary<TimeOnly, double>
        {
            { new(hour: 4, minute: 3, second: 2), 12.0 },
            { new(hour: 16, minute: 30, second: 20), 20.0 }
        };
        // |
        var bottomLeftPoint = new PointRequestTo("Bottom Left", bottomPedestriansSource);
        points.Add(bottomLeftPoint);
        // ^
        var bottomRightPoint = new PointRequestTo("Bottom Right", bottomPedestriansSource);
        points.Add(bottomRightPoint);

        var leftFlowSource = new PointRequestTo("Left Source");
        var rightFlowSource = new PointRequestTo("Right Source");
        var topFlowSourceLeft = new PointRequestTo("Top Source Left");
        var topFlowSourceRight = new PointRequestTo("Top Source Right");
        var bottomFlowSource = new PointRequestTo("Bottom Source");
        var leftFlowDest = new PointRequestTo("Left Dest");
        var rightFlowDest = new PointRequestTo("Right Dest");
        var bottomFlowDest = new PointRequestTo("Bottom Dest");

        points.AddRange([leftFlowSource, rightFlowSource, topFlowSourceLeft, topFlowSourceRight, bottomFlowSource, leftFlowDest, rightFlowDest, 
            bottomFlowDest]);
        // Points end

        // Edges
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
        
        var topSourceLeftToTopLeft = new EdgeRequestTo($"{topFlowSourceLeft.Id} => {topLeftPoint.Id}", speed, distance, topFlowSourceLeft.Id, 
            topLeftPoint.Id);
        
        var topSourceRightToTopRight = new EdgeRequestTo($"{topFlowSourceRight.Id} => {topRightPoint.Id}", speed, distance, topFlowSourceRight.Id, 
            topRightPoint.Id);

        // Change taffic lights
        var leftBottomToBottomLeft = new EdgeRequestTo($"{leftBottomPoint.Id} => {bottomLeftPoint.Id}", speed, distance, leftBottomPoint.Id, 
            bottomLeftPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 10, YellowSeconds: 2, GreenSeconds: 20, TrafficLightState.Red));

        var leftBottomToRightBottom = new EdgeRequestTo($"{leftBottomPoint.Id} => {rightBottomPoint.Id}", speed, distance, leftBottomPoint.Id, 
            rightBottomPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 10, YellowSeconds: 2, GreenSeconds: 20, TrafficLightState.Red));

        var bottomRightToRightBottom = new EdgeRequestTo($"{bottomRightPoint.Id} => {rightBottomPoint.Id}", speed, distance, bottomRightPoint.Id, 
            rightBottomPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 10, YellowSeconds: 2, GreenSeconds: 20, TrafficLightState.Red));

        var bottomRightToLeftTop = new EdgeRequestTo($"{bottomRightPoint.Id} => {leftTopPoint.Id}", speed, distance, bottomRightPoint.Id, 
            leftTopPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 10, YellowSeconds: 2, GreenSeconds: 20, TrafficLightState.Red));

        var rightTopToLeftTop = new EdgeRequestTo($"{rightTopPoint.Id} => {leftTopPoint.Id}", speed, distance, rightTopPoint.Id, leftTopPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 10, YellowSeconds: 2, GreenSeconds: 20, TrafficLightState.Red));

        var rightTopToBottomLeft = new EdgeRequestTo($"{rightTopPoint.Id} => {bottomLeftPoint.Id}", speed, distance, rightTopPoint.Id, 
            bottomLeftPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 10, YellowSeconds: 2, GreenSeconds: 20, TrafficLightState.Red));

        var topRightToRightBottom = new EdgeRequestTo($"{topRightPoint.Id} => {rightBottomPoint.Id}", speed, distance, topRightPoint.Id, 
            rightBottomPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 10, YellowSeconds: 2, GreenSeconds: 20, TrafficLightState.Green));

        var topLeftToLeftTop = new EdgeRequestTo($"{topLeftPoint.Id} => {leftTopPoint.Id}", speed, distance, topLeftPoint.Id, leftTopPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 10, YellowSeconds: 2, GreenSeconds: 20, TrafficLightState.Green));

        var topLeftToBottomLeft = new EdgeRequestTo($"{topLeftPoint.Id} => {bottomLeftPoint.Id}", speed, distance, topLeftPoint.Id, 
            bottomLeftPoint.Id,
            new TrafficLightRequestTo(RedSeconds: 10, YellowSeconds: 2, GreenSeconds: 20, TrafficLightState.Green));

        edges.AddRange([leftSourceToLeftBottom, leftTopToLeftDest, bottomLeftToBottomDest, bottomSourceToBottomRight, rightBottomToRightDest,
            rightSourceToRightTop, topSourceLeftToTopLeft, topSourceRightToTopRight, leftBottomToBottomLeft, leftBottomToRightBottom,
            bottomRightToRightBottom, bottomRightToLeftTop, rightTopToLeftTop, rightTopToBottomLeft, topRightToRightBottom, topLeftToLeftTop,
            topLeftToBottomLeft]);
        // Edges end

        // Routes
        List<RouteRequestTo> routes = [];

        routes.AddRange(
            [
                [leftFlowSource.Id, leftBottomPoint.Id, bottomLeftPoint.Id, bottomFlowDest.Id],
                [leftFlowSource.Id, leftBottomPoint.Id, rightBottomPoint.Id, rightFlowDest.Id],
                [bottomFlowSource.Id, bottomRightPoint.Id, rightBottomPoint.Id, rightFlowDest.Id],
                [bottomFlowSource.Id, bottomRightPoint.Id, leftTopPoint.Id, leftFlowDest.Id],
                [rightFlowSource.Id, rightTopPoint.Id, bottomLeftPoint.Id, bottomFlowDest.Id],
                [rightFlowSource.Id, rightTopPoint.Id, leftTopPoint.Id, leftFlowDest.Id],
                [topFlowSourceRight.Id, topRightPoint.Id, rightBottomPoint.Id, rightFlowDest.Id],
                [topFlowSourceLeft.Id, topLeftPoint.Id, bottomLeftPoint.Id, bottomFlowDest.Id],
                [topFlowSourceLeft.Id, topLeftPoint.Id, rightTopPoint.Id, rightTopPoint.Id]
            ]);
        // Routes end

        // Flows
        List<FlowRequestTo> flows = [];

        flows.AddRange(
            [
                new FlowRequestTo(leftFlowSource.Id, Density: new Dictionary<TimeOnly, double>
                {
                    { new TimeOnly(hour: 0, minute: 0, second: 0), 2.0 },
                    { new TimeOnly(hour: 12, minute: 0, second: 0), 4.0 }
                }),
                new FlowRequestTo(bottomFlowSource.Id, Density: new Dictionary<TimeOnly, double>
                {
                    { new TimeOnly(hour: 0, minute: 0, second: 0), 1.0 },
                    { new TimeOnly(hour: 13, minute: 0, second: 0), 5.0 }
                }),
                new FlowRequestTo(rightFlowSource.Id, Density: new Dictionary<TimeOnly, double>
                {
                    { new TimeOnly(hour: 0, minute: 0, second: 0), 8.0 },
                    { new TimeOnly(hour: 14, minute: 0, second: 0), 2.0 }
                }),
                new FlowRequestTo(topFlowSourceLeft.Id, Density: new Dictionary<TimeOnly, double>
                {
                    { new(hour: 0, minute: 0, second: 0), 4.0 }
                }),
                new FlowRequestTo(topFlowSourceRight.Id, Density: new Dictionary<TimeOnly, double>
                {
                    { new(hour: 0, minute: 0, second: 0), 5.0 }
                })
            ]);
        // Flows end

        return new SimulationParamsRequestTo(points, edges, flows, routes);
    }

    public string GetSimulationParamsJson()
    {
        var simulationParams = GetSimulationParams();

        return JsonSerializer.Serialize(simulationParams);
    }
}
