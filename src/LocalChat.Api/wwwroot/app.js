const state = {
  characters: [],
  selectedCharacterId: null,
  selectedCharacterDetail: null,
  characterEditorMode: "edit",

  personas: [],
  selectedPersonaId: null,
  selectedPersonaDetail: null,
  personaEditorMode: "edit",

  modelProfiles: [],
  selectedModelProfileId: null,
  selectedModelProfileDetail: null,
  modelProfileEditorMode: "edit",

  generationPresets: [],
  selectedGenerationPresetId: null,
  selectedGenerationPresetDetail: null,
  generationPresetEditorMode: "edit",

  conversations: [],
  activeConversationId: null,
  activeCharacterDetail: null,
  messages: [],
  isStreaming: false,
  memories: [],
  proposals: [],
  lorebooks: [],
};

const authoringEnhancementState = {
  targetElementId: null,
  entityType: null,
  fieldName: null,
};

const pendingTurnRuntimeOverride = {
  active: false,
  provider: null,
  modelIdentifier: null,
};

let appRuntimeDefaults = {
  defaultPersonaId: null,
  defaultModelProfileId: null,
  defaultGenerationPresetId: null,
};

const characterSelect = document.getElementById("characterSelect");
const refreshCharactersBtn = document.getElementById("refreshCharactersBtn");
const newCharacterBtn = document.getElementById("newCharacterBtn");
const saveCharacterBtn = document.getElementById("saveCharacterBtn");
const deleteCharacterBtn = document.getElementById("deleteCharacterBtn");
const characterNameInput = document.getElementById("characterNameInput");
const characterDescriptionInput = document.getElementById(
  "characterDescriptionInput",
);
const characterGreetingInput = document.getElementById(
  "characterGreetingInput",
);
const characterScenarioInput = document.getElementById(
  "characterScenarioInput",
);
const characterPersonalityInput = document.getElementById(
  "characterPersonalityInput",
);
const characterModelProfileSelect = document.getElementById(
  "characterModelProfileSelect",
);
const characterGenerationPresetSelect = document.getElementById(
  "characterGenerationPresetSelect",
);
const characterDefaultTtsVoiceInput = document.getElementById(
  "characterDefaultTtsVoiceInput",
);
const characterImageFileInput = document.getElementById(
  "characterImageFileInput",
);
const uploadCharacterImageBtn = document.getElementById(
  "uploadCharacterImageBtn",
);
const removeCharacterImageBtn = document.getElementById(
  "removeCharacterImageBtn",
);
const characterImageStatus = document.getElementById("characterImageStatus");
const characterImagePreview = document.getElementById("characterImagePreview");
const characterDefaultVisualStylePresetSelect = document.getElementById(
  "characterDefaultVisualStylePresetSelect",
);
const characterDefaultVisualPromptPrefixInput = document.getElementById(
  "characterDefaultVisualPromptPrefixInput",
);
const characterDefaultVisualNegativePromptInput = document.getElementById(
  "characterDefaultVisualNegativePromptInput",
);
const addSampleDialogueBtn = document.getElementById("addSampleDialogueBtn");
const characterSampleDialogueList = document.getElementById(
  "characterSampleDialogueList",
);

const personaSelect = document.getElementById("personaSelect");
const refreshPersonasBtn = document.getElementById("refreshPersonasBtn");
const newPersonaBtn = document.getElementById("newPersonaBtn");
const savePersonaBtn = document.getElementById("savePersonaBtn");
const deletePersonaBtn = document.getElementById("deletePersonaBtn");
const personaNameInput = document.getElementById("personaNameInput");
const personaDisplayNameInput = document.getElementById(
  "personaDisplayNameInput",
);
const personaDescriptionInput = document.getElementById(
  "personaDescriptionInput",
);
const personaTraitsInput = document.getElementById("personaTraitsInput");
const personaPreferencesInput = document.getElementById(
  "personaPreferencesInput",
);
const personaInstructionsInput = document.getElementById(
  "personaInstructionsInput",
);

const refreshConversationsBtn = document.getElementById(
  "refreshConversationsBtn",
);
const newConversationBtn = document.getElementById("newConversationBtn");
const conversationList = document.getElementById("conversationList");
const conversationTitle = document.getElementById("conversationTitle");
const activeConversationHeaderAvatar = document.getElementById(
  "activeConversationHeaderAvatar",
);
const statusText = document.getElementById("statusText");
const directorStatusText = document.getElementById("directorStatusText");
const sceneStatusText = document.getElementById("sceneStatusText");
const activeConversationRuntimeBadge = document.getElementById(
  "activeConversationRuntimeBadge",
);
const activeConversationRuntimeModel = document.getElementById(
  "activeConversationRuntimeModel",
);
const activeConversationRuntimeSource = document.getElementById(
  "activeConversationRuntimeSource",
);
const conversationPersonaSelect = document.getElementById(
  "conversationPersonaSelect",
);
const conversationModelProfileOverrideSelect = document.getElementById(
  "conversationModelProfileOverrideSelect",
);
const conversationGenerationPresetOverrideSelect = document.getElementById(
  "conversationGenerationPresetOverrideSelect",
);
const saveConversationSettingsBtn = document.getElementById(
  "saveConversationSettingsBtn",
);
const clearConversationRuntimeOverrideBtn = document.getElementById(
  "clearConversationRuntimeOverrideBtn",
);
const conversationSettingsStatus = document.getElementById(
  "conversationSettingsStatus",
);
const defaultPersonaSelect = document.getElementById("defaultPersonaSelect");
const defaultModelProfileSelect = document.getElementById(
  "defaultModelProfileSelect",
);
const defaultGenerationPresetSelect = document.getElementById(
  "defaultGenerationPresetSelect",
);
const saveAppDefaultsBtn = document.getElementById("saveAppDefaultsBtn");
const appDefaultsStatus = document.getElementById("appDefaultsStatus");
const datasetExportCurrentConversationOnlyInput = document.getElementById(
  "datasetExportCurrentConversationOnlyInput",
);
const datasetExportSelectedOnlyInput = document.getElementById(
  "datasetExportSelectedOnlyInput",
);
const datasetExportProviderInput = document.getElementById(
  "datasetExportProviderInput",
);
const datasetExportModelContainsInput = document.getElementById(
  "datasetExportModelContainsInput",
);
const datasetExportCreatedFromInput = document.getElementById(
  "datasetExportCreatedFromInput",
);
const datasetExportCreatedToInput = document.getElementById(
  "datasetExportCreatedToInput",
);
const datasetExportConversationCreatedFromInput = document.getElementById(
  "datasetExportConversationCreatedFromInput",
);
const datasetExportConversationCreatedToInput = document.getElementById(
  "datasetExportConversationCreatedToInput",
);
const datasetExportMaxCountInput = document.getElementById(
  "datasetExportMaxCountInput",
);
const datasetExportFormatInput = document.getElementById(
  "datasetExportFormatInput",
);
const exportPromptDatasetBtn = document.getElementById(
  "exportPromptDatasetBtn",
);
const datasetExportStatus = document.getElementById("datasetExportStatus");
const memoryMergeSourceIdInput = document.getElementById(
  "memoryMergeSourceIdInput",
);
const memoryMergeTargetIdInput = document.getElementById(
  "memoryMergeTargetIdInput",
);
const memoryMergeStrategyInput = document.getElementById(
  "memoryMergeStrategyInput",
);
const previewMergeMemoryItemsBtn = document.getElementById(
  "previewMergeMemoryItemsBtn",
);
const memoryMergePreview = document.getElementById("memoryMergePreview");
const mergeMemoryItemsBtn = document.getElementById("mergeMemoryItemsBtn");
const memoryMergeStatus = document.getElementById("memoryMergeStatus");
const memoryExportCurrentConversationOnlyInput = document.getElementById(
  "memoryExportCurrentConversationOnlyInput",
);
const memoryExportScopeInput = document.getElementById(
  "memoryExportScopeInput",
);
const memoryExportKindInput = document.getElementById("memoryExportKindInput");
const memoryExportCategoryContainsInput = document.getElementById(
  "memoryExportCategoryContainsInput",
);
const memoryExportActiveOnlyInput = document.getElementById(
  "memoryExportActiveOnlyInput",
);
const memoryExportCreatedFromInput = document.getElementById(
  "memoryExportCreatedFromInput",
);
const memoryExportCreatedToInput = document.getElementById(
  "memoryExportCreatedToInput",
);
const memoryExportUpdatedFromInput = document.getElementById(
  "memoryExportUpdatedFromInput",
);
const memoryExportUpdatedToInput = document.getElementById(
  "memoryExportUpdatedToInput",
);
const memoryExportMaxCountInput = document.getElementById(
  "memoryExportMaxCountInput",
);
const memoryExportFormatInput = document.getElementById(
  "memoryExportFormatInput",
);
const exportMemoryDatasetBtn = document.getElementById(
  "exportMemoryDatasetBtn",
);
const memoryExportStatus = document.getElementById("memoryExportStatus");
const memorySuggestionConflictsCurrentConversationOnlyInput =
  document.getElementById(
    "memorySuggestionConflictsCurrentConversationOnlyInput",
  );
const memorySuggestionConflictsMaxCountInput = document.getElementById(
  "memorySuggestionConflictsMaxCountInput",
);
const memoryConflictsStrategyInput = document.getElementById(
  "memoryConflictsStrategyInput",
);
const loadMemorySuggestionConflictsBtn = document.getElementById(
  "loadMemorySuggestionConflictsBtn",
);
const resolveMemoryConflictsBulkBtn = document.getElementById(
  "resolveMemoryConflictsBulkBtn",
);
const memorySuggestionConflictsStatus = document.getElementById(
  "memorySuggestionConflictsStatus",
);
const memorySuggestionConflictsList = document.getElementById(
  "memorySuggestionConflictsList",
);
const memoryImportFileInput = document.getElementById("memoryImportFileInput");
const memoryImportFormatInput = document.getElementById(
  "memoryImportFormatInput",
);
const memoryImportStrategyInput = document.getElementById(
  "memoryImportStrategyInput",
);
const memoryImportCurrentConversationOverrideInput = document.getElementById(
  "memoryImportCurrentConversationOverrideInput",
);
const importMemoryBtn = document.getElementById("importMemoryBtn");
const memoryImportStatus = document.getElementById("memoryImportStatus");
const memoryProvenanceStatus = document.getElementById(
  "memoryProvenanceStatus",
);
const memoryProvenanceDetails = document.getElementById(
  "memoryProvenanceDetails",
);
const messagesEl = document.getElementById("messages");
const messageInput = document.getElementById("messageInput");
const sendBtn = document.getElementById("sendBtn");
const suggestUserMessageBtn = document.getElementById("suggestUserMessageBtn");
const suggestedUserMessageMeta = document.getElementById(
  "suggestedUserMessageMeta",
);
const ttsVoiceOverrideInput = document.getElementById("ttsVoiceOverrideInput");

const imagePromptInput = document.getElementById("imagePromptInput");
const imageNegativePromptInput = document.getElementById("imageNegativePromptInput");
const imageWidthInput = document.getElementById("imageWidthInput");
const imageHeightInput = document.getElementById("imageHeightInput");
const imageStepsInput = document.getElementById("imageStepsInput");
const imageCfgInput = document.getElementById("imageCfgInput");
const imageSeedInput = document.getElementById("imageSeedInput");
const generateImageBtn = document.getElementById("generateImageBtn");
const imageStylePresetSelect = document.getElementById(
  "imageStylePresetSelect",
);
const buildAndGenerateImageBtn = document.getElementById(
  "buildAndGenerateImageBtn",
);
const generatedImagesGallery = document.getElementById("generatedImagesGallery");
const buildImagePromptFromContextBtn = document.getElementById(
  "buildImagePromptFromContextBtn",
);
const contextualImagePromptPreview = document.getElementById(
  "contextualImagePromptPreview",
);

const refreshMemoryBtn = document.getElementById("refreshMemoryBtn");
const memoryUseConversationScope = document.getElementById(
  "memoryUseConversationScope",
);
const memoryCategorySelect = document.getElementById("memoryCategorySelect");
const memoryContentInput = document.getElementById("memoryContentInput");
const memoryPinnedInput = document.getElementById("memoryPinnedInput");
const memoryConversationScopedInput = document.getElementById(
  "memoryConversationScopedInput",
);
const createMemoryBtn = document.getElementById("createMemoryBtn");
const memoryList = document.getElementById("memoryList");

const pendingMemoryProposalsEl =
  document.getElementById("pendingMemoryProposals") ??
  document.getElementById("proposalList");
const generateMemoryProposalsBtn =
  document.getElementById("generateMemoryProposalsBtn") ??
  document.getElementById("generateProposalsBtn");
const refreshMemoryConflictsBtn = document.getElementById(
  "refreshMemoryConflictsBtn",
);
const memoryConflictsMeta = document.getElementById("memoryConflictsMeta");
const memoryConflictsList = document.getElementById("memoryConflictsList");
const memoryConflictActionResult = document.getElementById(
  "memoryConflictActionResult",
);
const retrievalMemoryExplanationsMeta = document.getElementById(
  "retrievalMemoryExplanationsMeta",
);
const retrievalMemoryExplanationsList = document.getElementById(
  "retrievalMemoryExplanationsList",
);
const retrievalLoreExplanationsMeta = document.getElementById(
  "retrievalLoreExplanationsMeta",
);
const retrievalLoreExplanationsList = document.getElementById(
  "retrievalLoreExplanationsList",
);
const promptSlotWinnersMeta = document.getElementById("promptSlotWinnersMeta");
const promptSlotWinnersList = document.getElementById("promptSlotWinnersList");
const promptSuppressedDurableMeta = document.getElementById(
  "promptSuppressedDurableMeta",
);
const promptSuppressedDurableList = document.getElementById(
  "promptSuppressedDurableList",
);
const backgroundProposalStatusPanel = document.getElementById(
  "backgroundProposalStatusPanel",
);
const refreshBackgroundProposalStatusBtn = document.getElementById(
  "refreshBackgroundProposalStatusBtn",
);
const runBackgroundProposalNowBtn = document.getElementById(
  "runBackgroundProposalNowBtn",
);
const refreshBackgroundWorkBtn = document.getElementById(
  "refreshBackgroundWorkBtn",
);
const backgroundWorkMeta = document.getElementById("backgroundWorkMeta");
const backgroundWorkPendingList = document.getElementById(
  "backgroundWorkPendingList",
);
const manualRefreshSummaryBtn = document.getElementById(
  "manualRefreshSummaryBtn",
);
const manualExtractMemoryBtn = document.getElementById(
  "manualExtractMemoryBtn",
);
const manualReindexRetrievalBtn = document.getElementById(
  "manualReindexRetrievalBtn",
);
const backgroundWorkTriggerResult = document.getElementById(
  "backgroundWorkTriggerResult",
);
const rebuildMemoryKeysBtn = document.getElementById("rebuildMemoryKeysBtn");
const reindexAllRetrievalBtn = document.getElementById(
  "reindexAllRetrievalBtn",
);
const pruneMemoryExtractionAuditBtn = document.getElementById(
  "pruneMemoryExtractionAuditBtn",
);
const exportMemoryExtractionAuditBtn = document.getElementById(
  "exportMemoryExtractionAuditBtn",
);
const pruneStaleSceneStateBtn = document.getElementById(
  "pruneStaleSceneStateBtn",
);
const maintenanceResult = document.getElementById("maintenanceResult");
const refreshSceneStateInspectionBtn = document.getElementById(
  "refreshSceneStateInspectionBtn",
);
const sceneStateInspectionMeta = document.getElementById(
  "sceneStateInspectionMeta",
);
const sceneStateActiveList = document.getElementById("sceneStateActiveList");
const sceneStateReplacementHistoryList = document.getElementById(
  "sceneStateReplacementHistoryList",
);
const sceneStateFamilyCollisionList = document.getElementById(
  "sceneStateFamilyCollisionList",
);
const refreshMemoryExtractionAuditBtn = document.getElementById(
  "refreshMemoryExtractionAuditBtn",
);
const memoryExtractionAuditMeta = document.getElementById(
  "memoryExtractionAuditMeta",
);
const memoryExtractionAuditList = document.getElementById(
  "memoryExtractionAuditList",
);
const promptSceneStateDebugMeta = document.getElementById(
  "promptSceneStateDebugMeta",
);
const promptSceneStateSelectedList = document.getElementById(
  "promptSceneStateSelectedList",
);
const promptSceneStateSuppressedList = document.getElementById(
  "promptSceneStateSuppressedList",
);
const promptDurableMemoryDebugMeta = document.getElementById(
  "promptDurableMemoryDebugMeta",
);
const promptDurableMemorySelectedList = document.getElementById(
  "promptDurableMemorySelectedList",
);
const promptDurableMemorySuppressedList = document.getElementById(
  "promptDurableMemorySuppressedList",
);

const inspectRetrievalBtn = document.getElementById("inspectRetrievalBtn");
const retrievalQueryInput = document.getElementById("retrievalQueryInput");
const retrievalInspectionResults = document.getElementById(
  "retrievalInspectionResults",
);
const retrievalInspectionRuntimeRow = document.getElementById(
  "retrievalInspectionRuntimeRow",
);
const retrievalInspectionRuntimeMeta = document.getElementById(
  "retrievalInspectionRuntimeMeta",
);

const inspectPromptBtn = document.getElementById("inspectPromptBtn");
const promptInspectionResults = document.getElementById(
  "promptInspectionResults",
);
const promptInspectionRuntimeRow = document.getElementById(
  "promptInspectionRuntimeRow",
);
const promptInspectionRuntimeMeta = document.getElementById(
  "promptInspectionRuntimeMeta",
);

const inspectSummaryBtn = document.getElementById("inspectSummaryBtn");
const summaryInspectionResults = document.getElementById(
  "summaryInspectionResults",
);

const refreshLorebooksBtn = document.getElementById("refreshLorebooksBtn");
const lorebookNameInput = document.getElementById("lorebookNameInput");
const lorebookDescriptionInput = document.getElementById(
  "lorebookDescriptionInput",
);
const createLorebookBtn = document.getElementById("createLorebookBtn");
const lorebookSelect = document.getElementById("lorebookSelect");
const loreEntryTitleInput = document.getElementById("loreEntryTitleInput");
const loreEntryContentInput = document.getElementById("loreEntryContentInput");
const loreEntryEnabledInput = document.getElementById("loreEntryEnabledInput");
const createLoreEntryBtn = document.getElementById("createLoreEntryBtn");
const lorebookList = document.getElementById("lorebookList");
const exportCharacterBtn = document.getElementById("exportCharacterBtn");
const importCharacterBtn = document.getElementById("importCharacterBtn");
const characterImportJsonInput = document.getElementById(
  "characterImportJsonInput",
);

const assignPersonaToConversationBtn = document.getElementById(
  "assignPersonaToConversationBtn",
);
const setDefaultPersonaBtn = document.getElementById("setDefaultPersonaBtn");
const exportPersonaBtn = document.getElementById("exportPersonaBtn");
const importPersonaBtn = document.getElementById("importPersonaBtn");
const personaImportJsonInput = document.getElementById(
  "personaImportJsonInput",
);

const modelProfileSelect = document.getElementById("modelProfileSelect");
const refreshModelProfilesBtn = document.getElementById(
  "refreshModelProfilesBtn",
);
const newModelProfileBtn = document.getElementById("newModelProfileBtn");
const saveModelProfileBtn = document.getElementById("saveModelProfileBtn");
const deleteModelProfileBtn = document.getElementById("deleteModelProfileBtn");
const modelProfileNameInput = document.getElementById("modelProfileNameInput");
const modelProfileProviderTypeInput = document.getElementById(
  "modelProfileProviderTypeInput",
);
const modelProfileModelIdentifierInput = document.getElementById(
  "modelProfileModelIdentifierInput",
);
const modelProfileModelHelp = document.getElementById("modelProfileModelHelp");
const modelProfileContextWindowInput = document.getElementById(
  "modelProfileContextWindowInput",
);
const modelProfileNotesInput = document.getElementById(
  "modelProfileNotesInput",
);
const turnOverrideProviderInput = document.getElementById(
  "turnOverrideProviderInput",
);
const turnOverrideModelIdentifierInput = document.getElementById(
  "turnOverrideModelIdentifierInput",
);
const activateTurnOverrideBtn = document.getElementById(
  "activateTurnOverrideBtn",
);
const clearTurnOverrideBtn = document.getElementById("clearTurnOverrideBtn");
const turnOverrideStatus = document.getElementById("turnOverrideStatus");

const generationPresetSelect = document.getElementById(
  "generationPresetSelect",
);
const refreshGenerationPresetsBtn = document.getElementById(
  "refreshGenerationPresetsBtn",
);
const newGenerationPresetBtn = document.getElementById(
  "newGenerationPresetBtn",
);
const saveGenerationPresetBtn = document.getElementById(
  "saveGenerationPresetBtn",
);
const deleteGenerationPresetBtn = document.getElementById(
  "deleteGenerationPresetBtn",
);
const generationPresetNameInput = document.getElementById(
  "generationPresetNameInput",
);
const generationPresetTemperatureInput = document.getElementById(
  "generationPresetTemperatureInput",
);
const generationPresetTopPInput = document.getElementById(
  "generationPresetTopPInput",
);
const generationPresetRepeatPenaltyInput = document.getElementById(
  "generationPresetRepeatPenaltyInput",
);
const generationPresetMaxOutputTokensInput = document.getElementById(
  "generationPresetMaxOutputTokensInput",
);
const generationPresetStopSequencesInput = document.getElementById(
  "generationPresetStopSequencesInput",
);
const generationPresetNotesInput = document.getElementById(
  "generationPresetNotesInput",
);

const promptAuthoringSummary = document.getElementById(
  "promptAuthoringSummary",
);
const authoringTemplatesMeta = document.getElementById(
  "authoringTemplatesMeta",
);
const authoringTemplatesList = document.getElementById(
  "authoringTemplatesList",
);
const authoringEnhancementMeta = document.getElementById(
  "authoringEnhancementMeta",
);
const authoringOriginalText = document.getElementById("authoringOriginalText");
const authoringSuggestedText = document.getElementById(
  "authoringSuggestedText",
);
const authoringEnhancementRationale = document.getElementById(
  "authoringEnhancementRationale",
);
const applyAuthoringEnhancementBtn = document.getElementById(
  "applyAuthoringEnhancementBtn",
);
const discardAuthoringEnhancementBtn = document.getElementById(
  "discardAuthoringEnhancementBtn",
);
const authoringStarterPacksMeta = document.getElementById(
  "authoringStarterPacksMeta",
);
const authoringStarterPacksList = document.getElementById(
  "authoringStarterPacksList",
);

const bundleConceptInput = document.getElementById("bundleConceptInput");
const bundleVibeInput = document.getElementById("bundleVibeInput");
const bundleRelationshipInput = document.getElementById(
  "bundleRelationshipInput",
);
const bundleSettingInput = document.getElementById("bundleSettingInput");
const generateFullBundleBtn = document.getElementById("generateFullBundleBtn");

const bundleGenerationMeta = document.getElementById("bundleGenerationMeta");
const generatedCharacterNameInput = document.getElementById(
  "generatedCharacterNameInput",
);
const generatedCharacterDescriptionInput = document.getElementById(
  "generatedCharacterDescriptionInput",
);
const generatedCharacterPersonalityInput = document.getElementById(
  "generatedCharacterPersonalityInput",
);
const generatedCharacterScenarioInput = document.getElementById(
  "generatedCharacterScenarioInput",
);
const generatedCharacterGreetingInput = document.getElementById(
  "generatedCharacterGreetingInput",
);
const generatedPersonaDisplayNameInput = document.getElementById(
  "generatedPersonaDisplayNameInput",
);
const generatedPersonaDescriptionInput = document.getElementById(
  "generatedPersonaDescriptionInput",
);
const generatedPersonaTraitsInput = document.getElementById(
  "generatedPersonaTraitsInput",
);
const generatedPersonaPreferencesInput = document.getElementById(
  "generatedPersonaPreferencesInput",
);
const generatedPersonaAdditionalInstructionsInput = document.getElementById(
  "generatedPersonaAdditionalInstructionsInput",
);
const bundleGenerationRationale = document.getElementById(
  "bundleGenerationRationale",
);
const applyGeneratedBundleBtn = document.getElementById(
  "applyGeneratedBundleBtn",
);
const discardGeneratedBundleBtn = document.getElementById(
  "discardGeneratedBundleBtn",
);

const checkAuthoringConsistencyBtn = document.getElementById(
  "checkAuthoringConsistencyBtn",
);
const authoringConsistencyMeta = document.getElementById(
  "authoringConsistencyMeta",
);
const authoringConsistencyList = document.getElementById(
  "authoringConsistencyList",
);

const IMAGE_STYLE_PRESETS = {
  none: {
    label: "None",
    positivePrefix: "",
    negativeAppend: "",
  },
  cinematic_romance: {
    label: "Cinematic Romance",
    positivePrefix:
      "cinematic romantic scene, soft natural lighting, emotionally intimate composition, detailed environment, expressive body language",
    negativeAppend: "flat lighting, awkward pose, emotionless expression",
  },
  anime_illustration: {
    label: "Anime Illustration",
    positivePrefix:
      "high quality anime illustration, expressive eyes, clean linework, beautiful shading, dynamic composition",
    negativeAppend:
      "photorealistic skin texture, muddy shading, low detail anime",
  },
  painterly_fantasy: {
    label: "Painterly Fantasy",
    positivePrefix:
      "lush painterly fantasy artwork, rich brushwork, atmospheric lighting, elegant composition, vivid storytelling moment",
    negativeAppend:
      "photographic realism, bland composition, low detail painting",
  },
  soft_slice_of_life: {
    label: "Soft Slice of Life",
    positivePrefix:
      "soft slice-of-life illustration, warm atmosphere, gentle facial expressions, cozy framing, natural pose",
    negativeAppend:
      "harsh lighting, aggressive action pose, overly dramatic scene",
  },
  dramatic_cinematic: {
    label: "Dramatic Cinematic",
    positivePrefix:
      "dramatic cinematic still, high contrast lighting, strong composition, vivid action framing, atmospheric depth",
    negativeAppend: "washed out colors, weak composition, static framing",
  },
};

function downloadJson(filename, data) {
  const blob = new Blob([JSON.stringify(data, null, 2)], {
    type: "application/json",
  });
  const url = URL.createObjectURL(blob);

  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  a.remove();

  URL.revokeObjectURL(url);
}

function setStatus(text) {
  statusText.textContent = text;
}

function toUtcIsoOrEmpty(localDateTimeValue) {
  if (!localDateTimeValue) {
    return "";
  }

  const date = new Date(localDateTimeValue);
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  return date.toISOString();
}

function buildPromptDatasetExportUrl() {
  const params = new URLSearchParams();

  const currentConversationOnly =
    !!datasetExportCurrentConversationOnlyInput?.checked;
  if (currentConversationOnly) {
    if (!state.activeConversationId) {
      throw new Error("No active conversation is selected.");
    }

    params.set("conversationId", state.activeConversationId);
  }

  if (datasetExportSelectedOnlyInput?.checked) {
    params.set("selectedOnly", "true");
  }

  const provider = (datasetExportProviderInput?.value || "").trim();
  if (provider) {
    params.set("provider", provider);
  }

  const modelContains = (datasetExportModelContainsInput?.value || "").trim();
  if (modelContains) {
    params.set("modelContains", modelContains);
  }

  const createdFromUtc = toUtcIsoOrEmpty(
    datasetExportCreatedFromInput?.value || "",
  );
  if (createdFromUtc) {
    params.set("createdFromUtc", createdFromUtc);
  }

  const createdToUtc = toUtcIsoOrEmpty(
    datasetExportCreatedToInput?.value || "",
  );
  if (createdToUtc) {
    params.set("createdToUtc", createdToUtc);
  }

  const conversationCreatedFromUtc = toUtcIsoOrEmpty(
    datasetExportConversationCreatedFromInput?.value || "",
  );
  if (conversationCreatedFromUtc) {
    params.set("conversationCreatedFromUtc", conversationCreatedFromUtc);
  }

  const conversationCreatedToUtc = toUtcIsoOrEmpty(
    datasetExportConversationCreatedToInput?.value || "",
  );
  if (conversationCreatedToUtc) {
    params.set("conversationCreatedToUtc", conversationCreatedToUtc);
  }

  const maxCountRaw = (datasetExportMaxCountInput?.value || "").trim();
  if (maxCountRaw) {
    params.set("maxCount", maxCountRaw);
  }

  const format = (datasetExportFormatInput?.value || "json")
    .trim()
    .toLowerCase();
  if (format) {
    params.set("format", format);
  }

  const query = params.toString();
  return `/api/prompt-snapshots/export${query ? `?${query}` : ""}`;
}

function exportPromptDataset() {
  const url = buildPromptDatasetExportUrl();
  window.location.href = url;
}

async function setDefaultPersona(personaId) {
  const response = await fetch(`/api/personas/${personaId}/set-default`, {
    method: "POST",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to set default persona. ${text}`);
  }
}

async function mergeMemoryItems() {
  const response = await fetch("/api/memory/merge", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      sourceMemoryId: (memoryMergeSourceIdInput?.value || "").trim(),
      targetMemoryId: (memoryMergeTargetIdInput?.value || "").trim(),
      strategy: (memoryMergeStrategyInput?.value || "append_unique").trim(),
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to merge memory items. ${text}`);
  }

  return await response.json();
}

async function previewMergeMemoryItems() {
  const response = await fetch("/api/memory/merge-preview", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      sourceMemoryId: (memoryMergeSourceIdInput?.value || "").trim(),
      targetMemoryId: (memoryMergeTargetIdInput?.value || "").trim(),
      strategy: (memoryMergeStrategyInput?.value || "append_unique").trim(),
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to preview memory merge. ${text}`);
  }

  return await response.json();
}

function renderMemoryMergePreview(preview) {
  if (!memoryMergePreview) {
    return;
  }

  memoryMergePreview.innerHTML = `
    <div><strong>Strategy:</strong> ${escapeHtml(preview.strategy || "append_unique")}</div>
    <div><strong>Content changes:</strong> ${preview.contentWillChange ? "Yes" : "No"}</div>
    <div><strong>Result normalized key:</strong> ${escapeHtml(preview.resultNormalizedKey || "—")}</div>
    <div><strong>Result category:</strong> ${escapeHtml(preview.resultCategory || "—")}</div>

    <hr />

    <div><strong>Source content</strong></div>
    <pre>${escapeHtml(preview.sourceContent || "")}</pre>

    <div><strong>Target content</strong></div>
    <pre>${escapeHtml(preview.targetContent || "")}</pre>

    <div><strong>Merged result</strong></div>
    <pre>${escapeHtml(preview.mergedContent || "")}</pre>
  `;
}

async function resolveMemoryConflictsBulk() {
  const response = await fetch("/api/memory/conflicts/resolve-bulk", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      conversationId: memorySuggestionConflictsCurrentConversationOnlyInput?.checked
        ? state.activeConversationId || null
        : null,
      characterId: null,
      maxCount: Number(memorySuggestionConflictsMaxCountInput?.value || 50),
      strategy: memoryConflictsStrategyInput?.value || "append_unique",
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to bulk resolve memory conflicts. ${text}`);
  }

  return await response.json();
}

