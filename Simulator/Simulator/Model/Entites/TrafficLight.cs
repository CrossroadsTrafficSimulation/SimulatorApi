using Simulator.Model.Enums;

namespace Simulator.Model.Entites;

public class TrafficLight()
{
    public Dictionary<TrafficLightState, int> States { get; set; } = [];
    public TrafficLightState CurrentState { get; set; }
    public TrafficLightState PreviousState {  get; set; } 
    public Edge Edge { get; set; } = null!;
    public string EdgeId { get; set; } = null!;

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
}
