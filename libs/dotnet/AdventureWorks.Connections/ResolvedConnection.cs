namespace AdventureWorks.Connections;

public sealed record ResolvedConnection(string Name, ConnectionKind Kind, string Value);
