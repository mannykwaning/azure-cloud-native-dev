using HW7DocumentSearchSolution.Models;
using HW7DocumentSearchSolution.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HW7DocumentSearchSolution.Pages
{
    public class IndexModel : PageModel
    {
        /// <summary>
        /// Search settings
        /// </summary>
        private SearchSettings _searchSettings;

        /// <summary>
        /// App configurations
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Gets or sets the search results.
        /// </summary>
        /// <value>The search results.</value>
        public List<BlobIndexSearchResults> SearchResults { get; set; }

        public PaginatedList<BlobIndexSearchResults> PagedResults { get; set; }

        /// <summary>
        /// Table columns to be sorted
        /// </summary>
        public string DocumentNameSort { get; set; }
        public string DocumentTypeSort { get; set; }
        public string LastModifiedDateSort { get; set; }
        public string CurrentSort { get; set; }
        public string CurrentFilter { get; set; }

        /// <summary>
        /// Search string getter/ setter
        /// </summary>
        [BindProperty]
        public string SearchString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_searchString))
                {
                    return String.Empty;
                }
                return _searchString;
            }
            set
            {
                _searchString = value;
            }
        }

        [BindProperty]
        public string DocSearchString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_docSearchString))
                {
                    return String.Empty;
                }
                return _docSearchString;
            }
            set
            {
                _docSearchString = value;
            }
        }

        /// <summary>
        /// Search string
        /// </summary>
        private string _searchString;

        private string _docSearchString;

        /// <summary>
        /// Search index client instance
        /// </summary>
        private SearchIndexClient _searchClientInstance;

        /// <summary>
        /// Gets the search client instance
        /// </summary>
        private SearchIndexClient SearchClientInstance
        {
            get
            {
                _searchClientInstance = (_searchClientInstance == null)
                    ? new SearchIndexClient(_searchSettings.ServiceName,
                                            _searchSettings.IndexName,
                                            new SearchCredentials(_searchSettings.QueryKey))
                    : _searchClientInstance;

                return _searchClientInstance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexModel"/> class.
        /// </summary>
        /// <param name="searchSettings">The search settings</param>
        public IndexModel(IOptions<SearchSettings> searchSettings, IConfiguration configuration)
        {
            _searchSettings = searchSettings.Value;
            _configuration = configuration;
        }

        /// <summary>
        /// Async Get with Sorting
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        public async Task OnGetAsync(string searchString, string docSearchString, string sortOrder, int? pageIndex)
        {
            DocumentNameSort = sortOrder == "DocumentName_Asc_Sort"
                                            ? "DocumentName_Desc_Sort"
                                            : "DocumentName_Asc_Sort";

            DocumentTypeSort = sortOrder == "DocumentType_Asc_Sort"
                                            ? "DocumentType_Desc_Sort"
                                            : "DocumentType_Asc_Sort";

            LastModifiedDateSort = sortOrder == "LastModifiedDate_Asc_Sort"
                                            ? "LastModifiedDate_Desc_Sort"
                                            : "LastModifiedDate_Asc_Sort";

            CurrentSort = sortOrder;
            SearchString = searchString;
            DocSearchString = docSearchString;
            CurrentFilter = searchString;

            if(searchString != null)
            {
                pageIndex = 1;
            }

            CurrentFilter = searchString;

            await ExecuteSearch();
            IQueryable<BlobIndexSearchResults> searchResultsIQ = SearchResults.AsQueryable();

            if (!String.IsNullOrEmpty(DocSearchString))
            {
                searchResultsIQ = searchResultsIQ.Where(s => s.DocumentType.Contains(DocSearchString));
            }

            switch (sortOrder)
            {
                case "DocumentName_Asc_Sort":
                    searchResultsIQ = searchResultsIQ.OrderBy(d => d.DocumentName);
                    break;
                case "DocumentName_Desc_Sort":
                    searchResultsIQ = searchResultsIQ.OrderByDescending(d => d.DocumentName);
                    break;
                case "DocumentType_Asc_Sort":
                    searchResultsIQ = searchResultsIQ.OrderBy(d => d.DocumentType);
                    break;
                case "DocumentType_Desc_Sort":
                    searchResultsIQ = searchResultsIQ.OrderByDescending(d => d.DocumentType);
                    break;
                case "LastModifiedDate_Asc_Sort":
                    searchResultsIQ = searchResultsIQ.OrderBy(d => d.LastModifiedDate);
                    break;
                case "LastModifiedDate_Desc_Sort":
                    searchResultsIQ = searchResultsIQ.OrderByDescending(d => d.LastModifiedDate);
                    break;
            }

            SearchResults = searchResultsIQ.ToList();

            int pageSize = 3;
            PagedResults = PaginatedList<BlobIndexSearchResults>.Create(
                    SearchResults, pageIndex ?? 1, pageSize);
        }

        /// <summary>
        /// Executes the search.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>Updates the SearchResults upon return.</returns>
        private async Task ExecuteSearch()
        {
            // Sort by document type ascending
            SearchParameters searchParameters = new SearchParameters()
            {
                OrderBy = new List<string>()
                {
                    "metadata_storage_content_type asc"

                },
                HighlightFields = new List<string> { "content" },
                HighlightPreTag = "<b>",
                HighlightPostTag = "</b>"
            };

            DocumentSearchResult<BlobDocument> result = await SearchClientInstance.Documents.SearchAsync<BlobDocument>(SearchString, searchParameters);

            SearchResults = new List<BlobIndexSearchResults>();

            foreach (var item in result.Results)
            {
                BlobIndexSearchResults resultModelItem = new BlobIndexSearchResults()
                {
                    DocumentName = item.Document.metadata_storage_name,
                    DocumentType = item.Document.metadata_storage_content_type,
                    LastModifiedDate = item.Document.metadata_storage_last_modified.ToLocalTime().ToString("yyyy/MM/dd")
                };

                resultModelItem.HighlightHits = (
                    item.Highlights != null && item.Highlights.Count > 0)
                    ? new List<string>(item.Highlights["content"])
                    : new List<string>();

                SearchResults.Add(resultModelItem);
            }
        }

        /// <summary>
        /// Download File from blob storage
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>The File</returns>
        public async Task<IActionResult> OnPostDownload(string fileName)
        {
            CloudBlockBlob blockBlob;
            await using (MemoryStream memoryStream = new MemoryStream())
            {
                string blobstorageConn = _configuration.GetValue<string>("BlobStorageAccount");

                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(blobstorageConn);
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("blobstoindex");
                blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);

                await blockBlob.DownloadToStreamAsync(memoryStream);
            }

            Stream blobStream = blockBlob.OpenReadAsync().Result;
            return File(blobStream, blockBlob.Properties.ContentType, blockBlob.Name);
        }

        /// <summary>
        /// Called when [HttpPost]
        /// </summary>
        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await ExecuteSearch();

            IQueryable<BlobIndexSearchResults> searchResultsIQ = SearchResults.AsQueryable();

            if (!String.IsNullOrEmpty(DocSearchString))
            {
                searchResultsIQ = searchResultsIQ.Where(s => s.DocumentType.Contains(DocSearchString));
            }

            SearchResults = searchResultsIQ.ToList();

            return Page();
        }
    }
}
