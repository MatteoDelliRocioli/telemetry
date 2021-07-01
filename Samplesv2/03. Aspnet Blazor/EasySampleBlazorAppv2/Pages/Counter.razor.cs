using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using EasySampleBlazorLib;

namespace EasySampleBlazorAppv2.Pages
{
    public partial class Counter : ComponentBase
    {
        [Inject]
        protected ILogger<Counter> _logger { get; set; }


        private int currentCount = 0;

        private async void IncrementCount()
        {
            using (var scope = _logger.BeginMethodScope())
            {
                IncrementCounterImpl();
            }
        }

        public int IncrementCounterImpl()
        {
            using (var scope = _logger.BeginMethodScope())
            {
                currentCount++;

                scope.LogDebug($"sample debug log within IncrementCounterImpl method");
                scope.LogInformation($"sample debug log within IncrementCounterImpl method");
                scope.LogWarning($"sample debug log within IncrementCounterImpl method");
                scope.LogError($"sample debug log within IncrementCounterImpl method");
                scope.LogException(new NullReferenceException("invalid data"));

                var sampleClass = new SampleClass();
                sampleClass.SampleMethod("sampleparameter", 5);

                scope.Result = currentCount;
                return currentCount;
            }
        }
    }
}
