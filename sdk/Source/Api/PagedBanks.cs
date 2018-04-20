using System.Collections.Generic;
using Io.Token.Proto.Common.Bank;
using Io.Token.Proto.Gateway;

namespace sdk.Api
{
    public class PagedBanks
    {
        public PagedBanks(GetBanksResponse response)
        {
            Banks = response.Banks;
            
            var paging = response.Paging;
            Page = paging.Page;
            PerPage = paging.PerPage;
            PageCount = paging.PageCount;
            TotalCount = paging.TotalCount;
        }

        public IList<Bank> Banks { get; }

        /// <summary>
        /// Index of current page.
        /// </summary>
        public int Page { get; }
        
        /// <summary>
        /// Number of records per page.
        /// </summary>
        public int PerPage { get; }
        
        /// <summary>
        /// Number of pages in total.
        /// </summary>
        public int PageCount { get; }
        
        /// <summary>
        /// Number of records in total.
        /// </summary>
        public int TotalCount { get; }
    }
}
