using BoynerML.Core;
using BoynerML.Core.Extensions;
using BoynerML.Models.Commons;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;

namespace BoynerML.Services
{
    public class ProductRecommender
    {

        #region | Properties |
        public string[] Outputs { get; private set; }

        private static DataTable trainingDatatable;
        private static DataTable productTypesTrainingDatatable;
        private static DataTable colorGroupsTrainingDatatable;
        private static DataTable gendersTrainingDatatable;
        private static DateTime datasetLastPullDate;

        private DataTable _trainingDatatable
        {
            get
            {
                var recommendationDatasetTableName = string.Empty;
                var activeFrom = new DateTime(3000, 01, 01);

                var dtDatasetStatus = DataProvider.GetRecommendationDatasetStatus();
                if (dtDatasetStatus != null && dtDatasetStatus.Rows.Count == 1)
                {
                    activeFrom = Convert.ToDateTime(dtDatasetStatus.Rows[0]["ActiveFrom"]);
                    recommendationDatasetTableName = dtDatasetStatus.Rows[0]["RecommendationDatasetTableName"].ToString();
                }

                if (trainingDatatable == null || activeFrom > _datasetLastPullDate)
                {
                    trainingDatatable = DataProvider.GetRecommendationDatasetFromCsv();
                    _datasetLastPullDate = DateTime.Now;
                }

                return trainingDatatable;
            }
        }

        private DataTable _productTypesTrainingDatatable
        {
            get
            {
                var recommendationDatasetTableName = string.Empty;
                var activeFrom = new DateTime(3000, 01, 01);

                var dtDatasetStatus = DataProvider.GetRecommendationDatasetStatus();
                if (dtDatasetStatus != null && dtDatasetStatus.Rows.Count == 1)
                {
                    activeFrom = Convert.ToDateTime(dtDatasetStatus.Rows[0]["ActiveFrom"]);
                    recommendationDatasetTableName = dtDatasetStatus.Rows[0]["RecommendationDatasetTableName"].ToString();
                }

                if (productTypesTrainingDatatable == null || activeFrom > _datasetLastPullDate)
                {
                    productTypesTrainingDatatable = DataProvider.GetProductTypeRecommendationDatasetFromCsv();
                    _datasetLastPullDate = DateTime.Now;
                }

                return productTypesTrainingDatatable;
            }
        }

        private DataTable _colorGroupsTrainingDatatable
        {
            get
            {
                var recommendationDatasetTableName = string.Empty;
                var activeFrom = new DateTime(3000, 01, 01);

                var dtDatasetStatus = DataProvider.GetRecommendationDatasetStatus();
                if (dtDatasetStatus != null && dtDatasetStatus.Rows.Count == 1)
                {
                    activeFrom = Convert.ToDateTime(dtDatasetStatus.Rows[0]["ActiveFrom"]);
                    recommendationDatasetTableName = dtDatasetStatus.Rows[0]["RecommendationDatasetTableName"].ToString();
                }

                if (colorGroupsTrainingDatatable == null || activeFrom > _datasetLastPullDate)
                {
                    colorGroupsTrainingDatatable = DataProvider.GetColorGroupRecommendationDatasetFromCsv();
                    _datasetLastPullDate = DateTime.Now;
                }

                return colorGroupsTrainingDatatable;
            }
        }

        private DataTable _gendersTrainingDatatable
        {
            get
            {
                var recommendationDatasetTableName = string.Empty;
                var activeFrom = new DateTime(3000, 01, 01);

                var dtDatasetStatus = DataProvider.GetRecommendationDatasetStatus();
                if (dtDatasetStatus != null && dtDatasetStatus.Rows.Count == 1)
                {
                    activeFrom = Convert.ToDateTime(dtDatasetStatus.Rows[0]["ActiveFrom"]);
                    recommendationDatasetTableName = dtDatasetStatus.Rows[0]["RecommendationDatasetTableName"].ToString();
                }

                if (gendersTrainingDatatable == null || activeFrom > _datasetLastPullDate)
                {
                    gendersTrainingDatatable = DataProvider.GetGenderRecommendationDatasetFromCsv();
                    _datasetLastPullDate = DateTime.Now;
                }

                return gendersTrainingDatatable;
            }
        }

