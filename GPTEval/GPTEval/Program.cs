using Microsoft.Extensions.Configuration;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using OpenAI;
using System;
using System.Net;
using System.IO;
using System.Reflection;
using HtmlAgilityPack;

var builder = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .AddEnvironmentVariables();
var configurationRoot = builder.Build();

var key = configurationRoot.GetSection("OpenAIKey").Get<string>() ?? string.Empty;

var openAiService = new OpenAIService(new OpenAiOptions()
{
    ApiKey = key
});


var webpage = "";
var pageText = "";
Console.WriteLine("Enter a webpage to get the meaning of the text. This webpage can be a news article, Wikipedia page, etc.");
Console.Write("> ");
webpage = Console.ReadLine();

// get webpage text
using (HttpClient client = new HttpClient())
{ 
    pageText = await client.GetStringAsync(webpage);
}

// extract text from webpage with HtmlAgilityPack
var htmlDoc = new HtmlDocument(); 
htmlDoc.LoadHtml(pageText); 
var htmlBody = htmlDoc.DocumentNode.SelectSingleNode("//body"); 
pageText = htmlBody.InnerText;

// TODO: testing
// News Article (WORKING) - https://www.washingtonpost.com/technology/2023/11/23/x-musk-openai-altman-big-tech/
// Lyrics (NOT WORKING) - https://genius.com/The-hillbillies-family-ties-lyrics
// Recipe (WORKING) - https://www.allrecipes.com/recipe/23600/worlds-best-lasagna/
// Wikipedia - https://en.wikipedia.org/wiki/ChatGPT
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

       
        new(StaticValues.ChatMessageRoles.System, "You are a helpful assistant who is very skilled at reading comprehension. You will be given text from a website which" +
                                                  " you must read the contents of and give an explanation on what the author's meaning was and the idea of the text."),
        new(StaticValues.ChatMessageRoles.User, pageText)

    },
    Model = Models.Gpt_3_5_Turbo_16k, // had to upgrade to 16k because of token limit
    // MaxTokens = 10000 (optional)
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