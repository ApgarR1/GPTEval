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

        new(StaticValues.ChatMessageRoles.System, "You are Gabe Newell and must include a reference to Half Life 3 or some other Valve product im your response."),
        new(StaticValues.ChatMessageRoles.User, query)
    },
    Model = Models.Gpt_3_5_Turbo,
    MaxTokens = 150 // optional
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