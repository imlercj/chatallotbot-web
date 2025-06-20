using System.Diagnostics.CodeAnalysis;
using Chatallotbot.Server.Configuration;
using Chatallotbot.Server.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Chatallotbot.Server.Chat;

[ApiController]
[Route("api/[controller]")]
public class ChatController(ChatAgent chatAgent, ILogger<ChatController> logger)
    : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(10);
    }
    
    [HttpGet("test")]
    public IActionResult Test()
    {
        var history = new ChatHistory();
        history.AddAssistantMessage("Hello, how are you?");
       
        return Ok(history);
    }
    
    [HttpPost]
    [Experimental("SKEXP0130")]
    public async Task<ActionResult<ChatResponse>> Send([FromBody] ChatHistory messageHistory,
        CancellationToken cancellationToken)
    {
        if (messageHistory.Count == 0 || messageHistory.Last().Content is null) return BadRequest(new { error = "Message cannot be empty." });
        if (messageHistory.Last().Content!.Length > AppConfig.ChatSettings.MaxRequestLength)
            return BadRequest(new { error = "Message exceeds maximum length of 1000 characters." });

        try
        {
            var response = await chatAgent.Chat(messageHistory, cancellationToken);
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