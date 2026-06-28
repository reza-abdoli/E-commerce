using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.DTOs.Product;

public class CreateSizeDto
{
    public string Title { get; set; }
    public string SizeType { get; set; }
    public int Order { get; set; }
}