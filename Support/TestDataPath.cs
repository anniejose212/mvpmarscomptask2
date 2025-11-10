using System;
using System.IO;

namespace mars_nunit_json.Support
{
    public static class TestDataPath
    {
      
        public static string Resolve(string fileName)
        {
            var baseDir = AppContext.BaseDirectory;
            return Path.Combine(baseDir, "TestData", fileName);
        }
    }
}
