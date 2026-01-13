using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;   

namespace mars_nunit_json.Support
{
    public abstract class LoggerHelper
    {
        private readonly List<string> _logBuffer = new();

        public IReadOnlyList<string> GetLogs() => _logBuffer;
        public void ClearLogs() => _logBuffer.Clear();

        public void Info(string message)
        {
            var safe = WebUtility.HtmlEncode(message ?? string.Empty);

            _logBuffer.Add(safe);

            TestContext.WriteLine($"[INFO] {message}");

        }
    }
}
