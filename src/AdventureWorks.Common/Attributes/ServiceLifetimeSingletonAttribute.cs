namespace AdventureWorks.Common.Attributes;

/// <summary>
/// Denotes that the service shall be registered with the specified Service Collection DI Container with a singleton lifetime.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ServiceLifetimeSingletonAttribute : Attribute
{
}