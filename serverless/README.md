# Axiom Skill Generation Serverless Function

This dependency-free Node.js function keeps the OpenAI API key outside the Unity
WebGL build and returns the exact JSON shape expected by
`SkillGenerationResponseDto`.

## Deploy

1. Import this repository into a serverless host that supports Vercel-style Node
   functions, using `serverless` as the project root.
2. Add a newly issued `OPENAI_API_KEY` in the host's encrypted environment settings.
3. Keep `OPENAI_MODEL=gpt-5.6-luna`, or replace it after testing another compatible
   model.
4. Set `AXIOM_ALLOWED_ORIGIN=https://ppapigo.github.io`.
5. Deploy and copy the resulting `/api/generate-skill` HTTPS URL into Unity's
   `SkillGenerationApiSettings` asset.

The current production endpoint is
`https://axiom-skill-api.vercel.app/api/generate-skill`. It is connected to the
`main` branch with `serverless` as the Vercel project root.

Never commit a real `.env` file or put an API key in Unity, JavaScript, GitHub Pages,
screenshots, issues, or chat messages. If a key is exposed, revoke it and create a
new one before deployment.

## Local validation

Run `npm test` or `node --test` inside this folder. No package installation is
required.

The function uses the OpenAI Responses API with strict Structured Outputs. The Unity
client still runs its own mapper, 100-point validator, and auto-corrector, so the
server response is never trusted as final balance data.

- [OpenAI Responses API](https://developers.openai.com/api/reference/resources/responses/methods/create)
- [OpenAI model guide](https://developers.openai.com/api/docs/models)
