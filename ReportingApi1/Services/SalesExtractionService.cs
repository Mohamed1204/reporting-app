using Microsoft.Extensions.AI;

namespace ReportingApi1.Services
{
    public interface ISalesExtractionService
    {
        public Task<string> ExtractAsync(string text, CancellationToken ct);
    }
    public class SalesExtractionService : ISalesExtractionService
    {
        private readonly IChatClient _chatClient;

        public SalesExtractionService(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        // Temporary Step 1 body: proves the model call works end to end.
        // Step 2 replaces this with a system prompt and structured output.
        public async Task<string> ExtractAsync(string text, CancellationToken ct)
        {
            var options = new ChatOptions { MaxOutputTokens = 4000 };
            var response = await _chatClient.GetResponseAsync(text, options, ct);
            return response.Text;
        }
    }
}