async function importMemoryDataset() {
  const file = memoryImportFileInput?.files?.[0];
  if (!file) {
    throw new Error("Choose a file to import.");
  }

  const formData = new FormData();
  formData.append("file", file);

  const params = new URLSearchParams();
  params.set("format", (memoryImportFormatInput?.value || "json").trim().toLowerCase());
  params.set(
    "strategy",
    (memoryImportStrategyInput?.value || "upsert_normalized_key")
      .trim()
      .toLowerCase(),
  );

  if (memoryImportCurrentConversationOverrideInput?.checked) {
    if (!state.activeConversationId) {
      throw new Error("No active conversation selected.");
    }

    params.set("conversationIdOverride", state.activeConversationId);
  }

  const response = await fetch(`/api/memory/import?${params.toString()}`, {
    method: "POST",
    body: formData,
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to import memory. ${text}`);
  }

  return await response.json();
}

async function loadMemoryProvenance(memoryId) {
  const response = await fetch(`/api/memory/${memoryId}/provenance`);

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to load memory provenance. ${text}`);
  }

  const data = await response.json();
  renderMemoryProvenance(data);
}

function renderMemoryProvenance(data) {
  if (!memoryProvenanceStatus || !memoryProvenanceDetails) {
    return;
  }

  memoryProvenanceStatus.textContent = `Loaded provenance for memory ${data.id}`;
  memoryProvenanceDetails.innerHTML = `
    <div><strong>Scope:</strong> ${escapeHtml(data.scopeType)}</div>
    <div><strong>Kind:</strong> ${escapeHtml(data.kind)}</div>
    <div><strong>Category:</strong> ${escapeHtml(data.category || "—")}</div>
    <div><strong>Normalized Key:</strong> ${escapeHtml(data.normalizedKey || "—")}</div>
    <div><strong>Source Seq:</strong> ${data.sourceMessageSequenceNumber ?? "—"}</div>
    <div><strong>Last Seen Seq:</strong> ${data.lastObservedSequenceNumber ?? "—"}</div>
    <div><strong>Superseded Seq:</strong> ${data.supersededAtSequenceNumber ?? "—"}</div>
    <div><strong>Content:</strong></div>
    <pre>${escapeHtml(data.content || "")}</pre>

    <hr />

    <div><strong>Audit Trail</strong></div>
    ${
      (data.auditEntries || []).length === 0
        ? `<div class="muted-text">No audit entries found.</div>`
        : data.auditEntries
            .map(
              (entry) => `
            <div class="proposal-card">
              <div><strong>${escapeHtml(entry.operationType)}</strong></div>
              <div class="muted-text">
                ${entry.createdAt} • Seq: ${entry.messageSequenceNumber ?? "—"} • Undone: ${entry.isUndone ? "Yes" : "No"}
              </div>
              <div class="muted-text">${escapeHtml(entry.note || "")}</div>
              <div><strong>Before:</strong> ${escapeHtml(entry.beforeContentPreview || "—")}</div>
              <div><strong>After:</strong> ${escapeHtml(entry.afterContentPreview || "—")}</div>
              ${
                entry.canUndo
                  ? `<div class="message-actions">
                      <button type="button" data-action="undo-memory-operation" data-audit-id="${entry.id}" data-memory-id="${data.id}">
                        Undo Operation
                      </button>
                    </div>`
                  : ""
              }
            </div>
          `,
            )
            .join("")
    }
  `;

  memoryProvenanceDetails
    .querySelectorAll('button[data-action="undo-memory-operation"]')
    .forEach((button) => {
      button.addEventListener("click", async () => {
        const auditId = button.dataset.auditId;
        const memoryId = button.dataset.memoryId;

        try {
          await undoMemoryOperation(auditId);
          await refreshMemoryViews();
          await loadMemoryProvenance(memoryId);
          try {
            await loadMemoryConflictSuggestions();
          } catch (refreshError) {
            console.error(refreshError);
          }
          setStatus("Memory operation undone");
        } catch (error) {
          console.error(error);
          setStatus("Memory undo failed");
        }
      });
    });
}

async function undoMemoryOperation(auditId) {
  const response = await fetch(`/api/memory/operations/${auditId}/undo`, {
    method: "POST",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to undo memory operation. ${text}`);
  }
}

async function loadMemoryConflictSuggestions() {
  const params = new URLSearchParams();

  if (memorySuggestionConflictsCurrentConversationOnlyInput?.checked) {
    if (!state.activeConversationId) {
      throw new Error("No active conversation selected.");
    }

    params.set("conversationId", state.activeConversationId);
  }

  const maxCount = (memorySuggestionConflictsMaxCountInput?.value || "50").trim();
  if (maxCount) {
    params.set("maxCount", maxCount);
  }

  const response = await fetch(`/api/memory/conflicts?${params.toString()}`);

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to load memory conflict suggestions. ${text}`);
  }

  const conflicts = await response.json();
  renderMemoryConflictSuggestions(conflicts);
}

function renderMemoryConflictSuggestions(conflicts) {
  if (!memorySuggestionConflictsList || !memorySuggestionConflictsStatus) {
    return;
  }

  memorySuggestionConflictsList.innerHTML = "";
  memorySuggestionConflictsStatus.textContent = `Conflict suggestions: ${(conflicts || []).length}`;

  if (!conflicts || conflicts.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No memory conflicts found.";
    memorySuggestionConflictsList.appendChild(empty);
    return;
  }

  for (const conflict of conflicts) {
    const card = document.createElement("div");
    card.className = "proposal-card";
    card.innerHTML = `
      <div><strong>Reason:</strong> ${escapeHtml(conflict.reason || "—")}</div>
      <div><strong>Suggested strategy:</strong> ${escapeHtml(conflict.suggestedStrategy || "append_unique")}</div>
      <div><strong>Target score:</strong> ${Number(conflict.targetScore || 0).toFixed(2)}</div>
      <div class="muted-text">
        ${(conflict.rankingExplanation || []).map((x) => escapeHtml(x)).join(" • ")}
      </div>

      <hr />

      <div><strong>Source</strong></div>
      <div class="muted-text">ID: ${escapeHtml(conflict.source.id)}</div>
      <div class="muted-text">Scope: ${escapeHtml(conflict.source.scopeType)} • Kind: ${escapeHtml(conflict.source.kind)}</div>
      <div class="muted-text">Key: ${escapeHtml(conflict.source.normalizedKey || "—")}</div>
      <div>${escapeHtml(conflict.source.content || "")}</div>

      <hr />

      <div><strong>Suggested Target</strong></div>
      <div class="muted-text">ID: ${escapeHtml(conflict.target.id)}</div>
      <div class="muted-text">Scope: ${escapeHtml(conflict.target.scopeType)} • Kind: ${escapeHtml(conflict.target.kind)}</div>
      <div class="muted-text">Key: ${escapeHtml(conflict.target.normalizedKey || "—")}</div>
      <div>${escapeHtml(conflict.target.content || "")}</div>

      <div class="message-actions">
        <button type="button" data-action="preview-memory-merge-suggestion"
          data-source-memory-id="${conflict.sourceMemoryId}"
          data-target-memory-id="${conflict.targetMemoryId}"
          data-strategy="${escapeHtml(conflict.suggestedStrategy || "append_unique")}">
          Preview Merge
        </button>
        <button type="button" data-action="accept-memory-merge-suggestion"
          data-source-memory-id="${conflict.sourceMemoryId}"
          data-target-memory-id="${conflict.targetMemoryId}"
          data-strategy="${escapeHtml(conflict.suggestedStrategy || "append_unique")}">
          Merge Into Suggested Target
        </button>
      </div>
    `;

    const previewButton = card.querySelector(
      'button[data-action="preview-memory-merge-suggestion"]',
    );
    if (previewButton) {
      previewButton.addEventListener("click", async () => {
        try {
          if (memoryMergeSourceIdInput) {
            memoryMergeSourceIdInput.value =
              previewButton.dataset.sourceMemoryId || "";
          }
          if (memoryMergeTargetIdInput) {
            memoryMergeTargetIdInput.value =
              previewButton.dataset.targetMemoryId || "";
          }
          if (memoryMergeStrategyInput) {
            memoryMergeStrategyInput.value =
              previewButton.dataset.strategy || "append_unique";
          }

          if (memoryMergePreview) {
            memoryMergePreview.textContent = "Loading merge preview...";
          }
          const preview = await previewMergeMemoryItems();
          renderMemoryMergePreview(preview);
          setStatus("Memory merge preview loaded");
        } catch (error) {
          console.error(error);
          if (memoryMergePreview) {
            memoryMergePreview.textContent =
              error.message || "Failed to preview merge suggestion.";
          }
          setStatus("Memory merge preview failed");
        }
      });
    }

    const mergeButton = card.querySelector(
      'button[data-action="accept-memory-merge-suggestion"]',
    );
    if (mergeButton) {
      mergeButton.addEventListener("click", async () => {
        try {
          const sourceMemoryId = mergeButton.dataset.sourceMemoryId;
          const targetMemoryId = mergeButton.dataset.targetMemoryId;
          const strategy = mergeButton.dataset.strategy || "append_unique";

          const response = await fetch("/api/memory/merge", {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({
              sourceMemoryId,
              targetMemoryId,
              strategy,
            }),
          });

          if (!response.ok) {
            const text = await response.text();
            throw new Error(text || "Failed to merge memory items.");
          }

          await refreshMemoryViews();
          await loadMemoryConflictSuggestions();
          setStatus("Memory conflict merged");
        } catch (error) {
          console.error(error);
          setStatus("Memory conflict merge failed");
        }
      });
    }

    memorySuggestionConflictsList.appendChild(card);
  }
}

async function previewPersonaDelete(personaId) {
  const response = await fetch(`/api/personas/${personaId}/delete-preview`);

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to load persona delete preview. ${text}`);
  }

  return await response.json();
}

async function deletePersonaWithPreview(personaId) {
  const preview = await previewPersonaDelete(personaId);

  let confirmMessage = `Delete persona "${preview.displayName}"?`;

  if (preview.isDefault) {
    if (preview.willPromoteReplacement) {
      confirmMessage += `\n\nThis is the default persona. "${preview.replacementDisplayName}" will become the new default automatically.`;
    } else {
      confirmMessage += "\n\nThis is the default persona. No replacement persona exists, so the default persona will be cleared.";
    }
  }

  if (!window.confirm(confirmMessage)) {
    return false;
  }

  const response = await fetch(`/api/personas/${personaId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to delete persona. ${text}`);
  }

  return true;
}

function buildMemoryExportUrl() {
  const params = new URLSearchParams();

  if (memoryExportCurrentConversationOnlyInput?.checked) {
    if (!state.activeConversationId) {
      throw new Error("No active conversation is selected.");
    }

    params.set("conversationId", state.activeConversationId);
  }

  const scope = (memoryExportScopeInput?.value || "").trim();
  if (scope) {
    params.set("scope", scope);
  }

  const kind = (memoryExportKindInput?.value || "").trim();
  if (kind) {
    params.set("kind", kind);
  }

  const categoryContains = (
    memoryExportCategoryContainsInput?.value || ""
  ).trim();
  if (categoryContains) {
    params.set("categoryContains", categoryContains);
  }

  if (memoryExportActiveOnlyInput?.checked) {
    params.set("activeOnly", "true");
  }

  const createdFromUtc = toUtcIsoOrEmpty(
    memoryExportCreatedFromInput?.value || "",
  );
  if (createdFromUtc) {
    params.set("createdFromUtc", createdFromUtc);
  }

  const createdToUtc = toUtcIsoOrEmpty(memoryExportCreatedToInput?.value || "");
  if (createdToUtc) {
    params.set("createdToUtc", createdToUtc);
  }

  const updatedFromUtc = toUtcIsoOrEmpty(
    memoryExportUpdatedFromInput?.value || "",
  );
  if (updatedFromUtc) {
    params.set("updatedFromUtc", updatedFromUtc);
  }

  const updatedToUtc = toUtcIsoOrEmpty(memoryExportUpdatedToInput?.value || "");
  if (updatedToUtc) {
    params.set("updatedToUtc", updatedToUtc);
  }

  const maxCount = (memoryExportMaxCountInput?.value || "").trim();
  if (maxCount) {
    params.set("maxCount", maxCount);
  }

  const format = (memoryExportFormatInput?.value || "json")
    .trim()
    .toLowerCase();
  params.set("format", format);

  const query = params.toString();
  return `/api/memory/export${query ? `?${query}` : ""}`;
}

function exportMemoryDataset() {
  const url = buildMemoryExportUrl();
  window.location.href = url;
}

function renderSuggestedUserMessageMeta(result) {
  if (!suggestedUserMessageMeta) {
    return;
  }

  const lines = [];

  if (result.tone) {
    lines.push(`Tone: ${result.tone}`);
  }

  if (result.reasoningSummary) {
    lines.push(`Why this suggestion: ${result.reasoningSummary}`);
  }

  suggestedUserMessageMeta.textContent =
    lines.length > 0
      ? lines.join(" • ")
      : "Reply suggestion generated from conversation context.";
}

function renderDirectorStatus(directorInstructions) {
  if (!directorInstructions || !directorInstructions.trim()) {
    directorStatusText.textContent = "No director instructions active.";
    return;
  }

  directorStatusText.textContent = `Director: ${directorInstructions}`;
}

function renderSceneStatus(sceneContext, isOocModeEnabled) {
  const sceneLine =
    sceneContext && sceneContext.trim()
      ? `Scene: ${sceneContext}`
      : "Scene: none";

  const oocLine = `OOC: ${isOocModeEnabled ? "enabled" : "disabled"}`;

  sceneStatusText.textContent = `${sceneLine} • ${oocLine}`;
}

function setStreaming(isStreaming) {
  state.isStreaming = isStreaming;

    const controls = [
      sendBtn,
      suggestUserMessageBtn,
      messageInput,
    characterSelect,
    refreshCharactersBtn,
    newCharacterBtn,
    saveCharacterBtn,
    deleteCharacterBtn,
    addSampleDialogueBtn,
    personaSelect,
    refreshPersonasBtn,
    newPersonaBtn,
    savePersonaBtn,
    deletePersonaBtn,
    refreshConversationsBtn,
    newConversationBtn,
    refreshMemoryBtn,
    createMemoryBtn,
    generateMemoryProposalsBtn,
    refreshMemoryConflictsBtn,
    inspectRetrievalBtn,
    inspectPromptBtn,
    inspectSummaryBtn,
    refreshLorebooksBtn,
    createLorebookBtn,
    createLoreEntryBtn,
    activateTurnOverrideBtn,
    clearTurnOverrideBtn,
    turnOverrideProviderInput,
    turnOverrideModelIdentifierInput,
  ];

  for (const control of controls) {
    if (control) {
      control.disabled = isStreaming;
    }
  }
}

function escapeHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;");
}

function stripProviderPrefix(modelIdentifier) {
  const value = (modelIdentifier || "").trim();
  const lower = value.toLowerCase();

  if (lower.startsWith("ollama:")) {
    return value.slice("ollama:".length);
  }

  if (lower.startsWith("openrouter:")) {
    return value.slice("openrouter:".length);
  }

  if (lower.startsWith("hf:")) {
    return value.slice("hf:".length);
  }

  if (lower.startsWith("huggingface:")) {
    return value.slice("huggingface:".length);
  }

  if (lower.startsWith("llamacpp:")) {
    return value.slice("llamacpp:".length);
  }

  if (lower.startsWith("llama.cpp:")) {
    return value.slice("llama.cpp:".length);
  }

  if (lower.startsWith("llama-cpp:")) {
    return value.slice("llama-cpp:".length);
  }

  return value;
}

function parseProviderFromModelIdentifier(modelIdentifier) {
  const value = (modelIdentifier || "").trim().toLowerCase();

  if (value.startsWith("openrouter:")) {
    return "openrouter";
  }

  if (value.startsWith("hf:") || value.startsWith("huggingface:")) {
    return "huggingface";
  }

  if (
    value.startsWith("llamacpp:") ||
    value.startsWith("llama.cpp:") ||
    value.startsWith("llama-cpp:")
  ) {
    return "llama.cpp";
  }

  return "ollama";
}

function providerBadgeLabel(provider) {
  const normalized = (provider || "").toLowerCase();

  if (normalized === "openrouter") {
    return "OpenRouter";
  }

  if (normalized === "huggingface" || normalized === "hf") {
    return "Hugging Face";
  }

  if (
    normalized === "llama.cpp" ||
    normalized === "llamacpp" ||
    normalized === "llama-cpp"
  ) {
    return "LLAMA.CPP";
  }

  return "Ollama";
}

function renderProviderBadgeHtml(provider) {
  const label = providerBadgeLabel(provider);
  return `<span class="runtime-meta-pill runtime-meta-pill--provider">${escapeHtml(label)}</span>`;
}

function renderProviderModelInlineHtml(provider, modelIdentifier) {
  const model = stripProviderPrefix(modelIdentifier || "");
  const resolvedProvider = provider || parseProviderFromModelIdentifier(modelIdentifier);

  const providerBadge = renderProviderBadgeHtml(resolvedProvider);
  const modelText = model
    ? `<span class="runtime-meta-pill">${escapeHtml(model)}</span>`
    : "";

  return `${providerBadge}${modelText}`;
}

function formatResponseTimeMs(ms) {
  if (ms === null || ms === undefined || Number.isNaN(Number(ms))) {
    return "";
  }

  const value = Number(ms);

  if (value < 1000) {
    return `${value} ms`;
  }

  return `${(value / 1000).toFixed(2)} s`;
}

function renderTimingPillHtml(responseTimeMs) {
  const text = formatResponseTimeMs(responseTimeMs);
  if (!text) {
    return "";
  }

  return `<span class="runtime-meta-pill runtime-meta-pill--timing">${escapeHtml(text)}</span>`;
}

function renderRuntimeInlineHtml(provider, modelIdentifier, responseTimeMs) {
  const providerModel = renderProviderModelInlineHtml(
    provider,
    modelIdentifier,
  );
  const timing = renderTimingPillHtml(responseTimeMs);

  if (!providerModel && !timing) {
    return "";
  }

  return `
    <div class="provider-model-inline">
      ${providerModel}
      ${timing}
    </div>
  `;
}

function findLatestAssistantRuntime(messages) {
  if (!Array.isArray(messages) || messages.length === 0) {
    return null;
  }

  for (let i = messages.length - 1; i >= 0; i -= 1) {
    const message = messages[i];
    if ((message.role || "").toLowerCase() !== "assistant") {
      continue;
    }

    if (
      message.selectedProvider ||
      message.selectedModelIdentifier ||
      message.selectedRuntimeSource
    ) {
      return {
        provider: message.selectedProvider || null,
        modelIdentifier: message.selectedModelIdentifier || null,
        responseTimeMs: message.selectedResponseTimeMs ?? null,
        runtimeSource: message.selectedRuntimeSource || null,
      };
    }
  }

  return null;
}

function getInspectionRuntimeSource() {
  if (pendingTurnRuntimeOverride.active) {
    return {
      provider: pendingTurnRuntimeOverride.provider,
      modelIdentifier: pendingTurnRuntimeOverride.modelIdentifier,
      responseTimeMs: null,
      source: "Next-turn override",
    };
  }

  const latest = findLatestAssistantRuntime(state.messages || []);
  if (latest) {
    return {
      provider: latest.provider,
      modelIdentifier: latest.modelIdentifier,
      responseTimeMs: latest.responseTimeMs ?? null,
      source: "Latest assistant runtime",
    };
  }

  return null;
}

function renderInspectionRuntimeBadges() {
  const runtime = getInspectionRuntimeSource();

  const rows = [
    { row: promptInspectionRuntimeRow, meta: promptInspectionRuntimeMeta },
    { row: retrievalInspectionRuntimeRow, meta: retrievalInspectionRuntimeMeta },
  ];

  for (const target of rows) {
    if (!target.row || !target.meta) {
      continue;
    }

    if (!runtime) {
      target.row.innerHTML = "";
      target.meta.textContent = "No runtime metadata available yet.";
      continue;
    }

    target.row.innerHTML = renderRuntimeInlineHtml(
      runtime.provider,
      runtime.modelIdentifier,
      runtime.responseTimeMs,
    );
    target.meta.textContent = runtime.source;
  }
}

function renderActiveConversationRuntime(messages) {
  if (!activeConversationRuntimeBadge || !activeConversationRuntimeModel) {
    renderActiveRuntimeSource(messages);
    return;
  }

  const runtime = getInspectionRuntimeSource();

  if (!runtime) {
    activeConversationRuntimeBadge.innerHTML = "";
    activeConversationRuntimeModel.textContent =
      "No assistant runtime metadata yet.";
    renderActiveRuntimeSource(messages);
    return;
  }

  activeConversationRuntimeBadge.innerHTML = renderProviderBadgeHtml(
    runtime.provider,
  );
  const model = stripProviderPrefix(runtime.modelIdentifier || "");
  const timing = formatResponseTimeMs(runtime.responseTimeMs);
  activeConversationRuntimeModel.textContent = `${model || ""}${timing ? ` • ${timing}` : ""}`;
  renderActiveRuntimeSource(messages);
}

function runtimeSourceLabel(source) {
  switch ((source || "").toString()) {
    case "OneTurnOverride":
      return "Using one-turn override";
    case "ConversationStickyOverride":
      return "Using conversation sticky override";
    case "CharacterDefault":
      return "Using character default profile";
    case "AppDefault":
      return "Using app default profile";
    case "ProviderDefault":
      return "Using provider fallback";
    default:
      return "Runtime source unknown";
  }
}

function renderActiveRuntimeSource(messages) {
  if (!activeConversationRuntimeSource) {
    return;
  }

  if (pendingTurnRuntimeOverride.active) {
    activeConversationRuntimeSource.textContent =
      runtimeSourceLabel("OneTurnOverride");
    return;
  }

  const latestAssistant = findLatestAssistantRuntime(messages);
  if (!latestAssistant || !latestAssistant.runtimeSource) {
    activeConversationRuntimeSource.textContent =
      "No runtime source metadata yet.";
    return;
  }

  activeConversationRuntimeSource.textContent = runtimeSourceLabel(
    latestAssistant.runtimeSource,
  );
}

function fillSelectWithOptions(
  selectEl,
  items,
  valueKey,
  labelBuilder,
  includeEmpty = true,
  emptyLabel = "(none)",
) {
  if (!selectEl) {
    return;
  }

  const current = selectEl.value;
  selectEl.innerHTML = "";

  if (includeEmpty) {
    const emptyOption = document.createElement("option");
    emptyOption.value = "";
    emptyOption.textContent = emptyLabel;
    selectEl.appendChild(emptyOption);
  }

  for (const item of items || []) {
    const option = document.createElement("option");
    option.value = item[valueKey] || "";
    option.textContent = labelBuilder(item);
    selectEl.appendChild(option);
  }

  if ([...selectEl.options].some((x) => x.value === current)) {
    selectEl.value = current;
  }
}

async function loadAppRuntimeDefaults() {
  const response = await fetch("/api/app-defaults");
  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to load app defaults. ${text}`);
  }

  appRuntimeDefaults = await response.json();
  applyAppDefaultsToUi();
}

function applyAppDefaultsToUi() {
  if (defaultPersonaSelect) {
    defaultPersonaSelect.value = appRuntimeDefaults.defaultPersonaId || "";
  }

  if (defaultModelProfileSelect) {
    defaultModelProfileSelect.value =
      appRuntimeDefaults.defaultModelProfileId || "";
  }

  if (defaultGenerationPresetSelect) {
    defaultGenerationPresetSelect.value =
      appRuntimeDefaults.defaultGenerationPresetId || "";
  }

  if (appDefaultsStatus) {
    appDefaultsStatus.textContent = "App defaults loaded.";
  }
}

async function saveAppRuntimeDefaults() {
  const response = await fetch("/api/app-defaults", {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      defaultPersonaId: defaultPersonaSelect?.value || null,
      defaultModelProfileId: defaultModelProfileSelect?.value || null,
      defaultGenerationPresetId: defaultGenerationPresetSelect?.value || null,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to save app defaults. ${text}`);
  }

  appRuntimeDefaults = await response.json();
  applyAppDefaultsToUi();
  await loadPersonas();

  if (appDefaultsStatus) {
    appDefaultsStatus.textContent = "App defaults saved.";
  }
}

function applyConversationSettingsToUi(conversation) {
  if (!conversation) {
    return;
  }

  if (conversationPersonaSelect) {
    conversationPersonaSelect.value = conversation.userPersonaId || "";
  }

  if (conversationModelProfileOverrideSelect) {
    conversationModelProfileOverrideSelect.value =
      conversation.runtimeModelProfileOverrideId || "";
  }

  if (conversationGenerationPresetOverrideSelect) {
    conversationGenerationPresetOverrideSelect.value =
      conversation.runtimeGenerationPresetOverrideId || "";
  }
}

async function saveConversationSettings() {
  if (!state.activeConversationId) {
    throw new Error("No active conversation.");
  }

  const response = await fetch(
    `/api/conversations/${state.activeConversationId}/settings`,
    {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        userPersonaId: conversationPersonaSelect?.value || null,
        runtimeModelProfileOverrideId:
          conversationModelProfileOverrideSelect?.value || null,
        runtimeGenerationPresetOverrideId:
          conversationGenerationPresetOverrideSelect?.value || null,
      }),
    },
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to save conversation settings. ${text}`);
  }

  if (conversationSettingsStatus) {
    conversationSettingsStatus.textContent = "Conversation settings saved.";
  }
}

function clearConversationRuntimeOverrideUi() {
  if (conversationModelProfileOverrideSelect) {
    conversationModelProfileOverrideSelect.value = "";
  }

  if (conversationGenerationPresetOverrideSelect) {
    conversationGenerationPresetOverrideSelect.value = "";
  }
}

function refreshTurnOverrideStatus() {
  if (!turnOverrideStatus) {
    return;
  }

  if (!pendingTurnRuntimeOverride.active) {
    turnOverrideStatus.textContent = "No one-turn runtime override active.";
    return;
  }

  const provider = providerBadgeLabel(pendingTurnRuntimeOverride.provider);
  const model = stripProviderPrefix(
    pendingTurnRuntimeOverride.modelIdentifier || "",
  );

  turnOverrideStatus.textContent = `Active for next generation only: ${provider}${model ? ` • ${model}` : ""}`;
}

function activateTurnRuntimeOverride() {
  const provider = (turnOverrideProviderInput?.value || "ollama")
    .trim()
    .toLowerCase();
  const modelIdentifier = (
    turnOverrideModelIdentifierInput?.value || ""
  ).trim();

  if (!modelIdentifier) {
    throw new Error(
      "Enter a model identifier before activating the one-turn override.",
    );
  }

  pendingTurnRuntimeOverride.active = true;
  pendingTurnRuntimeOverride.provider = provider;
  pendingTurnRuntimeOverride.modelIdentifier = modelIdentifier;
  refreshTurnOverrideStatus();
}

function clearTurnRuntimeOverride() {
  pendingTurnRuntimeOverride.active = false;
  pendingTurnRuntimeOverride.provider = null;
  pendingTurnRuntimeOverride.modelIdentifier = null;
  refreshTurnOverrideStatus();
}

function buildTurnOverrideQueryString() {
  if (
    !pendingTurnRuntimeOverride.active ||
    !pendingTurnRuntimeOverride.modelIdentifier
  ) {
    return "";
  }

  const params = new URLSearchParams();
  params.set(
    "overrideProvider",
    pendingTurnRuntimeOverride.provider || "ollama",
  );
  params.set(
    "overrideModelIdentifier",
    pendingTurnRuntimeOverride.modelIdentifier,
  );

  return `?${params.toString()}`;
}

const AUTHORING_ASSISTANT_FIELDS = [
  {
    entityType: "character",
    fieldName: "description",
    elementId: "characterDescriptionInput",
    label: "Character Description",
  },
  {
    entityType: "character",
    fieldName: "personalityDefinition",
    elementId: "characterPersonalityInput",
    label: "Character Personality",
  },
  {
    entityType: "character",
    fieldName: "scenario",
    elementId: "characterScenarioInput",
    label: "Character Scenario",
  },
  {
    entityType: "character",
    fieldName: "greeting",
    elementId: "characterGreetingInput",
    label: "Character Greeting",
  },
  {
    entityType: "persona",
    fieldName: "description",
    elementId: "personaDescriptionInput",
    label: "Persona Description",
  },
  {
    entityType: "persona",
    fieldName: "traits",
    elementId: "personaTraitsInput",
    label: "Persona Traits",
  },
  {
    entityType: "persona",
    fieldName: "preferences",
    elementId: "personaPreferencesInput",
    label: "Persona Preferences",
  },
  {
    entityType: "persona",
    fieldName: "additionalInstructions",
    elementId: "personaInstructionsInput",
    label: "Persona Additional Instructions",
  },
];

function safeValueById(id) {
  const el = document.getElementById(id);
  return el ? el.value || "" : "";
}

function collectCharacterAuthoringContext() {
  return {
    name: safeValueById("characterNameInput"),
    description: safeValueById("characterDescriptionInput"),
    personalityDefinition: safeValueById("characterPersonalityInput"),
    scenario: safeValueById("characterScenarioInput"),
    greeting: safeValueById("characterGreetingInput"),
  };
}

function collectPersonaAuthoringContext() {
  return {
    displayName: safeValueById("personaDisplayNameInput"),
    description: safeValueById("personaDescriptionInput"),
    traits: safeValueById("personaTraitsInput"),
    preferences: safeValueById("personaPreferencesInput"),
    additionalInstructions: safeValueById("personaInstructionsInput"),
  };
}

function getAuthoringContext(entityType) {
  return entityType === "character"
    ? collectCharacterAuthoringContext()
    : collectPersonaAuthoringContext();
}

const AUTHORING_REPAIR_FIELD_TARGETS = {
  characterDescription: "characterDescriptionInput",
  characterPersonalityDefinition: "characterPersonalityInput",
  characterScenario: "characterScenarioInput",
  characterGreeting: "characterGreetingInput",
  personaDescription: "personaDescriptionInput",
  personaTraits: "personaTraitsInput",
  personaPreferences: "personaPreferencesInput",
  personaAdditionalInstructions: "personaInstructionsInput",
};

