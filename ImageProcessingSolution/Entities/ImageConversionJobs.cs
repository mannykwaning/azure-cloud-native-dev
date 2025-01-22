using Microsoft.Azure.Cosmos.Table;
using System.ComponentModel.DataAnnotations;

namespace ImageProducer.Entities
{
    public class ImageConversionJobs : TableEntity
    {
        public int Status { get; set; }

        [MaxLength(512)]
        public string StatusDescription { get; set; }

        [MaxLength(512)]
        public string ImageSource { get; set; }

        [MaxLength(512)]
        public string ImageResult { get; set; }
    }
}
