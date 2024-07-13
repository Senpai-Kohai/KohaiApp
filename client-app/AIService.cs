using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace client_app
{
    public class AIService
    {
        private readonly HttpClient _httpClient;
        private AppConfiguration _config;

        public AIService(HttpClient httpClient, AppConfiguration config)
        {
            _config = config;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string?> GetAIResponse(string prompt)
        {
            if (string.IsNullOrWhiteSpace(_config.ChatGPTApiKey) || string.IsNullOrWhiteSpace(_config.ChatGPTApiUrl))
            {
                Debug.WriteLine($"Error communicating with ChatGPT API endpoint: API key or url not configured.");

                return null;
            }

            var requestBody = new JObject
            {
                { "prompt", prompt },
                { "max_tokens", 100 }
            };

            string jsonBody = requestBody.ToString();

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, _config.ChatGPTApiUrl))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ChatGPTApiKey);
                requestMessage.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject responseObject = JObject.Parse(jsonResponse);

                    JArray? choices = responseObject["choices"] as JArray;
                    if (choices != null && choices.Count > 0)
                    {
                        string? text = choices[0]?["text"]?.ToString();
                        return text;
                    }
                    else
                    {
                        return "No choices found in response.";
                    }
                }
                else
                {
                    Debug.WriteLine($"Error communicating with ChatGPT API endpoint at {_config.ChatGPTApiUrl}. Reason: {response.ReasonPhrase}");

                    return $"Error: {response.ReasonPhrase}";
                }
            }
        }
    }
}
