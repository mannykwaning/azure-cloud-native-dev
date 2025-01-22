using System.Collections.Generic;

namespace HW7DocumentSearchSolution.Models
{
    public class BlobIndexSearchResults
    {
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public string LastModifiedDate { get; set; }
        public List<string> HighlightHits { get; set; }
    }
}
