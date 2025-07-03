using AdventureWorks.API.libs.Services;
using Microsoft.AspNetCore.Http;

namespace AdventureWorks.UnitTests.API.libs.Services;

public sealed class CorrelationIdAccessorTests
{
    [Fact]
    public void Constructor_NullHttpContextAccessor_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new CorrelationIdAccessor(null!));
        Assert.Equal("httpContextAccessor", exception.ParamName);
    }

    [Fact]
    public void CorrelationId_NoHttpContext_ReturnsNull()
    {
        var httpContextAccessor = new HttpContextAccessor();
        var accessor = new CorrelationIdAccessor(httpContextAccessor);
        var result = accessor.CorrelationId;
        Assert.Null(result);
    }

    [Fact]
    public void CorrelationId_NoCorrelationIdSet_ReturnsNull()
    {
        var httpContext = new DefaultHttpContext();
        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
        var accessor = new CorrelationIdAccessor(httpContextAccessor);
        var result = accessor.CorrelationId;
        Assert.Null(result);
    }

    [Fact]
    public void SetCorrelationId_ValidValue_StoresInHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
        var accessor = new CorrelationIdAccessor(httpContextAccessor);
        var correlationId = "test-correlation-id-123";
        accessor.SetCorrelationId(correlationId);
        var result = accessor.CorrelationId;
        Assert.Equal(correlationId, result);
    }

    [Fact]
    public void SetCorrelationId_NullValue_ThrowsArgumentNullException()
    {
        var httpContext = new DefaultHttpContext();
        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
        var accessor = new CorrelationIdAccessor(httpContextAccessor);
        Assert.Throws<ArgumentNullException>(() => accessor.SetCorrelationId(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetCorrelationId_EmptyOrWhitespace_ThrowsArgumentException(string correlationId)
    {
        var httpContext = new DefaultHttpContext();
        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
        var accessor = new CorrelationIdAccessor(httpContextAccessor);
        Assert.Throws<ArgumentException>(() => accessor.SetCorrelationId(correlationId));
    }

    [Fact]
    public void SetCorrelationId_NoHttpContext_DoesNotThrow()
    {
        var httpContextAccessor = new HttpContextAccessor();
        var accessor = new CorrelationIdAccessor(httpContextAccessor);
        var correlationId = "test-correlation-id-123";

        accessor.SetCorrelationId(correlationId);
    }

    [Fact]
    public void CorrelationId_MultipleSetAndGet_RetainsLatestValue()
    {
        var httpContext = new DefaultHttpContext();
        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
        var accessor = new CorrelationIdAccessor(httpContextAccessor);
        var firstId = "first-id";
        var secondId = "second-id";
        accessor.SetCorrelationId(firstId);
        var firstResult = accessor.CorrelationId;
        accessor.SetCorrelationId(secondId);
        var secondResult = accessor.CorrelationId;
        Assert.Equal(firstId, firstResult);
        Assert.Equal(secondId, secondResult);
    }
}
