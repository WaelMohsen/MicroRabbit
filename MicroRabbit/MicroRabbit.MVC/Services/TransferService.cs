using MicroRabbit.MVC.Models.DTO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace MicroRabbit.MVC.Services
{
    public sealed class TransferService : ITransferService
    {
        private readonly HttpClient _apiClient;

        public TransferService(HttpClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task Transfer(TransferDto transferDto)
        {
            const string uri = "https://localhost:5001/api/Banking";
            var transferContent = new StringContent(JsonConvert.SerializeObject(transferDto),
                                            System.Text.Encoding.UTF8, "application/json");
            var response = await _apiClient.PostAsync(uri, transferContent);
            response.EnsureSuccessStatusCode();
        }
    }
}
