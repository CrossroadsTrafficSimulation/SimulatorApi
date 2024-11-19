using AutoMapper;
using Simulator.Model.Dtos.Request;
using Simulator.Model.Entites;
using Simulator.Model.Enums;

namespace Simulator.Utils;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        _ = CreateMap<EdgeRequestTo, Edge>()
            .ForMember(dst => dst.SpeedLimit, opt => opt.MapFrom(src => src.Speed));
        _ = CreateMap<PointRequestTo, Point>();
        _ = CreateMap<TrafficLightRequestTo, TrafficLight>()
            .ForMember(dst => dst.CurrentState, opt => opt.MapFrom(src => src.InitialState))
            .ForMember(dst => dst.PreviousState, opt => opt.MapFrom(stc => stc.InitialState))
            .ForMember(dst => dst.States, opt => opt.MapFrom(src =>
                new Dictionary<TrafficLightState, int>
                {
                    { TrafficLightState.Red, src.RedSeconds },
                    { TrafficLightState.Yellow, src.YellowSeconds },
                    { TrafficLightState.Green, src.GreenSeconds }
                }
            ));
        _ = CreateMap<FlowRequestTo, Flow>();
    }
}

