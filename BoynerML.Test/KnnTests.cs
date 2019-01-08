using System;
using System.Collections.Generic;
using System.Linq;
using BoynerML.Core.Extensions;
using BoynerML.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoynerML.Test
{
    [TestClass]
    public class KnnTests
    {
        [TestMethod]
        public void LearnAndPredict()
        {
            var experimentName = "boys";
            var knnLearner = new KnnLearner(experimentName);
            var knnPredictor = new KnnPredictor(experimentName);

            var inputs = new Dictionary<int, string>();
            var test = "mavi,ayakkabı,kadın";

            inputs.Add(1, "mavi,tshirt,erkek");
            inputs.Add(2, "beyaz,gömlek,kadın");
            inputs.Add(3, "mavi,gömlek,kadın");
            inputs.Add(4, "yeşil,ayakkabı,erkek");
            inputs.Add(5, "yeşil,beyaz,gömlek,kadın");
            inputs.Add(6, "mavi,yeşil,ayakkabı,kadın");
            inputs.Add(7, "beyaz,mavi,gömlek,erkek");
            inputs.Add(8, "beyaz,yeşil,kadın,ayakkabı");
            inputs.Add(9, "beyaz,etek,kadın");
            inputs.Add(10, "mavi,erkek,ayakkabı");

            var tags = knnLearner.InitTrainingModel(inputs);
            knnLearner.Learn();
            tags.SetItemPresence(test, ',');
            knnPredictor.LoadLearnedModel(knnLearner.KnnModel);
            knnPredictor.SetK(3);
            var output = knnPredictor.Predict(tags.Values.ToArray());

            var acc = knnLearner.EvaluateAccuracy();
        }

        [TestMethod]
        public void KnnTest()
        {
            var experimentName = "boys";
            var knnLearner = new KnnLearner(experimentName);
            knnLearner.KnnTest();
        }
    }
}