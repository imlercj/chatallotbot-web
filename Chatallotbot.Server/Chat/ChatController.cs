using Chatallotbot.Server.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Chatallotbot.Server.Chat;

[ApiController]
[Route("api/[controller]")]
public class ChatController(IChatService chatService, ILogger<ChatController> logger)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Message)) return BadRequest(new { error = "Message cannot be empty." });
        if (request.Message.Length > 1000)
            return BadRequest(new { error = "Message exceeds maximum length of 1000 characters." });

        try
        {
            var response = await chatService.SendMessage(request, cancellationToken);
            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Chat request was cancelled.");
            return StatusCode(499, new { error = "Request was cancelled." });
        }
        catch (EmptyChatResponse ex)
        {
            logger.LogError(ex, "Error processing chat request.");
            return StatusCode(502, new { error = "No response from chat service." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing chat request.");
            return StatusCode(500, new { error = "Internal server error." });
        }
    }
}