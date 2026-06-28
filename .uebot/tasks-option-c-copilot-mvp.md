# Tasks - Option C MVP (Forum Co-Pilot)

## Objective

Deliver practical Co-Pilot capabilities on top of Option A, focused on high user value and low implementation risk.

## Milestone C1 - Assistant Tools Proxy (Forum Backend)

### Task C1.1 - Create tools controller
- **Target prefix**: `/api/v1/assistant/tools`
- **Suggested endpoints**:
  - `POST /summarize-post`
  - `POST /related-posts`
  - `POST /draft-reply`
- **Acceptance criteria**:
  - All endpoints require auth.
  - Request validation with clear error messages.
  - Consistent response envelope.

### Task C1.2 - Implement summarize-post tool
- **Input**: `postId`, optional `maxPoints`
- **Dependencies**:
  - Forum post + comments data
  - AI summarization service
- **Acceptance criteria**:
  - Returns summary + key points.
  - Graceful handling when post/comments are missing.

### Task C1.3 - Implement related-posts tool
- **Input**: `query` or `postId`
- **Dependencies**:
  - Forum search API (`/api/v1/search`)
- **Acceptance criteria**:
  - Returns ranked related posts with short reason.
  - No mock data in production path.

### Task C1.4 - Implement draft-reply tool
- **Input**: `postId`, `intent`, `tone`
- **Acceptance criteria**:
  - Returns draft only (no auto-publish).
  - Includes optional citations to related posts.

## Milestone C2 - Frontend Co-Pilot Actions

### Task C2.1 - Post detail inline actions
- **Actions**:
  - Summarize post
  - Suggest reply
  - Show related posts
- **Acceptance criteria**:
  - Actions are visible and responsive.
  - Result panels support copy/insert draft flow.

### Task C2.2 - Create-post helper actions
- **Actions**:
  - Suggest title and tags
  - Rewrite for clarity
- **Acceptance criteria**:
  - Suggestions are editable before user accepts.

## Milestone C3 - Moderation Assist (Read-only)

### Task C3.1 - Add moderation hint endpoint
- **Target**: `POST /api/v1/assistant/tools/moderation-hint`
- **Acceptance criteria**:
  - Returns risk flags and recommended action.
  - Does not execute moderation actions automatically.

### Task C3.2 - Moderation dashboard widget
- **Acceptance criteria**:
  - Displays AI hints separately from official moderation state.

## Milestone C4 - Guardrails and Quality

### Task C4.1 - Citation policy
- **Acceptance criteria**:
  - Fact-based outputs include references when available.

### Task C4.2 - Prompt/result audit
- **Acceptance criteria**:
  - Logs include endpoint, user, timestamp, latency, status.
  - Sensitive content handling policy documented.

### Task C4.3 - Replace placeholder search behavior
- **Acceptance criteria**:
  - Smart search path uses real Forum data, not mock fallback in normal mode.

## Definition of Done (Option C MVP)

- Users can summarize posts, get related posts, and generate draft replies from Forum UI.
- Moderators can view advisory moderation hints.
- Co-Pilot outputs are controllable (user confirms actions), observable, and auditable.
