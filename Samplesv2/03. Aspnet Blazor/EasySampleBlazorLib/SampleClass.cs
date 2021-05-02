using Common;
using Microsoft.Extensions.Logging;
using System;

namespace EasySampleBlazorLib
{
    public class SampleClass
    {
        protected ILogger<SampleClass> _logger { get; set; }

        public string SampleMethod(string s1, int i1)
        {
            using (var scope = _logger.BeginMethodScope(new { s1, i1 }))
            {
                string res = "result string";


                scope.LogDebug($"sample debug log within sample method");

                scope.Result = res;
                return res;
            }
        }
    }
}
