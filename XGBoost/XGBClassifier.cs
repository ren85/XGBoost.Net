﻿using System.Collections.Generic;
using System.Linq;
using XGBoost.lib;

namespace XGBoost
{
    public class XGBClassifier : BaseXgbModel
    {

        /// <summary>
        ///   Implementation of the Scikit-Learn API for XGBoost
        /// </summary>
        /// <param name="maxDepth">
        ///   Maximum tree depth for base learners
        /// </param>
        /// <param name="learningRate">
        ///   Boosting learning rate (xgb's "eta")
        /// </param>
        /// <param name="nEstimators">
        ///   Number of boosted trees to fit
        /// </param>
        /// <param name="silent">
        ///   Whether to print messages while running boosting
        /// </param>
        /// <param name="objective">
        ///   Specify the learning task and the corresponding learning objective or
        ///   a custom objective function to be used(see note below)
        /// </param>
        /// <param name="nThread">
        ///   Number of parallel threads used to run xgboost
        /// </param>
        /// <param name="gamma">
        ///   Minimum loss reduction required to make a further partition on a leaf node of the tree
        /// </param>
        /// <param name="minChildWeight">
        ///   Minimum sum of instance weight(hessian) needed in a child
        /// </param>
        /// <param name="maxDeltaStep">
        ///   Maximum delta step we allow each tree's weight estimation to be
        /// </param>
        /// <param name="subsample">
        ///   Subsample ratio of the training instance
        /// </param>
        /// <param name="colSampleByTree">
        ///   Subsample ratio of columns when constructing each tree TODO prevent error for bigger range of vals
        /// </param>
        /// <param name="colSampleByLevel">
        ///   Subsample ratio of columns for each split, in each level TODO prevent error for bigger range of vals
        /// </param>
        /// <param name="regAlpha">
        ///   L1 regularization term on weights
        /// </param>
        /// <param name="regLambda">
        ///   L2 regularization term on weights
        /// </param>
        /// <param name="scalePosWeight">
        ///   Balancing of positive and negative weights
        /// </param>
        /// <param name="baseScore">
        ///   The initial prediction score of all instances, global bias
        /// </param>
        /// <param name="seed">
        ///   Random number seed
        /// </param>
        /// <param name="missing">
        ///   Value in the data which needs to be present as a missing value
        /// </param>
        /// <param name="eval_metric">
        ///   The metric to be used for validation data.
        /// </param>
        public XGBClassifier(int maxDepth = 3, float learningRate = 0.1F, int nEstimators = 100,
                    bool silent = true, string objective = "binary:logistic",
                    int nThread = -1, float gamma = 0, int minChildWeight = 1,
                    int maxDeltaStep = 0, float subsample = 1, float colSampleByTree = 1,
                    float colSampleByLevel = 1, float regAlpha = 0, float regLambda = 1,
                    float scalePosWeight = 1, float baseScore = 0.5F, int seed = 0,
                    float missing = float.NaN, float numClass = 0, string eval_metric = "error")
        {
            parameters["max_depth"] = maxDepth;
            parameters["learning_rate"] = learningRate;
            parameters["n_estimators"] = nEstimators;
            parameters["silent"] = silent;
            parameters["objective"] = objective;

            parameters["nthread"] = nThread;
            parameters["gamma"] = gamma;
            parameters["min_child_weight"] = minChildWeight;
            parameters["max_delta_step"] = maxDeltaStep;
            parameters["subsample"] = subsample;
            parameters["colsample_bytree"] = colSampleByTree;
            parameters["colsample_bylevel"] = colSampleByLevel;
            parameters["reg_alpha"] = regAlpha;
            parameters["reg_lambda"] = regLambda;
            parameters["scale_pos_weight"] = scalePosWeight;

            parameters["base_score"] = baseScore;
            parameters["seed"] = seed;
            parameters["missing"] = missing;
            parameters["_Booster"] = null;
            parameters["eval_metric"] = eval_metric;
        }


        //public XGBClassifier(IDictionary<string, object> p_parameters)
        //{
        //    parameters = p_parameters;
        //}

        /// <summary>
        ///   Fit the gradient boosting model
        /// </summary>
        /// <param name="data">
        ///   Feature matrix
        /// </param>
        /// <param name="labels">
        ///   Labels
        /// </param>
        public void Fit(float[][] data, float[] labels)
        {
            var train = new DMatrix(data, labels);
            booster = Train(parameters, train, ((int)parameters["n_estimators"]));
        }

        //public void Fit(float[][] data, float[] labels, IDictionary<string, object> p_parameters)
        //{
        //  var train = new DMatrix(data, labels);
        //  booster = Train(parameters, train, ((int)parameters["n_estimators"]),p_parameters);
        //}

        public static Dictionary<string, object> GetDefaultParameters()
        {
            var defaultParameters = new Dictionary<string, object>
            {
                ["max_depth"] = 3,
                ["learning_rate"] = 0.1f,
                ["n_estimators"] = 100,
                ["silent"] = true,
                ["objective"] = "binary:logistic",
                ["nthread"] = -1,
                ["gamma"] = 0,
                ["min_child_weight"] = 1,
                ["max_delta_step"] = 0,
                ["subsample"] = 1,
                ["colsample_bytree"] = 1,
                ["colsample_bylevel"] = 1,
                ["reg_alpha"] = 0,
                ["reg_lambda"] = 1,
                ["scale_pos_weight"] = 1,
                ["base_score"] = 0.5f,
                ["seed"] = 0,
                ["missing"] = float.NaN,
                ["_Booster"] = null,
                ["eval_metric"] = "error"
            };

            return defaultParameters;
        }

        public void SetParameter(string parameterName, object parameterValue)
        {
            parameters[parameterName] = parameterValue;
        }

        /// <summary>
        ///   Predict using the gradient boosted model
        /// </summary>
        /// <param name="data">
        ///   Feature matrix to do predicitons on
        /// </param>
        /// <returns>
        ///   Predictions
        /// </returns>
        public float[] Predict(float[][] data)
        {
            var test = new DMatrix(data);
            return booster.Predict(test).Select(v => v > 0.5f ? 1f : 0f).ToArray();
        }

        public float[] PredictRaw(float[][] data)
        {
            var test = new DMatrix(data);
            return booster.Predict(test);
        }
        /// <summary>
        ///   Predict using the gradient boosted model
        /// </summary>
        /// <param name="data">
        ///   Feature matrix to do predicitons on
        /// </param>
        /// <returns>
        ///   The probabilities for each classification being the actual
        ///   classification for each row
        /// </returns>
        public float[][] PredictProba(float[][] data)
        {
            var dTest = new DMatrix(data);
            var preds = booster.Predict(dTest);
            return preds.Select(v => new[] { 1 - v, v }).ToArray();
        }

        private Booster Train(IDictionary<string, object> args, DMatrix dTrain, int numBoostRound = 10)
        {
            var bst = new Booster(args, dTrain);
            for (int i = 0; i < numBoostRound; i++) { bst.Update(dTrain, i); }
            return bst;
        }

        //private Booster Train(IDictionary<string, object> args, DMatrix dTrain, int numBoostRound = 10, IDictionary<string, object> p_parameters = null)
        //{
        //    var bst = new Booster(dTrain);
        //    bst.SetParametersGeneric(parameters);
        //    for (int i = 0; i < numBoostRound; i++) { bst.Update(dTrain, i); }
        //    return bst;
        //}
    }
}
