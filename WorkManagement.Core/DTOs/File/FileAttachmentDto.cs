using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkManagement.Core.DTOs.File
{
    public class FileAttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public long FileSize { get; set; }
        public int UploadedByRole { get; set; }
        public string UploadedByName { get; set; } = ""; // ← thêm
        public DateTime UploadedAt { get; set; }
    }
}