function collectFullBundleContext() {
  return {
    characterName: safeValueById("characterNameInput"),
    characterDescription: safeValueById("characterDescriptionInput"),
    characterPersonalityDefinition: safeValueById("characterPersonalityInput"),
    characterScenario: safeValueById("characterScenarioInput"),
    characterGreeting: safeValueById("characterGreetingInput"),
    personaDisplayName: safeValueById("personaDisplayNameInput"),
    personaDescription: safeValueById("personaDescriptionInput"),
    personaTraits: safeValueById("personaTraitsInput"),
    personaPreferences: safeValueById("personaPreferencesInput"),
    personaAdditionalInstructions: safeValueById("personaInstructionsInput"),
  };
}

function collectFullAuthoringConsistencyContext() {
  return {
    characterName: safeValueById("characterNameInput"),
    characterDescription: safeValueById("characterDescriptionInput"),
    characterPersonalityDefinition: safeValueById("characterPersonalityInput"),
    characterScenario: safeValueById("characterScenarioInput"),
    characterGreeting: safeValueById("characterGreetingInput"),

    personaDisplayName: safeValueById("personaDisplayNameInput"),
    personaDescription: safeValueById("personaDescriptionInput"),
    personaTraits: safeValueById("personaTraitsInput"),
    personaPreferences: safeValueById("personaPreferencesInput"),
    personaAdditionalInstructions: safeValueById("personaInstructionsInput"),
  };
}

async function loadAuthoringTemplates(entityType, fieldName) {
  const response = await fetch(
    `/api/authoring/templates?entityType=${encodeURIComponent(entityType)}&fieldName=${encodeURIComponent(fieldName)}`,
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to load authoring templates. ${text}`);
  }

  const templates = await response.json();
  renderAuthoringTemplates(entityType, fieldName, templates);
}

async function loadAuthoringStarterPacks() {
  if (!authoringStarterPacksList || !authoringStarterPacksMeta) {
    return;
  }

  const response = await fetch("/api/authoring/starter-packs");

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to load starter packs. ${text}`);
  }

  const packs = await response.json();
  renderAuthoringStarterPacks(packs);
}

function renderAuthoringStarterPacks(packs) {
  if (!authoringStarterPacksList || !authoringStarterPacksMeta) {
    return;
  }

  authoringStarterPacksList.innerHTML = "";
  authoringStarterPacksMeta.textContent = `Starter packs: ${packs.length}`;

  if (!packs || packs.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No starter packs available.";
    authoringStarterPacksList.appendChild(empty);
    return;
  }

  for (const pack of packs) {
    const card = document.createElement("div");
    card.className = "authoring-template-card";
    card.innerHTML = `
      <h4>${escapeHtml(pack.title)}</h4>
      <div class="muted-text">${escapeHtml(pack.summary)}</div>
      <div><strong>Concept:</strong> ${escapeHtml(pack.concept)}</div>
      <div><strong>Vibe:</strong> ${escapeHtml(pack.vibe || "—")}</div>
      <div><strong>Relationship:</strong> ${escapeHtml(pack.relationship || "—")}</div>
      <div><strong>Setting:</strong> ${escapeHtml(pack.setting || "—")}</div>
      <div class="message-actions">
        <button type="button">Use Pack</button>
      </div>
    `;

    const useBtn = card.querySelector("button");
    useBtn.addEventListener("click", () => {
      if (bundleConceptInput) {
        bundleConceptInput.value = pack.concept || "";
      }
      if (bundleVibeInput) {
        bundleVibeInput.value = pack.vibe || "";
      }
      if (bundleRelationshipInput) {
        bundleRelationshipInput.value = pack.relationship || "";
      }
      if (bundleSettingInput) {
        bundleSettingInput.value = pack.setting || "";
      }
      setStatus(`Loaded starter pack: ${pack.title}`);
    });

    authoringStarterPacksList.appendChild(card);
  }
}

function renderAuthoringTemplates(entityType, fieldName, templates) {
  if (!authoringTemplatesList || !authoringTemplatesMeta) {
    return;
  }

  authoringTemplatesList.innerHTML = "";
  authoringTemplatesMeta.textContent = `Templates for ${entityType} / ${fieldName}: ${templates.length}`;

  if (!templates || templates.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No templates found for this field.";
    authoringTemplatesList.appendChild(empty);
    return;
  }

  const fieldConfig = AUTHORING_ASSISTANT_FIELDS.find(
    (x) => x.entityType === entityType && x.fieldName === fieldName,
  );

  const targetElement = fieldConfig
    ? document.getElementById(fieldConfig.elementId)
    : null;

  for (const template of templates) {
    const card = document.createElement("div");
    card.className = "authoring-template-card";
    card.innerHTML = `
      <h4>${escapeHtml(template.title)}</h4>
      <div class="muted-text">${escapeHtml(template.summary)}</div>
      <pre>${escapeHtml(template.content)}</pre>
      <div class="message-actions">
        <button type="button" data-action="replace">Use Template</button>
        <button type="button" data-action="append">Append Template</button>
      </div>
    `;

    card.querySelectorAll("button").forEach((button) => {
      button.addEventListener("click", () => {
        if (!targetElement) {
          return;
        }

        const action = button.dataset.action;
        if (action === "replace") {
          targetElement.value = template.content;
        } else {
          targetElement.value = targetElement.value
            ? `${targetElement.value.trim()}\n\n${template.content}`
            : template.content;
        }

        targetElement.dispatchEvent(new Event("input", { bubbles: true }));
        setStatus("Template applied");
      });
    });

    authoringTemplatesList.appendChild(card);
  }
}

async function enhanceAuthoringField(
  entityType,
  fieldName,
  targetElement,
  mode,
) {
  const response = await fetch("/api/authoring/enhance", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      entityType,
      fieldName,
      currentText: targetElement.value || "",
      mode,
      modelOverride: null,
      context: getAuthoringContext(entityType),
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to enhance authoring field. ${text}`);
  }

  const result = await response.json();

  authoringEnhancementState.targetElementId = targetElement.id;
  authoringEnhancementState.entityType = entityType;
  authoringEnhancementState.fieldName = fieldName;

  if (authoringEnhancementMeta) {
    authoringEnhancementMeta.textContent = `Suggestion loaded for ${entityType} / ${fieldName} / mode: ${mode}`;
  }
  if (authoringOriginalText) {
    authoringOriginalText.value = result.originalText || "";
  }
  if (authoringSuggestedText) {
    authoringSuggestedText.value = result.suggestedText || "";
  }
  if (authoringEnhancementRationale) {
    authoringEnhancementRationale.textContent =
      result.rationale || "No rationale provided.";
  }
}

function clearAuthoringEnhancementReview() {
  authoringEnhancementState.targetElementId = null;
  authoringEnhancementState.entityType = null;
  authoringEnhancementState.fieldName = null;

  if (authoringEnhancementMeta) {
    authoringEnhancementMeta.textContent =
      "No enhancement suggestion loaded yet.";
  }
  if (authoringOriginalText) {
    authoringOriginalText.value = "";
  }
  if (authoringSuggestedText) {
    authoringSuggestedText.value = "";
  }
  if (authoringEnhancementRationale) {
    authoringEnhancementRationale.textContent = "";
  }
}

async function generateFullBundle() {
  if (
    !bundleConceptInput ||
    !bundleVibeInput ||
    !bundleRelationshipInput ||
    !bundleSettingInput ||
    !bundleGenerationMeta ||
    !bundleGenerationRationale
  ) {
    return;
  }

  const response = await fetch("/api/authoring/generate-full-bundle", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      concept: bundleConceptInput.value || "",
      vibe: bundleVibeInput.value || "",
      relationship: bundleRelationshipInput.value || "",
      setting: bundleSettingInput.value || "",
      modelOverride: null,
      existingContext: collectFullBundleContext(),
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to generate full bundle. ${text}`);
  }

  const result = await response.json();

  if (generatedCharacterNameInput) {
    generatedCharacterNameInput.value = result.characterName || "";
  }
  if (generatedCharacterDescriptionInput) {
    generatedCharacterDescriptionInput.value = result.characterDescription || "";
  }
  if (generatedCharacterPersonalityInput) {
    generatedCharacterPersonalityInput.value =
      result.characterPersonalityDefinition || "";
  }
  if (generatedCharacterScenarioInput) {
    generatedCharacterScenarioInput.value = result.characterScenario || "";
  }
  if (generatedCharacterGreetingInput) {
    generatedCharacterGreetingInput.value = result.characterGreeting || "";
  }
  if (generatedPersonaDisplayNameInput) {
    generatedPersonaDisplayNameInput.value = result.personaDisplayName || "";
  }
  if (generatedPersonaDescriptionInput) {
    generatedPersonaDescriptionInput.value = result.personaDescription || "";
  }
  if (generatedPersonaTraitsInput) {
    generatedPersonaTraitsInput.value = result.personaTraits || "";
  }
  if (generatedPersonaPreferencesInput) {
    generatedPersonaPreferencesInput.value = result.personaPreferences || "";
  }
  if (generatedPersonaAdditionalInstructionsInput) {
    generatedPersonaAdditionalInstructionsInput.value =
      result.personaAdditionalInstructions || "";
  }

  bundleGenerationMeta.textContent = "Generated full bundle loaded.";
  bundleGenerationRationale.textContent =
    result.rationale || "No rationale provided.";
}

function clearGeneratedBundleReview() {
  if (generatedCharacterNameInput) {
    generatedCharacterNameInput.value = "";
  }
  if (generatedCharacterDescriptionInput) {
    generatedCharacterDescriptionInput.value = "";
  }
  if (generatedCharacterPersonalityInput) {
    generatedCharacterPersonalityInput.value = "";
  }
  if (generatedCharacterScenarioInput) {
    generatedCharacterScenarioInput.value = "";
  }
  if (generatedCharacterGreetingInput) {
    generatedCharacterGreetingInput.value = "";
  }
  if (generatedPersonaDisplayNameInput) {
    generatedPersonaDisplayNameInput.value = "";
  }
  if (generatedPersonaDescriptionInput) {
    generatedPersonaDescriptionInput.value = "";
  }
  if (generatedPersonaTraitsInput) {
    generatedPersonaTraitsInput.value = "";
  }
  if (generatedPersonaPreferencesInput) {
    generatedPersonaPreferencesInput.value = "";
  }
  if (generatedPersonaAdditionalInstructionsInput) {
    generatedPersonaAdditionalInstructionsInput.value = "";
  }
  if (bundleGenerationMeta) {
    bundleGenerationMeta.textContent = "No generated bundle loaded yet.";
  }
  if (bundleGenerationRationale) {
    bundleGenerationRationale.textContent = "";
  }
}

function applyGeneratedBundle() {
  const map = [
    ["characterNameInput", generatedCharacterNameInput?.value],
    ["characterDescriptionInput", generatedCharacterDescriptionInput?.value],
    ["characterPersonalityInput", generatedCharacterPersonalityInput?.value],
    ["characterScenarioInput", generatedCharacterScenarioInput?.value],
    ["characterGreetingInput", generatedCharacterGreetingInput?.value],
    ["personaDisplayNameInput", generatedPersonaDisplayNameInput?.value],
    ["personaDescriptionInput", generatedPersonaDescriptionInput?.value],
    ["personaTraitsInput", generatedPersonaTraitsInput?.value],
    ["personaPreferencesInput", generatedPersonaPreferencesInput?.value],
    [
      "personaInstructionsInput",
      generatedPersonaAdditionalInstructionsInput?.value,
    ],
  ];

  for (const [id, value] of map) {
    const el = document.getElementById(id);
    if (!el) {
      continue;
    }

    el.value = value || "";
    el.dispatchEvent(new Event("input", { bubbles: true }));
  }

  clearGeneratedBundleReview();
  setStatus("Generated bundle applied");
}

async function repairConsistencyIssue(issue) {
  const targetElementId = AUTHORING_REPAIR_FIELD_TARGETS[issue.fieldName];
  if (!targetElementId) {
    throw new Error(
      `No repair target is mapped for field '${issue.fieldName}'.`,
    );
  }

  const target = document.getElementById(targetElementId);
  if (!target) {
    throw new Error(
      `Repair target element '${targetElementId}' was not found.`,
    );
  }

  const response = await fetch("/api/authoring/repair-issue", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      entityType: "characterPersonaBundle",
      fieldName: issue.fieldName,
      issueType: issue.issueType,
      issueDescription: issue.description,
      suggestedFixHint: issue.suggestion || null,
      currentText: target.value || "",
      modelOverride: null,
      context: collectFullAuthoringConsistencyContext(),
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to repair consistency issue. ${text}`);
  }

  const result = await response.json();

  authoringEnhancementState.targetElementId = targetElementId;
  authoringEnhancementState.entityType = "characterPersonaBundle";
  authoringEnhancementState.fieldName = issue.fieldName;

  if (authoringEnhancementMeta) {
    authoringEnhancementMeta.textContent = `Repair suggestion loaded for ${issue.fieldName}`;
  }
  if (authoringOriginalText) {
    authoringOriginalText.value = result.originalText || "";
  }
  if (authoringSuggestedText) {
    authoringSuggestedText.value = result.suggestedText || "";
  }
  if (authoringEnhancementRationale) {
    authoringEnhancementRationale.textContent =
      result.rationale || "No rationale provided.";
  }
}

async function checkAuthoringConsistency() {
  const response = await fetch("/api/authoring/consistency-check", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      entityType: "characterPersonaBundle",
      fields: collectFullAuthoringConsistencyContext(),
      modelOverride: null,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to run authoring consistency check. ${text}`);
  }

  const result = await response.json();
  renderAuthoringConsistency(result);
}

function renderAuthoringConsistency(result) {
  if (!authoringConsistencyList || !authoringConsistencyMeta) {
    return;
  }

  authoringConsistencyList.innerHTML = "";

  authoringConsistencyMeta.textContent = `${result.summary || "Consistency analysis completed."} Issues: ${(result.issues || []).length}`;

  if (!result.issues || result.issues.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No major consistency issues found.";
    authoringConsistencyList.appendChild(empty);
    return;
  }

  for (const issue of result.issues) {
    const card = document.createElement("div");
    card.className = "proposal-card";
    card.innerHTML = `
      <div><strong>Severity:</strong> ${escapeHtml(issue.severity)}</div>
      <div><strong>Field:</strong> ${escapeHtml(issue.fieldName)}</div>
      <div><strong>Type:</strong> ${escapeHtml(issue.issueType)}</div>
      <div><strong>Issue:</strong> ${escapeHtml(issue.description)}</div>
      <div><strong>Suggestion:</strong> ${escapeHtml(issue.suggestion || "—")}</div>
      <div class="message-actions">
        <button type="button">Repair This Field</button>
      </div>
    `;

    const repairBtn = card.querySelector("button");
    repairBtn.addEventListener("click", async () => {
      try {
        await repairConsistencyIssue(issue);
        setStatus(`Repair suggestion loaded for ${issue.fieldName}`);
      } catch (error) {
        console.error(error);
        authoringConsistencyMeta.textContent =
          error.message || "Repair failed.";
        setStatus("Consistency issue repair failed");
      }
    });

    authoringConsistencyList.appendChild(card);
  }
}

function attachAuthoringAssistantToolbars() {
  for (const config of AUTHORING_ASSISTANT_FIELDS) {
    const targetElement = document.getElementById(config.elementId);
    if (!targetElement) {
      continue;
    }

    if (targetElement.dataset.authoringAssistantAttached === "true") {
      continue;
    }

    const toolbar = document.createElement("div");
    toolbar.className = "authoring-assistant-toolbar";
    toolbar.innerHTML = `
      <select>
        <option value="clarify">Clarify</option>
        <option value="expand">Expand</option>
        <option value="tighten">Tighten</option>
        <option value="immersive">More immersive</option>
        <option value="efficient">Reduce tokens</option>
      </select>
      <button type="button">Examples</button>
      <button type="button">Enhance</button>
    `;

    const modeSelect = toolbar.querySelector("select");
    const examplesBtn = toolbar.querySelectorAll("button")[0];
    const enhanceBtn = toolbar.querySelectorAll("button")[1];

    examplesBtn.addEventListener("click", async () => {
      try {
        await loadAuthoringTemplates(config.entityType, config.fieldName);
        setStatus(`${config.label} templates loaded`);
      } catch (error) {
        console.error(error);
        if (authoringTemplatesMeta) {
          authoringTemplatesMeta.textContent =
            error.message || "Failed to load templates.";
        }
        setStatus("Authoring templates failed");
      }
    });

    enhanceBtn.addEventListener("click", async () => {
      try {
        await enhanceAuthoringField(
          config.entityType,
          config.fieldName,
          targetElement,
          modeSelect.value,
        );
        setStatus(`${config.label} suggestion loaded`);
      } catch (error) {
        console.error(error);
        if (authoringEnhancementMeta) {
          authoringEnhancementMeta.textContent =
            error.message || "Failed to enhance field.";
        }
        setStatus("Authoring enhancement failed");
      }
    });

    targetElement.insertAdjacentElement("afterend", toolbar);
    targetElement.dataset.authoringAssistantAttached = "true";
  }
}

function buildMemoryMetaHtml(memory) {
  const rows = [];

  rows.push(
    `<div><strong>Category:</strong> ${escapeHtml(memory.category)}</div>`,
  );

  if (memory.kind) {
    rows.push(`<div><strong>Kind:</strong> ${escapeHtml(memory.kind)}</div>`);
  }

  if (memory.slotKey) {
    rows.push(`<div><strong>Slot:</strong> ${escapeHtml(memory.slotKey)}</div>`);
  }
  if (memory.slotFamily) {
    rows.push(
      `<div><strong>Slot Family:</strong> ${escapeHtml(memory.slotFamily)}</div>`,
    );
  }

  if (memory.reviewStatus) {
    rows.push(
      `<div><strong>Status:</strong> ${escapeHtml(memory.reviewStatus)}</div>`,
    );
  }

  if (memory.confidenceScore !== null && memory.confidenceScore !== undefined) {
    rows.push(
      `<div><strong>Confidence:</strong> ${escapeHtml(String(memory.confidenceScore))}</div>`,
    );
  }

  if (memory.conflictsWithMemoryItemId) {
    rows.push(
      `<div><strong>Conflicts With:</strong> ${escapeHtml(memory.conflictsWithMemoryItemId)}</div>`,
    );
  }

  if (memory.expiresAt) {
    rows.push(
      `<div><strong>Expires:</strong> ${escapeHtml(formatDateTime(memory.expiresAt))}</div>`,
    );
  }

  return rows.join("");
}

async function copyTextToClipboard(text) {
  const normalized = String(text ?? "");

  if (navigator.clipboard?.writeText) {
    await navigator.clipboard.writeText(normalized);
    return;
  }

  const temp = document.createElement("textarea");
  temp.value = normalized;
  temp.setAttribute("readonly", "true");
  temp.style.position = "absolute";
  temp.style.left = "-9999px";
  document.body.appendChild(temp);
  temp.select();
  document.execCommand("copy");
  temp.remove();
}

const markdownRenderer =
  typeof window !== "undefined" && typeof window.markdownit === "function"
    ? window.markdownit({
        html: false,
        breaks: true,
        linkify: true,
        typographer: true,
      })
    : null;

if (markdownRenderer) {
  const defaultLinkRender =
    markdownRenderer.renderer.rules.link_open ||
    ((tokens, idx, options, env, self) =>
      self.renderToken(tokens, idx, options));

  markdownRenderer.renderer.rules.link_open = (
    tokens,
    idx,
    options,
    env,
    self,
  ) => {
    const token = tokens[idx];
    const targetIndex = token.attrIndex("target");
    if (targetIndex < 0) {
      token.attrPush(["target", "_blank"]);
    } else {
      token.attrs[targetIndex][1] = "_blank";
    }

    const relIndex = token.attrIndex("rel");
    const relValue = "noopener noreferrer nofollow";
    if (relIndex < 0) {
      token.attrPush(["rel", relValue]);
    } else {
      token.attrs[relIndex][1] = relValue;
    }

    return defaultLinkRender(tokens, idx, options, env, self);
  };
}

function stripSpeakerPrefix(role, text) {
  if (!text) {
    return "";
  }

  const normalizedRole = String(role || "").trim().toLowerCase();
  const patterns = [];

  if (normalizedRole === "assistant") {
    patterns.push(/^\s*assistant\s*:\s*/i);
  } else if (normalizedRole === "user") {
    patterns.push(/^\s*user\s*:\s*/i);
  } else if (normalizedRole === "system") {
    patterns.push(/^\s*system\s*:\s*/i);
  }

  let output = String(text);
  for (const pattern of patterns) {
    output = output.replace(pattern, "");
  }

  return output;
}

function renderMessageContent(role, text) {
  const normalized = stripSpeakerPrefix(role, text);

  if (!markdownRenderer) {
    return escapeHtml(normalized).replaceAll("\n", "<br>");
  }

  const rendered = markdownRenderer.render(normalized);
  if (typeof window !== "undefined" && window.DOMPurify?.sanitize) {
    return window.DOMPurify.sanitize(rendered, {
      ALLOWED_TAGS: [
        "p",
        "br",
        "em",
        "strong",
        "del",
        "code",
        "pre",
        "blockquote",
        "ul",
        "ol",
        "li",
        "h1",
        "h2",
        "h3",
        "h4",
        "h5",
        "h6",
        "a",
        "hr",
      ],
      ALLOWED_ATTR: ["href", "title", "target", "rel"],
    });
  }

  return rendered;
}

function formatDate(value) {
  try {
    return new Date(value).toLocaleString();
  } catch {
    return value;
  }
}

