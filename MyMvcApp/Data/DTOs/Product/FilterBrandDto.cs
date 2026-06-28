using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;

namespace Data.DTOs.Product;

public class FilterBrandDto
{
    public string Title { get; set; }
    public string Url { get; set; }
    public List<Brand> Data { get; set; }
}