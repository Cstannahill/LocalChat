using LocalChat.Application.Prompting.Composition;

namespace LocalChat.Application.Abstractions.Prompting;

public interface IPromptComposer
{
    PromptCompositionResult Compose(PromptCompositionContext context);
}