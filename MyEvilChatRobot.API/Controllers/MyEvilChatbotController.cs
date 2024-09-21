using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyEvilChatRobot.API.Models;
using MyEvilChatRobot.SemanticKernel;

namespace MyEvilChatRobot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyEvilChatbotController : ControllerBase
    {
        private readonly IChatService _chat;


        public MyEvilChatbotController(IChatService chat)
        {
            _chat = chat;
        }

        [HttpPost("stream")]
        public async Task StreamLlmResponse([FromBody] ChatInput userInput)
        {
            try
            {
                Response.ContentType = "text/event-stream"; // Using Server-Sent Events (SSE)

                await foreach (var chunk in GetLLMResponseChunks(userInput.InputText))
                {
                    await Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes($"data: {chunk}\n\n"));
                    await Response.Body.FlushAsync();
                }
            }
            catch (Exception e)
            {
                await Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes($"Error fetching the response"));
            }

        }

        // Mock method: simulate LLM streaming chunks
        private async IAsyncEnumerable<string> GetLLMResponseChunks(string input)
        {
            await foreach (var chunk in _chat.GetResponse(input))
            {
                yield return chunk;
            }
        }

    }
}
