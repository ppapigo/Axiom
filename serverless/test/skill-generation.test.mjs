import assert from "node:assert/strict";
import test from "node:test";
import { handleGenerateSkill } from "../api/generate-skill.js";
import {
  SKILL_RESPONSE_SCHEMA,
  buildResponsesRequest,
  extractOutputText,
  parseSkillDraft,
  validateGenerationRequest
} from "../lib/skill-generation.mjs";

const validDraft = {
  displayName: "Flame Arc",
  description: "Launches a short fire projectile.",
  skillType: "Projectile",
  crowdControl: "Slow",
  element: "Fire",
  damageIncreasePercent: 20,
  radiusIncrease: 1,
  rangeIncrease: 1,
  cooldownReduction: 0,
  addsMobility: false,
  createsShield: false,
  heals: false
};

test("validates the Unity request contract", () => {
  const result = validateGenerationRequest({
    prompt: "  create a fire projectile  ",
    role: "Mage",
    slot: "Q"
  });

  assert.equal(result.ok, true);
  assert.equal(result.value.prompt, "create a fire projectile");
});

test("rejects unsupported roles, slots, and oversized prompts", () => {
  assert.equal(validateGenerationRequest({
    prompt: "valid prompt",
    role: "Warrior",
    slot: "Q"
  }).ok, false);
  assert.equal(validateGenerationRequest({
    prompt: "valid prompt",
    role: "Mage",
    slot: "BasicAttack"
  }).ok, false);
  assert.equal(validateGenerationRequest({
    prompt: "x".repeat(501),
    role: "Mage",
    slot: "Q"
  }).ok, false);
});

test("builds a strict Responses API JSON schema request", () => {
  const request = buildResponsesRequest({
    prompt: "fire projectile",
    role: "Mage",
    slot: "Q"
  }, "test-model");

  assert.equal(request.model, "test-model");
  assert.equal(request.text.format.type, "json_schema");
  assert.equal(request.text.format.strict, true);
  assert.equal(request.text.format.schema, SKILL_RESPONSE_SCHEMA);
});

test("extracts and validates the generated skill draft", () => {
  const text = extractOutputText({
    output: [{
      type: "message",
      content: [{ type: "output_text", text: JSON.stringify(validDraft) }]
    }]
  });

  assert.deepEqual(parseSkillDraft(text), validDraft);
  assert.throws(() => parseSkillDraft("{}"), /displayName/);
});

test("handles CORS preflight without calling OpenAI", async () => {
  const response = createResponseRecorder();
  let called = false;

  await handleGenerateSkill(
    { method: "OPTIONS", headers: {} },
    response,
    { fetchImplementation: async () => { called = true; } }
  );

  assert.equal(response.statusCode, 204);
  assert.equal(called, false);
  assert.equal(response.headers["Access-Control-Allow-Origin"],
    "https://ppapigo.github.io");
});

test("returns the exact Unity DTO without exposing the API key", async () => {
  const response = createResponseRecorder();
  let capturedRequest;

  await handleGenerateSkill({
    method: "POST",
    headers: { origin: "https://ppapigo.github.io" },
    body: { prompt: "fire projectile", role: "Mage", slot: "Q" }
  }, response, {
    apiKey: "test-key",
    model: "test-model",
    fetchImplementation: async (url, options) => {
      capturedRequest = { url, options };
      return {
        ok: true,
        status: 200,
        async json() {
          return { output_text: JSON.stringify(validDraft) };
        }
      };
    }
  });

  assert.equal(response.statusCode, 200);
  assert.deepEqual(response.body, validDraft);
  assert.equal(capturedRequest.url, "https://api.openai.com/v1/responses");
  assert.equal(capturedRequest.options.headers.Authorization, "Bearer test-key");
  assert.equal(JSON.stringify(response.body).includes("test-key"), false);
});

function createResponseRecorder() {
  return {
    statusCode: 200,
    headers: {},
    body: undefined,
    setHeader(name, value) {
      this.headers[name] = value;
    },
    status(statusCode) {
      this.statusCode = statusCode;
      return this;
    },
    json(body) {
      this.body = body;
      return this;
    },
    end() {
      return this;
    }
  };
}
