using Simulator.Model.Enums;

namespace Simulator.Model.Entites;

public class TrafficLight()
{
    public Dictionary<TrafficLightState, int> States { get; set; } = [];
    public TrafficLightState CurrentState { get; set; }
    public TrafficLightState InitialState { get; set; }
    public TrafficLightState PreviousState { get; set; }
    public Edge Edge { get; set; } = null!;
    public string EdgeId { get; set; } = null!;
    public int RedSeconds => States[TrafficLightState.Red];
    public int YellowSeconds => States[TrafficLightState.Yellow];
    public int GreenSeconds => States[TrafficLightState.Green];

    public TrafficLight(Dictionary<TrafficLightState, int> states, TrafficLightState currentState, Edge edge) : this()
    {
        States = states;
        CurrentState = currentState;
        Edge = edge;
        EdgeId = edge.Id;
    }

    public void SwitchState()
    {
        if (CurrentState == TrafficLightState.Green)
        {
            PreviousState = TrafficLightState.Green;
            CurrentState = TrafficLightState.Yellow;
        }
        else if (CurrentState == TrafficLightState.Red)
        {
            PreviousState = TrafficLightState.Red;
            CurrentState = TrafficLightState.Yellow;
        }
        else
        {
            CurrentState = PreviousState == TrafficLightState.Green ? TrafficLightState.Red : TrafficLightState.Green;
            PreviousState = TrafficLightState.Yellow;
        }
    }

    public int SecondsTillRed(int currentTimeSeconds)
    {
        currentTimeSeconds++;
        int totalCycleTime = States[TrafficLightState.Green] + States[TrafficLightState.Yellow] * 2 + States[TrafficLightState.Red];
        currentTimeSeconds %= totalCycleTime;

        if (CurrentState == TrafficLightState.Red)
        {
            return 0;
        }

        switch (InitialState)
        {
            // g 20 y 2 r 10 y 2
            case TrafficLightState.Green:
                if (CurrentState == TrafficLightState.Yellow && PreviousState == TrafficLightState.Green)
                {
                    return GreenSeconds + YellowSeconds - currentTimeSeconds;
                }
                if (CurrentState == TrafficLightState.Green)
                {
                    return GreenSeconds - currentTimeSeconds + YellowSeconds;
                }
                if (CurrentState == TrafficLightState.Yellow && PreviousState == TrafficLightState.Red)
                {
                    return YellowSeconds * 2 + RedSeconds + GreenSeconds - currentTimeSeconds + GreenSeconds + YellowSeconds;
                }
                break;
            // r 10 y 2 g 20 y 2
            case TrafficLightState.Red:
                if (CurrentState == TrafficLightState.Yellow && PreviousState == TrafficLightState.Green)
                {
                    return RedSeconds + YellowSeconds * 2 + GreenSeconds - currentTimeSeconds;
                }
                else if (CurrentState == TrafficLightState.Green)
                {
                    return RedSeconds + YellowSeconds + GreenSeconds - currentTimeSeconds + YellowSeconds;
                }
                else if (CurrentState == TrafficLightState.Yellow && PreviousState == TrafficLightState.Red)
                {
                    return RedSeconds + YellowSeconds - currentTimeSeconds + GreenSeconds + YellowSeconds;
                }
                break;
            default:
                return 0;
        }

        return 0;
    }

    public int SecondsTillGreen(int currentTimeSeconds)
    {
        int totalCycleTime = States[TrafficLightState.Green] + States[TrafficLightState.Yellow] * 2 + States[TrafficLightState.Red];
        currentTimeSeconds %= totalCycleTime;

        if (CurrentState == TrafficLightState.Green)
        {
            return 0;
        }

        switch (InitialState)
        {
            // g y r y
            case TrafficLightState.Green:
                if (CurrentState == TrafficLightState.Yellow && PreviousState == TrafficLightState.Green)
                {
                    return GreenSeconds + YellowSeconds - currentTimeSeconds + RedSeconds + YellowSeconds;
                }
                if (CurrentState == TrafficLightState.Red)
                {
                    return RedSeconds + YellowSeconds + GreenSeconds - currentTimeSeconds + YellowSeconds;
                }
                if (CurrentState == TrafficLightState.Yellow && PreviousState == TrafficLightState.Red)
                {
                    return YellowSeconds * 2 + RedSeconds + GreenSeconds - currentTimeSeconds;
                }
                break;
            // r y g y
            case TrafficLightState.Red:
                if (CurrentState == TrafficLightState.Yellow && PreviousState == TrafficLightState.Green)
                {
                    return RedSeconds + YellowSeconds + GreenSeconds + YellowSeconds - currentTimeSeconds + RedSeconds + YellowSeconds;
                }
                else if (CurrentState == TrafficLightState.Red)
                {
                    return RedSeconds - currentTimeSeconds + YellowSeconds;
                }
                else if (CurrentState == TrafficLightState.Yellow && PreviousState == TrafficLightState.Red)
                {
                    return RedSeconds + YellowSeconds - currentTimeSeconds;
                }
                break;
            default:
                return 0;
        }

        return 0;
    }
}
