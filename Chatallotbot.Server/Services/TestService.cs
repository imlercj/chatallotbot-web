namespace Chatallotbot.Server.Services;

public class TestService(IConfiguration configuration)
{
    public void TestMethod()
    {
        var test = configuration["openaikey"];
    }
}