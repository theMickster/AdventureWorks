using Azure;

namespace AdventureWorks.Common.Helpers.Fakes;

/// <summary>
/// Fake Response
/// </summary>
/// <typeparam name="T"></typeparam>
public class FakeResponse<T> : Response<T>
{
    private readonly T _value;

    public FakeResponse(T value)
    {
        _value = value;
    }

    public override T Value => _value;

    /// <summary>
    /// Not Implemented
    /// </summary>
    /// <returns></returns>
    public override Response GetRawResponse()
    {
        throw new NotImplementedException();
    }
}