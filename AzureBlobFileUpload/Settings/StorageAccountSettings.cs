using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBlobFileUpload.Settings
{
    public class StorageAccountSettings : IStorageAccountSettings
    {
        /// <summary>
        /// Storage accont connection string
        /// </summary>
        public string StorageAccountConnectionString { get; set; }
    }
}