function formatDateTime(value) {
  if (!value) {
    return "—";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleString();
}

function formatConfidence(value) {
  if (value === null || typeof value === "undefined") {
    return "n/a";
  }

  return `${Math.round(value * 100)}%`;
}

function formatProposalMeta(proposal) {
  const lines = [];

  if (proposal.proposalReason) {
    lines.push(`Reason: ${proposal.proposalReason}`);
  }

  if (proposal.sourceExcerpt) {
    lines.push(`Evidence: ${proposal.sourceExcerpt}`);
  }

  if (proposal.normalizedKey) {
    lines.push(`Key: ${proposal.normalizedKey}`);
  }

  if (proposal.conflictsWithMemoryItemId) {
    lines.push(`Conflicts with memory: ${proposal.conflictsWithMemoryItemId}`);
  }

  return lines.join("\n");
}

function populateImageStylePresetSelect(selectEl, selectedValue) {
  if (!selectEl) {
    return;
  }

  selectEl.innerHTML = "";

  for (const [key, preset] of Object.entries(IMAGE_STYLE_PRESETS)) {
    const option = document.createElement("option");
    option.value = key;
    option.textContent = preset.label;
    option.selected = key === (selectedValue || "none");
    selectEl.appendChild(option);
  }
}

function mergePromptParts(parts) {
  return parts
    .map((x) => (x || "").trim())
    .filter(Boolean)
    .join(", ");
}

function mergeNegativePromptParts(parts) {
  return parts
    .map((x) => (x || "").trim())
    .filter(Boolean)
    .join(", ");
}

function getSelectedImageStylePresetKey() {
  return imageStylePresetSelect?.value || "none";
}

function getSelectedImageStylePreset() {
  return (
    IMAGE_STYLE_PRESETS[getSelectedImageStylePresetKey()] ||
    IMAGE_STYLE_PRESETS.none
  );
}

function getActiveCharacterVisualDefaults() {
  const character = state.activeCharacterDetail;
  if (!character) {
    return {
      defaultVisualStylePreset: null,
      defaultVisualPromptPrefix: null,
      defaultVisualNegativePrompt: null,
    };
  }

  return {
    defaultVisualStylePreset: character.defaultVisualStylePreset ?? null,
    defaultVisualPromptPrefix: character.defaultVisualPromptPrefix ?? null,
    defaultVisualNegativePrompt: character.defaultVisualNegativePrompt ?? null,
  };
}

function applyVisualDefaultsAndPreset(basePositivePrompt, baseNegativePrompt) {
  const defaults = getActiveCharacterVisualDefaults();
  const preset = getSelectedImageStylePreset();

  const positivePrompt = mergePromptParts([
    defaults.defaultVisualPromptPrefix,
    preset.positivePrefix,
    basePositivePrompt,
  ]);

  const negativePrompt = mergeNegativePromptParts([
    baseNegativePrompt,
    defaults.defaultVisualNegativePrompt,
    preset.negativeAppend,
  ]);

  return {
    positivePrompt,
    negativePrompt,
    stylePresetKey: getSelectedImageStylePresetKey(),
  };
}

function initializeSidebarAccordions() {
  const sections = document.querySelectorAll(".sidebar-section");

  sections.forEach((section, index) => {
    if (section.dataset.accordionInitialized === "true") {
      return;
    }

    const heading = section.querySelector("h2");
    if (!heading) {
      return;
    }

    const titleText = heading.textContent?.trim() || `Section ${index + 1}`;
    const storageKey = `localchat.sidebar.${titleText}`;
    const shouldStartExpanded = section.dataset.expandedDefault === "true";

    const toggle = document.createElement("button");
    toggle.type = "button";
    toggle.className = "sidebar-section-toggle";
    toggle.setAttribute(
      "aria-expanded",
      shouldStartExpanded ? "true" : "false",
    );
    toggle.innerHTML = `<span class="sidebar-section-title">${escapeHtml(titleText)}</span>`;

    const body = document.createElement("div");
    body.className = "sidebar-section-body";

    const children = Array.from(section.children).filter((x) => x !== heading);
    for (const child of children) {
      body.appendChild(child);
    }

    heading.remove();
    section.prepend(body);
    section.prepend(toggle);

    const savedState = localStorage.getItem(storageKey);
    const expanded =
      savedState === null ? shouldStartExpanded : savedState === "open";

    section.classList.toggle("collapsed", !expanded);
    toggle.setAttribute("aria-expanded", expanded ? "true" : "false");

    toggle.addEventListener("click", () => {
      const isCollapsed = section.classList.toggle("collapsed");
      const isExpanded = !isCollapsed;

      toggle.setAttribute("aria-expanded", isExpanded ? "true" : "false");
      localStorage.setItem(storageKey, isExpanded ? "open" : "closed");
    });

    section.dataset.accordionInitialized = "true";
  });
}

function initializeMemoryPanelAccordions() {
  const sections = document.querySelectorAll(".memory-panel .memory-panel-section");

  sections.forEach((section, index) => {
    if (section.dataset.accordionInitialized === "true") {
      return;
    }

    const heading = section.querySelector(
      ":scope > .memory-panel-header h2, :scope > .memory-panel-header h3, :scope > h2, :scope > h3",
    );
    if (!heading) {
      return;
    }

    section.classList.add("sidebar-section");

    const titleText = heading.textContent?.trim() || `Debug Section ${index + 1}`;
    const storageKey = `localchat.debug.${titleText}`;
    const shouldStartExpanded = section.dataset.expandedDefault === "true";

    const toggle = document.createElement("button");
    toggle.type = "button";
    toggle.className = "sidebar-section-toggle";
    toggle.setAttribute(
      "aria-expanded",
      shouldStartExpanded ? "true" : "false",
    );
    toggle.innerHTML = `<span class="sidebar-section-title">${escapeHtml(titleText)}</span>`;

    const body = document.createElement("div");
    body.className = "sidebar-section-body";

    const children = Array.from(section.children);
    for (const child of children) {
      body.appendChild(child);
    }

    heading.remove();
    section.prepend(body);
    section.prepend(toggle);

    const savedState = localStorage.getItem(storageKey);
    const expanded =
      savedState === null ? shouldStartExpanded : savedState === "open";

    section.classList.toggle("collapsed", !expanded);
    toggle.setAttribute("aria-expanded", expanded ? "true" : "false");

    toggle.addEventListener("click", () => {
      const isCollapsed = section.classList.toggle("collapsed");
      const isExpanded = !isCollapsed;

      toggle.setAttribute("aria-expanded", isExpanded ? "true" : "false");
      localStorage.setItem(storageKey, isExpanded ? "open" : "closed");
    });

    section.dataset.accordionInitialized = "true";
  });
}

function toNullableGuid(value) {
  return value && value.trim() ? value : null;
}

function toNullableInt(value) {
  if (value == null || value === "") return null;
  const parsed = Number.parseInt(value, 10);
  return Number.isNaN(parsed) ? null : parsed;
}

function toNumberOrDefault(value, fallback) {
  const parsed = Number.parseFloat(value);
  return Number.isNaN(parsed) ? fallback : parsed;
}

function clearMessages() {
  messagesEl.innerHTML = "";
}

function renderEmptyMessages(text) {
  messagesEl.innerHTML = `<div class="empty-state">${escapeHtml(text)}</div>`;
}

function getSelectedCharacter() {
  return (
    (state.characters || []).find((x) => x.id === state.selectedCharacterId) ||
    null
  );
}

function getActiveConversationCharacter() {
  const activeConversation = (state.conversations || []).find(
    (x) => x.id === state.activeConversationId,
  );
  if (!activeConversation) {
    return state.activeCharacterDetail || getSelectedCharacter();
  }

  return (
    (state.characters || []).find(
      (x) => x.id === activeConversation.characterId,
    ) ||
    state.activeCharacterDetail ||
    getSelectedCharacter()
  );
}

function renderAssistantAvatarHtml() {
  const character = getActiveConversationCharacter();
  if (!character || !character.imageUrl) {
    return "";
  }

  return `<img class="character-avatar" src="${escapeHtml(character.imageUrl)}" alt="${escapeHtml(character.name || "Character")}" />`;
}

function renderActiveConversationHeaderAvatar() {
  if (!activeConversationHeaderAvatar) {
    return;
  }

  const character = getActiveConversationCharacter();
  if (!character || !character.imageUrl) {
    activeConversationHeaderAvatar.innerHTML = "";
    return;
  }

  activeConversationHeaderAvatar.innerHTML = `<img class="character-avatar character-avatar--large" src="${escapeHtml(character.imageUrl)}" alt="${escapeHtml(character.name || "Character")}" />`;
}

function renderCharacterImagePreview(character) {
  if (!characterImagePreview || !characterImageStatus) {
    return;
  }

  if (!character || !character.imageUrl) {
    characterImagePreview.innerHTML = "";
    characterImageStatus.textContent = "No image uploaded.";
    return;
  }

  characterImagePreview.innerHTML = `
    <div class="character-image-preview">
      <img src="${escapeHtml(character.imageUrl)}" alt="${escapeHtml(character.name || "Character image")}" />
    </div>
  `;

  characterImageStatus.textContent = "Character image uploaded.";
}

async function uploadCharacterImage() {
  const character = getSelectedCharacter();
  if (!character) {
    throw new Error("No active character selected.");
  }

  const file = characterImageFileInput?.files?.[0];
  if (!file) {
    throw new Error("Choose an image file first.");
  }

  const formData = new FormData();
  formData.append("file", file);

  const response = await fetch(`/api/characters/${character.id}/image`, {
    method: "POST",
    body: formData,
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to upload character image. ${text}`);
  }

  await loadCharacters();
  await loadCharacterDetail(character.id);
  renderActiveConversationHeaderAvatar();
}

async function removeCharacterImage() {
  const character = getSelectedCharacter();
  if (!character) {
    throw new Error("No active character selected.");
  }

  const response = await fetch(`/api/characters/${character.id}/image`, {
    method: "DELETE",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to remove character image. ${text}`);
  }

  await loadCharacters();
  await loadCharacterDetail(character.id);
  if (characterImageFileInput) {
    characterImageFileInput.value = "";
  }
  renderActiveConversationHeaderAvatar();
}

function appendMessage(role, content) {
  const wrapper = document.createElement("div");
  wrapper.className = `message ${role.toLowerCase()}`;
  const isAssistant = (role || "").toLowerCase() === "assistant";
  const messageHeaderHtml = isAssistant
    ? `
      <div class="message-header-with-avatar">
        ${renderAssistantAvatarHtml()}
        <div class="message-role">${escapeHtml(role)}</div>
      </div>
    `
    : `<div class="message-role">${escapeHtml(role)}</div>`;

  wrapper.innerHTML = `
    ${messageHeaderHtml}
    <div class="message-content">${renderMessageContent(role, content)}</div>
  `;

  messagesEl.appendChild(wrapper);
  messagesEl.scrollTop = messagesEl.scrollHeight;
  return wrapper.querySelector(".message-content");
}

function renderMessages(messages) {
  clearMessages();

  if (!messages || messages.length === 0) {
    renderEmptyMessages("No messages yet.");
    return;
  }

  const latestAssistantMessageId = [...messages]
    .reverse()
    .find((x) => x.role === "Assistant")?.id;

  for (const message of messages) {
    const wrapper = document.createElement("div");
    wrapper.className = `message ${message.role.toLowerCase()}`;

    const selectedVariantDisplay = (message.selectedVariantIndex ?? 0) + 1;
    const variantCountDisplay = Math.max(
      selectedVariantDisplay,
      message.variantCount ?? (message.role === "Assistant" ? 1 : 0),
    );
    const isAssistant = (message.role || "").toLowerCase() === "assistant";
    const messageHeaderHtml = isAssistant
      ? `
        <div class="message-header-with-avatar">
          ${renderAssistantAvatarHtml()}
          <div class="message-role">${escapeHtml(message.role)}</div>
        </div>
      `
      : `<div class="message-role">${escapeHtml(message.role)}</div>`;

    const variantMeta =
      isAssistant
        ? `<div class="message-variant-meta">Selected variant: ${selectedVariantDisplay} • Variant count: ${variantCountDisplay}</div>`
        : "";

    const selectedRuntimeHtml =
      isAssistant &&
      (message.selectedProvider ||
        message.selectedModelIdentifier ||
        message.selectedResponseTimeMs)
        ? `
        <div class="message-runtime-row">
          ${renderRuntimeInlineHtml(
            message.selectedProvider,
            message.selectedModelIdentifier,
            message.selectedResponseTimeMs,
          )}
        </div>
      `
        : "";

    const variantCardsHtml =
      isAssistant && Array.isArray(message.variants) && message.variants.length > 0
        ? `
        <div class="message-variants">
          ${message.variants
            .slice()
            .sort((a, b) => (a.variantIndex ?? 0) - (b.variantIndex ?? 0))
            .map(
              (variant) => `
              <div class="message-variant-card ${variant.variantIndex === message.selectedVariantIndex ? "selected" : ""}">
                <div class="message-variant-title">Variant ${(variant.variantIndex ?? 0) + 1}</div>
                <div class="message-runtime-row message-runtime-row--variant">
                  ${renderRuntimeInlineHtml(
                    variant.provider,
                    variant.modelIdentifier,
                    variant.responseTimeMs,
                  )}
                </div>
              </div>`,
            )
            .join("")}
        </div>
      `
        : "";

    const branchButton = state.activeConversationId
      ? `<button type="button" data-action="branch" data-message-id="${message.id}">Branch Here</button>`
      : "";

    const regenerateButton =
      state.activeConversationId &&
      isAssistant &&
      message.id === latestAssistantMessageId
        ? `<button type="button" data-action="regenerate" data-message-id="${message.id}">Regenerate</button>`
        : "";

    const continueButton =
      state.activeConversationId &&
      isAssistant &&
      message.id === latestAssistantMessageId
        ? `<button type="button" data-action="continue" data-message-id="${message.id}">Continue</button>`
        : "";

    const prevSwipeButton =
      state.activeConversationId &&
      isAssistant &&
      (message.variantCount ?? 0) > 1
        ? `<button type="button" data-action="prev-swipe" data-message-id="${message.id}">Prev Swipe</button>`
        : "";

    const nextSwipeButton =
      state.activeConversationId &&
      isAssistant &&
      (message.variantCount ?? 0) > 1
        ? `<button type="button" data-action="next-swipe" data-message-id="${message.id}">Next Swipe</button>`
        : "";

    const speakButton =
      isAssistant
        ? `<button type="button" data-action="speak" data-message-id="${message.id}">Speak</button>`
        : "";

    const copyButton =
      message.role === "Assistant" || message.role === "User"
        ? `<button type="button" data-action="copy" data-message-id="${message.id}">Copy</button>`
        : "";

    const editButton = state.activeConversationId
      ? `<button type="button" data-action="edit" data-message-id="${message.id}">Edit</button>`
      : "";

    const deleteButton = state.activeConversationId
      ? `<button type="button" data-action="delete" data-message-id="${message.id}">Delete</button>`
      : "";

    wrapper.innerHTML = `
      ${messageHeaderHtml}
      <div class="message-content">${renderMessageContent(message.role, message.content)}</div>
      ${variantMeta}
      ${selectedRuntimeHtml}
      <div class="message-actions">
        ${branchButton}
        ${regenerateButton}
        ${continueButton}
        ${prevSwipeButton}
        ${nextSwipeButton}
        ${speakButton}
        ${copyButton}
        ${editButton}
        ${deleteButton}
      </div>
      ${variantCardsHtml}
      <div class="audio-preview" data-audio-slot="${message.id}"></div>
    `;

    wrapper.querySelectorAll("button").forEach((button) => {
      button.addEventListener("click", async () => {
        const action = button.dataset.action;
        const messageId = button.dataset.messageId;

        try {
          if (action === "branch") {
            await branchConversation(messageId);
          } else if (action === "regenerate") {
            await regenerateAssistantMessage(messageId);
          } else if (action === "continue") {
            await continueConversation();
          } else if (action === "prev-swipe") {
            await cycleSwipe(messages, messageId, -1);
          } else if (action === "next-swipe") {
            await cycleSwipe(messages, messageId, 1);
          } else if (action === "speak") {
            await synthesizeMessageSpeech(messageId);
          } else if (action === "copy") {
            await copyTextToClipboard(message.content);
            setStatus("Message copied");
          } else if (action === "edit") {
            const current = messages.find((x) => x.id === messageId);
            await editConversationMessage(current);
          } else if (action === "delete") {
            const current = messages.find((x) => x.id === messageId);
            await deleteConversationMessage(current);
          }
        } catch (error) {
          console.error(error);
          alert(error.message || "Conversation action failed.");
        }
      });
    });

    messagesEl.appendChild(wrapper);
  }

  messagesEl.scrollTop = messagesEl.scrollHeight;
}
function renderConversationList() {
  conversationList.innerHTML = "";

  if (!state.conversations.length) {
    conversationList.innerHTML = `<div class="empty-state">No conversations yet.</div>`;
    return;
  }

  for (const conversation of state.conversations) {
    const item = document.createElement("div");
    item.className = "conversation-item";
    if (conversation.id === state.activeConversationId) {
      item.classList.add("active");
    }

    item.innerHTML = `
      <div class="conversation-title">${escapeHtml(conversation.title)}</div>
      <div class="conversation-meta">
        Updated ${escapeHtml(formatDate(conversation.updatedAt))}
        ${conversation.userPersonaId ? " • Persona linked" : ""}
      </div>
    `;

    item.addEventListener("click", async () => {
      await openConversation(conversation.id);
    });

    conversationList.appendChild(item);
  }
}

function renderMemoryList() {
  memoryList.innerHTML = "";

  if (!state.memories.length) {
    memoryList.innerHTML = `<div class="empty-state">No memory items.</div>`;
    return;
  }

  for (const memory of state.memories) {
    const item = document.createElement("div");
    item.className = "memory-item";
    const scopeType =
      (memory.scopeType || "").trim() ||
      (memory.conversationId ? "Conversation" : "Character");
    const isConversationScope =
      scopeType.toLowerCase() === "conversation";
    const promoteButtonHtml = isConversationScope
      ? `<button type="button" data-action="promote-memory" data-memory-id="${memory.id}">Promote to Character</button>`
      : "";
    const copyIdButtonHtml = `<button type="button" data-action="copy-memory-id" data-memory-id="${memory.id}">Copy ID</button>`;
    const provenanceButtonHtml = `<button type="button" data-action="view-memory-provenance" data-memory-id="${memory.id}">View Provenance</button>`;
    const demoteButtonHtml =
      scopeType.toLowerCase() === "character" && state.activeConversationId
        ? `<button type="button" data-action="demote-memory" data-memory-id="${memory.id}">Demote to Current Conversation</button>`
        : "";

    item.innerHTML = `
      <div class="memory-item-header">
        <div>
          <div class="memory-item-category">${escapeHtml(memory.category)}</div>
          <div class="memory-item-meta">
            ${escapeHtml(scopeType)}-scoped
            ${memory.isPinned ? " • Pinned" : ""}
            ${memory.isDerived ? " • Derived" : ""}
            • ${escapeHtml(memory.reviewStatus)}
          </div>
        </div>
        <div class="memory-item-meta">${escapeHtml(formatDate(memory.updatedAt))}</div>
      </div>

      <div class="memory-item-content">${escapeHtml(memory.content)}</div>
      <div class="memory-item-meta">${buildMemoryMetaHtml(memory)}</div>
      <div class="memory-item-meta">
        Source Seq: ${memory.sourceMessageSequenceNumber ?? "—"} • Last Seen Seq: ${memory.lastObservedSequenceNumber ?? "—"} • Superseded Seq: ${memory.supersededAtSequenceNumber ?? "—"}
      </div>

      <div class="memory-item-actions">
        ${copyIdButtonHtml}
        <button type="button" data-action="pin-memory" data-memory-id="${memory.id}">${memory.isPinned ? "Unpin" : "Pin"}</button>
        <button type="button" data-action="edit-memory" data-memory-id="${memory.id}">Edit</button>
        <button type="button" data-action="merge-memory" data-memory-id="${memory.id}">Merge Into...</button>
        ${provenanceButtonHtml}${promoteButtonHtml}${demoteButtonHtml}
      </div>
    `;

    item.querySelectorAll("button").forEach((button) => {
      button.addEventListener("click", async () => {
        const action = button.dataset.action;
        const memoryId = button.dataset.memoryId;
        const current = state.memories.find((x) => x.id === memoryId);
        if (!current) {
          return;
        }

        try {
          if (action === "pin-memory") {
            await updateAcceptedMemoryPin(current.id, !current.isPinned);
          } else if (action === "edit-memory") {
            await editMemoryItem(current);
          } else if (action === "merge-memory") {
            await mergeMemoryItem(current);
          } else if (action === "promote-memory") {
            await promoteMemoryToCharacter(current.id);
          } else if (action === "copy-memory-id") {
            await copyTextToClipboard(current.id);
            setStatus("Memory id copied");
          } else if (action === "demote-memory") {
            await demoteMemoryToConversation(current.id);
          } else if (action === "view-memory-provenance") {
            if (memoryProvenanceStatus) {
              memoryProvenanceStatus.textContent = "Loading memory provenance...";
            }
            await loadMemoryProvenance(current.id);
            setStatus("Memory provenance loaded");
          }
        } catch (error) {
          console.error(error);
          alert(error.message || "Memory action failed.");
        }
      });
    });

    memoryList.appendChild(item);
  }
}

function renderPendingMemoryProposals(proposals) {
  if (!pendingMemoryProposalsEl) {
    return;
  }

  pendingMemoryProposalsEl.innerHTML = "";

  if (!proposals || proposals.length === 0) {
    pendingMemoryProposalsEl.innerHTML = `<div class="empty-state">No pending proposals.</div>`;
    return;
  }

  for (const proposal of proposals) {
    const wrapper = document.createElement("div");
    wrapper.className = `proposal-card ${proposal.conflictsWithMemoryItemId ? "conflict" : ""}`;

    const meta = formatProposalMeta(proposal);

    wrapper.innerHTML = `
      <div class="proposal-header">
        <div class="proposal-category">${escapeHtml(proposal.category)}</div>
        <div class="proposal-confidence">Confidence: ${escapeHtml(formatConfidence(proposal.confidenceScore))}</div>
      </div>
      <div class="proposal-body">${escapeHtml(proposal.content)}</div>
      ${meta ? `<div class="proposal-meta">${escapeHtml(meta)}</div>` : ""}
      <div class="proposal-actions">
        <button type="button" data-action="approve" data-id="${proposal.id}">Approve</button>
        <button type="button" data-action="reject" data-id="${proposal.id}">Reject</button>
        <button type="button" data-action="pin" data-id="${proposal.id}">${proposal.isPinned ? "Unpin" : "Pin"}</button>
      </div>
    `;

    wrapper.querySelectorAll("button").forEach(button => {
      button.addEventListener("click", async () => {
        const action = button.dataset.action;
        const id = button.dataset.id;

        try {
          if (action === "approve") {
            await reviewMemoryProposal(id, "Accepted");
          } else if (action === "reject") {
            await reviewMemoryProposal(id, "Rejected");
          } else if (action === "pin") {
            await toggleMemoryPin(id, !proposal.isPinned);
          }
        } catch (error) {
          console.error(error);
          alert(error.message || "Proposal action failed.");
        }
      });
    });

    pendingMemoryProposalsEl.appendChild(wrapper);
  }
}

function renderProposalList() {
  renderPendingMemoryProposals(state.proposals);
}

function renderMemoryConflicts(conflicts) {
  if (!memoryConflictsList || !memoryConflictsMeta) {
    return;
  }

  memoryConflictsList.innerHTML = "";
  memoryConflictsMeta.textContent = `Conflicting proposals: ${conflicts.length}`;

  if (!conflicts || conflicts.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No conflicting proposals found.";
    memoryConflictsList.appendChild(empty);
    return;
  }

  for (const conflict of conflicts) {
    const el = document.createElement("div");
    el.className = "proposal-card";
    el.innerHTML = `
      <div><strong>Proposal Slot:</strong> ${escapeHtml(conflict.proposalSlotKey || "—")}</div>
      <div><strong>Proposal Category:</strong> ${escapeHtml(conflict.proposalCategory)}</div>
      <div><strong>Proposal Kind:</strong> ${escapeHtml(conflict.proposalKind || "—")}</div>
      <div><strong>Proposal:</strong> ${escapeHtml(conflict.proposalContent)}</div>
      <div><strong>Proposal Confidence:</strong> ${escapeHtml(String(conflict.proposalConfidenceScore ?? "—"))}</div>
      <div><strong>Proposal Reason:</strong> ${escapeHtml(conflict.proposalReason || "—")}</div>
      <div><strong>Proposal Evidence:</strong> ${escapeHtml(conflict.proposalSourceExcerpt || "—")}</div>
      <hr />
      <div><strong>Conflicts With Slot:</strong> ${escapeHtml(conflict.conflictingMemorySlotKey || "—")}</div>
      <div><strong>Conflicting Category:</strong> ${escapeHtml(conflict.conflictingMemoryCategory)}</div>
      <div><strong>Conflicting Kind:</strong> ${escapeHtml(conflict.conflictingMemoryKind || "—")}</div>
      <div><strong>Conflicting Memory:</strong> ${escapeHtml(conflict.conflictingMemoryContent)}</div>
      <div><strong>Conflicting Status:</strong> ${escapeHtml(conflict.conflictingMemoryReviewStatus)}</div>
      <div class="message-actions">
        <button type="button" data-action="accept" data-proposal-id="${conflict.proposalMemoryId}">Accept Proposal</button>
        <button type="button" data-action="reject" data-proposal-id="${conflict.proposalMemoryId}">Reject Proposal</button>
      </div>
    `;

    el.querySelectorAll("button").forEach((button) => {
      button.addEventListener("click", async () => {
        const action = button.dataset.action;
        const proposalId = button.dataset.proposalId;

        try {
          await resolveMemoryConflict(proposalId, action);
          setStatus(`Memory conflict ${action} completed`);
        } catch (error) {
          console.error(error);
          if (memoryConflictActionResult) {
            memoryConflictActionResult.textContent =
              error.message || "Conflict action failed.";
          }
          setStatus("Memory conflict action failed");
        }
      });
    });

    memoryConflictsList.appendChild(el);
  }
}

function renderRetrievalMemoryExplanations(explanations) {
  if (!retrievalMemoryExplanationsList || !retrievalMemoryExplanationsMeta) {
    return;
  }

  retrievalMemoryExplanationsList.innerHTML = "";

  const count = explanations?.length || 0;
  retrievalMemoryExplanationsMeta.textContent = `Selected memory explanations: ${count}`;

  if (!explanations || explanations.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No memory explanations available.";
    retrievalMemoryExplanationsList.appendChild(empty);
    return;
  }

  for (const explanation of explanations) {
    const suppressedHtml =
      explanation.suppressedMemories &&
      explanation.suppressedMemories.length > 0
        ? explanation.suppressedMemories
            .map(
              (s) => `
          <div class="proposal-card">
            <div><strong>Suppressed Memory:</strong> ${escapeHtml(s.content)}</div>
            <div><strong>Slot:</strong> ${escapeHtml(s.slotKey || "—")}</div>
            <div><strong>Kind:</strong> ${escapeHtml(s.kind)}</div>
            <div><strong>Final Score:</strong> ${escapeHtml(String(s.finalScore))}</div>
            <div><strong>Reason:</strong> ${escapeHtml(s.reason)}</div>
          </div>
        `,
            )
            .join("")
        : `<div class="muted-text">Nothing suppressed by this memory.</div>`;

    const el = document.createElement("div");
    el.className = "proposal-card";
    el.innerHTML = `
      <div><strong>Selected Memory:</strong> ${escapeHtml(explanation.content)}</div>
      <div><strong>Category:</strong> ${escapeHtml(explanation.category)}</div>
      <div><strong>Kind:</strong> ${escapeHtml(explanation.kind)}</div>
      <div><strong>Slot:</strong> ${escapeHtml(explanation.slotKey || "—")}</div>
      <div><strong>Semantic Score:</strong> ${escapeHtml(String(explanation.semanticScore))}</div>
      <div><strong>Final Score:</strong> ${escapeHtml(String(explanation.finalScore))}</div>
      <div><strong>Why Selected:</strong> ${escapeHtml(explanation.whySelected)}</div>
      <div><strong>Suppressed:</strong></div>
      ${suppressedHtml}
    `;

    retrievalMemoryExplanationsList.appendChild(el);
  }
}

function renderPromptMemorySelectionDebug(selectedMemoryExplanations) {
  if (
    !promptSlotWinnersList ||
    !promptSuppressedDurableList ||
    !promptSlotWinnersMeta ||
    !promptSuppressedDurableMeta
  ) {
    return;
  }

  promptSlotWinnersList.innerHTML = "";
  promptSuppressedDurableList.innerHTML = "";

  const sceneStateWinners = (selectedMemoryExplanations || []).filter(
    (x) => x.kind === "SceneState" && x.slotKey,
  );

  const suppressedDurable = (selectedMemoryExplanations || [])
    .flatMap((x) => x.suppressedMemories || [])
    .filter((x) => x.kind !== "SceneState");

  promptSlotWinnersMeta.textContent = `Scene-state slot winners: ${sceneStateWinners.length}`;
  promptSuppressedDurableMeta.textContent = `Suppressed durable memories: ${suppressedDurable.length}`;

  if (sceneStateWinners.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No scene-state slot winners found.";
    promptSlotWinnersList.appendChild(empty);
  } else {
    for (const item of sceneStateWinners) {
      const el = document.createElement("div");
      el.className = "proposal-card";
      el.innerHTML = `
        <div><strong>Slot:</strong> ${escapeHtml(item.slotKey || "—")}</div>
        <div><strong>Memory:</strong> ${escapeHtml(item.content)}</div>
        <div><strong>Why Won:</strong> ${escapeHtml(item.whySelected)}</div>
      `;
      promptSlotWinnersList.appendChild(el);
    }
  }

  if (suppressedDurable.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No durable memories were suppressed.";
    promptSuppressedDurableList.appendChild(empty);
  } else {
    for (const item of suppressedDurable) {
      const el = document.createElement("div");
      el.className = "proposal-card";
      el.innerHTML = `
        <div><strong>Slot:</strong> ${escapeHtml(item.slotKey || "—")}</div>
        <div><strong>Memory:</strong> ${escapeHtml(item.content)}</div>
        <div><strong>Reason:</strong> ${escapeHtml(item.reason)}</div>
      `;
      promptSuppressedDurableList.appendChild(el);
    }
  }
}

function renderRetrievalLoreExplanations(explanations) {
  if (!retrievalLoreExplanationsList || !retrievalLoreExplanationsMeta) {
    return;
  }

  retrievalLoreExplanationsList.innerHTML = "";
  retrievalLoreExplanationsMeta.textContent = `Selected lore explanations: ${explanations?.length || 0}`;

  if (!explanations || explanations.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No lore explanations available.";
    retrievalLoreExplanationsList.appendChild(empty);
    return;
  }

  for (const item of explanations) {
    const el = document.createElement("div");
    el.className = "proposal-card";
    el.innerHTML = `
      <div><strong>Title:</strong> ${escapeHtml(item.title)}</div>
      <div><strong>Content:</strong> ${escapeHtml(item.content)}</div>
      <div><strong>Semantic Score:</strong> ${escapeHtml(String(item.semanticScore))}</div>
      <div><strong>Final Score:</strong> ${escapeHtml(String(item.finalScore))}</div>
      <div><strong>Why Selected:</strong> ${escapeHtml(item.whySelected)}</div>
    `;
    retrievalLoreExplanationsList.appendChild(el);
  }
}

function renderLorebooks() {
  lorebookSelect.innerHTML = "";
  lorebookList.innerHTML = "";

  if (!state.lorebooks.length) {
    lorebookSelect.innerHTML = `<option value="">No lorebooks</option>`;
    lorebookList.innerHTML = `<div class="empty-state">No lorebooks yet.</div>`;
    return;
  }

  for (const lorebook of state.lorebooks) {
    const option = document.createElement("option");
    option.value = lorebook.id;
    option.textContent = lorebook.name;
    lorebookSelect.appendChild(option);

    const block = document.createElement("div");
    block.className = "lorebook-block";

    const entriesHtml =
      lorebook.entries && lorebook.entries.length
        ? lorebook.entries
            .map(
              (entry) => `
          <div class="lore-entry">
            <div class="lore-entry-title">${escapeHtml(entry.title)}</div>
            <div class="memory-item-meta">
              ${entry.isEnabled ? "Enabled" : "Disabled"} • ${escapeHtml(formatDate(entry.updatedAt))}
            </div>
            <div class="lore-entry-content">${escapeHtml(entry.content)}</div>
            <div class="memory-item-actions">
              <button type="button" data-entry-id="${entry.id}">
                ${entry.isEnabled ? "Disable" : "Enable"}
              </button>
            </div>
          </div>
        `,
            )
            .join("")
        : `<div class="empty-state">No entries.</div>`;

    block.innerHTML = `
      <div class="lorebook-title">${escapeHtml(lorebook.name)}</div>
      <div class="lorebook-description">${escapeHtml(lorebook.description)}</div>
      ${entriesHtml}
    `;

    const buttons = block.querySelectorAll("button[data-entry-id]");
    buttons.forEach((button) => {
      button.addEventListener("click", async () => {
        const entryId = button.dataset.entryId;
        const lorebookEntry = lorebook.entries.find((x) => x.id === entryId);
        if (!lorebookEntry) return;

        await updateLoreEntry(lorebookEntry, !lorebookEntry.isEnabled);
      });
    });

    lorebookList.appendChild(block);
  }
}

function renderRetrievalInspection(data) {
  retrievalInspectionResults.innerHTML = "";

  const sections = [
    {
      title: "Pinned Memory Hits",
      items: data.pinnedMemoryHits || [],
      renderItem: (item) => `
        <div class="inspection-item">
          <div class="inspection-item-title">${escapeHtml(item.category)}${item.included ? " • included" : ""}</div>
          <div class="inspection-item-score">${item.score != null ? `score ${item.score}` : "pinned"}</div>
          <div class="inspection-item-reason">${escapeHtml(item.reason)}</div>
          <div class="inspection-item-content">${escapeHtml(item.content)}</div>
        </div>
      `,
    },
    {
      title: "Semantic Memory Hits",
      items: data.semanticMemoryHits || [],
      renderItem: (item) => `
        <div class="inspection-item">
          <div class="inspection-item-title">${escapeHtml(item.category)}${item.included ? " • included" : ""}</div>
          <div class="inspection-item-score">${item.score != null ? `score ${item.score}` : "no score"}</div>
          <div class="inspection-item-reason">${escapeHtml(item.reason)}</div>
          <div class="inspection-item-content">${escapeHtml(item.content)}</div>
        </div>
      `,
    },
    {
      title: "Lore Hits",
      items: data.loreHits || [],
      renderItem: (item) => `
        <div class="inspection-item">
          <div class="inspection-item-title">${escapeHtml(item.title)}${item.included ? " • included" : ""}</div>
          <div class="inspection-item-score">score ${item.score}</div>
          <div class="inspection-item-reason">${escapeHtml(item.reason)}</div>
          <div class="inspection-item-content">${escapeHtml(item.content)}</div>
        </div>
      `,
    },
  ];

  for (const section of sections) {
    const block = document.createElement("div");
    block.className = "inspection-block";

    const itemsHtml = section.items.length
      ? section.items.map(section.renderItem).join("")
      : `<div class="empty-state">None</div>`;

    block.innerHTML = `
      <div class="inspection-title">${escapeHtml(section.title)}</div>
      ${itemsHtml}
    `;

    retrievalInspectionResults.appendChild(block);
  }

  renderRetrievalMemoryExplanations(data.selectedMemoryExplanations || []);
  renderPromptMemorySelectionDebug(data.selectedMemoryExplanations || []);
  renderRetrievalLoreExplanations(data.selectedLoreExplanations || []);
  renderInspectionRuntimeBadges();
}

function renderPromptInspection(data) {
  promptInspectionResults.innerHTML = "";
  renderPromptAuthoringSummary(data);

  const summary = document.createElement("div");
  summary.className = "prompt-summary-block";
  summary.innerHTML = `
    <div class="inspection-title">Prompt Budget</div>
    <div class="inspection-item-score">Model: ${escapeHtml(data.modelName)}</div>
    <div class="inspection-item-score">Model Profile: ${escapeHtml(data.modelProfileName || "(none)")}</div>
    <div class="inspection-item-score">Generation Preset: ${escapeHtml(data.generationPresetName || "(none)")}</div>
    <div class="inspection-item-score">Effective Context: ${escapeHtml(data.effectiveContextLength)}</div>
    <div class="inspection-item-score">Max Prompt Tokens: ${escapeHtml(data.maxPromptTokens)}</div>
    <div class="inspection-item-score">Estimated Prompt Tokens: ${escapeHtml(data.estimatedPromptTokens)}</div>
    <div class="inspection-item-reason">${data.fitsWithinBudget ? "Fits within budget." : "Exceeds budget."}</div>
  `;
  promptInspectionResults.appendChild(summary);
  renderPromptSceneStateDebug(data);
  renderPromptDurableMemoryDebug(data);

  if (!data.sections || data.sections.length === 0) {
    promptInspectionResults.innerHTML += `<div class="empty-state">No prompt sections.</div>`;
    return;
  }

  for (const section of data.sections) {
    const block = document.createElement("div");
    block.className = "prompt-section-block";
    block.innerHTML = `
      <div class="prompt-section-title">${escapeHtml(section.name)}</div>
      <div class="prompt-section-meta">Estimated tokens: ${escapeHtml(section.estimatedTokens)}</div>
      <div class="prompt-section-content">${escapeHtml(section.content)}</div>
    `;
    promptInspectionResults.appendChild(block);
  }

  renderInspectionRuntimeBadges();
}

function renderSummaryInspection(data) {
  summaryInspectionResults.innerHTML = "";

  const block = document.createElement("div");
  block.className = "inspection-block";

  if (!data.hasSummaryCheckpoint) {
    block.innerHTML = `
      <div class="inspection-title">Summary Status</div>
      <div class="empty-state">No summary checkpoint exists yet for this conversation.</div>
      <div class="summary-metrics" style="margin-top: 10px;">
        <div class="summary-metric">Total prior messages: ${escapeHtml(data.totalPriorMessageCount)}</div>
        <div class="summary-metric">Included raw messages: ${escapeHtml(data.includedRawMessageCount)}</div>
        <div class="summary-metric">Excluded raw messages: ${escapeHtml(data.excludedRawMessageCount)}</div>
      </div>
    `;
    summaryInspectionResults.appendChild(block);
    return;
  }

  block.innerHTML = `
    <div class="inspection-title">Latest Summary Checkpoint</div>

    <div class="summary-metrics">
      <div class="summary-metric">Checkpoint ID: ${escapeHtml(data.summaryCheckpointId)}</div>
      <div class="summary-metric">Created: ${escapeHtml(formatDate(data.summaryCreatedAt))}</div>
      <div class="summary-metric">Range: ${escapeHtml(data.startSequenceNumber)} → ${escapeHtml(data.endSequenceNumber)}</div>
      <div class="summary-metric">Covered messages: ${escapeHtml(data.summaryCoveredMessageCount)}</div>
      <div class="summary-metric">Used in current prompt: ${data.summaryUsedInPrompt ? "Yes" : "No"}</div>
      <div class="summary-metric">Total prior messages: ${escapeHtml(data.totalPriorMessageCount)}</div>
      <div class="summary-metric">Included raw messages: ${escapeHtml(data.includedRawMessageCount)}</div>
      <div class="summary-metric">Excluded raw messages: ${escapeHtml(data.excludedRawMessageCount)}</div>
    </div>

    <div class="summary-text">${escapeHtml(data.summaryText || "(empty)")}</div>
  `;

  summaryInspectionResults.appendChild(block);
}

function renderCharacterEditor(character) {
  state.selectedCharacterDetail = character;
  state.activeCharacterDetail = character;
  state.characterEditorMode = "edit";

  characterNameInput.value = character?.name ?? "";
  characterDescriptionInput.value = character?.description ?? "";
  characterGreetingInput.value = character?.greeting ?? "";
  characterScenarioInput.value = character?.scenario ?? "";
  characterPersonalityInput.value = character?.personalityDefinition ?? "";
  characterModelProfileSelect.value = character?.defaultModelProfileId ?? "";
  characterGenerationPresetSelect.value =
    character?.defaultGenerationPresetId ?? "";
  characterDefaultTtsVoiceInput.value = character?.defaultTtsVoice ?? "";
  characterDefaultVisualStylePresetSelect.value =
    character?.defaultVisualStylePreset ?? "none";
  characterDefaultVisualPromptPrefixInput.value =
    character?.defaultVisualPromptPrefix ?? "";
  characterDefaultVisualNegativePromptInput.value =
    character?.defaultVisualNegativePrompt ?? "";

  renderSampleDialogueEditor(character?.sampleDialogues ?? []);
  renderCharacterImagePreview(character);
  renderActiveConversationHeaderAvatar();
}

function clearCharacterEditor() {
  state.selectedCharacterDetail = null;
  state.activeCharacterDetail = null;
  state.characterEditorMode = "create";

  characterNameInput.value = "";
  characterDescriptionInput.value = "";
  characterGreetingInput.value = "";
  characterScenarioInput.value = "";
  characterPersonalityInput.value = "";
  characterModelProfileSelect.value = "";
  characterGenerationPresetSelect.value = "";
  characterDefaultTtsVoiceInput.value = "";
  characterDefaultVisualStylePresetSelect.value = "none";
  characterDefaultVisualPromptPrefixInput.value = "";
  characterDefaultVisualNegativePromptInput.value = "";

  renderSampleDialogueEditor([]);
  renderCharacterImagePreview(null);
  renderActiveConversationHeaderAvatar();
}

function renderSampleDialogueEditor(sampleDialogues) {
  characterSampleDialogueList.innerHTML = "";

  if (!sampleDialogues.length) {
    characterSampleDialogueList.innerHTML = `<div class="empty-state">No sample dialogue yet.</div>`;
    return;
  }

  sampleDialogues.forEach((dialogue, index) => {
    const item = document.createElement("div");
    item.className = "sample-dialogue-item";
    item.innerHTML = `
      <label class="memory-label">User Message</label>
      <textarea rows="3" data-kind="user">${escapeHtml(dialogue.userMessage ?? "")}</textarea>

      <label class="memory-label">Assistant Message</label>
      <textarea rows="3" data-kind="assistant">${escapeHtml(dialogue.assistantMessage ?? "")}</textarea>

      <div class="sample-dialogue-actions">
        <button type="button" data-remove-index="${index}">Remove</button>
      </div>
    `;

    item.querySelector("button").addEventListener("click", () => {
      const current = collectSampleDialogues();
      current.splice(index, 1);
      renderSampleDialogueEditor(current);
    });

    characterSampleDialogueList.appendChild(item);
  });
}

function collectSampleDialogues() {
  const items = [];
  const blocks = characterSampleDialogueList.querySelectorAll(
    ".sample-dialogue-item",
  );

  for (const block of blocks) {
    const userMessage = block
      .querySelector('textarea[data-kind="user"]')
      .value.trim();
    const assistantMessage = block
      .querySelector('textarea[data-kind="assistant"]')
      .value.trim();

    if (!userMessage || !assistantMessage) {
      continue;
    }

    items.push({
      userMessage,
      assistantMessage,
    });
  }

  return items;
}

function buildCharacterPayload() {
  return {
    name: characterNameInput.value.trim(),
    description: characterDescriptionInput.value.trim(),
    greeting: characterGreetingInput.value.trim(),
    scenario: characterScenarioInput.value.trim(),
    personalityDefinition: characterPersonalityInput.value.trim(),
    defaultModelProfileId: toNullableGuid(characterModelProfileSelect.value),
    defaultGenerationPresetId: toNullableGuid(
      characterGenerationPresetSelect.value,
    ),
    defaultTtsVoice: characterDefaultTtsVoiceInput.value.trim() || null,
    defaultVisualStylePreset:
      characterDefaultVisualStylePresetSelect.value || null,
    defaultVisualPromptPrefix:
      characterDefaultVisualPromptPrefixInput.value.trim() || null,
    defaultVisualNegativePrompt:
      characterDefaultVisualNegativePromptInput.value.trim() || null,
    sampleDialogues: collectSampleDialogues(),
  };
}

function renderPersonaEditor(persona) {
  state.selectedPersonaDetail = persona;
  state.personaEditorMode = "edit";

  personaNameInput.value = persona?.name ?? "";
  personaDisplayNameInput.value = persona?.displayName ?? "";
  personaDescriptionInput.value = persona?.description ?? "";
  personaTraitsInput.value = persona?.traits ?? "";
  personaPreferencesInput.value = persona?.preferences ?? "";
  personaInstructionsInput.value = persona?.additionalInstructions ?? "";
}

function clearPersonaEditor() {
  state.selectedPersonaDetail = null;
  state.personaEditorMode = "create";

  personaNameInput.value = "";
  personaDisplayNameInput.value = "";
  personaDescriptionInput.value = "";
  personaTraitsInput.value = "";
  personaPreferencesInput.value = "";
  personaInstructionsInput.value = "";
}

function buildPersonaPayload() {
  return {
    name: personaNameInput.value.trim(),
    displayName: personaDisplayNameInput.value.trim(),
    description: personaDescriptionInput.value.trim(),
    traits: personaTraitsInput.value.trim(),
    preferences: personaPreferencesInput.value.trim(),
    additionalInstructions: personaInstructionsInput.value.trim(),
  };
}

function renderModelProfileEditor(profile) {
  state.selectedModelProfileDetail = profile;
  state.modelProfileEditorMode = "edit";

  modelProfileNameInput.value = profile?.name ?? "";
  modelProfileProviderTypeInput.value = (
    profile?.providerType ??
    "ollama"
  ).toLowerCase();
  modelProfileModelIdentifierInput.value = profile?.modelIdentifier ?? "";
  modelProfileContextWindowInput.value = profile?.contextWindow ?? "";
  modelProfileNotesInput.value = profile?.notes ?? "";
  refreshModelProfileProviderUi();
}

function clearModelProfileEditor() {
  state.selectedModelProfileDetail = null;
  state.modelProfileEditorMode = "create";

  modelProfileNameInput.value = "";
  modelProfileProviderTypeInput.value = "ollama";
  modelProfileModelIdentifierInput.value = "";
  modelProfileContextWindowInput.value = "";
  modelProfileNotesInput.value = "";
  refreshModelProfileProviderUi();
}

function refreshModelProfileProviderUi() {
  if (!modelProfileProviderTypeInput || !modelProfileModelHelp) {
    return;
  }

  const provider = (modelProfileProviderTypeInput.value || "ollama").toLowerCase();

  if (provider === "openrouter") {
    modelProfileModelHelp.textContent =
      "Enter the OpenRouter model id only, for example: anthropic/claude-3.7-sonnet or openai/gpt-4.1-mini";
    if (
      modelProfileModelIdentifierInput &&
      !modelProfileModelIdentifierInput.value
    ) {
      modelProfileModelIdentifierInput.placeholder =
        "anthropic/claude-3.7-sonnet";
    }
  } else if (provider === "huggingface" || provider === "hf") {
    modelProfileModelHelp.textContent =
      "Enter the Hugging Face model id only, for example: Qwen/Qwen2.5-7B-Instruct";
    if (
      modelProfileModelIdentifierInput &&
      !modelProfileModelIdentifierInput.value
    ) {
      modelProfileModelIdentifierInput.placeholder = "Qwen/Qwen2.5-7B-Instruct";
    }
  } else if (
    provider === "llama.cpp" ||
    provider === "llamacpp" ||
    provider === "llama-cpp"
  ) {
    modelProfileModelHelp.textContent =
      "Enter the llama.cpp model alias or id only, for example: local-gguf-model";
    if (
      modelProfileModelIdentifierInput &&
      !modelProfileModelIdentifierInput.value
    ) {
      modelProfileModelIdentifierInput.placeholder = "local-gguf-model";
    }
  } else {
    modelProfileModelHelp.textContent =
      "Enter the Ollama model name, for example: qwen2.5:14b-instruct";
    if (
      modelProfileModelIdentifierInput &&
      !modelProfileModelIdentifierInput.value
    ) {
      modelProfileModelIdentifierInput.placeholder = "qwen2.5:14b-instruct";
    }
  }
}

function buildModelProfilePayload() {
  return {
    name: modelProfileNameInput.value.trim(),
    providerType:
      (modelProfileProviderTypeInput.value || "ollama").trim().toLowerCase(),
    modelIdentifier: modelProfileModelIdentifierInput.value.trim(),
    contextWindow: toNullableInt(modelProfileContextWindowInput.value),
    notes: modelProfileNotesInput.value.trim(),
  };
}

async function loadModelProfiles() {
  const response = await fetch("/api/model-profiles");
  if (!response.ok) {
    throw new Error("Failed to load model profiles.");
  }

  state.modelProfiles = await response.json();

  modelProfileSelect.innerHTML = `<option value="">No Model Profile</option>`;
  characterModelProfileSelect.innerHTML = `<option value="">No Model Profile</option>`;

  for (const item of state.modelProfiles) {
    const editorOption = document.createElement("option");
    editorOption.value = item.id;
    editorOption.textContent = item.name;
    modelProfileSelect.appendChild(editorOption);

    const characterOption = document.createElement("option");
    characterOption.value = item.id;
    characterOption.textContent = item.name;
    characterModelProfileSelect.appendChild(characterOption);
  }

  modelProfileSelect.value = state.selectedModelProfileId ?? "";

  fillSelectWithOptions(
    conversationModelProfileOverrideSelect,
    state.modelProfiles || [],
    "id",
    (x) => `${x.name} • ${providerBadgeLabel(x.provider || "ollama")}`,
    true,
    "(none)",
  );

  fillSelectWithOptions(
    defaultModelProfileSelect,
    state.modelProfiles || [],
    "id",
    (x) => `${x.name} • ${providerBadgeLabel(x.provider || "ollama")}`,
    true,
    "(none)",
  );
}

async function loadModelProfileDetail(id) {
  if (!id) {
    clearModelProfileEditor();
    return;
  }

  const response = await fetch(`/api/model-profiles/${id}`);
  if (!response.ok) {
    throw new Error("Failed to load model profile details.");
  }

  const data = await response.json();
  renderModelProfileEditor(data);
}

async function createModelProfile() {
  const payload = buildModelProfilePayload();

  if (!payload.name || !payload.modelIdentifier) {
    alert("Model profile name and model identifier are required.");
    return;
  }

  const response = await fetch("/api/model-profiles", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to create model profile. ${text}`);
  }

  const created = await response.json();
  state.selectedModelProfileId = created.id;
  await loadModelProfiles();
  modelProfileSelect.value = created.id;
  await loadModelProfileDetail(created.id);
  setStatus("Model profile created");
}

async function saveCurrentModelProfile() {
  if (
    state.modelProfileEditorMode === "create" ||
    !state.selectedModelProfileId
  ) {
    await createModelProfile();
    return;
  }

  const payload = buildModelProfilePayload();

  const response = await fetch(
    `/api/model-profiles/${state.selectedModelProfileId}`,
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    },
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to save model profile. ${text}`);
  }

  const updated = await response.json();
  await loadModelProfiles();
  modelProfileSelect.value = updated.id;
  await loadModelProfileDetail(updated.id);
  setStatus("Model profile saved");
}

