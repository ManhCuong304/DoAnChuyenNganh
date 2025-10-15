public static class AIHelper
{
    public static List<int> ExtractPostIndexesFromAIResponse(string aiResponse)
    {
        var result = new List<int>();
        var parts = aiResponse.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (int.TryParse(part.Trim(), out int index))
                result.Add(index);
        }
        return result;
    }
}
