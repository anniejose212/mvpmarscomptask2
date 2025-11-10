// FILE: TestDataHelper.cs
// PURPOSE: Provides decoding of test placeholders (e.g., {DQ}, {EQ:n}) into actual characters for input validation/XSS scenarios.

using Microsoft.AspNetCore.Razor.Language.Intermediate;
using mars_nunit_json.TestDataModel;
using Newtonsoft.Json;
using mars_nunit_json.Support;

public static class TestDataHelper
{
    public static string NormalizeTestInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        
        input = input.Replace("{DQ}", "\"");

        // Replace {EQ:n} → n '=' characters
        if (input.Contains("{EQ:"))
        {
            int start = input.IndexOf("{EQ:");
            int end = input.IndexOf("}", start);

            if (start != -1 && end != -1)
            {
                string token = input.Substring(start, end - start + 1);
                string num = token.Replace("{EQ:", "").Replace("}", "");

                if (int.TryParse(num, out int count))
                    input = input.Replace(token, new string('=', count));
            }
        }

        return input.Trim();
    }
    public static EducationRecord GetEducationRecord(string fileName, int index)
    {
        var path = TestDataPath.Resolve(fileName);
        var list = JsonFileReader.Load<List<EducationRecord>>(path);

        if (list == null || list.Count == 0)
            throw new InvalidOperationException($"Dataset '{fileName}' is empty or unreadable.");

        if (index < 0 || index >= list.Count)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Index {index} is out of range (0–{list.Count - 1}) in '{fileName}'.");

        return list[index];
    }
}
