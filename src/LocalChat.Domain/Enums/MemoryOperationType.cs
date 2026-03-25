namespace LocalChat.Domain.Enums;

public enum MemoryOperationType
{
    CreatedAutomatic = 0,
    ReinforcedAutomatic = 1,
    SupersededAutomatic = 2,
    PromotedToAgent = 3,
    DemotedToConversation = 4,
    MergedIntoTarget = 5,
    UndoApplied = 6,
    Imported = 7
}
