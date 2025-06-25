using FluentAssertions;
using Moq;
using System.Reflection;

namespace AdventureWorks.Test.Common.Extensions;

public static class AssertionExtensions
{
    public static void ConstructorNullExceptions(this Type type)
    {
        foreach (var constructor in type.GetConstructors())
        {
            foreach (var parameter in constructor.GetParameters())
            {
                var parameters = constructor.GetParameters()
                    .Select(p =>
                    {
                        if (parameter.Name == p.Name)
                        {
                            return null;
                        }
                        else
                        {
                            var mockType = typeof(Mock<>).MakeGenericType(p.ParameterType);
                            dynamic? mock = Activator.CreateInstance(mockType);
                            return mock?.Object;
                        }
                    })
                    .ToArray();

                var act = () => constructor.Invoke(parameters);

                    var ex = act.Should()
                    .Throw<TargetInvocationException>()
                    .Which.InnerException;

                ex.Should().BeOfType<ArgumentNullException>();
                ((ArgumentNullException)ex).ParamName.Should().Be(parameter.Name);
            }
        }
    }
}
