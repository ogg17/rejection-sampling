using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using RejectionApp.Models;
using RejectionApp.Utilities;

namespace RejectionApp.Pages
{
    public class Result : PageModel
    {
        public Models.Result MyResult { get; set; }
        public DrawParam MyDrawParam { get; set; } = new ();
        public double Xi2 { get; set; }
        public double SignficatiobLevel { get; set; }
        public List<double> Sampling { get; set; } = new ();

        private readonly IMemoryCache _cache;

        public Result(IMemoryCache cache)
        {
            _cache = cache;
        }

        public IActionResult OnPost(Models.Result myResult)
        {
            try
            {
                MyResult = myResult;
                myResult.SetInterval();
                myResult.C = Calculator.FindNormal(myResult);
                myResult.Maximum = Calculator.FindMaximum(myResult);

                var sampling = Calculator.GenerateSampling(myResult);
                Sampling = sampling;

                var dens = Calculator.PerformDensity(myResult, myResult.Maximum);
                var max = (int) MathF.Ceiling((float) dens);
                MyDrawParam.UpdateParam(myResult, max);
                MyDrawParam.Offset = 150;

                var prob = Calculator.СalculateProbabilities(myResult, MyDrawParam);
                var freq = Calculator.CalculateFrequencies(myResult, Sampling);
                Xi2 = Calculator.CalculateX2(myResult, prob, freq);
                SignficatiobLevel = Calculator.SignificanceLevelXi2(myResult, Xi2, myResult.IntervalCount);
                
                using (var chacheEntry = _cache.CreateEntry("Sampling"))
                {
                    _cache.Set("SamplingValue", sampling);
                }
                TempData.Put("MyDrawParam", MyDrawParam);
                TempData.Put("MyResult", myResult);
                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                var url = Url.Page("/ResultError", new {ExMessage = ex.Message});
                return Redirect(url);
            }
        }

        public IActionResult OnGetGraph()
        {
            try
            {
                var result = TempData.Get<Models.Result>("MyResult");
                if (result == null)
                    result = new Models.Result();

                var myDrawParam = TempData.Get<DrawParam>("MyDrawParam");
                if (myDrawParam == null)
                {
                    myDrawParam = new DrawParam();
                    var max = (int) MathF.Ceiling((float) Calculator.PerformDensity(result, result.Maximum));
                    myDrawParam.UpdateParam(result, max);
                }

                var sampling = new List<double>();
                using (var chacheEntry = _cache.CreateEntry("Sampling"))
                {
                    sampling = _cache.Get<List<double>>("SamplingValue");
                }

                var drawFunctionPoints = DrawPlots.DrawGraph(myDrawParam, result);
                var frequencies = Calculator.CalculateFrequencies(result, sampling);
                var drawFuncHistRects = DrawPlots.DrawHistogram(myDrawParam, 
                    result.IntervalCount, frequencies, result.SampleSize);

                var jsonStr = JsonSerializer.Serialize(new
                {
                    drawParam = myDrawParam,
                    drawFuncPoints = drawFunctionPoints,
                    drawFuncPointsLength = drawFunctionPoints.Count,
                    drawFuncHistRects = drawFuncHistRects,
                    drawFuncHistRectsLength = drawFuncHistRects.Count
                });
                return new JsonResult(jsonStr);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                var url = Url.Page("/ResultError", new {ExMessage = "Input Error!"});
                return Redirect(url);
            }
        }
    }
}