using ElasticSearchApp;

internal class Program
{
   
    private static async Task Main(string[] args)
    {

        string elasticsearchUrl = "http://localhost:9200"; // Replace with your Elasticsearch URL
        ElasticsearchService elasticsearchService = new ElasticsearchService(elasticsearchUrl);

        string searchTerm = "Test";

        Console.WriteLine("Enter Search Term");
        searchTerm = Console.ReadLine();

        await elasticsearchService.SearchDocumentsAsync(searchTerm);
       




    }

    static async void DocumentIndexing()
    {

        string elasticsearchUrl = "http://localhost:9200"; // Replace with your Elasticsearch URL
        var elasticsearchService = new ElasticsearchService(elasticsearchUrl);
        var pdfService = new PdfProcessingService();

        string pdfDirectory = @"C:\Users\humayun.aziz\source\repos\ElasticSearchApp\ElasticSearchApp\Docus"; // Directory where your PDFs are stored
        var batchSize = 1000; // Number of documents per batch for bulk indexing

        var files = Directory.GetFiles(pdfDirectory, "*.pdf");
        var totalFiles = files.Length;

        var documentsToIndex = new List<Document>();

        for (int i = 0; i < totalFiles; i++)
        {
            var pdfFilePath = files[i];
            string documentContent = pdfService.ExtractTextFromPdf(pdfFilePath);

            if (string.IsNullOrEmpty(documentContent)) continue;

            var document = new Document
            {
                Id = Path.GetFileNameWithoutExtension(pdfFilePath),
                Title = Path.GetFileName(pdfFilePath),
                Content = documentContent,
                CreatedDate = File.GetCreationTime(pdfFilePath)
            };

            documentsToIndex.Add(document);

            // Index in batches of batchSize
            if (documentsToIndex.Count >= batchSize || i == totalFiles - 1)
            {
                await elasticsearchService.BulkIndexDocumentsAsync(documentsToIndex);
                documentsToIndex.Clear(); // Clear the batch after indexing
            }

            Console.WriteLine($"Processed {i + 1}/{totalFiles} PDF documents.");
        }

        Console.WriteLine("Bulk indexing completed.");
    }

    static async void DocumentSearching() {
      


    }




}