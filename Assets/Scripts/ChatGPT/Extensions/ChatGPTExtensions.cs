using System.Linq;

public static class ChatGPTExtensions
{
    public const string KEYWORD_USING = "using UnityEngine";
    public const string KEYWORD_PUBLIC_CLASS = "public class";
    public static readonly string[] filters = { "C#", "c#","csharp","CSHARP" };

    public static ChatGPTResponse Populate(this ChatGPTResponse chatGPTResponse)
    {
        var message = chatGPTResponse.Choices.FirstOrDefault().Message.Content
            .Trim();

        // apply filters
        filters.ToList().ForEach(f =>
        {
            message = message.Replace(f, string.Empty);
        });

        // split due to explanations
        var codeLines = message.Split("```");

        // extract source code
        chatGPTResponse.SourceCode = codeLines.FirstOrDefault(c => c.Contains(KEYWORD_USING) ||
            c.Contains(KEYWORD_PUBLIC_CLASS)).Trim();

        // extract explanations
        var explanations = codeLines.Where(e => !e.Contains(KEYWORD_PUBLIC_CLASS) && !e.Contains(KEYWORD_PUBLIC_CLASS) && !string.IsNullOrEmpty(e))
            .ToArray();

        // join explanations
        explanations.ToList().ForEach(f =>
        {
            if (f.Trim() != "\n\n") 
                chatGPTResponse.Explanation += f;
            chatGPTResponse.Explanation = chatGPTResponse.Explanation.Replace("\n\n", string.Empty);
        });

        return chatGPTResponse;
    }
}