async function deleteCurrentModelProfile() {
  if (
    !state.selectedModelProfileId ||
    state.modelProfileEditorMode === "create"
  ) {
    alert("Select an existing model profile first.");
    return;
  }

  if (!confirm("Delete this model profile?")) {
    return;
  }

  const response = await fetch(
    `/api/model-profiles/${state.selectedModelProfileId}`,
    {
      method: "DELETE",
    },
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to delete model profile. ${text}`);
  }

  state.selectedModelProfileId = null;
  await loadModelProfiles();
  clearModelProfileEditor();
  setStatus("Model profile deleted");
}

function renderGenerationPresetEditor(preset) {
  state.selectedGenerationPresetDetail = preset;
  state.generationPresetEditorMode = "edit";

  generationPresetNameInput.value = preset?.name ?? "";
  generationPresetTemperatureInput.value = preset?.temperature ?? 0.8;
  generationPresetTopPInput.value = preset?.topP ?? 0.95;
  generationPresetRepeatPenaltyInput.value = preset?.repeatPenalty ?? 1.05;
  generationPresetMaxOutputTokensInput.value = preset?.maxOutputTokens ?? "";
  generationPresetStopSequencesInput.value = preset?.stopSequencesText ?? "";
  generationPresetNotesInput.value = preset?.notes ?? "";
}

function clearGenerationPresetEditor() {
  state.selectedGenerationPresetDetail = null;
  state.generationPresetEditorMode = "create";

  generationPresetNameInput.value = "";
  generationPresetTemperatureInput.value = 0.8;
  generationPresetTopPInput.value = 0.95;
  generationPresetRepeatPenaltyInput.value = 1.05;
  generationPresetMaxOutputTokensInput.value = "";
  generationPresetStopSequencesInput.value = "";
  generationPresetNotesInput.value = "";
}

function buildGenerationPresetPayload() {
  return {
    name: generationPresetNameInput.value.trim(),
    temperature: toNumberOrDefault(generationPresetTemperatureInput.value, 0.8),
    topP: toNumberOrDefault(generationPresetTopPInput.value, 0.95),
    repeatPenalty: toNumberOrDefault(
      generationPresetRepeatPenaltyInput.value,
      1.05,
    ),
    maxOutputTokens: toNullableInt(generationPresetMaxOutputTokensInput.value),
    stopSequencesText: generationPresetStopSequencesInput.value,
    notes: generationPresetNotesInput.value.trim(),
  };
}

async function loadGenerationPresets() {
  const response = await fetch("/api/generation-presets");
  if (!response.ok) {
    throw new Error("Failed to load generation presets.");
  }

  state.generationPresets = await response.json();

  generationPresetSelect.innerHTML = `<option value="">No Generation Preset</option>`;
  characterGenerationPresetSelect.innerHTML = `<option value="">No Generation Preset</option>`;

  for (const item of state.generationPresets) {
    const editorOption = document.createElement("option");
    editorOption.value = item.id;
    editorOption.textContent = item.name;
    generationPresetSelect.appendChild(editorOption);

    const characterOption = document.createElement("option");
    characterOption.value = item.id;
    characterOption.textContent = item.name;
    characterGenerationPresetSelect.appendChild(characterOption);
  }

  generationPresetSelect.value = state.selectedGenerationPresetId ?? "";

  fillSelectWithOptions(
    conversationGenerationPresetOverrideSelect,
    state.generationPresets || [],
    "id",
    (x) => x.name,
    true,
    "(none)",
  );

  fillSelectWithOptions(
    defaultGenerationPresetSelect,
    state.generationPresets || [],
    "id",
    (x) => x.name,
    true,
    "(none)",
  );
}

async function loadGenerationPresetDetail(id) {
  if (!id) {
    clearGenerationPresetEditor();
    return;
  }

  const response = await fetch(`/api/generation-presets/${id}`);
  if (!response.ok) {
    throw new Error("Failed to load generation preset details.");
  }

  const data = await response.json();
  renderGenerationPresetEditor(data);
}

async function createGenerationPreset() {
  const payload = buildGenerationPresetPayload();

  if (!payload.name) {
    alert("Generation preset name is required.");
    return;
  }

  const response = await fetch("/api/generation-presets", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to create generation preset. ${text}`);
  }

  const created = await response.json();
  state.selectedGenerationPresetId = created.id;
  await loadGenerationPresets();
  generationPresetSelect.value = created.id;
  await loadGenerationPresetDetail(created.id);
  setStatus("Generation preset created");
}

async function saveCurrentGenerationPreset() {
  if (
    state.generationPresetEditorMode === "create" ||
    !state.selectedGenerationPresetId
  ) {
    await createGenerationPreset();
    return;
  }

  const payload = buildGenerationPresetPayload();

  const response = await fetch(
    `/api/generation-presets/${state.selectedGenerationPresetId}`,
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    },
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to save generation preset. ${text}`);
  }

  const updated = await response.json();
  await loadGenerationPresets();
  generationPresetSelect.value = updated.id;
  await loadGenerationPresetDetail(updated.id);
  setStatus("Generation preset saved");
}

async function deleteCurrentGenerationPreset() {
  if (
    !state.selectedGenerationPresetId ||
    state.generationPresetEditorMode === "create"
  ) {
    alert("Select an existing generation preset first.");
    return;
  }

  if (!confirm("Delete this generation preset?")) {
    return;
  }

  const response = await fetch(
    `/api/generation-presets/${state.selectedGenerationPresetId}`,
    {
      method: "DELETE",
    },
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to delete generation preset. ${text}`);
  }

  state.selectedGenerationPresetId = null;
  await loadGenerationPresets();
  clearGenerationPresetEditor();
  setStatus("Generation preset deleted");
}

async function loadCharacters() {
  const response = await fetch("/api/characters");
  if (!response.ok) {
    throw new Error("Failed to load characters.");
  }

  const data = await response.json();
  state.characters = data;

  characterSelect.innerHTML = "";

  for (const character of state.characters) {
    const option = document.createElement("option");
    option.value = character.id;
    option.textContent = character.name;
    characterSelect.appendChild(option);
  }

  if (state.characters.length > 0) {
    const stillExists = state.characters.some(
      (x) => x.id === state.selectedCharacterId,
    );
    state.selectedCharacterId = stillExists
      ? state.selectedCharacterId
      : state.characters[0].id;

    characterSelect.value = state.selectedCharacterId;
  } else {
    state.selectedCharacterId = null;
  }

  renderCharacterImagePreview(getSelectedCharacter());
  renderActiveConversationHeaderAvatar();
}

async function loadCharacterDetail(characterId) {
  if (!characterId) {
    clearCharacterEditor();
    return;
  }

  const response = await fetch(`/api/characters/${characterId}`);
  if (!response.ok) {
    throw new Error("Failed to load character details.");
  }

  const data = await response.json();
  renderCharacterEditor(data);
}

async function loadActiveCharacterDetail() {
  if (!state.selectedCharacterId) {
    state.activeCharacterDetail = null;
    return null;
  }

  const response = await fetch(`/api/characters/${state.selectedCharacterId}`);
  if (!response.ok) {
    throw new Error("Failed to load character detail.");
  }

  const detail = await response.json();
  state.activeCharacterDetail = detail;
  renderActiveConversationHeaderAvatar();
  return detail;
}

async function createCharacter() {
  const payload = buildCharacterPayload();

  if (
    !payload.name ||
    !payload.description ||
    !payload.greeting ||
    !payload.scenario ||
    !payload.personalityDefinition
  ) {
    alert("All character fields are required.");
    return;
  }

  const response = await fetch("/api/characters", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to create character. ${text}`);
  }

  const created = await response.json();
  await loadCharacters();
  state.selectedCharacterId = created.id;
  characterSelect.value = created.id;
  await loadCharacterDetail(created.id);
  await loadConversations();
  await loadMemories();
  await loadProposals();
  await loadLorebooks();
  startNewConversation();
  setStatus("Character created");
}

async function saveCurrentCharacter() {
  if (state.characterEditorMode === "create" || !state.selectedCharacterId) {
    await createCharacter();
    return;
  }

  const payload = buildCharacterPayload();

  if (
    !payload.name ||
    !payload.description ||
    !payload.greeting ||
    !payload.scenario ||
    !payload.personalityDefinition
  ) {
    alert("All character fields are required.");
    return;
  }

  const response = await fetch(`/api/characters/${state.selectedCharacterId}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to save character. ${text}`);
  }

  const updated = await response.json();
  await loadCharacters();
  characterSelect.value = updated.id;
  await loadCharacterDetail(updated.id);
  setStatus("Character saved");
}

async function deleteCurrentCharacter() {
  if (!state.selectedCharacterId || state.characterEditorMode === "create") {
    alert("Select an existing character first.");
    return;
  }

  if (!confirm("Delete this character?")) {
    return;
  }

  const response = await fetch(`/api/characters/${state.selectedCharacterId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to delete character. ${text}`);
  }

  await loadCharacters();

  if (state.selectedCharacterId) {
    characterSelect.value = state.selectedCharacterId;
    await loadCharacterDetail(state.selectedCharacterId);
  } else {
    clearCharacterEditor();
  }

  await loadConversations();
  await loadMemories();
  await loadProposals();
  await loadLorebooks();
  startNewConversation();
  setStatus("Character deleted");
}

async function loadPersonas() {
  const response = await fetch("/api/personas");
  if (!response.ok) {
    throw new Error("Failed to load personas.");
  }

  const data = await response.json();
  state.personas = data;

  personaSelect.innerHTML = `<option value="">No Persona</option>`;

  for (const persona of state.personas) {
    const option = document.createElement("option");
    option.value = persona.id;
    option.textContent = `${persona.displayName || persona.name || persona.id}${persona.isDefault ? " • DEFAULT" : ""}`;
    personaSelect.appendChild(option);
  }

  const stillExists = state.personas.some(
    (x) => x.id === state.selectedPersonaId,
  );
  state.selectedPersonaId = stillExists ? state.selectedPersonaId : null;
  personaSelect.value = state.selectedPersonaId ?? "";

  fillSelectWithOptions(
    conversationPersonaSelect,
    state.personas || [],
    "id",
    (x) => `${x.displayName || x.name || x.id}${x.isDefault ? " • DEFAULT" : ""}`,
    true,
    "(none)",
  );

  fillSelectWithOptions(
    defaultPersonaSelect,
    state.personas || [],
    "id",
    (x) => `${x.displayName || x.name || x.id}${x.isDefault ? " • DEFAULT" : ""}`,
    true,
    "(none)",
  );
}

async function loadPersonaDetail(personaId) {
  if (!personaId) {
    clearPersonaEditor();
    return;
  }

  const response = await fetch(`/api/personas/${personaId}`);
  if (!response.ok) {
    throw new Error("Failed to load persona details.");
  }

  const data = await response.json();
  renderPersonaEditor(data);
}

async function createPersona() {
  const payload = buildPersonaPayload();

  if (
    !payload.name ||
    !payload.displayName ||
    !payload.description ||
    !payload.traits ||
    !payload.preferences ||
    !payload.additionalInstructions
  ) {
    alert("All persona fields are required.");
    return;
  }

  const response = await fetch("/api/personas", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to create persona. ${text}`);
  }

  const created = await response.json();
  await loadPersonas();
  state.selectedPersonaId = created.id;
  personaSelect.value = created.id;
  await loadPersonaDetail(created.id);
  setStatus("Persona created");
}

async function saveCurrentPersona() {
  if (state.personaEditorMode === "create" || !state.selectedPersonaId) {
    await createPersona();
    return;
  }

  const payload = buildPersonaPayload();

  if (
    !payload.name ||
    !payload.displayName ||
    !payload.description ||
    !payload.traits ||
    !payload.preferences ||
    !payload.additionalInstructions
  ) {
    alert("All persona fields are required.");
    return;
  }

  const response = await fetch(`/api/personas/${state.selectedPersonaId}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to save persona. ${text}`);
  }

  const updated = await response.json();
  await loadPersonas();
  state.selectedPersonaId = updated.id;
  personaSelect.value = updated.id;
  await loadPersonaDetail(updated.id);
  setStatus("Persona saved");
}

async function deleteCurrentPersona() {
  if (!state.selectedPersonaId || state.personaEditorMode === "create") {
    alert("Select an existing persona first.");
    return;
  }

  const deleted = await deletePersonaWithPreview(state.selectedPersonaId);
  if (!deleted) {
    return;
  }

  state.selectedPersonaId = null;
  await loadPersonas();
  await loadAppRuntimeDefaults();
  clearPersonaEditor();
  setStatus("Persona deleted");
}

async function loadConversations() {
  if (!state.selectedCharacterId) {
    state.conversations = [];
    renderConversationList();
    return;
  }

  const response = await fetch(
    `/api/conversations/by-character/${state.selectedCharacterId}`,
  );
  if (!response.ok) {
    throw new Error("Failed to load conversations.");
  }

  state.conversations = await response.json();
  renderConversationList();
}

async function loadMemories() {
  if (!state.selectedCharacterId) {
    state.memories = [];
    renderMemoryList();
    return;
  }

  const useConversationScope = memoryUseConversationScope.checked;
  const query =
    useConversationScope && state.activeConversationId
      ? `?conversationId=${encodeURIComponent(state.activeConversationId)}`
      : "";

  const response = await fetch(
    `/api/memory/by-character/${state.selectedCharacterId}${query}`,
  );
  if (!response.ok) {
    throw new Error("Failed to load memory items.");
  }

  state.memories = await response.json();
  renderMemoryList();
}

async function loadProposals() {
  if (!state.activeConversationId) {
    state.proposals = [];
    renderProposalList();
    if (pendingMemoryProposalsEl) {
      pendingMemoryProposalsEl.innerHTML = `<div class="empty-state">Open a conversation to review proposals.</div>`;
    }
    return;
  }

  const response = await fetch(
    `/api/memory/proposals/by-character/${state.selectedCharacterId}?conversationId=${encodeURIComponent(state.activeConversationId)}`,
  );
  if (!response.ok) {
    throw new Error("Failed to load memory proposals.");
  }

  const items = await response.json();
  state.proposals = items.filter((x) => x.reviewStatus === "Proposed");
  renderProposalList();
}

async function loadMemoryConflicts() {
  if (!memoryConflictsMeta || !memoryConflictsList) {
    return;
  }

  if (!state.activeConversationId) {
    memoryConflictsMeta.textContent =
      "Open a conversation to inspect memory conflicts.";
    memoryConflictsList.innerHTML = "";
    return;
  }

  const response = await fetch(
    `/api/memory/conflicts/by-conversation/${state.activeConversationId}`,
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to load memory conflicts. ${text}`);
  }

  const conflicts = await response.json();
  renderMemoryConflicts(conflicts);
}

async function resolveMemoryConflict(proposalMemoryId, action) {
  const url =
    action === "accept"
      ? `/api/memory/conflicts/${proposalMemoryId}/accept`
      : `/api/memory/conflicts/${proposalMemoryId}/reject`;

  const response = await fetch(url, {
    method: "POST",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to resolve memory conflict. ${text}`);
  }

  const result = await response.json();

  if (memoryConflictActionResult) {
    memoryConflictActionResult.textContent =
      `Action: ${result.action} • ` +
      `Succeeded: ${result.succeeded ? "yes" : "no"} • ` +
      `Message: ${result.message || "—"} • ` +
      `Conflicting memory rejected: ${result.conflictingMemoryRejected ? "yes" : "no"} • ` +
      `Retrieval reindexed: ${result.retrievalReindexed ? "yes" : "no"}`;
  }

  if (typeof loadMemoryConflicts === "function") {
    await loadMemoryConflicts();
  }

  if (typeof loadRetrievalInspection === "function") {
    try {
      await loadRetrievalInspection();
    } catch (error) {
      console.error(error);
    }
  }

  if (typeof loadMemories === "function") {
    try {
      await loadMemories();
    } catch (error) {
      console.error(error);
    }
  }

  return result;
}

async function editMemoryItem(memory) {
  const content = window.prompt("Edit memory content", memory.content);
  if (content === null) {
    return;
  }

  const slotKey = window.prompt("Edit slot key", memory.slotKey || "");
  if (slotKey === null) {
    return;
  }

  const slotFamily = window.prompt("Edit slot family", memory.slotFamily || "");
  if (slotFamily === null) {
    return;
  }

  const response = await fetch(`/api/memory/${memory.id}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      content: content.trim(),
      category: memory.category,
      kind: memory.kind,
      reviewStatus: memory.reviewStatus,
      slotKey: slotKey.trim() || null,
      slotFamily: slotFamily.trim() || null,
      isPinned: memory.isPinned,
      proposalReason: memory.proposalReason || null,
      sourceExcerpt: memory.sourceExcerpt || null,
      expiresAt: memory.expiresAt || null,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to update memory. ${text}`);
  }

  await refreshMemoryViews();
}

async function mergeMemoryItem(memory) {
  const targetId = window.prompt(
    `Merge memory ${memory.id} into target memory id.\n\nCurrent slot: ${memory.slotKey || "—"}`,
  );

  if (!targetId) {
    return;
  }

  const preferSourceContent = window.confirm(
    "Use the source memory's content/category/kind as the merged target content?",
  );

  const slotKeyOverride = window.prompt(
    "Optional slot override (leave blank to keep target/source slot)",
    memory.slotKey || "",
  );

  const slotFamilyOverride = window.prompt(
    "Optional slot family override (leave blank to keep target/source family)",
    memory.slotFamily || "",
  );

  const response = await fetch(
    `/api/memory/${memory.id}/merge-into/${targetId.trim()}`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        preferSourceContent,
        slotKeyOverride: slotKeyOverride?.trim() || null,
        slotFamilyOverride: slotFamilyOverride?.trim() || null,
        rejectSourceAfterMerge: true,
        preserveTargetReviewStatus: true,
      }),
    },
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to merge memory. ${text}`);
  }

  const result = await response.json();
  alert(result.message || "Merge completed.");

  await refreshMemoryViews();
}

async function refreshMemoryViews() {
  if (typeof loadMemories === "function") {
    await loadMemories();
  }

  if (typeof loadMemoryConflicts === "function") {
    try {
      await loadMemoryConflicts();
    } catch (error) {
      console.error(error);
    }
  }

  if (typeof loadRetrievalInspection === "function") {
    try {
      await loadRetrievalInspection();
    } catch (error) {
      console.error(error);
    }
  }

  if (typeof loadSceneStateInspection === "function") {
    try {
      await loadSceneStateInspection();
    } catch (error) {
      console.error(error);
    }
  }

  if (typeof loadMemoryExtractionAudit === "function") {
    try {
      await loadMemoryExtractionAudit();
    } catch (error) {
      console.error(error);
    }
  }
}

async function loadSceneStateInspection() {
  if (!sceneStateInspectionMeta || !sceneStateActiveList || !sceneStateReplacementHistoryList || !sceneStateFamilyCollisionList) {
    return;
  }

  if (!state.activeConversationId) {
    sceneStateInspectionMeta.textContent =
      "Open a conversation to inspect scene-state.";
    sceneStateActiveList.innerHTML = "";
    sceneStateReplacementHistoryList.innerHTML = "";
    sceneStateFamilyCollisionList.innerHTML = "";
    return;
  }

  const response = await fetch(
    `/api/inspection/scene-state/conversations/${state.activeConversationId}`,
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to load scene-state inspection. ${text}`);
  }

  const data = await response.json();
  renderSceneStateInspection(data);
}

