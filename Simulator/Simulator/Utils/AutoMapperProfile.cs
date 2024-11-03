using DTO = Simulator.Model.RequestDto;
using PROC = Simulator.Processes;

using AutoMapper;

namespace Simulator.Utils;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<DTO.Edge, PROC.Edge>();
        CreateMap<DTO.Flow, PROC.Flow>();
        CreateMap<DTO.Point, PROC.Point>();
        CreateMap<DTO.TrafficLight, PROC.TrafficLight>();
    }
}

