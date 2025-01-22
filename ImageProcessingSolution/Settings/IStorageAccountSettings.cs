namespace ImageProducer.Settings
{
    public interface IStorageAccountSettings
    {
        /// <summary>
        /// Standard name for storage account connection string
        /// </summary>
        public string StorageAccountConnectionString { get; }
    }
}