function renderSceneStateInspection(data) {
  if (!sceneStateInspectionMeta || !sceneStateActiveList || !sceneStateReplacementHistoryList || !sceneStateFamilyCollisionList) {
    return;
  }

  sceneStateActiveList.innerHTML = "";
  sceneStateReplacementHistoryList.innerHTML = "";
  sceneStateFamilyCollisionList.innerHTML = "";

  sceneStateInspectionMeta.textContent =
    `Active families: ${data.activeSceneState?.length || 0} • ` +
    `Replacements: ${data.replacementHistory?.length || 0} • ` +
    `Family collisions: ${data.familyCollisions?.length || 0}`;

  if (!data.activeSceneState || data.activeSceneState.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No active scene-state.";
    sceneStateActiveList.appendChild(empty);
  } else {
    for (const item of data.activeSceneState) {
      const el = document.createElement("div");
      el.className = "proposal-card";
      el.innerHTML = `
        <div><strong>Family:</strong> ${escapeHtml(item.slotFamily)}</div>
        <div><strong>Slot:</strong> ${escapeHtml(item.slotKey || "—")}</div>
        <div><strong>Content:</strong> ${escapeHtml(item.content)}</div>
        <div><strong>Expires:</strong> ${escapeHtml(formatDateTime(item.expiresAt))}</div>
        <div><strong>Updated:</strong> ${escapeHtml(formatDateTime(item.updatedAt))}</div>
      `;
      sceneStateActiveList.appendChild(el);
    }
  }

  if (!data.replacementHistory || data.replacementHistory.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No replacement history.";
    sceneStateReplacementHistoryList.appendChild(empty);
  } else {
    for (const item of data.replacementHistory) {
      const el = document.createElement("div");
      el.className = "proposal-card";
      el.innerHTML = `
        <div><strong>Action:</strong> ${escapeHtml(item.action)}</div>
        <div><strong>Family:</strong> ${escapeHtml(item.slotFamily)}</div>
        <div><strong>Slot:</strong> ${escapeHtml(item.slotKey || "—")}</div>
        <div><strong>Candidate:</strong> ${escapeHtml(item.candidateContent)}</div>
        <div><strong>Replaced:</strong> ${escapeHtml(item.replacedMemoryContent || "—")}</div>
        <div><strong>Notes:</strong> ${escapeHtml(item.notes || "—")}</div>
        <div><strong>At:</strong> ${escapeHtml(formatDateTime(item.createdAt))}</div>
      `;
      sceneStateReplacementHistoryList.appendChild(el);
    }
  }

  if (!data.familyCollisions || data.familyCollisions.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No family collisions detected.";
    sceneStateFamilyCollisionList.appendChild(empty);
  } else {
    for (const item of data.familyCollisions) {
      const el = document.createElement("div");
      el.className = "proposal-card";
      el.innerHTML = `
        <div><strong>Family:</strong> ${escapeHtml(item.slotFamily)}</div>
        <div><strong>Candidate:</strong> ${escapeHtml(item.candidateContent)}</div>
        <div><strong>Replaced:</strong> ${escapeHtml(item.replacedMemoryContent || "—")}</div>
        <div><strong>Notes:</strong> ${escapeHtml(item.notes || "—")}</div>
        <div><strong>At:</strong> ${escapeHtml(formatDateTime(item.createdAt))}</div>
      `;
      sceneStateFamilyCollisionList.appendChild(el);
    }
  }
}

async function loadMemoryExtractionAudit() {
  if (!memoryExtractionAuditMeta || !memoryExtractionAuditList) {
    return;
  }

  if (!state.activeConversationId) {
    memoryExtractionAuditMeta.textContent =
      "Open a conversation to inspect memory extraction.";
    memoryExtractionAuditList.innerHTML = "";
    return;
  }

  const response = await fetch(
    `/api/inspection/memory-extraction/conversations/${state.activeConversationId}`,
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to load memory extraction audit. ${text}`);
  }

  const data = await response.json();
  renderMemoryExtractionAudit(data);
}

function renderMemoryExtractionAudit(data) {
  if (!memoryExtractionAuditMeta || !memoryExtractionAuditList) {
    return;
  }

  memoryExtractionAuditList.innerHTML = "";

  memoryExtractionAuditMeta.textContent =
    `Events: ${data.totalEventCount} • ` +
    `Durable: ${data.durableEventCount} • ` +
    `Scene-state: ${data.sceneStateEventCount}`;

  if (!data.events || data.events.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No extraction audit events found.";
    memoryExtractionAuditList.appendChild(empty);
    return;
  }

  for (const item of data.events) {
    const el = document.createElement("div");
    el.className = "proposal-card";
    el.innerHTML = `
      <div><strong>Action:</strong> ${escapeHtml(item.action)}</div>
      <div><strong>Category:</strong> ${escapeHtml(item.category)}</div>
      <div><strong>Kind:</strong> ${escapeHtml(item.kind)}</div>
      <div><strong>Family:</strong> ${escapeHtml(item.slotFamily)}</div>
      <div><strong>Slot:</strong> ${escapeHtml(item.slotKey || "—")}</div>
      <div><strong>Candidate:</strong> ${escapeHtml(item.candidateContent)}</div>
      <div><strong>Confidence:</strong> ${escapeHtml(String(item.confidenceScore))}</div>
      <div><strong>Existing:</strong> ${escapeHtml(item.existingMemoryContent || "—")}</div>
      <div><strong>Notes:</strong> ${escapeHtml(item.notes || "—")}</div>
      <div><strong>At:</strong> ${escapeHtml(formatDateTime(item.createdAt))}</div>
    `;

    memoryExtractionAuditList.appendChild(el);
  }
}

function renderPromptSceneStateDebug(data) {
  if (!promptSceneStateDebugMeta || !promptSceneStateSelectedList || !promptSceneStateSuppressedList) {
    return;
  }

  promptSceneStateSelectedList.innerHTML = "";
  promptSceneStateSuppressedList.innerHTML = "";

  const selected = data.selectedSceneState || [];
  const suppressed = data.suppressedSceneState || [];

  promptSceneStateDebugMeta.textContent = `Selected families: ${selected.length} • Suppressed families: ${suppressed.length}`;

  if (selected.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No selected scene-state prompt entries.";
    promptSceneStateSelectedList.appendChild(empty);
  } else {
    for (const item of selected) {
      const el = document.createElement("div");
      el.className = "proposal-card";
      el.innerHTML = `
        <div><strong>Family:</strong> ${escapeHtml(item.slotFamily)}</div>
        <div><strong>Slot:</strong> ${escapeHtml(item.slotKey || "—")}</div>
        <div><strong>Memory:</strong> ${escapeHtml(item.content)}</div>
        <div><strong>Prompt Content:</strong> ${escapeHtml(item.promptContent)}</div>
      `;
      promptSceneStateSelectedList.appendChild(el);
    }
  }

  if (suppressed.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No suppressed scene-state prompt entries.";
    promptSceneStateSuppressedList.appendChild(empty);
  } else {
    for (const item of suppressed) {
      const el = document.createElement("div");
      el.className = "proposal-card";
      el.innerHTML = `
        <div><strong>Family:</strong> ${escapeHtml(item.slotFamily)}</div>
        <div><strong>Memory:</strong> ${escapeHtml(item.content)}</div>
        <div><strong>Reason:</strong> ${escapeHtml(item.reason)}</div>
      `;
      promptSceneStateSuppressedList.appendChild(el);
    }
  }
}

function renderPromptDurableMemoryDebug(data) {
  if (!promptDurableMemoryDebugMeta || !promptDurableMemorySelectedList || !promptDurableMemorySuppressedList) {
    return;
  }

  promptDurableMemorySelectedList.innerHTML = "";
  promptDurableMemorySuppressedList.innerHTML = "";

  const selected = data.selectedDurableMemory || [];
  const suppressed = data.suppressedDurableMemory || [];

  promptDurableMemoryDebugMeta.textContent = `Selected durable: ${selected.length} • Suppressed durable: ${suppressed.length}`;

  if (selected.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No selected durable-memory prompt entries.";
    promptDurableMemorySelectedList.appendChild(empty);
  } else {
    for (const item of selected) {
      const el = document.createElement("div");
      el.className = "proposal-card";
      el.innerHTML = `
        <div><strong>Category:</strong> ${escapeHtml(item.category)}</div>
        <div><strong>Memory:</strong> ${escapeHtml(item.content)}</div>
        <div><strong>Prompt Content:</strong> ${escapeHtml(item.promptContent)}</div>
      `;
      promptDurableMemorySelectedList.appendChild(el);
    }
  }

  if (suppressed.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No suppressed durable-memory prompt entries.";
    promptDurableMemorySuppressedList.appendChild(empty);
  } else {
    for (const item of suppressed) {
      const el = document.createElement("div");
      el.className = "proposal-card";
      el.innerHTML = `
        <div><strong>Category:</strong> ${escapeHtml(item.category)}</div>
        <div><strong>Memory:</strong> ${escapeHtml(item.content)}</div>
        <div><strong>Reason:</strong> ${escapeHtml(item.reason)}</div>
      `;
      promptDurableMemorySuppressedList.appendChild(el);
    }
  }
}

async function pruneMemoryExtractionAudit() {
  const raw = window.prompt(
    "Delete audit events older than how many days?",
    "30",
  );
  if (raw === null) {
    return;
  }

  const olderThanDays = Number.parseInt(raw, 10);
  const response = await fetch(
    `/api/admin/maintenance/audit/memory-extraction/prune?olderThanDays=${olderThanDays}`,
    {
      method: "POST",
    },
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to prune memory extraction audit. ${text}`);
  }

  const result = await response.json();

  maintenanceResult.textContent =
    `Audit prune • succeeded: ${result.succeeded ? "yes" : "no"} • ` +
    `older than days: ${result.olderThanDays} • ` +
    `deleted: ${result.deletedCount} • ` +
    `message: ${result.message || "—"}`;

  return result;
}

function exportMemoryExtractionAudit() {
  if (!state.activeConversationId) {
    alert("Open a conversation first.");
    return;
  }

  window.location.href = `/api/admin/maintenance/audit/memory-extraction/export/conversations/${state.activeConversationId}?maxCount=250`;
}

async function pruneStaleSceneState() {
  const response = await fetch(
    "/api/admin/maintenance/memory/prune-stale-scene-state",
    {
      method: "POST",
    },
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to prune stale scene-state. ${text}`);
  }

  const result = await response.json();

  const familyText = Object.entries(result.removedByFamily || {})
    .map(([key, value]) => `${key}:${value}`)
    .join(", ");

  maintenanceResult.textContent =
    `Scene-state cleanup • succeeded: ${result.succeeded ? "yes" : "no"} • ` +
    `scanned: ${result.scannedCount} • ` +
    `removed: ${result.removedCount} • ` +
    `families: ${familyText || "—"} • ` +
    `message: ${result.message || "—"}`;

  return result;
}

async function rebuildMemoryKeys() {
  const response = await fetch("/api/admin/maintenance/memory/rebuild-keys", {
    method: "POST",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to rebuild memory keys. ${text}`);
  }

  const result = await response.json();

  maintenanceResult.textContent =
    `Memory key rebuild • succeeded: ${result.succeeded ? "yes" : "no"} • ` +
    `scanned: ${result.scannedCount} • ` +
    `normalized rebuilt: ${result.rebuiltNormalizedKeyCount} • ` +
    `slot rebuilt: ${result.rebuiltSlotKeyCount} • ` +
    `slot family rebuilt: ${result.rebuiltSlotFamilyCount} • ` +
    `updated: ${result.updatedCount} • ` +
    `message: ${result.message || "—"}`;

  return result;
}

async function reindexAllRetrieval() {
  const response = await fetch("/api/admin/maintenance/retrieval/reindex-all", {
    method: "POST",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to reindex retrieval. ${text}`);
  }

  const result = await response.json();

  maintenanceResult.textContent =
    `Retrieval reindex-all • succeeded: ${result.succeeded ? "yes" : "no"} • ` +
    `documents: ${result.reindexedDocumentCount} • ` +
    `message: ${result.message || "—"}`;

  return result;
}

async function loadLorebooks() {
  if (!state.selectedCharacterId) {
    state.lorebooks = [];
    renderLorebooks();
    return;
  }

  const response = await fetch(
    `/api/lorebooks?characterId=${encodeURIComponent(state.selectedCharacterId)}`,
  );
  if (!response.ok) {
    throw new Error("Failed to load lorebooks.");
  }

  state.lorebooks = await response.json();
  renderLorebooks();
}

async function executeSlashCommand(commandText) {
  const response = await fetch("/api/commands/execute", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      characterId: state.selectedCharacterId,
      conversationId: state.activeConversationId,
      commandText
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to execute command. ${text}`);
  }

  const result = await response.json();

  if (typeof result.directorInstructions !== "undefined") {
    renderDirectorStatus(result.directorInstructions);
  }

  if (
    typeof result.sceneContext !== "undefined" ||
    typeof result.isOocModeEnabled !== "undefined"
  ) {
    renderSceneStatus(result.sceneContext, !!result.isOocModeEnabled);
  }

  if (result.reloadConversation && result.conversationId) {
    state.activeConversationId = result.conversationId;
    await loadConversations();
    await openConversation(result.conversationId);
    await loadMemories();
    await loadProposals();
  }

  appendMessage("System", result.message);
  setStatus(
    result.succeeded ? `Command: /${result.commandName}` : "Command failed",
  );
}

async function openConversation(conversationId) {
  const response = await fetch(`/api/conversations/${conversationId}`);
  if (!response.ok) {
    throw new Error("Failed to load conversation.");
  }

  const conversation = await response.json();
  state.activeConversationId = conversation.id;
  await loadActiveCharacterDetail();

  const characterDefaults = getActiveCharacterVisualDefaults();
  populateImageStylePresetSelect(
    imageStylePresetSelect,
    characterDefaults.defaultVisualStylePreset || "none",
  );

  state.selectedPersonaId = conversation.userPersonaId ?? null;
  personaSelect.value = state.selectedPersonaId ?? "";
  if (state.selectedPersonaId) {
    await loadPersonaDetail(state.selectedPersonaId);
  } else {
    clearPersonaEditor();
  }
  applyConversationSettingsToUi(conversation);

  conversationTitle.textContent = conversation.parentConversationId
    ? `${conversation.title} · branched`
    : conversation.title;

  renderDirectorStatus(conversation.directorInstructions);
  renderSceneStatus(conversation.sceneContext, conversation.isOocModeEnabled);
  state.messages = Array.isArray(conversation.messages)
    ? conversation.messages
    : [];
  renderActiveConversationHeaderAvatar();
  renderMessages(state.messages);
  renderActiveConversationRuntime(state.messages || []);
  renderInspectionRuntimeBadges();
  renderConversationList();
    if (contextualImagePromptPreview) {
      contextualImagePromptPreview.textContent =
        "Build a prompt from the active conversation to preview scene summary and unknowns.";
    }
    if (suggestedUserMessageMeta) {
      suggestedUserMessageMeta.textContent =
        "Suggest a reply from the current conversation context.";
    }
    await loadMemories();
  await loadProposals();
  try {
    await loadMemoryConflicts();
  } catch (error) {
    console.error(error);
  }
  try {
    await loadSceneStateInspection();
  } catch (error) {
    console.error(error);
  }
  try {
    await loadMemoryExtractionAudit();
  } catch (error) {
    console.error(error);
  }
  await loadGeneratedImages();
  await loadBackgroundProposalStatus();
  setStatus(`Loaded conversation ${conversation.id}`);
}

async function createConversation() {
  if (!state.selectedCharacterId) {
    alert("No character selected.");
    return;
  }

  const response = await fetch("/api/conversations", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      characterId: state.selectedCharacterId,
      userPersonaId: state.selectedPersonaId,
      title: "New Conversation",
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to create conversation. ${text}`);
  }

  const created = await response.json();
  const createdConversationId = created?.conversationId || created?.id || null;
  if (!createdConversationId) {
    throw new Error("Conversation created, but no conversation id was returned.");
  }

  await loadConversations();
  await openConversation(createdConversationId);
}

function startNewConversation() {
  state.activeConversationId = null;
  state.messages = [];
  conversationTitle.textContent = "New Conversation";
  renderActiveConversationHeaderAvatar();
  clearMessages();
  renderEmptyMessages("Start a new conversation.");
  renderActiveConversationRuntime(state.messages || []);
  renderInspectionRuntimeBadges();
  renderConversationList();
  applyConversationSettingsToUi(null);
  clearConversationRuntimeOverrideUi();
  if (conversationPersonaSelect) {
    conversationPersonaSelect.value = "";
  }
  loadMemories().catch(console.error);
  loadProposals().catch(console.error);
  loadSceneStateInspection().catch(console.error);
  loadMemoryExtractionAudit().catch(console.error);
  promptAuthoringSummary.innerHTML = `<div class="empty-state">Run a prompt inspection to see authoring sections clearly separated.</div>`;
  retrievalInspectionResults.innerHTML = `<div class="empty-state">Run an inspection to see why memory or lore would be included.</div>`;
  promptInspectionResults.innerHTML = `<div class="empty-state">Run a prompt inspection to see exact final prompt sections.</div>`;
  summaryInspectionResults.innerHTML = `<div class="empty-state">Start or open a conversation, then inspect summary status.</div>`;
  renderDirectorStatus(null);
  renderSceneStatus(null, false);
  generatedImagesGallery.innerHTML = `<div class="empty-state">Open a conversation to view generated images.</div>`;
  populateImageStylePresetSelect(imageStylePresetSelect, "none");
  if (contextualImagePromptPreview) {
    contextualImagePromptPreview.textContent =
      "Build a prompt from the active conversation to preview scene summary and unknowns.";
  }
  if (pendingMemoryProposalsEl) {
    pendingMemoryProposalsEl.innerHTML = `<div class="empty-state">Open a conversation to review proposals.</div>`;
  }
  if (memoryConflictsMeta) {
    memoryConflictsMeta.textContent =
      "Open a conversation to inspect memory conflicts.";
  }
  if (memoryConflictsList) {
    memoryConflictsList.innerHTML = "";
  }
  if (memoryConflictActionResult) {
    memoryConflictActionResult.textContent = "No conflict action taken yet.";
  }
  if (retrievalMemoryExplanationsMeta) {
    retrievalMemoryExplanationsMeta.textContent =
      "No retrieval explanation data loaded yet.";
  }
  if (retrievalMemoryExplanationsList) {
    retrievalMemoryExplanationsList.innerHTML = "";
  }
  if (retrievalLoreExplanationsMeta) {
    retrievalLoreExplanationsMeta.textContent =
      "No lore explanation data loaded yet.";
  }
  if (retrievalLoreExplanationsList) {
    retrievalLoreExplanationsList.innerHTML = "";
  }
  if (promptSlotWinnersMeta) {
    promptSlotWinnersMeta.textContent = "No prompt slot data loaded yet.";
  }
  if (promptSlotWinnersList) {
    promptSlotWinnersList.innerHTML = "";
  }
  if (promptSuppressedDurableMeta) {
    promptSuppressedDurableMeta.textContent =
      "No suppressed durable memory data loaded yet.";
  }
  if (promptSuppressedDurableList) {
    promptSuppressedDurableList.innerHTML = "";
  }
  if (promptSceneStateDebugMeta) {
    promptSceneStateDebugMeta.textContent =
      "No prompt scene-state debug data loaded yet.";
  }
  if (promptSceneStateSelectedList) {
    promptSceneStateSelectedList.innerHTML = "";
  }
  if (promptSceneStateSuppressedList) {
    promptSceneStateSuppressedList.innerHTML = "";
  }
  if (promptDurableMemoryDebugMeta) {
    promptDurableMemoryDebugMeta.textContent =
      "No prompt durable-memory debug data loaded yet.";
  }
  if (promptDurableMemorySelectedList) {
    promptDurableMemorySelectedList.innerHTML = "";
  }
  if (promptDurableMemorySuppressedList) {
    promptDurableMemorySuppressedList.innerHTML = "";
  }
  if (backgroundProposalStatusPanel) {
    backgroundProposalStatusPanel.innerHTML = `<div class="empty-state">Open a conversation or refresh worker status.</div>`;
  }
  setStatus("Ready for a new conversation");
}

function parseSseBlock(block) {
  const lines = block.split("\n");
  let eventName = "message";
  let data = "";

  for (const line of lines) {
    if (line.startsWith("event:")) {
      eventName = line.slice("event:".length).trim();
    } else if (line.startsWith("data:")) {
      data += line.slice("data:".length).trim();
    }
  }

  if (!data) {
    return null;
  }

  return {
    eventName,
    payload: JSON.parse(data),
  };
}

function appendStreamingAssistantPlaceholder() {
  const wrapper = document.createElement("div");
  wrapper.className = "message assistant";

  wrapper.innerHTML = `
    <span class="role">Assistant</span>
    <div class="content"></div>
  `;

  messagesEl.appendChild(wrapper);
  messagesEl.scrollTop = messagesEl.scrollHeight;

  return wrapper.querySelector(".content");
}

async function consumeSseResponseStream(response, handlers) {
  if (!response.body) {
    throw new Error("Streaming response body was empty.");
  }

  const reader = response.body.getReader();
  const decoder = new TextDecoder();
  let buffer = "";

  while (true) {
    const { value, done } = await reader.read();
    if (done) {
      break;
    }

    buffer += decoder.decode(value, { stream: true });

    let boundaryIndex;
    while ((boundaryIndex = buffer.indexOf("\n\n")) >= 0) {
      const rawEvent = buffer.slice(0, boundaryIndex).trim();
      buffer = buffer.slice(boundaryIndex + 2);

      if (!rawEvent) {
        continue;
      }

      const parsed = parseSseBlock(rawEvent);
      if (!parsed) {
        continue;
      }

      const { eventName, payload } = parsed;
      if (eventName === "started" && handlers.onStarted) {
        await handlers.onStarted(payload);
      } else if (eventName === "token-delta" && handlers.onTokenDelta) {
        await handlers.onTokenDelta(payload);
      } else if (eventName === "completed" && handlers.onCompleted) {
        await handlers.onCompleted(payload);
      } else if (eventName === "error" && handlers.onError) {
        await handlers.onError(payload);
      }
    }
  }
}

async function sendMessage() {
  if (state.isStreaming) {
    return;
  }

  const message = messageInput.value.trim();
  if (!message) {
    return;
  }

  if (message.startsWith("/")) {
    try {
      messageInput.value = "";
      await executeSlashCommand(message);
    } catch (error) {
      console.error(error);
      setStatus("Command error");
      alert(error.message || "Failed to execute command.");
    }

    return;
  }

  if (!state.selectedCharacterId) {
    alert("No character selected.");
    return;
  }

  setStreaming(true);
  setStatus("Streaming response...");

  if (!state.activeConversationId) {
    conversationTitle.textContent = "New Conversation";
    clearMessages();
  }

  appendMessage("User", message);
  messageInput.value = "";
  clearAuthoringEnhancementReview();

  let assistantContentEl = null;

  try {
    const sendUrl = `/api/chat/send/stream${buildTurnOverrideQueryString()}`;
    const response = await fetch(sendUrl, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        characterId: state.selectedCharacterId,
        conversationId: state.activeConversationId,
        userPersonaId: state.activeConversationId
          ? null
          : state.selectedPersonaId,
        message,
      }),
    });

    if (!response.ok || !response.body) {
      throw new Error("Failed to start chat stream.");
    }

    const reader = response.body.getReader();
    const decoder = new TextDecoder();
    let buffer = "";

    while (true) {
      const { value, done } = await reader.read();
      if (done) {
        break;
      }

      buffer += decoder.decode(value, { stream: true });

      let separatorIndex;
      while ((separatorIndex = buffer.indexOf("\n\n")) >= 0) {
        const block = buffer.slice(0, separatorIndex).trim();
        buffer = buffer.slice(separatorIndex + 2);

        if (!block) {
          continue;
        }

        const parsed = parseSseBlock(block);
        if (!parsed) {
          continue;
        }

        if (parsed.eventName === "started") {
          setStatus("Assistant is thinking...");
        } else if (parsed.eventName === "token-delta") {
          if (!assistantContentEl) {
            assistantContentEl = appendMessage("Assistant", "");
          }

          assistantContentEl.textContent += parsed.payload.delta;
          messagesEl.scrollTop = messagesEl.scrollHeight;
        } else if (parsed.eventName === "completed") {
          clearTurnRuntimeOverride();
          state.activeConversationId = parsed.payload.conversationId;
          await loadConversations();
          await openConversation(state.activeConversationId);
          await loadMemories();
          await loadProposals();
          renderInspectionRuntimeBadges();
          setStatus("Completed");

          const currentConversation = state.conversations.find(
            (x) => x.id === state.activeConversationId,
          );

          if (currentConversation) {
            conversationTitle.textContent = currentConversation.title;
          }
        } else if (parsed.eventName === "error") {
          throw new Error(parsed.payload.message || "Unknown streaming error.");
        }
      }
    }
  } catch (error) {
    console.error(error);
    setStatus("Error");
    alert(error.message || "Something went wrong.");
  } finally {
    setStreaming(false);
  }
}

async function suggestUserMessage() {
  if (!state.activeConversationId) {
    alert("Open a conversation first.");
    return;
  }

  if (state.isStreaming) {
    return;
  }

  setStatus("Generating suggested reply...");

  const response = await fetch("/api/chat/suggest-user-message", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      conversationId: state.activeConversationId,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to generate suggested reply. ${text}`);
  }

  const result = await response.json();

  messageInput.value = result.suggestedMessage ?? "";
  renderSuggestedUserMessageMeta(result);
  messageInput.focus();
  messageInput.setSelectionRange(
    messageInput.value.length,
    messageInput.value.length,
  );

  setStatus("Suggested reply ready");
}

async function createMemory() {
  if (!state.selectedCharacterId) {
    alert("No character selected.");
    return;
  }

  const content = memoryContentInput.value.trim();
  if (!content) {
    alert("Memory content cannot be empty.");
    return;
  }

  const scopeToConversation = memoryConversationScopedInput.checked;
  const conversationId = scopeToConversation
    ? state.activeConversationId
    : null;

  if (scopeToConversation && !conversationId) {
    alert(
      "You need an active conversation to create conversation-scoped memory.",
    );
    return;
  }

  const response = await fetch("/api/memory", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      characterId: state.selectedCharacterId,
      conversationId,
      category: memoryCategorySelect.value,
      content,
      isPinned: memoryPinnedInput.checked,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to create memory. ${text}`);
  }

  memoryContentInput.value = "";
  memoryConversationScopedInput.checked = false;
  await loadMemories();
  setStatus("Memory created");
}

async function updateAcceptedMemoryPin(memoryId, isPinned) {
  const response = await fetch(`/api/memory/${memoryId}/review`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      status: "Accepted",
      isPinned,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to update memory pin state. ${text}`);
  }

  await loadMemories();
  await loadProposals();
  setStatus("Memory updated");
}

async function promoteMemoryToCharacter(memoryId) {
  const response = await fetch(`/api/memory/${memoryId}/promote-to-character`, {
    method: "POST",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || "Failed to promote memory.");
  }

  await refreshMemoryViews();
  setStatus("Memory promoted to character scope");
}

async function demoteMemoryToConversation(memoryId) {
  if (!state.activeConversationId) {
    throw new Error("No active conversation selected.");
  }

  const response = await fetch(
    `/api/memory/${memoryId}/demote-to-conversation/${state.activeConversationId}`,
    {
      method: "POST",
    },
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || "Failed to demote memory.");
  }

  await refreshMemoryViews();
  setStatus("Memory demoted to current conversation");
}

function summarizeProposalGenerationResult(result) {
  return [
    `Attempted: ${result.attemptedCandidates}`,
    `Created: ${result.createdProposalCount}`,
    `Skipped low confidence: ${result.skippedLowConfidenceCount}`,
    `Skipped duplicates: ${result.skippedDuplicateCount}`,
    `Conflicts annotated: ${result.conflictAnnotatedCount}`,
    `Invalid: ${result.invalidCandidateCount}`,
  ].join(" • ");
}

async function generateMemoryProposals() {
  if (!state.activeConversationId) {
    alert("Open a conversation first.");
    return;
  }

  setStatus("Generating proposals...");

  const response = await fetch(
    `/api/memory/proposals/generate/${state.activeConversationId}`,
    {
      method: "POST",
    },
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to generate proposals. ${text}`);
  }

  const result = await response.json();

  await loadProposals();
  await loadMemories();

  const summary = summarizeProposalGenerationResult(result);
  appendMessage("System", `Proposal generation complete.\n${summary}`);
  setStatus("Proposal generation complete");
}

async function generateProposals() {
  await generateMemoryProposals();
}

async function reviewMemoryProposal(memoryId, status) {
  await reviewProposal(memoryId, status);
}

async function toggleMemoryPin(memoryId, isPinned) {
  await updateAcceptedMemoryPin(memoryId, isPinned);
}

async function reviewProposal(memoryId, status) {
  const response = await fetch(`/api/memory/${memoryId}/review`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      status,
      isPinned: false,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to review proposal. ${text}`);
  }

  await loadProposals();
  await loadMemories();
  setStatus(`Proposal ${status.toLowerCase()}`);
}

function renderBackgroundProposalStatus(status) {
  if (!backgroundProposalStatusPanel) {
    return;
  }

  const enabledClass = status.enabled ? "status-good" : "status-warn";
  const runningClass = status.isSweepRunning ? "status-warn" : "status-good";

  backgroundProposalStatusPanel.innerHTML = `
    <div><strong>Enabled:</strong> <span class="${enabledClass}">${status.enabled ? "Yes" : "No"}</span></div>
    <div><strong>Sweep Running:</strong> <span class="${runningClass}">${status.isSweepRunning ? "Yes" : "No"}</span></div>
    <div><strong>Last Sweep Started:</strong> ${status.lastSweepStartedAt ?? "n/a"}</div>
    <div><strong>Last Sweep Completed:</strong> ${status.lastSweepCompletedAt ?? "n/a"}</div>
    <div><strong>Triggered Conversations:</strong> ${status.lastSweepTriggeredConversationCount}</div>
    <div><strong>Cooldown Tracked:</strong> ${status.cooldownTrackedConversationCount}</div>
    <div><strong>Last Message:</strong> ${escapeHtml(status.lastSweepMessage ?? "n/a")}</div>
  `;
}

async function loadBackgroundProposalStatus() {
  if (!backgroundProposalStatusPanel) {
    return;
  }

  const response = await fetch("/api/admin/memory-proposals/background/status");

  if (!response.ok) {
    throw new Error("Failed to load background proposal worker status.");
  }

  const status = await response.json();
  renderBackgroundProposalStatus(status);
}

