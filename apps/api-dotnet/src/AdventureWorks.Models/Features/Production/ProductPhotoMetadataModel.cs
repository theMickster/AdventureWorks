namespace AdventureWorks.Models.Features.Production;

public sealed class ProductPhotoMetadataModel
{
    public int PhotoId { get; set; }

    public string? ThumbnailPhotoFileName { get; set; }

    public string? LargePhotoFileName { get; set; }

    public bool IsPrimary { get; set; }
}
