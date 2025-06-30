using Azure;
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Common.Helpers.Fakes;

/// <summary>
/// Fake Response
/// </summary>
/// <typeparam name="T"></typeparam>
[ExcludeFromCodeCoverage]
public class FakeResponse<T>(T value) : Response<T>
{
    public override T Value => value;

    /// <summary>
    /// Not Implemented
    /// </summary>
    /// <returns></returns>
    public override Response GetRawResponse()
    {
        throw new NotImplementedException();
    }
}