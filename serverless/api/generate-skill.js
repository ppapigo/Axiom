import {
  DEFAULT_ALLOWED_ORIGIN,
  DEFAULT_MODEL,
  buildResponsesRequest,
  enforceRoleElementRules,
  extractOutputText,
  parseSkillDraft,
  validateGenerationRequest
} from "../lib/skill-generation.mjs";

const OPENAI_RESPONSES_URL =
  "https://api.openai.com/v1/responses";

export async function handleGenerateSkill(request, response, dependencies = {}) {
  const allowedOrigin = dependencies.allowedOrigin ??
    process.env.AXIOM_ALLOWED_ORIGIN ??
    DEFAULT_ALLOWED_ORIGIN;
  setResponseHeaders(response, allowedOrigin);

  if (request.method === "OPTIONS") {
    return response.status(204).end();
  }

  if (request.method !== "POST") {
    response.setHeader("Allow", "POST, OPTIONS");
    return response.status(405).json({ error: "Method not allowed." });
  }

  if (request.headers?.origin && request.headers.origin !== allowedOrigin) {
    return response.status(403).json({ error: "Origin not allowed." });
  }

  const validation = validateGenerationRequest(request.body);
  if (!validation.ok) {
    return response.status(400).json({ error: validation.error });
  }

  const apiKey = dependencies.apiKey ?? process.env.OPENAI_API_KEY;
  if (!apiKey) {
    return response.status(503).json({
      error: "Skill generation is not configured."
    });
  }

  const fetchImplementation = dependencies.fetchImplementation ?? globalThis.fetch;
  if (typeof fetchImplementation !== "function") {
    return response.status(500).json({ error: "Server fetch is unavailable." });
  }

  const model = dependencies.model ?? process.env.OPENAI_MODEL ?? DEFAULT_MODEL;
  try {
    const openAIResponse = await fetchImplementation(OPENAI_RESPONSES_URL, {
      method: "POST",
      headers: {
        "Authorization": `Bearer ${apiKey}`,
        "Content-Type": "application/json"
      },
      body: JSON.stringify(buildResponsesRequest(validation.value, model))
    });

    const responseBody = await openAIResponse.json();
    if (!openAIResponse.ok) {
      console.error("OpenAI skill generation failed with status", openAIResponse.status);
      return response.status(502).json({ error: "AI provider request failed." });
    }

    const draft = enforceRoleElementRules(
      parseSkillDraft(extractOutputText(responseBody)),
      validation.value.role);
    return response.status(200).json(draft);
  } catch (error) {
    console.error("Axiom skill generation failed:", error?.message ?? "unknown error");
    return response.status(502).json({ error: "Skill generation failed." });
  }
}

function setResponseHeaders(response, allowedOrigin) {
  response.setHeader("Access-Control-Allow-Origin", allowedOrigin);
  response.setHeader("Access-Control-Allow-Methods", "POST, OPTIONS");
  response.setHeader("Access-Control-Allow-Headers", "Content-Type");
  response.setHeader("Cache-Control", "no-store");
  response.setHeader("Vary", "Origin");
}

export default async function generateSkill(request, response) {
  return handleGenerateSkill(request, response);
}
