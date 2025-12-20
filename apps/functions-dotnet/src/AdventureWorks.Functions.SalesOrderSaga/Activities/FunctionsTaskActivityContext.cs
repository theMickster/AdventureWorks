using Microsoft.DurableTask;

namespace AdventureWorks.Functions.SalesOrderSaga.Activities;

/// <summary>
/// Minimal concrete <see cref="TaskActivityContext"/> for the <c>[Function]</c> activity
/// adapters in this namespace. The WebJobs isolated-worker <c>[ActivityTrigger]</c> binding
/// (<c>Microsoft.Azure.Functions.Worker.Extensions.DurableTask</c> 1.6.0) has no support for
/// injecting a real <see cref="TaskActivityContext"/> — an unattributed
/// <c>TaskActivityContext context</c> parameter binds to <see langword="null"/> at runtime
/// rather than throwing, which is what caused every saga activity to fail with
/// <see cref="System.ArgumentNullException"/> on <c>context</c>. None of the Core activity
/// classes read <see cref="Name"/>/<see cref="InstanceId"/>, so this exists only to satisfy
/// <see cref="TaskActivity{TInput,TResult}"/>'s required parameter.
/// </summary>
internal sealed class FunctionsTaskActivityContext(TaskName name) : TaskActivityContext
{
    public override TaskName Name { get; } = name;

    public override string InstanceId { get; } = string.Empty;
}
