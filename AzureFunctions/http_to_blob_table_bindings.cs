using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace AzureFunctions
{
    public static class http_to_blob_table_bindings
    {
        [FunctionName("http_to_blob_table_bindings")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] RequestModel model,
            [Blob("certifications/{Id}.txt", FileAccess.Write, Connection = "AzureWebJobsStorage")] Stream stream,
            ILogger log)
        {
            using var writer = new StreamWriter(stream);

            string blobContent = $"ID: {model.Id}\nName: {model.Name}\nCertification: {model.Certification}\nExp: {model.Exp} years\nProgramming: {model.Programming}";

            await writer.WriteLineAsync(blobContent);

            return new OkResult();
        }
    }

    public class RequestModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Certification { get; set; }
        public int Exp { get; set; }
        public string Programming { get; set; }
    }
}