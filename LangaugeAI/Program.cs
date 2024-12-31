using System;
using Azure;
using Microsoft.Extensions.Configuration;
using System.Text;
using Azure.AI.TextAnalytics;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;

string userText = "";
while (userText.ToLower() != "quit")
{
    Console.WriteLine("Enter some text ('quit' to stop)");
    userText = Console.ReadLine();

    if (userText.ToLower() != "quit")
    {
        Console.WriteLine("Language: " + Detectlanguage(userText));

    }

}

static string Detectlanguage(string text)
{
    var credentials = GetCredential();
    if (credentials.key == null || credentials.endpoint == null)
        return "Please configure endpoint and keys first";

    var client = new TextAnalyticsClient(credentials.endpoint, credentials.key);
    DetectedLanguage detectedLanguage = client.DetectLanguage(text);
    return (detectedLanguage.Name);

}


static (AzureKeyCredential? key, Uri? endpoint) GetCredential()
{

    IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
    IConfigurationRoot configuration = builder.Build();

    string? endpointstr = configuration["AIServicesEndpoint"];
    string? keystr = configuration["AIServicesKey"];

    AzureKeyCredential? key = string.IsNullOrEmpty(keystr) ? null : new AzureKeyCredential(keystr);
    Uri? endpoint = string.IsNullOrEmpty(endpointstr) ? null : new Uri(endpointstr);

    return (key, endpoint);
}