using Accord.IO;
using Accord.MachineLearning;
using Accord.Statistics.Analysis;
using BoynerML.Models.Commons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace BoynerML.Services
{
    public class KnnPredictor : IPredictor
    {
        private string _experimentName;
        private int _k;
        private KNearestNeighbors<double[]> _loadedKnn;

        private KnnPredictor() { }

        public KnnPredictor(string experimentName)
        {
            _experimentName = experimentName;
        }

        public void LoadLearnedModel(KNearestNeighbors<double[]> knn = null)
        {
            if (knn == null)
                _loadedKnn = Serializer.Load<KNearestNeighbors<double[]>>(Path.Combine(Constants.BasePath, _experimentName + ".bin"));
            else
                _loadedKnn = knn;
        }

        public void SetK(int k)
        {
            _k = k;
        }

        public int[] Predict(double[] input)
        {
            _loadedKnn.K = _k > 0 ? _k : 1;

            int[] result = new int[_k];

            var knnMatrix = _loadedKnn.GetNearestNeighbors(input, out result);

            return result;
        }
    }
}