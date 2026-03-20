using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkManagement.Core.Entities
{
    public class FileAttachment
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string FileName { get; set; } = string.Empty;       // Tên file gốc
        public string FilePath { get; set; } = string.Empty;       // Đường dẫn lưu trên server
        public long FileSize { get; set; }                          // Kích thước (bytes)
        public int UploadedBy { get; set; }
        public int UploadedByRole { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public Task Task { get; set; } = null!;
        [ForeignKey("UploadedBy")]
        public User UploadedByUser { get; set; } = null!;
    }
}
