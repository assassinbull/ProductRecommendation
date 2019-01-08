using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoynerML.Models
{
    public class ProductRecommendationAttribute
    {
        public string Name { get; set; }
        public int ConfidenceRank { get; set; }
    }
}