        private DateTime _datasetLastPullDate
        {
            get
            {
                if (datasetLastPullDate == null)
                    return new DateTime(1900, 01, 01);

                return datasetLastPullDate;
            }
            set
            {
                datasetLastPullDate = value;
            }
        }

        private RecommendationType _recommendationType;
        private string _tags;
        private int _topK;
        private int _bestN;
        private KnnLearner _knnLearner;
        private KnnPredictor _knnPredictor;
        private Dictionary<string, double> _learnedTags;
        private Dictionary<string, int> _classIndexes;
        private bool _knnLearned;
        private bool _knnLearningInProgress;
        private const int _knnInitRetryDuration = 2000;
        private const int _knnInitMaxRetry = 5;
        #endregion

        public ProductRecommender(RecommendationType type, string tags, int topK, int bestN)
        {
            _recommendationType = type;
            _tags = tags;
            _topK = topK;
            _bestN = bestN;
        }

        public void CalculateOutputs()
        {
            Outputs = null;

            if (PredictorReady(_recommendationType))
            {
                _learnedTags.SetItemScore(_tags, ',', ':');
                _knnPredictor.SetK(_topK);
                var predictedIndexes = _knnPredictor.Predict(_learnedTags.Values.ToArray());
                Outputs = _classIndexes.Where(x => predictedIndexes.Contains(x.Value)).Select(x => x.Key).ToArray();
                Outputs = new string[predictedIndexes.Length];
                for (int i = 0; i < predictedIndexes.Length; i++)
                {
                    Outputs[i] = _classIndexes.FirstOrDefault(x => x.Value == predictedIndexes[i]).Key;
                }
            }
            if (_bestN > 0)
                Outputs = Outputs.TakeBestItems(_bestN);
        }

        private bool PredictorReady(RecommendationType recommendationType)
        {
            var isReady = false;

            if (_knnLearned)
                isReady = true;
            if (!_knnLearned && !_knnLearningInProgress)
            {
                DataTable trainingDt;
                switch (recommendationType)
                {
                    case RecommendationType.ProductId:
                        trainingDt = _trainingDatatable;
                        break;
                    case RecommendationType.ProductType:
                        trainingDt = _productTypesTrainingDatatable;
                        break;
                    case RecommendationType.ColorGroup:
                        trainingDt = _colorGroupsTrainingDatatable;
                        break;
                    case RecommendationType.Gender:
                        trainingDt = _gendersTrainingDatatable;
                        break;
                    default:
                        trainingDt = _trainingDatatable;
                        break;
                }
                InitKnnLearner(trainingDt);
                isReady = true;
            }
            else if (!_knnLearned && _knnLearningInProgress)
            {
                //Wait Knn learning process to end, check every x seconds, up to n times
                var waitCounter = 0;
                while (!_knnLearned && waitCounter < _knnInitMaxRetry)
                {
                    Thread.Sleep(_knnInitRetryDuration);
                    waitCounter++;
                }

                isReady = _knnLearned;
            }

            return isReady;
        }

        private void InitKnnLearner(DataTable trainingDatatable)
        {
            //>>
            //_knnLearningInProgress = true;

            var experimentName = "boys";
            _knnLearner = new KnnLearner(experimentName);
            _knnPredictor = new KnnPredictor(experimentName);

            var learnedTags = new Dictionary<string, double>();
            var classIndexes = new Dictionary<string, int>();
            _knnLearner.InitTrainingModel(trainingDatatable, out learnedTags, out classIndexes);
            _learnedTags = learnedTags;
            _classIndexes = classIndexes;

            _knnLearner.Learn();
            _knnPredictor.LoadLearnedModel(_knnLearner.KnnModel);

            //>>
            //_knnLearned = true;
            //_knnLearningInProgress = false;
        }
    }
}