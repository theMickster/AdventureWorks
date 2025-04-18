namespace AdventureWorks.Models.Slim;

public class GenericSlimModel
{
    public int Id { get; set; }

    public required string Name { get; set; } = string.Empty;

    public required string Code { get; set; } = string.Empty;

}
