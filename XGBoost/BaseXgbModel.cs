﻿using System.Collections.Generic;
using XGBoost.lib;

namespace XGBoost
{
    public class BaseXgbModel
    {
        protected IDictionary<string, object> parameters = new Dictionary<string, object>();
        protected Booster booster;

        public void SaveModelToFile(string fileName)
        {
            booster.Save(fileName);
        }

        public static XGBClassifier LoadClassifierFromFile(string fileName)
        {
            return new XGBClassifier { booster = new Booster(fileName) };
        }

        public static XGBRegressor LoadRegressorFromFile(string fileName)
        {
            return new XGBRegressor { booster = new Booster(fileName) };
        }

        public string[] DumpModelEx(string fmap = "", int with_stats = 0, string format = "json")
        {
            return booster.DumpModelEx(fmap, with_stats, format);
        }

        //public string[] XGBoosterDumpModelWithFeatures(string[] features, int with_stats = 0)
        //{
        //    return booster.XGBoosterDumpModelWithFeatures(features, with_stats);
        //}
    }
}