using OpenAI;
using Microsoft.Extensions.AI;
using System.ClientModel;
using A2A;
using A2A.AspNetCore;

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
        Endpoint = new Uri(endpoint)
    })
    .GetChatClient(model).AsIChatClient();

var agent = chatClient.AsAIAgent(
        name: "WeatherAssistant",
        instructions: @"
You're a concise and practical weather assistant.

You will receive structured weather data in JSON format with 'current' and 'today' sections.

Your job:
1. Summarize the current weather in 1 sentence.
2. Summarize today's forecast in 1-2 sentences.
3. Highlight anything the user should watch out for (rain, wind, temperature drops, etc.).
4. Optionally give a short suggestion (e.g., bring a jacket, umbrella).

Guidelines:
- Be concise (3–4 sentences total).
- Use natural, friendly language.
- Do NOT repeat raw JSON or field names.
- Convert values into readable phrases (e.g., '13°C' → '13 degrees').
- Prioritize important changes (rain, big temperature swings, strong wind).
- If conditions are mild, explicitly say it's a good day.

Output format:
- Paragraph only (no bullet points, no JSON).", // TODO: to return structured response
        tools: tools);

builder.Services.AddSingleton(chatClient);

var app = builder.Build();

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

// Expose the agent via A2A protocol. You can also customize the agentCard
app.MapA2A(
    agent, 
    path: "/", 
    agentCard: weatherAgentCard,
    taskManager => app.MapWellKnownAgentCard(taskManager, "/")
    );



app.Run();