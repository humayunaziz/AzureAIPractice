
using System.Text;
using Azure;
using Azure.AI.TextAnalytics;

namespace ElasticSearchApp
{
    public class AzureAIService
    {
        string endpoint = "https://humayunazureaiservice.openai.azure.com/"; 
        string apiKey = "FNDJ04hBLqkxEFUIpgn7KrkZ9VBPP9rfE8iNguzQCHcBfMU7ixSRJQQJ99ALACYeBjFXJ3w3AAAAACOGzYnJ";
        private readonly TextAnalyticsClient _client;
        public AzureAIService()
        {
            _client = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

        }


        public async Task<string> DetectLanguage(string text)
        {
         
            DetectedLanguage detectedLanguage =await _client.DetectLanguageAsync(text.Substring(1,10));
            return (detectedLanguage.Name);

        }

        public async Task<string> GetSummary(string text)
        {
            StringBuilder builder=new StringBuilder();
            TextAnalyticsActions actions = new TextAnalyticsActions()
            {
                ExtractiveSummarizeActions = new List<ExtractiveSummarizeAction>() { new ExtractiveSummarizeAction() }
            };

            var batchInput = new List<string>
            {
                text
            };

            AnalyzeActionsOperation operation = await _client.StartAnalyzeActionsAsync(batchInput, actions);
            await operation.WaitForCompletionAsync();

            await foreach (AnalyzeActionsResult documentsInPage in operation.Value) 
            {

                IReadOnlyCollection<ExtractiveSummarizeActionResult> summaryResults = documentsInPage.ExtractiveSummarizeResults;
                foreach (ExtractiveSummarizeActionResult summaryActionResults in summaryResults)
                {
                    if (summaryActionResults.HasError)continue;

                    foreach (ExtractiveSummarizeResult documentResults in summaryActionResults.DocumentsResults)
                    {
                        if (documentResults.HasError)continue ;

                        foreach (ExtractiveSummarySentence sentence in documentResults.Sentences)
                        {
                            builder.AppendLine(sentence.Text);
                        }

                    }
                }


            }

            return builder.ToString();   
        }


        public async Task<string> GetTextSentiment(string text)
        {

            var sentiment = await _client.AnalyzeSentimentAsync(text.Substring(1,2000));
            return sentiment.Value.Sentiment.ToString();

        }

        public async Task<string> GetTextKeyPhrases(string text)
        {
            var keyPhrases = await _client.ExtractKeyPhrasesAsync(text.Substring(1, 2000));
            return string.Join("", keyPhrases.Value);
              
        }

        public async Task<string> GetRecognizeEntities(string text)
        {
            StringBuilder builder = new StringBuilder();
            var entities = await _client.RecognizeEntitiesAsync(text.Substring(1, 2000));
            foreach (var entity in entities.Value)
                builder.AppendLine($" - {entity.Text} (Category: {entity.Category})");

            return builder.ToString();
        }

    }
}
