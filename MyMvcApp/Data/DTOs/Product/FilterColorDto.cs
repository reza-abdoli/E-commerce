using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;

namespace Data.DTOs.Product;

public class FilterColorDto
{
    public string Title { get; set; }
    public List<ProductColor> Data { get; set; }
}