using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoynerML.Models
{
    public class ProductRecommendationModel
    {
        public string ProductId { get; set; }
        public string Gender { get; set; }
        public int GenderWeight { get; set; }
        public string ColorGroup { get; set; }
        public int ColorGroupWeight { get; set; }
        public string ProductType { get; set; }
        public int ProductTypeWeight { get; set; }
    }
}