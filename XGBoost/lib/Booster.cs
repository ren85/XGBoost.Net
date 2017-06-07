﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace XGBoost.lib
{
    public class Booster : IDisposable
    {
        private bool disposed;
        private readonly IntPtr handle;
        private const int normalPrediction = 0;  // optionMask value for XGBoosterPredict

        public IntPtr Handle => handle;

        public Booster(IDictionary<string, object> parameters, DMatrix train)
        {
            var dmats = new[] { train.Handle };
            var len = unchecked((ulong)dmats.Length);
            var output = XGBOOST_NATIVE_METHODS.XGBoosterCreate(dmats, len, out handle);
            if (output == -1) throw new DllFailException(XGBOOST_NATIVE_METHODS.XGBGetLastError());

            SetParameters(parameters);
        }

        public Booster(DMatrix train)
        {
            var dmats = new[] { train.Handle };
            var len = unchecked((ulong)dmats.Length);
            var output = XGBOOST_NATIVE_METHODS.XGBoosterCreate(dmats, len, out handle);
            if (output == -1) throw new DllFailException(XGBOOST_NATIVE_METHODS.XGBGetLastError());
        }

        public Booster(string fileName, int silent = 1)
        {
            IntPtr tempPtr;
            var newBooster = XGBOOST_NATIVE_METHODS.XGBoosterCreate(null, 0, out tempPtr);
            var output = XGBOOST_NATIVE_METHODS.XGBoosterLoadModel(tempPtr, fileName);
            if (output == -1) throw new DllFailException(XGBOOST_NATIVE_METHODS.XGBGetLastError());
            handle = tempPtr;
        }

        public void Update(DMatrix train, int iter)
        {
            var output = XGBOOST_NATIVE_METHODS.XGBoosterUpdateOneIter(Handle, iter, train.Handle);
            if (output == -1) throw new DllFailException(XGBOOST_NATIVE_METHODS.XGBGetLastError());
        }

        public float[] Predict(DMatrix test)
        {
            ulong predsLen;
            IntPtr predsPtr;
            var output = XGBOOST_NATIVE_METHODS.XGBoosterPredict(
                handle, test.Handle, normalPrediction, 0, out predsLen, out predsPtr);
            if (output == -1) throw new DllFailException(XGBOOST_NATIVE_METHODS.XGBGetLastError());
            return GetPredictionsArray(predsPtr, predsLen);
        }

        public float[] GetPredictionsArray(IntPtr predsPtr, ulong predsLen)
        {
            var length = unchecked((int)predsLen);
            var preds = new float[length];
            for (var i = 0; i < length; i++)
            {
                var floatBytes = new byte[4];
                for (var b = 0; b < 4; b++)
                {
                    floatBytes[b] = Marshal.ReadByte(predsPtr, 4 * i + b);
                }
                preds[i] = BitConverter.ToSingle(floatBytes, 0);
            }
            return preds;
        }

        public void SetParameters(IDictionary<string, Object> parameters)
        {
            // support internationalisation i.e. support floats with commas (e.g. 0,5F)
            var nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };

            SetParameter("max_depth", ((int)parameters["max_depth"]).ToString());
            SetParameter("learning_rate", ((float)parameters["learning_rate"]).ToString(nfi));
            SetParameter("n_estimators", ((int)parameters["n_estimators"]).ToString());
            SetParameter("silent", ((bool)parameters["silent"]).ToString());
            SetParameter("objective", (string)parameters["objective"]);

            SetParameter("nthread", ((int)parameters["nthread"]).ToString());
            SetParameter("gamma", ((float)parameters["gamma"]).ToString(nfi));
            SetParameter("min_child_weight", ((int)parameters["min_child_weight"]).ToString());
            SetParameter("max_delta_step", ((int)parameters["max_delta_step"]).ToString());
            SetParameter("subsample", ((float)parameters["subsample"]).ToString(nfi));
            SetParameter("colsample_bytree", ((float)parameters["colsample_bytree"]).ToString(nfi));
            SetParameter("colsample_bylevel", ((float)parameters["colsample_bylevel"]).ToString(nfi));
            SetParameter("reg_alpha", ((float)parameters["reg_alpha"]).ToString(nfi));
            SetParameter("reg_lambda", ((float)parameters["reg_lambda"]).ToString(nfi));
            SetParameter("scale_pos_weight", ((float)parameters["scale_pos_weight"]).ToString(nfi));

            SetParameter("base_score", ((float)parameters["base_score"]).ToString(nfi));
            SetParameter("seed", ((int)parameters["seed"]).ToString());
            SetParameter("missing", ((float)parameters["missing"]).ToString(nfi));
            SetParameter("eval_metric", (string)parameters["eval_metric"]);
        }

        // doesn't support floats with commas (e.g. 0,5F)
        //public void SetParametersGeneric(IDictionary<string, Object> parameters)
        //{
        //    foreach (var param in parameters)
        //    {
        //        if (param.Value != null)
        //            SetParameter(param.Key, param.Value.ToString());
        //    }
        //}

        public void PrintParameters(IDictionary<string, Object> parameters)
        {
            Console.WriteLine("max_depth: " + (int)parameters["max_depth"]);
            Console.WriteLine("learning_rate: " + (float)parameters["learning_rate"]);
            Console.WriteLine("n_estimators: " + (int)parameters["n_estimators"]);
            Console.WriteLine("silent: " + (bool)parameters["silent"]);
            Console.WriteLine("objective: " + (string)parameters["objective"]);

            Console.WriteLine("nthread: " + (int)parameters["nthread"]);
            Console.WriteLine("gamma: " + (float)parameters["gamma"]);
            Console.WriteLine("min_child_weight: " + (int)parameters["min_child_weight"]);
            Console.WriteLine("max_delta_step: " + (int)parameters["max_delta_step"]);
            Console.WriteLine("subsample: " + (float)parameters["subsample"]);
            Console.WriteLine("colsample_bytree: " + (float)parameters["colsample_bytree"]);
            Console.WriteLine("colsample_bylevel: " + (float)parameters["colsample_bylevel"]);
            Console.WriteLine("reg_alpha: " + (float)parameters["reg_alpha"]);
            Console.WriteLine("reg_lambda: " + (float)parameters["reg_lambda"]);
            Console.WriteLine("scale_pos_weight: " + (float)parameters["scale_pos_weight"]);

            Console.WriteLine("base_score: " + (float)parameters["base_score"]);
            Console.WriteLine("seed: " + (int)parameters["seed"]);
            Console.WriteLine("missing: " + (float)parameters["missing"]);
            Console.WriteLine("eval_metric: " + (string)parameters["eval_metric"]);
        }

        public void SetParameter(string name, string val)
        {
            int output = XGBOOST_NATIVE_METHODS.XGBoosterSetParam(handle, name, val);
            if (output == -1)
            {
                var a = 5;
                throw new DllFailException(XGBOOST_NATIVE_METHODS.XGBGetLastError());
            }
        }

        public void Save(string fileName)
        {
            XGBOOST_NATIVE_METHODS.XGBoosterSaveModel(handle, fileName);
        }

        public string[] DumpModelEx(string fmap, int with_stats, string format)
        {
            int length;
            string[] dumpStr;
            XGBOOST_NATIVE_METHODS.XGBoosterDumpModel(handle, fmap, with_stats, out length, out dumpStr);
            return dumpStr;
        }

        //public string[] XGBoosterDumpModelWithFeatures(string[] features, int with_stats)
        //{
        //    ulong length;
        //    string[] out_models;

        //    //{'q': quantitative, 'i': indicator}
        //    var res = XGBOOST_NATIVE_METHODS.XGBoosterDumpModelWithFeatures(handle, features.Length, features, Enumerable.Range(0, features.Length).Select(f => "q").ToArray(), with_stats, out length, out out_models);
        //    return out_models;
        //}

        // Dispose pattern from MSDN documentation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            XGBOOST_NATIVE_METHODS.XGDMatrixFree(handle);
            disposed = true;
        }
    }

}
