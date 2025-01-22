using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBlobFileUpload.Settings
{
    /// <summary>
    /// Implements PictureSettings
    /// </summary>
    public class FileSettings : IFileSettings
    {
        /// <summary>
        /// Container name
        /// </summary>
        public string FileContainerName { get; set; }
    }
}
