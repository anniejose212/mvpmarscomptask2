using System;
using System.IO;
using Newtonsoft.Json;

namespace mars_nunit_json.Support
{
    public static class JsonFileReader
    {
        public static T Load<T>(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"JSON file not found: {path}");

            using var reader = new StreamReader(path);
            var json = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(json)
                   ?? throw new InvalidOperationException($"Could not deserialize {path} to {typeof(T).Name}");
        }
    }
}
