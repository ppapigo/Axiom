using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Axiom.Data;
using Axiom.Role;

namespace Axiom.Skill.Generation
{
    public sealed class SkillGenerationPipeline
    {
        private readonly ISkillGenerationProvider _provider;

        public SkillGenerationPipeline(ISkillGenerationProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public async Task<SkillGenerationPipelineResult> GenerateAsync(
            string prompt,
            CharacterRoleDefinition role,
            SkillSlot slot,
            SkillDefinition baseDefinition,
            SkillBalanceProfile balance,
            RoleElementPool elementPool,
            CancellationToken cancellationToken = default)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            if (balance == null)
            {
                throw new ArgumentNullException(nameof(balance));
            }
            if (elementPool == null)
            {
                throw new ArgumentNullException(nameof(elementPool));
            }

            cancellationToken.ThrowIfCancellationRequested();
            SkillGenerationResponseDto response;
            try
            {
                response = await _provider.GenerateAsync(
                    prompt,
                    role.RoleId,
                    slot,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                return CreateFallback(
                    response: null,
                    baseDefinition,
                    role,
                    balance,
                    elementPool,
                    new[] { $"Skill generation failed: {exception.Message}" });
            }

            SkillDraftMappingResult mapping = SkillDraftMapper.Map(response, slot);
            if (!mapping.IsSuccess)
            {
                return CreateFallback(
                    response,
                    baseDefinition,
                    role,
                    balance,
                    elementPool,
                    mapping.Errors);
            }

            SkillAutoCorrectionResult correction = SkillAutoCorrector.Correct(
                mapping.Draft,
                baseDefinition,
                role,
                balance,
                elementPool);
            SkillPointCostBreakdown pointCost = SkillPointCostBreakdown.Create(
                correction.Draft,
                balance);
            return new SkillGenerationPipelineResult(
                response,
                correction.Draft,
                correction.Validation,
                pointCost,
                correction.UsedFallback,
                correction.Validation.IsValid
                    ? Array.Empty<string>()
                    : correction.Validation.Errors,
                correction.Changes);
        }

        private static SkillGenerationPipelineResult CreateFallback(
            SkillGenerationResponseDto response,
            in SkillDefinition baseDefinition,
            CharacterRoleDefinition role,
            SkillBalanceProfile balance,
            RoleElementPool elementPool,
            IReadOnlyCollection<string> errors)
        {
            SkillDraft fallback = SkillAutoCorrector.CreateFallbackDraft(
                baseDefinition,
                role);
            SkillRuleValidationResult validation = SkillRuleValidator.Validate(
                fallback,
                baseDefinition,
                role,
                balance,
                elementPool);
            var changes = new[] { "Returned a role-safe fallback preset." };
            return new SkillGenerationPipelineResult(
                response,
                fallback,
                validation,
                SkillPointCostBreakdown.Create(fallback, balance),
                usedFallback: true,
                errors,
                changes);
        }
    }
}
