using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;

namespace Data.DTOs.Category
{
    public class FilterCategoryDto
    {
        public long? ParentId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public FilterCategoryStatus CategoryStatus { get; set; }
        public List<ProductCategory> Data { get; set; }

        public enum FilterCategoryStatus
        {
            All,
            Active,
            DeActive
        }
    }
}