using LocalChat.Application.Features.Commands;

namespace LocalChat.Application.Tests;

public sealed class SlashCommandParserTests
{
    [Fact]
    public void Parse_Scene_Command_Produces_SceneSet()
    {
        var parser = new SlashCommandParser();

        var parsed = parser.Parse("/scene A ruined orbital station with flashing warning lights.");

        Assert.Equal(CommandType.SceneSet, parsed.Type);
        Assert.Equal("scene", parsed.CommandName);
        Assert.Equal("A ruined orbital station with flashing warning lights.", parsed.Argument);
    }

    [Fact]
    public void Parse_Ooc_Toggle_Produces_OocToggle()
    {
        var parser = new SlashCommandParser();

        var parsed = parser.Parse("/ooc toggle");

        Assert.Equal(CommandType.OocToggle, parsed.Type);
        Assert.Equal("ooc", parsed.CommandName);
    }
}