function renderBackgroundWorkStatus(data) {
  if (!backgroundWorkMeta || !backgroundWorkPendingList) {
    return;
  }

  backgroundWorkMeta.textContent =
    `Queue enabled: ${data.queueEnabled ? "yes" : "no"} • ` +
    `Pending conversations: ${data.pendingConversationCount} • ` +
    `Sweep enabled: ${data.backgroundProposalSweepEnabled ? "yes" : "no"} • ` +
    `Sweep running: ${data.backgroundProposalSweepRunning ? "yes" : "no"} • ` +
    `Last sweep triggered: ${data.lastSweepTriggeredConversationCount}`;

  backgroundWorkPendingList.innerHTML = "";

  if (!data.pendingItems || data.pendingItems.length === 0) {
    const empty = document.createElement("div");
    empty.className = "muted-text";
    empty.textContent = "No pending background work.";
    backgroundWorkPendingList.appendChild(empty);
    return;
  }

  for (const item of data.pendingItems) {
    const el = document.createElement("div");
    el.className = "proposal-card";
    el.innerHTML = `
      <div><strong>Conversation:</strong> ${escapeHtml(item.conversationId)}</div>
      <div><strong>Reason:</strong> ${escapeHtml(item.lastReason || "—")}</div>
      <div><strong>Last scheduled:</strong> ${escapeHtml(formatDateTime(item.lastScheduledAt))}</div>
      <div><strong>Retrieval due:</strong> ${escapeHtml(formatDateTime(item.retrievalDueAt))} ${item.retrievalDueNow ? "• due now" : ""}</div>
      <div><strong>Memory due:</strong> ${escapeHtml(formatDateTime(item.memoryDueAt))} ${item.memoryDueNow ? "• due now" : ""}</div>
      <div><strong>Summary due:</strong> ${escapeHtml(formatDateTime(item.summaryDueAt))} ${item.summaryDueNow ? "• due now" : ""}</div>
    `;

    backgroundWorkPendingList.appendChild(el);
  }
}

async function loadBackgroundWorkStatus() {
  if (!backgroundWorkMeta || !backgroundWorkPendingList) {
    return;
  }

  const response = await fetch("/api/admin/background-work/status");

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to load background work status. ${text}`);
  }

  const data = await response.json();
  renderBackgroundWorkStatus(data);
}

function renderBackgroundWorkTriggerResult(result) {
  if (!backgroundWorkTriggerResult) {
    return;
  }

  const parts = [
    `Operation: ${result.operation}`,
    `Succeeded: ${result.succeeded ? "yes" : "no"}`,
  ];

  if (result.message) {
    parts.push(`Message: ${result.message}`);
  }

  if (
    result.summaryRefreshed !== null &&
    result.summaryRefreshed !== undefined
  ) {
    parts.push(`Summary refreshed: ${result.summaryRefreshed ? "yes" : "no"}`);
  }

  if (result.summaryEndSequenceNumber) {
    parts.push(`Summary end seq: ${result.summaryEndSequenceNumber}`);
  }

  if (
    result.attemptedCandidates !== null &&
    result.attemptedCandidates !== undefined
  ) {
    parts.push(`Attempted: ${result.attemptedCandidates}`);
  }

  if (
    result.createdProposalCount !== null &&
    result.createdProposalCount !== undefined
  ) {
    parts.push(`Proposals: ${result.createdProposalCount}`);
  }

  if (
    result.autoSavedSceneStateCount !== null &&
    result.autoSavedSceneStateCount !== undefined
  ) {
    parts.push(`Auto scene-state: ${result.autoSavedSceneStateCount}`);
  }

  if (
    result.autoAcceptedDurableCount !== null &&
    result.autoAcceptedDurableCount !== undefined
  ) {
    parts.push(`Auto durable: ${result.autoAcceptedDurableCount}`);
  }

  if (
    result.retrievalReindexed !== null &&
    result.retrievalReindexed !== undefined
  ) {
    parts.push(`Reindexed: ${result.retrievalReindexed ? "yes" : "no"}`);
  }

  backgroundWorkTriggerResult.textContent = parts.join(" • ");
}

async function triggerBackgroundWork(operation) {
  if (!state.activeConversationId) {
    alert("Open a conversation first.");
    return;
  }

  let url = "";

  if (operation === "summary") {
    url = `/api/admin/background-work/conversations/${state.activeConversationId}/refresh-summary`;
  } else if (operation === "memory") {
    url = `/api/admin/background-work/conversations/${state.activeConversationId}/extract-memory`;
  } else if (operation === "reindex") {
    url = `/api/admin/background-work/conversations/${state.activeConversationId}/reindex-retrieval`;
  } else {
    throw new Error(`Unknown background work operation: ${operation}`);
  }

  const response = await fetch(url, {
    method: "POST",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed background work trigger. ${text}`);
  }

  const result = await response.json();
  renderBackgroundWorkTriggerResult(result);

  await loadBackgroundWorkStatus();

  return result;
}

async function runBackgroundProposalNow() {
  if (!state.activeConversationId) {
    alert("Open a conversation first.");
    return;
  }

  setStatus("Running background proposal generation...");

  const response = await fetch(
    `/api/admin/memory-proposals/background/run/${state.activeConversationId}`,
    {
      method: "POST",
    },
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to run background proposal generation. ${text}`);
  }

  const result = await response.json();

  await loadBackgroundProposalStatus();
  await loadProposals();
  await loadMemories();

  const summary = summarizeProposalGenerationResult(result);
  appendMessage(
    "System",
    `Background proposal run finished.\n${result.message}\n${summary}`,
  );
  setStatus("Background proposal run complete");
}

async function createLorebook() {
  if (!state.selectedCharacterId) {
    alert("No character selected.");
    return;
  }

  const name = lorebookNameInput.value.trim();
  const description = lorebookDescriptionInput.value.trim();

  if (!name || !description) {
    alert("Lorebook name and description are required.");
    return;
  }

  const response = await fetch("/api/lorebooks", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      characterId: state.selectedCharacterId,
      name,
      description,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to create lorebook. ${text}`);
  }

  lorebookNameInput.value = "";
  lorebookDescriptionInput.value = "";
  await loadLorebooks();
  setStatus("Lorebook created");
}

async function createLoreEntry() {
  const lorebookId = lorebookSelect.value;
  const title = loreEntryTitleInput.value.trim();
  const content = loreEntryContentInput.value.trim();

  if (!lorebookId) {
    alert("Select a lorebook.");
    return;
  }

  if (!title || !content) {
    alert("Lore entry title and content are required.");
    return;
  }

  const response = await fetch("/api/lorebooks/entries", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      lorebookId,
      title,
      content,
      isEnabled: loreEntryEnabledInput.checked,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to create lore entry. ${text}`);
  }

  loreEntryTitleInput.value = "";
  loreEntryContentInput.value = "";
  loreEntryEnabledInput.checked = true;
  await loadLorebooks();
  setStatus("Lore entry created");
}

async function updateLoreEntry(entry, isEnabled) {
  const response = await fetch(`/api/lorebooks/entries/${entry.id}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      title: entry.title,
      content: entry.content,
      isEnabled,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to update lore entry. ${text}`);
  }

  await loadLorebooks();
  setStatus("Lore entry updated");
}

async function inspectRetrieval() {
  if (!state.selectedCharacterId) {
    alert("No character selected.");
    return;
  }

  const query = retrievalQueryInput.value.trim() || messageInput.value.trim();
  if (!query) {
    alert("Enter an inspection query or type in the message box first.");
    return;
  }

  const response = await fetch("/api/inspection/retrieval", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      characterId: state.selectedCharacterId,
      conversationId: state.activeConversationId,
      query,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to inspect retrieval. ${text}`);
  }

  const data = await response.json();
  renderRetrievalInspection(data);
  setStatus("Retrieval inspected");
}

async function loadRetrievalInspection() {
  if (!state.selectedCharacterId) {
    return;
  }

  const query = retrievalQueryInput.value.trim() || messageInput.value.trim();
  if (!query) {
    return;
  }

  const response = await fetch("/api/inspection/retrieval", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      characterId: state.selectedCharacterId,
      conversationId: state.activeConversationId,
      query,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to inspect retrieval. ${text}`);
  }

  const data = await response.json();
  renderRetrievalInspection(data);
}

async function inspectPrompt() {
  if (!state.selectedCharacterId) {
    alert("No character selected.");
    return;
  }

  const query = messageInput.value.trim() || retrievalQueryInput.value.trim();
  if (!query) {
    alert("Enter a message or retrieval query first.");
    return;
  }

  const response = await fetch("/api/inspection/prompt", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      characterId: state.selectedCharacterId,
      conversationId: state.activeConversationId,
      userPersonaId: state.activeConversationId
        ? null
        : state.selectedPersonaId,
      query,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to inspect prompt. ${text}`);
  }

  const data = await response.json();
  renderPromptInspection(data);
  setStatus("Prompt inspected");
}

async function inspectSummary() {
  if (!state.selectedCharacterId) {
    alert("No character selected.");
    return;
  }

  if (!state.activeConversationId) {
    alert("Open an active conversation first.");
    return;
  }

  const query =
    messageInput.value.trim() ||
    retrievalQueryInput.value.trim() ||
    "Continue the conversation.";

  const response = await fetch("/api/inspection/summary", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      characterId: state.selectedCharacterId,
      conversationId: state.activeConversationId,
      query,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to inspect summary. ${text}`);
  }

  const data = await response.json();
  renderSummaryInspection(data);
  setStatus("Summary inspected");
}
async function exportCurrentCharacter() {
  if (!state.selectedCharacterId || state.characterEditorMode === "create") {
    alert("Select an existing character first.");
    return;
  }

  const response = await fetch(
    `/api/import-export/characters/${state.selectedCharacterId}`,
  );
  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to export character. ${text}`);
  }

  const data = await response.json();
  downloadJson(`${data.name || "character"}.character.json`, data);
  setStatus("Character exported");
}

async function importCharacter() {
  const raw = characterImportJsonInput.value.trim();
  if (!raw) {
    alert("Paste character JSON first.");
    return;
  }

  const payload = JSON.parse(raw);

  const response = await fetch("/api/import-export/characters", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to import character. ${text}`);
  }

  const created = await response.json();
  characterImportJsonInput.value = "";

  await loadCharacters();
  state.selectedCharacterId = created.id;
  characterSelect.value = created.id;
  await loadCharacterDetail(created.id);
  await loadConversations();
  await loadMemories();
  await loadProposals();
  await loadLorebooks();
  startNewConversation();
  setStatus("Character imported");
}

async function exportCurrentPersona() {
  if (!state.selectedPersonaId || state.personaEditorMode === "create") {
    alert("Select an existing persona first.");
    return;
  }

  const response = await fetch(
    `/api/import-export/personas/${state.selectedPersonaId}`,
  );
  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to export persona. ${text}`);
  }

  const data = await response.json();
  downloadJson(`${data.name || "persona"}.persona.json`, data);
  setStatus("Persona exported");
}

async function importPersona() {
  const raw = personaImportJsonInput.value.trim();
  if (!raw) {
    alert("Paste persona JSON first.");
    return;
  }

  const payload = JSON.parse(raw);

  const response = await fetch("/api/import-export/personas", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to import persona. ${text}`);
  }

  const created = await response.json();
  personaImportJsonInput.value = "";

  await loadPersonas();
  state.selectedPersonaId = created.id;
  personaSelect.value = created.id;
  await loadPersonaDetail(created.id);
  setStatus("Persona imported");
}

async function assignPersonaToActiveConversation() {
  if (!state.activeConversationId) {
    alert("Open an active conversation first.");
    return;
  }

  const response = await fetch(
    `/api/conversations/${state.activeConversationId}/persona`,
    {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        userPersonaId: state.selectedPersonaId || null,
      }),
    },
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to update conversation persona. ${text}`);
  }

  await loadConversations();
  setStatus(
    state.selectedPersonaId
      ? "Conversation persona updated"
      : "Conversation persona cleared",
  );
}

function renderPromptAuthoringSummary(data) {
  promptAuthoringSummary.innerHTML = "";

  const cards = [
    {
      title: "Character Definition",
      content: data.characterDefinitionSection
    },
    {
      title: "Character Scenario",
      content: data.characterScenarioSection
    },
    {
      title: "Sample Dialogue",
      content: data.sampleDialogueSection
    },
    {
      title: "User Persona",
      content: data.userPersonaSection
    },
    {
      title: "Director Instructions",
      content: data.directorSection
    },
    {
      title: "Scene Context",
      content: data.sceneContextSection
    },
    {
      title: "OOC Mode",
      content: data.oocModeSection
    }
  ].filter((x) => x.content && x.content.trim().length > 0);

  if (!cards.length) {
    promptAuthoringSummary.innerHTML = `<div class="empty-state">No authoring sections were included for this inspection.</div>`;
    return;
  }

  const grid = document.createElement("div");
  grid.className = "authoring-summary-grid";

  for (const card of cards) {
    const el = document.createElement("div");
    el.className = "authoring-summary-card";
    el.innerHTML = `
      <div class="authoring-summary-title">${escapeHtml(card.title)}</div>
      <div class="authoring-summary-content">${escapeHtml(card.content)}</div>
    `;
    grid.appendChild(el);
  }

  promptAuthoringSummary.appendChild(grid);
}

function renderContextualImagePromptPreview(result) {
  if (!contextualImagePromptPreview) {
    return;
  }

  const lines = [];

  if (result.sceneSummary) {
    lines.push(`Scene Summary: ${result.sceneSummary}`);
  }

  if (result.appliedStylePreset) {
    const preset = IMAGE_STYLE_PRESETS[result.appliedStylePreset];
    if (preset) {
      lines.push(`Applied Style Preset: ${preset.label}`);
    }
  }

  if (result.assumptionsOrUnknowns && result.assumptionsOrUnknowns.length > 0) {
    lines.push("");
    lines.push("Assumptions / Unknowns:");
    for (const item of result.assumptionsOrUnknowns) {
      lines.push(`- ${item}`);
    }
  }

  contextualImagePromptPreview.textContent = lines.length > 0
    ? lines.join("\n")
    : "Prompt drafted from conversation context.";
}

async function buildImagePromptFromConversation() {
  if (!state.activeConversationId) {
    alert("Open a conversation first.");
    return;
  }

  await loadActiveCharacterDetail();

  setStatus("Building image prompt from conversation...");

  const response = await fetch("/api/images/contextual-prompt", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      conversationId: state.activeConversationId,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to build contextual image prompt. ${text}`);
  }

  const result = await response.json();
  const merged = applyVisualDefaultsAndPreset(
    result.positivePrompt ?? "",
    result.negativePrompt ?? "",
  );

  imagePromptInput.value = merged.positivePrompt;
  imageNegativePromptInput.value = merged.negativePrompt;
  renderContextualImagePromptPreview({
    ...result,
    appliedStylePreset: merged.stylePresetKey,
  });

  setStatus("Image prompt drafted from conversation");
}

async function buildAndGenerateImageFromConversation() {
  await buildImagePromptFromConversation();
  await generateConversationImage();
}

function renderGeneratedImages(jobs) {
  generatedImagesGallery.innerHTML = "";

  if (!jobs || jobs.length === 0) {
    generatedImagesGallery.innerHTML = `<div class="empty-state">No generated images yet for this conversation.</div>`;
    return;
  }

  for (const job of jobs) {
    const wrapper = document.createElement("div");
    wrapper.className = "generated-image-job";

    const assetsHtml = (job.assets || [])
      .map(asset => `
        <div>
          <img src="${asset.url}" alt="Generated image" />
          <div class="generated-image-meta">${escapeHtml(asset.fileName)}</div>
        </div>
      `)
      .join("");

    wrapper.innerHTML = `
      <div><strong>Status:</strong> ${escapeHtml(job.status)}</div>
      <div class="generated-image-meta">Prompt: ${escapeHtml(job.promptText)}</div>
      ${job.negativePromptText ? `<div class="generated-image-meta">Negative: ${escapeHtml(job.negativePromptText)}</div>` : ""}
      <div class="generated-image-meta">Size: ${job.width}x${job.height} • Steps: ${job.steps} • CFG: ${job.cfg} • Seed: ${job.seed}</div>
      ${job.errorMessage ? `<div class="generated-image-meta">Error: ${escapeHtml(job.errorMessage)}</div>` : ""}
      <div class="generated-image-grid">${assetsHtml}</div>
    `;

    generatedImagesGallery.appendChild(wrapper);
  }
}

async function loadGeneratedImages() {
  if (!state.activeConversationId) {
    generatedImagesGallery.innerHTML = `<div class="empty-state">Open a conversation to view generated images.</div>`;
    return;
  }

  const response = await fetch(`/api/images/conversations/${state.activeConversationId}`);
  if (!response.ok) {
    if (response.status === 404) {
      generatedImagesGallery.innerHTML = `<div class="empty-state">Image generation endpoint is not available.</div>`;
      return;
    }

    throw new Error("Failed to load generated images.");
  }

  const jobs = await response.json();
  renderGeneratedImages(jobs);
}

async function generateConversationImage() {
  if (!state.activeConversationId) {
    alert("Open a conversation first.");
    return;
  }

  const prompt = imagePromptInput.value.trim();
  if (!prompt) {
    alert("Image prompt is required.");
    return;
  }

  setStatus("Generating image...");

  const response = await fetch("/api/images/generate", {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      conversationId: state.activeConversationId,
      prompt,
      negativePrompt: imageNegativePromptInput.value.trim() || null,
      width: Number.parseInt(imageWidthInput.value || "1024", 10),
      height: Number.parseInt(imageHeightInput.value || "1024", 10),
      steps: Number.parseInt(imageStepsInput.value || "28", 10),
      cfg: Number.parseFloat(imageCfgInput.value || "7"),
      seed: Number.parseInt(imageSeedInput.value || "-1", 10)
    })
  });

  if (!response.ok) {
    if (response.status === 404) {
      throw new Error("Image generation endpoint is not available.");
    }

    const text = await response.text();
    throw new Error(`Failed to generate image. ${text}`);
  }

  await loadGeneratedImages();
  setStatus("Image generated");
}
async function synthesizeMessageSpeech(messageId) {
  const speedValue = Number.parseFloat(ttsSpeedInput.value);
  const payload = {
    voice: ttsVoiceOverrideInput.value.trim() || null,
    modelIdentifier: null,
    speed: Number.isNaN(speedValue) ? 1.0 : speedValue
  };

  setStatus("Generating speech...");

  const response = await fetch(`/api/tts/messages/${messageId}/synthesize`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(payload)
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to synthesize speech. ${text}`);
  }

  const clip = await response.json();

  const slot = document.querySelector(`[data-audio-slot="${messageId}"]`);
  if (slot) {
    slot.innerHTML = `
      <audio controls autoplay src="${clip.url}"></audio>
      <div class="memory-item-meta">Voice: ${escapeHtml(clip.voice)} • Provider: ${escapeHtml(clip.provider)}</div>
    `;
  }

  setStatus("Speech generated");
}

async function branchConversation(messageId) {
  if (!state.activeConversationId) {
    alert("Open a conversation first.");
    return;
  }

  const response = await fetch(
    `/api/conversations/${state.activeConversationId}/branch/${messageId}`,
    { method: "POST" },
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to branch conversation. ${text}`);
  }

  const branched = await response.json();

  await loadConversations();
  state.activeConversationId = branched.id;
  await openConversation(branched.id);

  setStatus("Conversation branched");
}

async function editConversationMessage(message) {
  if (!state.activeConversationId || !message) {
    return;
  }

  const updatedContent = window.prompt("Edit message", message.content);
  if (updatedContent === null) {
    return;
  }

  const trimmed = updatedContent.trim();
  if (!trimmed) {
    alert("Message content cannot be empty.");
    return;
  }

  let regenerateAssistant = false;

  if (message.role === "User") {
    regenerateAssistant = window.confirm(
      "Regenerate the assistant reply after this edit if possible?\n\nThis will delete all later messages.",
    );
  } else {
    alert(
      "Editing an assistant message will delete all later messages and replace this assistant message content.",
    );
  }

  setStatus("Updating message...");

  const response = await fetch(
    `/api/conversations/${state.activeConversationId}/messages/${message.id}`,
    {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        content: trimmed,
        regenerateAssistant,
      }),
    },
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to update message. ${text}`);
  }

  const result = await response.json();

  await loadConversations();
  await openConversation(state.activeConversationId);

  appendMessage(
    "System",
    `${result.operation} completed. Deleted downstream messages: ${result.deletedMessageCount}.` +
      (result.assistantRegenerated ? ` Assistant reply regenerated.` : ""),
  );

  setStatus("Message updated");
}

async function deleteConversationMessage(message) {
  if (!state.activeConversationId || !message) {
    return;
  }

  const confirmed = window.confirm(
    "Delete this message and all messages after it?\n\nThis cannot be undone.",
  );

  if (!confirmed) {
    return;
  }

  let regenerateAssistant = false;

  if (message.role === "Assistant" || message.role === "User") {
    regenerateAssistant = window.confirm(
      "Try to regenerate an assistant reply after deletion if possible?",
    );
  }

  setStatus("Deleting message...");

  const response = await fetch(
    `/api/conversations/${state.activeConversationId}/messages/${message.id}/delete`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        regenerateAssistant,
      }),
    },
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to delete message. ${text}`);
  }

  const result = await response.json();

  await loadConversations();
  await openConversation(state.activeConversationId);

  appendMessage(
    "System",
    `${result.operation} completed. Deleted messages: ${result.deletedMessageCount}.` +
      (result.assistantRegenerated ? ` Assistant reply regenerated.` : ""),
  );

  setStatus("Message deleted");
}

async function regenerateAssistantMessage(messageId) {
  if (!state.activeConversationId) {
    alert("Open a conversation first.");
    return;
  }

  setStatus("Regenerating assistant message...");

  const regenerateUrl = `/api/chat/regenerate${buildTurnOverrideQueryString()}`;
  const response = await fetch(regenerateUrl, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      conversationId: state.activeConversationId,
      assistantMessageId: messageId,
    }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to regenerate assistant message. ${text}`);
  }

  clearTurnRuntimeOverride();
  await openConversation(state.activeConversationId);
  renderInspectionRuntimeBadges();
  setStatus("Assistant message regenerated");
}

async function continueConversation() {
  if (state.isStreaming) {
    return;
  }

  if (!state.activeConversationId) {
    alert("Open a conversation first.");
    return;
  }

  setStreaming(true);
  setStatus("Continuing conversation...");

  const placeholderContent = appendStreamingAssistantPlaceholder();
  let accumulated = "";

  try {
    const continueUrl = `/api/chat/continue/stream${buildTurnOverrideQueryString()}`;
    const response = await fetch(continueUrl, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        conversationId: state.activeConversationId,
      }),
    });

    if (!response.ok) {
      const text = await response.text();
      throw new Error(`Failed to continue conversation. ${text}`);
    }

    await consumeSseResponseStream(response, {
      onStarted: async () => {
        setStatus("Continuation started");
      },
      onTokenDelta: async (payload) => {
        accumulated += payload.delta ?? "";
        placeholderContent.textContent = accumulated;
        messagesEl.scrollTop = messagesEl.scrollHeight;
      },
      onCompleted: async (payload) => {
        clearTurnRuntimeOverride();
        state.activeConversationId = payload.conversationId;
        await loadConversations();
        await openConversation(payload.conversationId);
        renderInspectionRuntimeBadges();
        setStatus("Continuation complete");
      },
      onError: async (payload) => {
        throw new Error(payload?.message || "Continuation failed.");
      },
    });
  } catch (error) {
    placeholderContent.textContent = accumulated || "[Continuation failed]";
    throw error;
  } finally {
    setStreaming(false);
  }
}

