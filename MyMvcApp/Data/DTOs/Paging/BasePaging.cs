using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.DTOs.Paging
{
    public class BasePaging
    {
        // public BasePaging()
        // {
        //     PageId = 1;
        //     TakeEntity = 5;
        //     HowManyShowPageAfterAndBefore = 3;
        // }

        public int PageId { get; set; } = 1;
        public int PageCount { get; set; }
        public int AllEntitiesCount { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public int TakeEntity { get; set; } = 3;
        public int SkipEntity { get; set; }
        public int GetLastPage()
        {
            return (int)Math.Ceiling(AllEntitiesCount / (double)TakeEntity);
        }
        public int HowManyShowPageAfterAndBefore { get; set; } = 3;

        public string GetCurrentPagingStatus()
        {
            var startItem = 1;
            var endItem = AllEntitiesCount;

            if (EndPage > 1)
            {
                startItem = (PageId - 1) * TakeEntity + 1;
                endItem = PageId * TakeEntity > AllEntitiesCount ? AllEntitiesCount : PageId * TakeEntity;
            }

            return $"نمایش {startItem}-{endItem} از {AllEntitiesCount}";
        }

        public BasePaging GetCurrentPaging()
        {
            return this;
        }
    }
}