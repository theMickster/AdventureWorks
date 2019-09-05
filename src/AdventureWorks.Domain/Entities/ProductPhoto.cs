using System;
using System.Collections.Generic;
using AdventureWorks.Core.Interfaces;

namespace AdventureWorks.Core.Entities
{
    public class ProductPhoto : BaseEntity, IAggregateRoot
    {
        public int ProductPhotoId { get; set; }
        public byte[] ThumbNailPhoto { get; set; }
        public string ThumbnailPhotoFileName { get; set; }
        public byte[] LargePhoto { get; set; }
        public string LargePhotoFileName { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ICollection<ProductProductPhoto> ProductProductPhotos { get; set; }
    }
}
