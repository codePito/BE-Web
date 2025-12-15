using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model.Response
{
    public class ImageResponse
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string StorageKey { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public long FileSize { get; set; }
        public string FileSizeFormatted { get; set; } = string.Empty;
        public string? MimeType { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? Dimensions { get; set; }
        public DateTime UploadedAt { get; set; }
        public string StorageProvider { get; set; } = string.Empty;
    }
}
