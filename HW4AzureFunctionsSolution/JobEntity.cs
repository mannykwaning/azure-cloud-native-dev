using Microsoft.WindowsAzure.Storage.Table;
using System.ComponentModel.DataAnnotations;

namespace HW4AzureFunctionsSolution
{
    class JobEntity : TableEntity
    {
        [MaxLength(36)]
        public string JobId { get; set; }

        public string ImageConversionMode { get; set; }

        public int Status { get; set; }

        [MaxLength(512)]
        public string StatusDescription { get; set; }

        [MaxLength(512)]
        public string ImageSource { get; set; }
        
        [MaxLength(512)]
        public string ImageResult { get; set; }
    }
}
