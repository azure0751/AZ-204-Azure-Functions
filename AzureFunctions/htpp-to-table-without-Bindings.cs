using Azure;
using Azure.Data.Tables;
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
    public static class htpp_to_table_without_Bindings
    {
        [FunctionName("StoreJsonAsNoSql")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Read and parse the incoming JSON request
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var input = JsonConvert.DeserializeObject<InputTableModel>(requestBody);

            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            if (input == null)
            {
                //return (IActionResult)response;
                // return new StatusCodeResult(StatusCodes.Status400BadRequest);

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
                // Azure Table Storage handling
                string tableName = "CertificationsTable";

                // Create the TableServiceClient
                TableServiceClient tableServiceClient = new TableServiceClient(connectionString);

                // Create the table if it doesn't exist
                TableClient tableClient = tableServiceClient.GetTableClient(tableName);
                await tableClient.CreateIfNotExistsAsync();

                // Insert the data into the table
                CertificationEntity entity = new CertificationEntity
                {
                    PartitionKey = "CertificationRecords",
                    RowKey = input.Id,
                    Name = input.Name,
                    Certification = input.Certification,
                    Exp = input.Exp.ToString(),
                    Programming = input.Programming
                };

                await tableClient.AddEntityAsync(entity);

                return new OkObjectResult($"Data inserted with Row Key ID: {input.Id}");
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

                // response.StatusCode = 500;

                // return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }

    public class InputTableModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Certification { get; set; }
        public int Exp { get; set; }
        public string Programming { get; set; }
    }

    public class CertificationEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Name { get; set; }
        public string Certification { get; set; }
        public string Exp { get; set; }
        public string Programming { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}