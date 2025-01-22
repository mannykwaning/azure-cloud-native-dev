using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBlobFileUpload.Settings
{
    /// <summary>
    /// Picture settings definition
    /// </summary>
    public interface IFileSettings
    {
        /// <summary>
        /// Container name
        /// </summary>
        public string FileContainerName { get; set; }
    }
}
