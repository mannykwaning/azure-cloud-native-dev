using System;

namespace HW7DocumentSearchSolution.Models
{
    public class BlobDocument
    {
        public string content { get; set; }
        public string metadata_storage_content_type { get; set; }
        public DateTime metadata_storage_last_modified { get; set; }
        public string metadata_storage_name { get; set; }
        public string metadata_storage_path { get; set; }
        public string metadata_content_type { get; set; }
        public string metadata_language { get; set; }
    }
}
