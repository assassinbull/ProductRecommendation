using BoynerML.Core;
using BoynerML.Core.Extensions;
using BoynerML.Models;
using BoynerML.Models.Commons;
using BoynerML.Models.Response;
using BoynerML.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace BoynerML.Controllers
{
    public class ProductRecommendationController : ApiController
    {
        const int _bestNMultiplier = 10;

        public ResponseBase<ProductRecommendationResponse> Get(string tags, int topK = 5)
        {
            topK = 20;
            var bestN = 2;
            var response = new ResponseBase<ProductRecommendationResponse>();

            var colorRecommender = new ProductRecommender(RecommendationType.ColorGroup, tags, bestN * _bestNMultiplier, bestN);
            var productTypeRecommender = new ProductRecommender(RecommendationType.ProductType, tags, bestN * _bestNMultiplier, bestN);
            var productRecommender = new ProductRecommender(RecommendationType.ProductId, tags, topK * _bestNMultiplier, 0);

            this.RunSafely(() =>
            {
                var productRecommenders = new List<ProductRecommender>();
                productRecommenders.Add(colorRecommender);
                productRecommenders.Add(productTypeRecommender);
                productRecommenders.Add(productRecommender);

                Parallel.ForEach(productRecommenders, recommender =>
                {
                    recommender.CalculateOutputs();
                });

                var colorGroupResult = colorRecommender.Outputs;
                var productTypeResult = productTypeRecommender.Outputs;
                var productIdResult = productRecommender.Outputs;
                var products = DataProvider.GetProductDatasetFromCsv();

                var productIdColumnName = "ProductId";
                var genderColumnName = "Gender";
                var colorGroupColumnName = "ColorGroup";
                var productTypeColumnName = "ProductType";

                var dtDatasetStatus = DataProvider.GetRecommendationDatasetStatus();
                var gender = string.Empty;
                if (dtDatasetStatus != null && dtDatasetStatus.Rows.Count == 1)
                {
                    gender = dtDatasetStatus.Rows[0]["RecommendationDatasetTableName"].ToString();
                }

                var filteredProducts = products.AsEnumerable()
                                                .Where(x => productIdResult.Contains(x[productIdColumnName].ToString())
                                                            && colorGroupResult.Contains(x[colorGroupColumnName].ToString())
                                                            && productTypeResult.Contains(x[productTypeColumnName].ToString())
                                                            && (string.IsNullOrEmpty(gender) || x[genderColumnName].ToString() == gender))
                                                .Select(x => new ProductRecommendationModel
                                                {
                                                    ProductId = x[productIdColumnName].ToString(),
                                                    Gender = x[genderColumnName].ToString(),
                                                    ColorGroup = x[colorGroupColumnName].ToString(),
                                                    ProductType = x[productTypeColumnName].ToString(),
                                                    GenderWeight = 1,
                                                    ColorGroupWeight = colorGroupResult.Length - Array.IndexOf(colorGroupResult, x[colorGroupColumnName].ToString()),
                                                    ProductTypeWeight = productTypeResult.Length - Array.IndexOf(productTypeResult, x[productTypeColumnName].ToString())
                                                })
                                                .ToList();

                var productsArray = filteredProducts.OrderByDescending(x => x.GenderWeight * x.ColorGroupWeight * x.ProductTypeWeight)
                                                    .Take(topK)
                                                    .Select(x => x.ProductId)
                                                    .ToArray();

                var result = new ProductRecommendationResponse();
                foreach (var item in productsArray)
                {
                    result.Products.Add(new ProductRecommendationAttribute { Name = item, ConfidenceRank = (Array.IndexOf(productsArray, item) + 1) });
                }
                foreach (var item in colorGroupResult)
                {
                    result.Colors.Add(new ProductRecommendationAttribute { Name = item, ConfidenceRank = (Array.IndexOf(colorGroupResult, item) + 1) });
                }
                foreach (var item in productTypeResult)
                {
                    result.ProductTypes.Add(new ProductRecommendationAttribute { Name = item, ConfidenceRank = (Array.IndexOf(productTypeResult, item) + 1) });
                }

                //log recommendations
                Task.Run(() => LogResults(tags, result));

                response.Success(result);
            }, ex =>
            {
                response.Error(ex);
            });

            return response;
        }

        public ResponseBase<string[]> Get(string tags, RecommendationType type, int topK = 5)
        {
            var response = new ResponseBase<string[]>();
            var productRecommender = new ProductRecommender(type, tags, topK * _bestNMultiplier, topK);

            this.RunSafely(() =>
            {
                productRecommender.CalculateOutputs();
                var result = productRecommender.Outputs;

                //log recommendations
                var dt = DataProvider.GenerateProductRecommendationDataTable();
                var recommendationDate = DateTime.Now;
                for (int i = 0; i < result.Length; i++)
                {
                    dt.Rows.Add(null, tags, i + 1, result[i], recommendationDate);
                }
                DataProvider.WriteProductRecommendationToDatabase(dt);

                response.Success(result);
            }, ex =>
            {
                response.Error(ex);
            });

            return response;
        }

        private void LogResults(string tags, ProductRecommendationResponse result)
        {
            var dt = DataProvider.GenerateProductRecommendationDataTable();
            var recommendationDate = DateTime.Now;
            foreach (var item in result.Products)
            {
                dt.Rows.Add(null, tags, item.ConfidenceRank, item.Name, recommendationDate);
            }
            DataProvider.WriteProductRecommendationToDatabase(dt);
        }
    }
}