async function selectMessageVariant(messageId, variantIndex) {
  if (!state.activeConversationId) {
    alert("Open a conversation first.");
    return;
  }

  const response = await fetch(
    `/api/conversations/${state.activeConversationId}/messages/${messageId}/selected-variant`,
    {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        variantIndex,
      }),
    },
  );

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to select message variant. ${text}`);
  }

  await openConversation(state.activeConversationId);
  setStatus("Swipe selected");
}

async function cycleSwipe(messages, messageId, direction) {
  const message = messages.find((x) => x.id === messageId);
  if (!message || !message.variants || message.variants.length <= 1) {
    return;
  }

  const currentIndex = message.selectedVariantIndex ?? 0;
  const allIndices = message.variants
    .map((x) => x.variantIndex)
    .sort((a, b) => a - b);

  let currentPosition = allIndices.indexOf(currentIndex);
  if (currentPosition < 0) {
    currentPosition = 0;
  }

  const nextPosition =
    (currentPosition + direction + allIndices.length) % allIndices.length;

  const nextVariantIndex = allIndices[nextPosition];
  await selectMessageVariant(messageId, nextVariantIndex);
}

characterSelect.addEventListener("change", async (event) => {
  state.selectedCharacterId = event.target.value;
  state.activeConversationId = null;
  await loadCharacterDetail(state.selectedCharacterId);
  await loadConversations();
  await loadMemories();
  await loadProposals();
  await loadLorebooks();
  startNewConversation();
});

refreshCharactersBtn.addEventListener("click", async () => {
  await loadCharacters();
  if (state.selectedCharacterId) {
    characterSelect.value = state.selectedCharacterId;
    await loadCharacterDetail(state.selectedCharacterId);
  }
  setStatus("Characters refreshed");
});

newCharacterBtn.addEventListener("click", () => {
  clearCharacterEditor();
  setStatus("Creating new character");
});

saveCharacterBtn.addEventListener("click", async () => {
  try {
    await saveCurrentCharacter();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to save character.");
  }
});

if (uploadCharacterImageBtn) {
  uploadCharacterImageBtn.addEventListener("click", async () => {
    try {
      if (characterImageStatus) {
        characterImageStatus.textContent = "Uploading character image...";
      }
      await uploadCharacterImage();
      setStatus("Character image uploaded");
    } catch (error) {
      console.error(error);
      if (characterImageStatus) {
        characterImageStatus.textContent =
          error.message || "Character image upload failed.";
      }
      setStatus("Character image upload failed");
    }
  });
}

if (removeCharacterImageBtn) {
  removeCharacterImageBtn.addEventListener("click", async () => {
    try {
      if (characterImageStatus) {
        characterImageStatus.textContent = "Removing character image...";
      }
      await removeCharacterImage();
      setStatus("Character image removed");
    } catch (error) {
      console.error(error);
      if (characterImageStatus) {
        characterImageStatus.textContent =
          error.message || "Character image removal failed.";
      }
      setStatus("Character image removal failed");
    }
  });
}

deleteCharacterBtn.addEventListener("click", async () => {
  try {
    await deleteCurrentCharacter();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to delete character.");
  }
});

addSampleDialogueBtn.addEventListener("click", () => {
  const current = collectSampleDialogues();
  current.push({
    userMessage: "",
    assistantMessage: "",
  });
  renderSampleDialogueEditor(current);
});

personaSelect.addEventListener("change", async (event) => {
  const value = event.target.value;
  state.selectedPersonaId = value || null;

  if (state.selectedPersonaId) {
    await loadPersonaDetail(state.selectedPersonaId);
  } else {
    clearPersonaEditor();
  }

  setStatus("Persona selection updated");
});

refreshPersonasBtn.addEventListener("click", async () => {
  await loadPersonas();
  if (state.selectedPersonaId) {
    personaSelect.value = state.selectedPersonaId;
    await loadPersonaDetail(state.selectedPersonaId);
  } else {
    clearPersonaEditor();
  }
  setStatus("Personas refreshed");
});

newPersonaBtn.addEventListener("click", () => {
  state.selectedPersonaId = null;
  personaSelect.value = "";
  clearPersonaEditor();
  setStatus("Creating new persona");
});

savePersonaBtn.addEventListener("click", async () => {
  try {
    await saveCurrentPersona();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to save persona.");
  }
});

deletePersonaBtn.addEventListener("click", async () => {
  try {
    await deleteCurrentPersona();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to delete persona.");
  }
});

refreshConversationsBtn.addEventListener("click", async () => {
  await loadConversations();
  setStatus("Conversations refreshed");
});

newConversationBtn.addEventListener("click", async () => {
  try {
    await createConversation();
    setStatus("Conversation created");
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to create conversation.");
  }
});

refreshMemoryBtn.addEventListener("click", async () => {
  await loadMemories();
  await loadProposals();
  setStatus("Memory refreshed");
});

memoryUseConversationScope.addEventListener("change", async () => {
  await loadMemories();
});

createMemoryBtn.addEventListener("click", async () => {
  try {
    await createMemory();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to create memory.");
  }
});

if (generateMemoryProposalsBtn) {
  generateMemoryProposalsBtn.addEventListener("click", async () => {
    try {
      await generateMemoryProposals();
    } catch (error) {
      console.error(error);
      alert(error.message || "Failed to generate proposals.");
      setStatus("Proposal generation failed");
    }
  });
}

if (refreshBackgroundProposalStatusBtn) {
  refreshBackgroundProposalStatusBtn.addEventListener("click", async () => {
    try {
      await loadBackgroundProposalStatus();
      setStatus("Background worker status refreshed");
    } catch (error) {
      console.error(error);
      alert(error.message || "Failed to refresh worker status.");
    }
  });
}

if (runBackgroundProposalNowBtn) {
  runBackgroundProposalNowBtn.addEventListener("click", async () => {
    try {
      await runBackgroundProposalNow();
    } catch (error) {
      console.error(error);
      alert(error.message || "Failed to run background generation.");
      setStatus("Background generation failed");
    }
  });
}

if (refreshMemoryConflictsBtn) {
  refreshMemoryConflictsBtn.addEventListener("click", async () => {
    try {
      await loadMemoryConflicts();
      setStatus("Memory conflicts loaded");
    } catch (error) {
      console.error(error);
      if (memoryConflictsMeta) {
        memoryConflictsMeta.textContent =
          error.message || "Failed to load memory conflicts.";
      }
      setStatus("Memory conflicts failed");
    }
  });
}

if (loadMemorySuggestionConflictsBtn) {
  loadMemorySuggestionConflictsBtn.addEventListener("click", async () => {
    try {
      if (memorySuggestionConflictsStatus) {
        memorySuggestionConflictsStatus.textContent =
          "Loading memory conflicts...";
      }
      await loadMemoryConflictSuggestions();
      setStatus("Memory conflict suggestions loaded");
    } catch (error) {
      console.error(error);
      if (memorySuggestionConflictsStatus) {
        memorySuggestionConflictsStatus.textContent =
          error.message || "Failed to load memory conflicts.";
      }
      setStatus("Memory conflict suggestion load failed");
    }
  });
}

if (resolveMemoryConflictsBulkBtn) {
  resolveMemoryConflictsBulkBtn.addEventListener("click", async () => {
    try {
      if (memorySuggestionConflictsStatus) {
        memorySuggestionConflictsStatus.textContent =
          "Resolving suggested conflicts...";
      }

      const result = await resolveMemoryConflictsBulk();
      if (memorySuggestionConflictsStatus) {
        memorySuggestionConflictsStatus.textContent =
          `Bulk resolve complete. Merged: ${result.mergedCount}, ` +
          `Skipped: ${result.skippedCount}, ` +
          `Scanned: ${result.scannedConflictCount}`;
      }

      await refreshMemoryViews();
      try {
        await loadMemoryConflictSuggestions();
      } catch (refreshError) {
        console.error(refreshError);
      }
      setStatus("Bulk memory conflict resolution completed");
    } catch (error) {
      console.error(error);
      if (memorySuggestionConflictsStatus) {
        memorySuggestionConflictsStatus.textContent =
          error.message || "Bulk memory conflict resolution failed.";
      }
      setStatus("Bulk memory conflict resolution failed");
    }
  });
}

if (refreshBackgroundWorkBtn) {
  refreshBackgroundWorkBtn.addEventListener("click", async () => {
    try {
      await loadBackgroundWorkStatus();
      setStatus("Background work status loaded");
    } catch (error) {
      console.error(error);
      setStatus("Background work status failed");
    }
  });
}

if (manualRefreshSummaryBtn) {
  manualRefreshSummaryBtn.addEventListener("click", async () => {
    try {
      setStatus("Running summary refresh...");
      await triggerBackgroundWork("summary");
      setStatus("Summary refresh completed");
    } catch (error) {
      console.error(error);
      if (backgroundWorkTriggerResult) {
        backgroundWorkTriggerResult.textContent =
          error.message || "Summary refresh failed.";
      }
      setStatus("Summary refresh failed");
    }
  });
}

if (manualExtractMemoryBtn) {
  manualExtractMemoryBtn.addEventListener("click", async () => {
    try {
      setStatus("Running memory extraction...");
      await triggerBackgroundWork("memory");
      setStatus("Memory extraction completed");
    } catch (error) {
      console.error(error);
      if (backgroundWorkTriggerResult) {
        backgroundWorkTriggerResult.textContent =
          error.message || "Memory extraction failed.";
      }
      setStatus("Memory extraction failed");
    }
  });
}

if (manualReindexRetrievalBtn) {
  manualReindexRetrievalBtn.addEventListener("click", async () => {
    try {
      setStatus("Running retrieval reindex...");
      await triggerBackgroundWork("reindex");
      setStatus("Retrieval reindex completed");
    } catch (error) {
      console.error(error);
      if (backgroundWorkTriggerResult) {
        backgroundWorkTriggerResult.textContent =
          error.message || "Retrieval reindex failed.";
      }
      setStatus("Retrieval reindex failed");
    }
  });
}

if (rebuildMemoryKeysBtn) {
  rebuildMemoryKeysBtn.addEventListener("click", async () => {
    try {
      setStatus("Rebuilding memory keys...");
      await rebuildMemoryKeys();
      setStatus("Memory keys rebuilt");
      if (typeof refreshMemoryViews === "function") {
        await refreshMemoryViews();
      }
    } catch (error) {
      console.error(error);
      maintenanceResult.textContent = error.message || "Memory key rebuild failed.";
      setStatus("Memory key rebuild failed");
    }
  });
}

if (reindexAllRetrievalBtn) {
  reindexAllRetrievalBtn.addEventListener("click", async () => {
    try {
      setStatus("Reindexing all retrieval embeddings...");
      await reindexAllRetrieval();
      setStatus("Retrieval reindex-all completed");
      if (typeof loadRetrievalInspection === "function") {
        try {
          await loadRetrievalInspection();
        } catch (error) {
          console.error(error);
        }
      }
    } catch (error) {
      console.error(error);
      maintenanceResult.textContent =
        error.message || "Retrieval reindex-all failed.";
      setStatus("Retrieval reindex-all failed");
    }
  });
}

if (pruneMemoryExtractionAuditBtn) {
  pruneMemoryExtractionAuditBtn.addEventListener("click", async () => {
    try {
      setStatus("Pruning memory extraction audit...");
      await pruneMemoryExtractionAudit();
      setStatus("Memory extraction audit pruned");
      if (typeof loadMemoryExtractionAudit === "function") {
        await loadMemoryExtractionAudit();
      }
    } catch (error) {
      console.error(error);
      maintenanceResult.textContent = error.message || "Audit prune failed.";
      setStatus("Audit prune failed");
    }
  });
}

if (exportMemoryExtractionAuditBtn) {
  exportMemoryExtractionAuditBtn.addEventListener("click", () => {
    try {
      exportMemoryExtractionAudit();
      setStatus("Exporting memory extraction audit");
    } catch (error) {
      console.error(error);
      maintenanceResult.textContent = error.message || "Audit export failed.";
      setStatus("Audit export failed");
    }
  });
}

if (pruneStaleSceneStateBtn) {
  pruneStaleSceneStateBtn.addEventListener("click", async () => {
    try {
      setStatus("Pruning stale scene-state...");
      await pruneStaleSceneState();
      setStatus("Stale scene-state pruned");

      if (typeof refreshMemoryViews === "function") {
        await refreshMemoryViews();
      }

      if (typeof loadSceneStateInspection === "function") {
        await loadSceneStateInspection();
      }

      if (typeof loadPromptInspection === "function") {
        try {
          await loadPromptInspection();
        } catch (error) {
          console.error(error);
        }
      }
    } catch (error) {
      console.error(error);
      maintenanceResult.textContent =
        error.message || "Scene-state cleanup failed.";
      setStatus("Scene-state cleanup failed");
    }
  });
}

if (refreshSceneStateInspectionBtn) {
  refreshSceneStateInspectionBtn.addEventListener("click", async () => {
    try {
      await loadSceneStateInspection();
      setStatus("Scene-state inspection loaded");
    } catch (error) {
      console.error(error);
      sceneStateInspectionMeta.textContent =
        error.message || "Scene-state inspection failed.";
      setStatus("Scene-state inspection failed");
    }
  });
}

if (refreshMemoryExtractionAuditBtn) {
  refreshMemoryExtractionAuditBtn.addEventListener("click", async () => {
    try {
      await loadMemoryExtractionAudit();
      setStatus("Memory extraction audit loaded");
    } catch (error) {
      console.error(error);
      memoryExtractionAuditMeta.textContent =
        error.message || "Memory extraction audit failed.";
      setStatus("Memory extraction audit failed");
    }
  });
}

if (applyAuthoringEnhancementBtn) {
  applyAuthoringEnhancementBtn.addEventListener("click", () => {
    if (!authoringEnhancementState.targetElementId || !authoringSuggestedText) {
      return;
    }

    const target = document.getElementById(
      authoringEnhancementState.targetElementId,
    );
    if (!target) {
      return;
    }

    target.value = authoringSuggestedText.value || "";
    target.dispatchEvent(new Event("input", { bubbles: true }));
    setStatus("Authoring suggestion applied");
    clearAuthoringEnhancementReview();
  });
}

if (discardAuthoringEnhancementBtn) {
  discardAuthoringEnhancementBtn.addEventListener("click", () => {
    clearAuthoringEnhancementReview();
    setStatus("Authoring suggestion discarded");
  });
}

if (generateFullBundleBtn) {
  generateFullBundleBtn.addEventListener("click", async () => {
    try {
      await generateFullBundle();
      setStatus("Generated full bundle loaded");
    } catch (error) {
      console.error(error);
      if (bundleGenerationMeta) {
        bundleGenerationMeta.textContent =
          error.message || "Full bundle generation failed.";
      }
      setStatus("Full bundle generation failed");
    }
  });
}

if (applyGeneratedBundleBtn) {
  applyGeneratedBundleBtn.addEventListener("click", () => {
    applyGeneratedBundle();
  });
}

if (discardGeneratedBundleBtn) {
  discardGeneratedBundleBtn.addEventListener("click", () => {
    clearGeneratedBundleReview();
    setStatus("Generated bundle discarded");
  });
}
if (checkAuthoringConsistencyBtn) {
  checkAuthoringConsistencyBtn.addEventListener("click", async () => {
    try {
      await checkAuthoringConsistency();
      setStatus("Authoring consistency check completed");
    } catch (error) {
      console.error(error);
      if (authoringConsistencyMeta) {
        authoringConsistencyMeta.textContent =
          error.message || "Consistency check failed.";
      }
      setStatus("Consistency check failed");
    }
  });
}

setInterval(() => {
  loadBackgroundWorkStatus().catch((error) => console.error(error));
}, 10000);

inspectRetrievalBtn.addEventListener("click", async () => {
  try {
    await inspectRetrieval();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to inspect retrieval.");
  }
});

inspectPromptBtn.addEventListener("click", async () => {
  try {
    await inspectPrompt();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to inspect prompt.");
  }
});

inspectSummaryBtn.addEventListener("click", async () => {
  try {
    await inspectSummary();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to inspect summary.");
  }
});

refreshLorebooksBtn.addEventListener("click", async () => {
  try {
    await loadLorebooks();

    try {
      await loadAuthoringStarterPacks();
    } catch (error) {
      console.error(error);
      if (authoringStarterPacksMeta) {
        authoringStarterPacksMeta.textContent =
          error.message || "Failed to load starter packs.";
      }
    }
    setStatus("Lorebooks refreshed");
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to load lorebooks.");
  }
});

createLorebookBtn.addEventListener("click", async () => {
  try {
    await createLorebook();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to create lorebook.");
  }
});

createLoreEntryBtn.addEventListener("click", async () => {
  try {
    await createLoreEntry();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to create lore entry.");
  }
});

modelProfileSelect.addEventListener("change", async (event) => {
  state.selectedModelProfileId = event.target.value || null;
  if (state.selectedModelProfileId) {
    await loadModelProfileDetail(state.selectedModelProfileId);
  } else {
    clearModelProfileEditor();
  }
});

if (modelProfileProviderTypeInput) {
  modelProfileProviderTypeInput.addEventListener(
    "change",
    refreshModelProfileProviderUi,
  );
}

refreshModelProfilesBtn.addEventListener("click", async () => {
  await loadModelProfiles();
  if (state.selectedModelProfileId) {
    modelProfileSelect.value = state.selectedModelProfileId;
    await loadModelProfileDetail(state.selectedModelProfileId);
  } else {
    clearModelProfileEditor();
  }
  setStatus("Model profiles refreshed");
});

newModelProfileBtn.addEventListener("click", () => {
  state.selectedModelProfileId = null;
  modelProfileSelect.value = "";
  clearModelProfileEditor();
  setStatus("Creating new model profile");
});

saveModelProfileBtn.addEventListener("click", async () => {
  try {
    await saveCurrentModelProfile();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to save model profile.");
  }
});

deleteModelProfileBtn.addEventListener("click", async () => {
  try {
    await deleteCurrentModelProfile();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to delete model profile.");
  }
});

generationPresetSelect.addEventListener("change", async (event) => {
  state.selectedGenerationPresetId = event.target.value || null;
  if (state.selectedGenerationPresetId) {
    await loadGenerationPresetDetail(state.selectedGenerationPresetId);
  } else {
    clearGenerationPresetEditor();
  }
});

refreshGenerationPresetsBtn.addEventListener("click", async () => {
  await loadGenerationPresets();
  if (state.selectedGenerationPresetId) {
    generationPresetSelect.value = state.selectedGenerationPresetId;
    await loadGenerationPresetDetail(state.selectedGenerationPresetId);
  } else {
    clearGenerationPresetEditor();
  }
  setStatus("Generation presets refreshed");
});

newGenerationPresetBtn.addEventListener("click", () => {
  state.selectedGenerationPresetId = null;
  generationPresetSelect.value = "";
  clearGenerationPresetEditor();
  setStatus("Creating new generation preset");
});

saveGenerationPresetBtn.addEventListener("click", async () => {
  try {
    await saveCurrentGenerationPreset();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to save generation preset.");
  }
});

deleteGenerationPresetBtn.addEventListener("click", async () => {
  try {
    await deleteCurrentGenerationPreset();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to delete generation preset.");
  }
});

generateImageBtn.addEventListener("click", async () => {
  try {
    await generateConversationImage();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to generate image.");
    setStatus("Image generation failed");
  }
});

if (buildAndGenerateImageBtn) {
  buildAndGenerateImageBtn.addEventListener("click", async () => {
    try {
      await buildAndGenerateImageFromConversation();
    } catch (error) {
      console.error(error);
      alert(error.message || "Failed to build and generate image.");
      setStatus("Build + Generate failed");
    }
  });
}

if (buildImagePromptFromContextBtn) {
  buildImagePromptFromContextBtn.addEventListener("click", async () => {
    try {
      await buildImagePromptFromConversation();
    } catch (error) {
      console.error(error);
      alert(error.message || "Failed to build contextual image prompt.");
      setStatus("Contextual image prompt failed");
    }
  });
}
sendBtn.addEventListener("click", async () => {
  await sendMessage();
});

if (activateTurnOverrideBtn) {
  activateTurnOverrideBtn.addEventListener("click", () => {
    try {
      activateTurnRuntimeOverride();
      renderInspectionRuntimeBadges();
      renderActiveConversationRuntime(state.messages || []);
      setStatus("One-turn runtime override activated");
    } catch (error) {
      console.error(error);
      turnOverrideStatus.textContent =
        error.message || "Failed to activate one-turn override.";
      setStatus("One-turn runtime override failed");
    }
  });
}

if (clearTurnOverrideBtn) {
  clearTurnOverrideBtn.addEventListener("click", () => {
    clearTurnRuntimeOverride();
    renderInspectionRuntimeBadges();
    renderActiveConversationRuntime(state.messages || []);
    setStatus("One-turn runtime override cleared");
  });
}

if (saveConversationSettingsBtn) {
  saveConversationSettingsBtn.addEventListener("click", async () => {
    try {
      await saveConversationSettings();
      setStatus("Conversation settings saved");
    } catch (error) {
      console.error(error);
      if (conversationSettingsStatus) {
        conversationSettingsStatus.textContent =
          error.message || "Failed to save conversation settings.";
      }
      setStatus("Conversation settings save failed");
    }
  });
}

if (clearConversationRuntimeOverrideBtn) {
  clearConversationRuntimeOverrideBtn.addEventListener("click", async () => {
    try {
      clearConversationRuntimeOverrideUi();
      await saveConversationSettings();
      setStatus("Conversation sticky override cleared");
    } catch (error) {
      console.error(error);
      if (conversationSettingsStatus) {
        conversationSettingsStatus.textContent =
          error.message || "Failed to clear conversation override.";
      }
      setStatus("Conversation override clear failed");
    }
  });
}

if (saveAppDefaultsBtn) {
  saveAppDefaultsBtn.addEventListener("click", async () => {
    try {
      await saveAppRuntimeDefaults();
      setStatus("App defaults saved");
    } catch (error) {
      console.error(error);
      if (appDefaultsStatus) {
        appDefaultsStatus.textContent =
          error.message || "Failed to save app defaults.";
      }
      setStatus("App defaults save failed");
    }
  });
}

if (exportPromptDatasetBtn) {
  exportPromptDatasetBtn.addEventListener("click", () => {
    try {
      const format = (datasetExportFormatInput?.value || "json").toUpperCase();
      if (datasetExportStatus) {
        datasetExportStatus.textContent = `Starting ${format} export...`;
      }
      exportPromptDataset();
      if (datasetExportStatus) {
        datasetExportStatus.textContent = `${format} export requested.`;
      }
      setStatus("Prompt dataset export requested");
    } catch (error) {
      console.error(error);
      if (datasetExportStatus) {
        datasetExportStatus.textContent =
          error.message || "Dataset export failed.";
      }
      setStatus("Prompt dataset export failed");
    }
  });
}

if (mergeMemoryItemsBtn) {
  mergeMemoryItemsBtn.addEventListener("click", async () => {
    try {
      if (memoryMergeStatus) {
        memoryMergeStatus.textContent = "Merging memory items...";
      }
      const result = await mergeMemoryItems();
      if (memoryMergeStatus) {
        memoryMergeStatus.textContent = `Merged into memory ${result.mergedIntoMemoryId}.`;
      }
      await refreshMemoryViews();
      setStatus("Memory items merged");
    } catch (error) {
      console.error(error);
      if (memoryMergeStatus) {
        memoryMergeStatus.textContent = error.message || "Memory merge failed.";
      }
      setStatus("Memory merge failed");
    }
  });
}

if (previewMergeMemoryItemsBtn) {
  previewMergeMemoryItemsBtn.addEventListener("click", async () => {
    try {
      if (memoryMergePreview) {
        memoryMergePreview.textContent = "Loading merge preview...";
      }

      const preview = await previewMergeMemoryItems();
      renderMemoryMergePreview(preview);
      setStatus("Memory merge preview loaded");
    } catch (error) {
      console.error(error);
      if (memoryMergePreview) {
        memoryMergePreview.textContent =
          error.message || "Failed to load merge preview.";
      }
      setStatus("Memory merge preview failed");
    }
  });
}

if (exportMemoryDatasetBtn) {
  exportMemoryDatasetBtn.addEventListener("click", () => {
    try {
      const format = (memoryExportFormatInput?.value || "json").toUpperCase();
      if (memoryExportStatus) {
        memoryExportStatus.textContent = `Starting ${format} memory export...`;
      }
      exportMemoryDataset();
      if (memoryExportStatus) {
        memoryExportStatus.textContent = `${format} memory export requested.`;
      }
      setStatus("Memory export requested");
    } catch (error) {
      console.error(error);
      if (memoryExportStatus) {
        memoryExportStatus.textContent = error.message || "Memory export failed.";
      }
      setStatus("Memory export failed");
    }
  });
}

if (importMemoryBtn) {
  importMemoryBtn.addEventListener("click", async () => {
    try {
      if (memoryImportStatus) {
        memoryImportStatus.textContent = "Importing memory...";
      }

      const result = await importMemoryDataset();
      if (memoryImportStatus) {
        memoryImportStatus.textContent =
          `Import complete. Imported: ${result.importedCount}, ` +
          `Updated: ${result.updatedCount}, Skipped: ${result.skippedCount}`;
      }

      await refreshMemoryViews();
      try {
        await loadMemoryConflictSuggestions();
      } catch (refreshError) {
        console.error(refreshError);
      }
      setStatus("Memory import completed");
    } catch (error) {
      console.error(error);
      if (memoryImportStatus) {
        memoryImportStatus.textContent =
          error.message || "Memory import failed.";
      }
      setStatus("Memory import failed");
    }
  });
}

if (suggestUserMessageBtn) {
  suggestUserMessageBtn.addEventListener("click", async () => {
    try {
      await suggestUserMessage();
    } catch (error) {
      console.error(error);
      alert(error.message || "Failed to generate suggested reply.");
      setStatus("Suggested reply failed");
    }
  });
}

messageInput.addEventListener("keydown", async (event) => {
  if (event.key === "Enter" && !event.shiftKey) {
    event.preventDefault();
    await sendMessage();
  }
});

exportCharacterBtn.addEventListener("click", async () => {
  try {
    await exportCurrentCharacter();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to export character.");
  }
});

importCharacterBtn.addEventListener("click", async () => {
  try {
    await importCharacter();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to import character.");
  }
});

assignPersonaToConversationBtn.addEventListener("click", async () => {
  try {
    await assignPersonaToActiveConversation();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to update conversation persona.");
  }
});

if (setDefaultPersonaBtn) {
  setDefaultPersonaBtn.addEventListener("click", async () => {
    try {
      if (!state.selectedPersonaId) {
        throw new Error("Select a persona first.");
      }

      await setDefaultPersona(state.selectedPersonaId);
      await loadPersonas();
      await loadAppRuntimeDefaults();
      personaSelect.value = state.selectedPersonaId;
      setStatus("Default persona updated");
    } catch (error) {
      console.error(error);
      alert(error.message || "Failed to set default persona.");
      setStatus("Failed to set default persona");
    }
  });
}

exportPersonaBtn.addEventListener("click", async () => {
  try {
    await exportCurrentPersona();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to export persona.");
  }
});

importPersonaBtn.addEventListener("click", async () => {
  try {
    await importPersona();
  } catch (error) {
    console.error(error);
    alert(error.message || "Failed to import persona.");
  }
});

async function bootstrap() {
  try {
    initializeSidebarAccordions();
    initializeMemoryPanelAccordions();
    refreshModelProfileProviderUi();
    refreshTurnOverrideStatus();
    renderInspectionRuntimeBadges();
    renderActiveConversationRuntime(state.messages || []);
    attachAuthoringAssistantToolbars();
    clearAuthoringEnhancementReview();
    clearGeneratedBundleReview();
    if (authoringConsistencyMeta) {
      authoringConsistencyMeta.textContent = "No consistency check run yet.";
    }
    if (authoringConsistencyList) {
      authoringConsistencyList.innerHTML = "";
    }
    populateImageStylePresetSelect(imageStylePresetSelect, "none");
    populateImageStylePresetSelect(
      characterDefaultVisualStylePresetSelect,
      "none",
    );
    if (pendingMemoryProposalsEl) {
      pendingMemoryProposalsEl.innerHTML = `<div class="empty-state">Open a conversation to review proposals.</div>`;
    }
    if (memoryConflictsMeta) {
      memoryConflictsMeta.textContent =
        "Open a conversation to inspect memory conflicts.";
    }
    if (memoryConflictsList) {
      memoryConflictsList.innerHTML = "";
    }
    if (memoryConflictActionResult) {
      memoryConflictActionResult.textContent = "No conflict action taken yet.";
    }
    if (memorySuggestionConflictsStatus) {
      memorySuggestionConflictsStatus.textContent =
        "No conflict scan started yet.";
    }
    if (memorySuggestionConflictsList) {
      memorySuggestionConflictsList.innerHTML = "";
    }
    if (retrievalMemoryExplanationsMeta) {
      retrievalMemoryExplanationsMeta.textContent =
        "No retrieval explanation data loaded yet.";
    }
    if (retrievalMemoryExplanationsList) {
      retrievalMemoryExplanationsList.innerHTML = "";
    }
    if (retrievalLoreExplanationsMeta) {
      retrievalLoreExplanationsMeta.textContent =
        "No lore explanation data loaded yet.";
    }
    if (retrievalLoreExplanationsList) {
      retrievalLoreExplanationsList.innerHTML = "";
    }
    if (promptSlotWinnersMeta) {
      promptSlotWinnersMeta.textContent = "No prompt slot data loaded yet.";
    }
    if (promptSlotWinnersList) {
      promptSlotWinnersList.innerHTML = "";
    }
    if (promptSuppressedDurableMeta) {
      promptSuppressedDurableMeta.textContent =
        "No suppressed durable memory data loaded yet.";
    }
    if (promptSuppressedDurableList) {
      promptSuppressedDurableList.innerHTML = "";
    }
    if (sceneStateInspectionMeta) {
      sceneStateInspectionMeta.textContent =
        "No scene-state inspection loaded yet.";
    }
    if (sceneStateActiveList) {
      sceneStateActiveList.innerHTML = "";
    }
    if (sceneStateReplacementHistoryList) {
      sceneStateReplacementHistoryList.innerHTML = "";
    }
    if (sceneStateFamilyCollisionList) {
      sceneStateFamilyCollisionList.innerHTML = "";
    }
    if (memoryExtractionAuditMeta) {
      memoryExtractionAuditMeta.textContent =
        "No memory extraction audit loaded yet.";
    }
    if (memoryExtractionAuditList) {
      memoryExtractionAuditList.innerHTML = "";
    }
    if (promptSceneStateDebugMeta) {
      promptSceneStateDebugMeta.textContent =
        "No prompt scene-state debug data loaded yet.";
    }
    if (promptSceneStateSelectedList) {
      promptSceneStateSelectedList.innerHTML = "";
    }
    if (promptSceneStateSuppressedList) {
      promptSceneStateSuppressedList.innerHTML = "";
    }
    if (promptDurableMemoryDebugMeta) {
      promptDurableMemoryDebugMeta.textContent =
        "No prompt durable-memory debug data loaded yet.";
    }
    if (promptDurableMemorySelectedList) {
      promptDurableMemorySelectedList.innerHTML = "";
    }
    if (promptDurableMemorySuppressedList) {
      promptDurableMemorySuppressedList.innerHTML = "";
    }
    if (backgroundProposalStatusPanel) {
      backgroundProposalStatusPanel.innerHTML = `<div class="empty-state">Refresh worker status to inspect background proposal generation.</div>`;
    }
    generatedImagesGallery.innerHTML = `<div class="empty-state">Open a conversation to view generated images.</div>`;
    if (contextualImagePromptPreview) {
      contextualImagePromptPreview.textContent =
        "Build a prompt from the active conversation to preview scene summary and unknowns.";
    }
    if (suggestedUserMessageMeta) {
      suggestedUserMessageMeta.textContent =
        "Suggest a reply from the current conversation context.";
    }

    setStatus("Loading...");

    await loadModelProfiles();
    if (state.selectedModelProfileId) {
      modelProfileSelect.value = state.selectedModelProfileId;
      await loadModelProfileDetail(state.selectedModelProfileId);
    } else {
      clearModelProfileEditor();
    }

    await loadGenerationPresets();
    if (state.selectedGenerationPresetId) {
      generationPresetSelect.value = state.selectedGenerationPresetId;
      await loadGenerationPresetDetail(state.selectedGenerationPresetId);
    } else {
      clearGenerationPresetEditor();
    }

    await loadCharacters();
    if (state.selectedCharacterId) {
      characterSelect.value = state.selectedCharacterId;
      await loadCharacterDetail(state.selectedCharacterId);
    } else {
      clearCharacterEditor();
    }

    await loadPersonas();
    if (state.selectedPersonaId) {
      personaSelect.value = state.selectedPersonaId;
      await loadPersonaDetail(state.selectedPersonaId);
    } else {
      clearPersonaEditor();
    }

    try {
      await loadAppRuntimeDefaults();
    } catch (error) {
      console.error(error);
      if (appDefaultsStatus) {
        appDefaultsStatus.textContent =
          error.message || "Failed to load app defaults.";
      }
    }

    await loadConversations();
    await loadMemories();
    await loadProposals();
    try {
      await loadMemoryConflicts();
    } catch (error) {
      console.error(error);
    }
    await loadLorebooks();

    try {
      await loadAuthoringStarterPacks();
    } catch (error) {
      console.error(error);
      if (authoringStarterPacksMeta) {
        authoringStarterPacksMeta.textContent =
          error.message || "Failed to load starter packs.";
      }
    }

    promptAuthoringSummary.innerHTML = `<div class="empty-state">Run a prompt inspection to see authoring sections clearly separated.</div>`;
    retrievalInspectionResults.innerHTML = `<div class="empty-state">Run an inspection to see why memory or lore would be included.</div>`;
    if (retrievalMemoryExplanationsMeta) {
      retrievalMemoryExplanationsMeta.textContent =
        "No retrieval explanation data loaded yet.";
    }
    if (retrievalMemoryExplanationsList) {
      retrievalMemoryExplanationsList.innerHTML = "";
    }
    if (retrievalLoreExplanationsMeta) {
      retrievalLoreExplanationsMeta.textContent =
        "No lore explanation data loaded yet.";
    }
    if (retrievalLoreExplanationsList) {
      retrievalLoreExplanationsList.innerHTML = "";
    }
    if (promptSlotWinnersMeta) {
      promptSlotWinnersMeta.textContent = "No prompt slot data loaded yet.";
    }
    if (promptSlotWinnersList) {
      promptSlotWinnersList.innerHTML = "";
    }
    if (promptSuppressedDurableMeta) {
      promptSuppressedDurableMeta.textContent =
        "No suppressed durable memory data loaded yet.";
    }
    if (promptSuppressedDurableList) {
      promptSuppressedDurableList.innerHTML = "";
    }
    if (sceneStateInspectionMeta) {
      sceneStateInspectionMeta.textContent =
        "No scene-state inspection loaded yet.";
    }
    if (sceneStateActiveList) {
      sceneStateActiveList.innerHTML = "";
    }
    if (sceneStateReplacementHistoryList) {
      sceneStateReplacementHistoryList.innerHTML = "";
    }
    if (sceneStateFamilyCollisionList) {
      sceneStateFamilyCollisionList.innerHTML = "";
    }
    if (memoryExtractionAuditMeta) {
      memoryExtractionAuditMeta.textContent =
        "No memory extraction audit loaded yet.";
    }
    if (memoryExtractionAuditList) {
      memoryExtractionAuditList.innerHTML = "";
    }
    if (promptSceneStateDebugMeta) {
      promptSceneStateDebugMeta.textContent =
        "No prompt scene-state debug data loaded yet.";
    }
    if (promptSceneStateSelectedList) {
      promptSceneStateSelectedList.innerHTML = "";
    }
    if (promptSceneStateSuppressedList) {
      promptSceneStateSuppressedList.innerHTML = "";
    }
    if (promptDurableMemoryDebugMeta) {
      promptDurableMemoryDebugMeta.textContent =
        "No prompt durable-memory debug data loaded yet.";
    }
    if (promptDurableMemorySelectedList) {
      promptDurableMemorySelectedList.innerHTML = "";
    }
    if (promptDurableMemorySuppressedList) {
      promptDurableMemorySuppressedList.innerHTML = "";
    }
    promptInspectionResults.innerHTML = `<div class="empty-state">Run a prompt inspection to see exact final prompt sections.</div>`;
    summaryInspectionResults.innerHTML = `<div class="empty-state">Inspect an active conversation to see summary coverage and whether the current prompt is using it.</div>`;
    renderDirectorStatus(null);
    renderSceneStatus(null, false);
    generatedImagesGallery.innerHTML = `<div class="empty-state">Open a conversation to view generated images.</div>`;
    if (contextualImagePromptPreview) {
      contextualImagePromptPreview.textContent =
        "Build a prompt from the active conversation to preview scene summary and unknowns.";
    }
    if (suggestedUserMessageMeta) {
      suggestedUserMessageMeta.textContent =
        "Suggest a reply from the current conversation context.";
    }
    if (pendingMemoryProposalsEl) {
      pendingMemoryProposalsEl.innerHTML = `<div class="empty-state">Open a conversation to review proposals.</div>`;
    }
    if (memoryConflictActionResult) {
      memoryConflictActionResult.textContent = "No conflict action taken yet.";
    }
    if (memorySuggestionConflictsStatus) {
      memorySuggestionConflictsStatus.textContent =
        "No conflict scan started yet.";
    }
    if (memorySuggestionConflictsList) {
      memorySuggestionConflictsList.innerHTML = "";
    }
    if (backgroundProposalStatusPanel) {
      backgroundProposalStatusPanel.innerHTML = `<div class="empty-state">Refresh worker status to inspect background proposal generation.</div>`;
    }

    try {
      await loadBackgroundProposalStatus();
    } catch (error) {
      console.error(error);
      if (backgroundProposalStatusPanel) {
        backgroundProposalStatusPanel.innerHTML = `<div class="empty-state">Failed to load worker status.</div>`;
      }
    }

    try {
      await loadBackgroundWorkStatus();
    } catch (error) {
      console.error(error);
    }

    startNewConversation();
    setStatus("Ready");
  } catch (error) {
    console.error(error);
    setStatus("Failed to load");
    alert(error.message || "Failed to initialize app.");
  }
}

bootstrap();


















