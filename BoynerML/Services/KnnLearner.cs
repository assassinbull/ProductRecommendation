using Accord.IO;
using Accord.MachineLearning;
using Accord.Math.Distances;
using Accord.Statistics.Analysis;
using BoynerML.Core;
using BoynerML.Core.Extensions;
using BoynerML.Models.Commons;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BoynerML.Services
{
    public class KnnLearner
    {
        private string _experimentName;
        private Dictionary<string, double> _tagsDictionary;
        private static char _tagsDelimiter = ',';

        public double[][] TrainingModelInputs;
        public int[] TrainingModelOutputs;
        public KNearestNeighbors<double[]> KnnModel;

        private KnnLearner() { }

        public KnnLearner(string experimentName)
        {
            _experimentName = experimentName;
            KnnModel = new KNearestNeighbors<double[]>(1, new SquareEuclidean());
            KnnModel.ParallelOptions.MaxDegreeOfParallelism = 1;
            _tagsDictionary = new Dictionary<string, double>();
        }

        public Dictionary<string, double> InitTrainingModel(Dictionary<int, string> trainingDataset)
        {
            foreach (var trainingData in trainingDataset)
            {
                _tagsDictionary.FillDictionary(trainingData.Value, _tagsDelimiter);
            }

            TrainingModelInputs = new double[trainingDataset.Count][];
            TrainingModelOutputs = new int[trainingDataset.Count];

            var i = 0;
            foreach (var trainingData in trainingDataset)
            {
                _tagsDictionary.SetItemPresence(trainingData.Value, _tagsDelimiter);

                TrainingModelInputs[i] = _tagsDictionary.Values.ToArray();
                TrainingModelOutputs[i] = trainingData.Key;

                i++;
            }

            return _tagsDictionary;
        }

        public void InitTrainingModel(DataTable trainingDatatable, out Dictionary<string, double> tagsDictionary, out Dictionary<string, int> classDictionary)
        {
            var classDictionaryLocal = new Dictionary<string, int>();
            var trainingDatatableClone = trainingDatatable.Copy();

            TrainingModelInputs = new double[trainingDatatableClone.Rows.Count][];
            TrainingModelOutputs = new int[trainingDatatableClone.Rows.Count];

            var classes = trainingDatatableClone.AsEnumerable().Select(r => r.Field<string>("Class")).ToList();
            var classIndex = 0;
            foreach (var className in classes.Distinct())
            {
                classDictionaryLocal.Add(className, classIndex);
                classIndex++;
            }

            TrainingModelOutputs = trainingDatatableClone.AsEnumerable().Select(x => classDictionaryLocal[x["Class"].ToString()]).ToArray();

            trainingDatatableClone.Columns.Remove("Class");
            foreach (DataColumn trainingData in trainingDatatableClone.Columns)
            {
                _tagsDictionary.FillDictionary(trainingData.ColumnName, _tagsDelimiter);
            }
            TrainingModelInputs = trainingDatatableClone.AsEnumerable().Select(x => x.ItemArray.Select(y => y.ToString().ToLocaleDouble()).ToArray()).ToArray();

            tagsDictionary = _tagsDictionary;
            classDictionary = classDictionaryLocal;
        }

        public void Learn()
        {
            KnnModel.Learn(TrainingModelInputs, TrainingModelOutputs);
        }

        public double EvaluateAccuracy()
        {
            KnnModel.K = 1;
            var cm = GeneralConfusionMatrix.Estimate(KnnModel, TrainingModelInputs, TrainingModelOutputs);
            return cm.Accuracy;
        }

        public void SaveLearnedModel()
        {
            KnnModel.Save(Path.Combine(Constants.BasePath, _experimentName + ".bin"));
        }

        public void KnnTest()
        {
            var exitStatus = Task.Run(async () => await KnimeWrapper.SucceedingExecutionWithoutBinding().ConfigureAwait(false)).Result;
        }
    }
}