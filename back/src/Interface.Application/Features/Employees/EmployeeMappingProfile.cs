using AutoMapper;
using Interface.Domain.Employees;

namespace Interface.Application.Features.Employees;

public sealed class EmployeeMappingProfile : Profile
{
    public EmployeeMappingProfile()
    {
        CreateMap<PayComponent, PayComponentDto>();
        CreateMap<PayComponentDto, PayComponent>()
            .ConstructUsing(dto => new PayComponent(dto.Name, dto.Amount));

        CreateMap<Employee, EmployeeDto>();
    }
}
