using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoynerML.Models.Response
{
    public class ProductRecommendationResponse
    {
        public List<ProductRecommendationAttribute> Products { get; set; }
        public List<ProductRecommendationAttribute> Colors { get; set; }
        public List<ProductRecommendationAttribute> ProductTypes { get; set; }

        public ProductRecommendationResponse()
        {
            Products = new List<ProductRecommendationAttribute>();
            Colors = new List<ProductRecommendationAttribute>();
            ProductTypes = new List<ProductRecommendationAttribute>();
        }
    }
}