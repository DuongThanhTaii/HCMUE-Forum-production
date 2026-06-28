# Forum x UEBot Integration Master Task Plan

## Planning Horizon

4-week implementation plan with dependency ordering for Option A + Option C.

## Week 1 - Foundation (Must-Have)

- [ ] Implement Forum -> sync-api token exchange endpoint.
- [ ] Implement sync-api trusted exchange endpoint.
- [ ] Add integration config and secrets management.
- [ ] Add `/assistant` frontend route with iframe shell.
- [ ] Add secure postMessage token handoff.

**Exit criteria**:
- Authenticated Forum user can open assistant and create synced thread/message.

## Week 2 - First Co-Pilot Value

- [ ] Build `summarize-post` tool endpoint.
- [ ] Build `related-posts` tool endpoint.
- [ ] Add inline actions in post detail UI.
- [ ] Replace mock search output in Co-Pilot flow.

**Exit criteria**:
- Real forum post can be summarized and linked to related posts from assistant.

## Week 3 - Authoring Assistance

- [ ] Build `draft-reply` tool endpoint.
- [ ] Add draft insertion flow into reply editor.
- [ ] Add create-post helpers (title/tag suggestion).
- [ ] Improve prompt templates and failure fallbacks.

**Exit criteria**:
- User can generate and edit a useful reply draft before posting.

## Week 4 - Moderation and Hardening

- [ ] Build `moderation-hint` advisory endpoint.
- [ ] Add moderation dashboard widget for AI hints.
- [ ] Add audit logs and core metrics dashboards.
- [ ] Security review and rate-limit tuning.

**Exit criteria**:
- Moderation hints are available and traceable; integration path is operationally observable.

## Dependencies Map

- Option C depends on Option A token exchange and assistant route.
- Tool quality depends on stable Forum data query layer.
- Production release depends on security hardening and audit logging.

## Risks and Mitigations

1. **Token and identity mismatch**
   - Mitigation: explicit user mapping strategy and contract tests.

2. **Iframe session instability**
   - Mitigation: strict postMessage protocol with refresh and logout sync.

3. **Low-quality AI output**
   - Mitigation: prompt templates + citation policy + fallback responses.

4. **Operational blind spots**
   - Mitigation: endpoint-level logs, latency metrics, failure alerts.

## Suggested Owners

- Backend integration (Forum + sync-api): Platform/API team.
- Frontend assistant shell + inline actions: Forum frontend team.
- Co-Pilot prompt/tool quality: AI feature team.
- Security/observability review: Platform + DevOps.
