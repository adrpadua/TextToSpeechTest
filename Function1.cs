using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace TextToSpeechTest
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("Function1")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("Calling OpenAI API...");

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add(
                "Authorization", $"Bearer {Environment.GetEnvironmentVariable("OpenAIKey")}"
            );

            var payload = new
            {
                model = "gpt-4o-mini-tts",
                input = "May? Come on. You can't just sit here all day. How long you been sittin' here anyway? You want me to go outside and get you something? Some potato chips or something?",
                instructions = "You are a fast talking new yorker with a transatlantic accent. You're trying to comfort your depressed lover and you're trying to be gentle",
                voice = "verse"
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.openai.com/v1/audio/speech", content);
            var mp3Bytes = await response.Content.ReadAsByteArrayAsync();

            return new FileContentResult(mp3Bytes, "audio/mpeg")
            {
                FileDownloadName = $"speech_{DateTime.Now}.mp3"
            };
        }
    }
}
