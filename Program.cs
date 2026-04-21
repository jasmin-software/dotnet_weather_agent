using OpenAI;
using Microsoft.Extensions.AI;
using System.ClientModel;
using A2A;
using A2A.AspNetCore;
using Microsoft.Agents.AI;
using ChatResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

string githubToken = builder.Configuration["GitHub:Token"] // TODO: Get Token from user.
    ?? throw new InvalidOperationException("GitHub:Token is not set.");
string endpoint = builder.Configuration["GitHub:ApiEndpoint"] ?? "https://models.github.ai/inference";
string model = builder.Configuration["GitHub:Model"] ?? "openai/gpt-4o-mini";

// Create AI agent
AITool[] tools =
[
    AIFunctionFactory.Create(Tools.WeatherTool.GetWeather)
];

// Create AI agent
var chatClient = new OpenAIClient(
    new ApiKeyCredential(githubToken),
    new OpenAIClientOptions()
    {
        Endpoint = new Uri(endpoint),
    })
    .GetChatClient(model).AsIChatClient();

var agent = chatClient.AsAIAgent(options: new ChatClientAgentOptions
{
    Name = "WeatherAssistant",
    ChatOptions = new () 
    {
        ResponseFormat = ChatResponseFormat.ForJsonSchema<WeatherResponse>(), 
        Tools = tools
    },
});
builder.Services.AddSingleton(chatClient);

var app = builder.Build();
app.Use(async (context, next) =>
{
    var logger = context.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("A2A.WeatherAgent.RequestLogger");

    logger.LogInformation(@"    Time: {Time}
    Request Path: {Path}
    Request Method: {Method}
    Response Status Code: {StatusCode}
    ",
    DateTime.UtcNow,
    context.Request.Path, 
    context.Request.Method, 
    context.Response.StatusCode);

    await next();
});

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

AgentCard weatherAgentCard = new AgentCard
{
    Name = "Weather Agent",
    Description = "This is a weather agent.",
    Version = "1.0",
    Skills = [
        new AgentSkill {
            Id = "get_weather",
            Name = "Weather Agent",
            Description = "An agent that provides weather information.",
            Tags = ["weather", "forecast"],
            Examples = ["What is the weather like in Vancouver today?"]
        }
    ]
};

// Expose the agent via A2A protocol.
app.MapA2A(
    agent,
    path: "/",
    agentCard: weatherAgentCard,
    taskManager => app.MapWellKnownAgentCard(taskManager, "/")
    );

app.Run();