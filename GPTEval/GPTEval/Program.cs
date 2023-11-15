using Microsoft.Extensions.Configuration;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using OpenAI;
using System.Reflection;

var builder = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .AddEnvironmentVariables();
var configurationRoot = builder.Build();

var key = configurationRoot.GetSection("OpenAIKey").Get<string>() ?? string.Empty;

var openAiService = new OpenAIService(new OpenAiOptions()
{
    ApiKey = key
});

var query = "";
Console.WriteLine("Enter query: ");
query = Console.ReadLine();

/* cannot use for loops inside of completionResult, so list cannot be accessed
var userTyping = true;
var query = "";
var queries = new List<string>();
Console.WriteLine("Enter query. If done, enter singular exclamation mark: ");
while (userTyping)
{
    query = Console.ReadLine();
    if (query == "!")
    {
        userTyping = false;
    }
    else
    {
        queries.Add(query);
    }
}
*/
var completionResult = openAiService.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
{
    Messages = new List<ChatMessage>
    {
        /*
        new(StaticValues.ChatMessageRoles.System, "You are a helpful assistant."),
        new(StaticValues.ChatMessageRoles.User, "Who won the world series in 2020?"),
        new(StaticValues.ChatMessageRoles.System, "The Los Angeles Dodgers won the World Series in 2020."),
        new(StaticValues.ChatMessageRoles.User, "Tell me a story about The Los Angeles Dodgers")
        */

       
        new(StaticValues.ChatMessageRoles.System, "You are a helpful assistant who is knowledgeable about many topics, including but not limited to electronics repair and programming. You must give examples for all solutions."),
        new(StaticValues.ChatMessageRoles.User, query)

    },
    Model = Models.Gpt_3_5_Turbo,
    // MaxTokens = 150 (optional)
});

await foreach (var completion in completionResult)
{
    if (completion.Successful)
    {
        Console.Write(completion.Choices.First().Message.Content);
    }
    else
    {
        if (completion.Error == null)
        {
            throw new Exception("Unknown Error");
        }

        Console.WriteLine($"{completion.Error.Code}: {completion.Error.Message}");
    }
}