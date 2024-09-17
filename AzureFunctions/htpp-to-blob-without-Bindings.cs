using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureFunctions
{
    public static class htpp_to_blob_without_Bindings
    {
        [FunctionName("StoreJsonAsBlob")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Read and parse the incoming JSON request
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var input = JsonConvert.DeserializeObject<InputModel>(requestBody);

            if (input == null)
            {
                var errorResponse = new
                {
                    error = "Bad Request",
                    message = "Empty Json Input"
                };

                return new ObjectResult(errorResponse)
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            try
            {
                // Convert to the required text format
                string blobContent = $"ID: {input.Id}\nName: {input.Name}\nCertification: {input.Certification}\nExp: {input.Exp} years\nProgramming: {input.Programming}";

                // Azure Storage connection and container setup
                string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                string containerName = "certifications";

                // Create a BlobServiceClient
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

                // Get a reference to the container (creates it if it doesn't exist)
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName); ;
                // Ensure the container exists (creates if it doesn't)
                await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.None);
                // Get a reference to the blob
                BlobClient blobClient = containerClient.GetBlobClient($"{input.Id}.txt");

                // Upload the blob (overwrite if it exists)
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(blobContent)))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                return new OkObjectResult($"Blob created with ID: {input.Id}");
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    error = "InternalServerError",
                    message = ex.Message
                };

                return new ObjectResult(errorResponse)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }

    public class InputModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Certification { get; set; }
        public int Exp { get; set; }
        public string Programming { get; set; }
    }
}