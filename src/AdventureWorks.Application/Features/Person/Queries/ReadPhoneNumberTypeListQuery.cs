using AdventureWorks.Models.Features.Person;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Queries;

public sealed class ReadPhoneNumberTypeListQuery : IRequest<List<PhoneNumberTypeModel>>
{
}
