using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model.Entities
{
    public class Image
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(1000)]
        public string Url { get; set; } = string.Empty;
        [MaxLength(500)]
        public string OriginalFileName { get; set; } = string.Empty;
        [Required, MaxLength(500)]
        public string StorageKey { get; set; } = string.Empty;
        [Required, MaxLength(500)]
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsPrimary { get; set; } = false;
        public long FileSize { get; set; } = 0;
        [MaxLength(100)]
        public string? MimeType { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        [MaxLength(50)]
        public string StorageProvider { get; set; } = "CloudflareR2";
        public int UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        [NotMapped]
        public string FileSizeFormatted
        {
            get
            {
                if(FileSize < 1024) return $"{FileSize} B";
                else if (FileSize < 1024 * 1024)
                    return $"{(FileSize / 1024.0):F2} KB";
                else if (FileSize < 1024 * 1024 * 1024)
                    return $"{(FileSize / (1024.0 * 1024.0)):F2} MB";
                else
                    return $"{(FileSize / (1024.0 * 1024.0 * 1024.0)):F2} GB";
            }
        }
        [NotMapped]
        public string? Dimension => Width.HasValue && Height.HasValue ? $"{Width}x{Height}" : null;
    }

    public static class ImageEntityTypes
    {
        public const string Product = "Product";
        public const string Category = "Category";
        public const string User = "User";
    }
}
