using AdventureWorks.Models.Features.Person;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Queries;

public sealed class ReadPhoneNumberTypeQuery : IRequest<PhoneNumberTypeModel>
{
    public required int Id { get; set; } = int.MinValue;
}
