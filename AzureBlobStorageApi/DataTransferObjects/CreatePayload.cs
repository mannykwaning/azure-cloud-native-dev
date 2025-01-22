using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBlobStorageApi.DataTransferObjects
{
    public class CreatePayload
    {
        [Required(ErrorMessage = "3")]
        [StringLength(63, ErrorMessage = "2")]
        [MinLength(3, ErrorMessage = "5")]
        public string ContainerName { get; set; }

        [Required]
        [StringLength(75, ErrorMessage = "2")]
        public string FileName { get; set; }
    }
}
