## Where I left off (Dec 2025)

- Create page:
  - GET uses API
  - POST uses API with DB fallback
- Edit page:
  - GET uses API (edit-payload)
  - Answers + dependent answers load correctly
- Feature flags:
  - UseApiForAgreementLoad
  - UseApiForAgreementSave
  - AllowDbFallback

### Next step
- Implement Edit POST via API
  - PUT /api/agreements/{agreementId}
  - Reuse Create POST logic
  - Delete + reinsert answers
  - Keep DB fallback

When resuming:
- Start with Edit POST
- Try first, then ask Chat for help
