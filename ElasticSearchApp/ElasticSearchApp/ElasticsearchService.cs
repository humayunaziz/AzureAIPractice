using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using System.Collections.Generic;

namespace ElasticSearchApp
{
    public class ElasticsearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly AzureAIService _azureAIService;

        public ElasticsearchService(string elasticsearchUrl)
        {
            var settings = new ElasticsearchClientSettings(new Uri(elasticsearchUrl)).Authentication(new BasicAuthentication("elastic", "CIKg5llGCF3bKa9fKPOz"))
                .DefaultIndex("documents");


            _client = new ElasticsearchClient(settings);
            _azureAIService = new AzureAIService();
        }


      public async Task GetSummary(string term)
        {
            var searchRequest = new SearchRequest<Document>()
            {
                Query = new SimpleQueryStringQuery()
                {
                    Query = term,
                    Fields = new Field[]
                    {
                        new("content", boost: 2)
                    },
                    DefaultOperator = Operator.Or,
                    MinimumShouldMatch = 1,
                },

                Highlight = new Highlight()
                {
                    PreTags = ["<b>"],
                    PostTags = ["</b>"],
                    Encoder = HighlighterEncoder.Html,
                    Fields = new Dictionary<Field, HighlightField>
                            {
                                { new Field("content"), new HighlightField() { PreTags = ["<b>"], PostTags = ["</b>"] }},
                            },

                }
            };

            var searchResponse = await _client.SearchAsync<Document>(searchRequest);
            if (searchResponse.IsValidResponse)
            {
             
                foreach (var hit in searchResponse.Hits)
                {
                    if (hit.Highlight != null && hit.Highlight.ContainsKey("content"))
                    {
                        
                        Console.WriteLine("Highlights:");
                        foreach (var snippet in hit.Highlight["content"])
                        {
                            Console.WriteLine($"- {snippet}");
                        }
                    }

                    Console.WriteLine(new string('-', 50));
                }
            }
            else
            {
                Console.WriteLine($"Error: {searchResponse.ElasticsearchServerError?.ToString()}");
            }



        }


        public async Task SearchDocumentsAsync(string query)
        {
            var searchResponse = await _client.SearchAsync<Document>(s => s
                .Index("documents")
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.Content)
                        .Query(query)
                    )
                )
            );

            if (!searchResponse.IsValidResponse)
            {
                Console.WriteLine("Error during search:");
                Console.WriteLine(searchResponse.DebugInformation);
                return;
            }

            Console.WriteLine($"Found {searchResponse.Hits.Count} matching documents:");
            foreach (var hit in searchResponse.Hits)
            {
                
                Console.WriteLine($"- {hit.Source.Title}\n");
                await GetSummary(query);
                //Console.WriteLine($"- {hit.Source.Title}:\nDocument Language is -{await _azureAIService.DetectLanguage(hit.Source.Content)}");


                //Console.WriteLine($"\nSentiment - {await _azureAIService.GetTextSentiment(hit.Source.Content)}");
                //Console.WriteLine($"\nKeyPhrases - {await _azureAIService.GetTextKeyPhrases(hit.Source.Content)}");
                //Console.WriteLine($"\nEntity Recognization - {await _azureAIService.GetRecognizeEntities(hit.Source.Content)}");
                //                Console.WriteLine($"- Summary:\n {await _azureAIService.GetSummary(hit.Source.Content)}");
                //Console.WriteLine("-------------------------------------------------------------------------");
            }
        }

  
        public async Task BulkIndexDocumentsAsync(IEnumerable<Document> documents)
        {
            try
            {
                // Ensure the Operations list is initialized
                var bulkRequest = new BulkRequest
                {
                    Operations = new List<IBulkOperation>()
                };

                foreach (var document in documents)
                {
                    try
                    {
                        // Add each document as a BulkIndexOperation
                        bulkRequest.Operations.Add(new BulkIndexOperation<Document>(document));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding document to bulk request: {ex.Message}");
                    }
                }

                // Send the bulk request to Elasticsearch
                var bulkResponse = await _client.BulkAsync(bulkRequest);

                if (bulkResponse.IsValidResponse)
                {
                    Console.WriteLine($"Successfully indexed {bulkResponse.Items.Count} documents.");
                }
                else
                {
                    Console.WriteLine($"Error during bulk index: {bulkResponse.DebugInformation}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error during bulk indexing: {ex.Message}");
            }
        }

       
    }
}
