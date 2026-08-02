export const DEFAULT_MODEL = "gpt-5.6-luna";
export const DEFAULT_ALLOWED_ORIGIN = "https://ppapigo.github.io";

const ROLES = Object.freeze(["Tank", "Mage", "Assassin"]);
const SLOTS = Object.freeze(["Q", "E", "Ultimate"]);
const SKILL_TYPES = Object.freeze([
  "Target",
  "Projectile",
  "GroundArea",
  "Cone",
  "SelfArea",
  "Global"
]);
const CROWD_CONTROLS = Object.freeze(["None", "Slow", "Stun", "KnockUp"]);
const ELEMENTS = Object.freeze([
  "None",
  "Fire",
  "Ice",
  "Lightning",
  "Poison",
  "Water",
  "Wind",
  "Earth"
]);

export const SKILL_RESPONSE_SCHEMA = Object.freeze({
  type: "object",
  additionalProperties: false,
  required: [
    "displayName",
    "description",
    "skillType",
    "crowdControl",
    "element",
    "damageIncreasePercent",
    "radiusIncrease",
    "rangeIncrease",
    "cooldownReduction",
    "addsMobility",
    "createsShield",
    "heals"
  ],
  properties: {
    displayName: { type: "string", minLength: 1, maxLength: 48 },
    description: { type: "string", minLength: 1, maxLength: 240 },
    skillType: { type: "string", enum: SKILL_TYPES },
    crowdControl: { type: "string", enum: CROWD_CONTROLS },
    element: { type: "string", enum: ELEMENTS },
    damageIncreasePercent: { type: "number", minimum: 0, maximum: 200 },
    radiusIncrease: { type: "number", minimum: 0, maximum: 10 },
    rangeIncrease: { type: "number", minimum: 0, maximum: 15 },
    cooldownReduction: { type: "number", minimum: 0, maximum: 20 },
    addsMobility: { type: "boolean" },
    createsShield: { type: "boolean" },
    heals: { type: "boolean" }
  }
});

const INSTRUCTIONS = `You create one balanced PvP arena skill draft for Axiom.
Return only the requested JSON schema. Use exactly one skill type, at most one crowd
control, and at most one element for this draft. The Unity client enforces the final
100 point budget and may auto-correct values. Tank cannot use long-range attacks,
Mage may use area attacks, and Assassin should prefer focused or small-area attacks.
Do not include markdown, commentary, or fields outside the schema.`;

export function validateGenerationRequest(body) {
  let parsedBody = body;
  if (typeof parsedBody === "string") {
    try {
      parsedBody = JSON.parse(parsedBody);
    } catch {
      return invalid("Request body must be valid JSON.");
    }
  }

  if (!parsedBody || typeof parsedBody !== "object" || Array.isArray(parsedBody)) {
    return invalid("Request body must be a JSON object.");
  }

  const prompt = typeof parsedBody.prompt === "string"
    ? parsedBody.prompt.trim()
    : "";
  if (prompt.length < 3 || prompt.length > 500) {
    return invalid("prompt must contain between 3 and 500 characters.");
  }

  if (!ROLES.includes(parsedBody.role)) {
    return invalid(`role must be one of: ${ROLES.join(", ")}.`);
  }

  if (!SLOTS.includes(parsedBody.slot)) {
    return invalid(`slot must be one of: ${SLOTS.join(", ")}.`);
  }

  return {
    ok: true,
    value: {
      prompt,
      role: parsedBody.role,
      slot: parsedBody.slot
    }
  };
}

export function buildResponsesRequest(input, model = DEFAULT_MODEL) {
  return {
    model,
    instructions: INSTRUCTIONS,
    input: JSON.stringify(input),
    reasoning: { effort: "low" },
    max_output_tokens: 700,
    text: {
      verbosity: "low",
      format: {
        type: "json_schema",
        name: "axiom_skill_draft",
        strict: true,
        schema: SKILL_RESPONSE_SCHEMA
      }
    }
  };
}

export function extractOutputText(responseBody) {
  if (typeof responseBody?.output_text === "string" && responseBody.output_text) {
    return responseBody.output_text;
  }

  for (const item of responseBody?.output ?? []) {
    if (item?.type !== "message") {
      continue;
    }

    for (const content of item.content ?? []) {
      if (content?.type === "output_text" && typeof content.text === "string") {
        return content.text;
      }
    }
  }

  throw new Error("OpenAI response did not contain output text.");
}

export function parseSkillDraft(outputText) {
  let draft;
  try {
    draft = JSON.parse(outputText);
  } catch {
    throw new Error("OpenAI output was not valid JSON.");
  }

  assertString(draft, "displayName");
  assertString(draft, "description");
  assertEnum(draft, "skillType", SKILL_TYPES);
  assertEnum(draft, "crowdControl", CROWD_CONTROLS);
  assertEnum(draft, "element", ELEMENTS);
  assertNumber(draft, "damageIncreasePercent");
  assertNumber(draft, "radiusIncrease");
  assertNumber(draft, "rangeIncrease");
  assertNumber(draft, "cooldownReduction");
  assertBoolean(draft, "addsMobility");
  assertBoolean(draft, "createsShield");
  assertBoolean(draft, "heals");
  return draft;
}

function invalid(error) {
  return { ok: false, error };
}

function assertString(value, field) {
  if (typeof value?.[field] !== "string" || value[field].trim().length === 0) {
    throw new Error(`Generated ${field} must be a non-empty string.`);
  }
}

function assertEnum(value, field, allowed) {
  if (!allowed.includes(value?.[field])) {
    throw new Error(`Generated ${field} was not supported.`);
  }
}

function assertNumber(value, field) {
  if (typeof value?.[field] !== "number" ||
      !Number.isFinite(value[field]) ||
      value[field] < 0) {
    throw new Error(`Generated ${field} must be a finite non-negative number.`);
  }
}

function assertBoolean(value, field) {
  if (typeof value?.[field] !== "boolean") {
    throw new Error(`Generated ${field} must be a boolean.`);
  }
}
