# Forum x UEBot - Option C (Forum Co-Pilot)

## Goal

Turn UEBot into a "work-doing" assistant for Forum, not just a generic chat interface.

## Product Direction

UEBot becomes a Forum Co-Pilot with 4 value groups:

1. Read support
   - Summarize long threads.
   - Extract key points.
   - Explain posts at different levels (newbie, advanced).

2. Write support
   - Draft replies from post content + context.
   - Suggest titles, tags, and category.
   - Rewrite for clarity, civility, and community tone.

3. Moderation support
   - Flag spam/toxic/copy content.
   - Suggest action: warn, hide, queue for review.
   - Summarize moderation reasons for faster handling.

4. Discovery support
   - "Has this already been asked?"
   - Link related posts.
   - Produce curated answers with citations from related content.

## Existing APIs to Leverage Now

Forum already provides:
- `GET /api/v1/search` (post search)
- `GET /api/v1/posts`
- `GET /api/v1/posts/{id}`
- `GET /api/v1/posts/{id}/comments`
- `POST /api/v1/ai/summarize`
- `GET|POST /api/v1/ai/search`

This means the Co-Pilot MVP can be built immediately without a brand-new LLM stack.

## Co-Pilot Tool Contracts (MVP)

## Tool 1: summarize_post

- Input:
  - `postId`
  - `maxPoints` (optional)
- Flow:
  - fetch post + comments
  - call AI summarization service
- Output:
  - summary
  - key points
  - action items

## Tool 2: draft_reply

- Input:
  - `postId`
  - `intent` (`answer` | `clarify` | `follow-up`)
  - `tone` (`friendly` | `formal` | `concise`)
- Flow:
  - fetch post + related context search
  - build prompt template
- Output:
  - draft markdown
  - referenced post ids

## Tool 3: suggest_tags_title

- Input:
  - `title`
  - `content`
- Output:
  - title alternatives
  - suggested tags
  - confidence score

## Tool 4: related_posts

- Input:
  - `query` or `postId`
- Flow:
  - call `api/v1/search`
  - optionally rerank with AI search
- Output:
  - top related posts + reasoning

## Tool 5: moderation_hint

- Input:
  - `postId`
- Output:
  - risk flags (toxicity/spam/abuse)
  - recommendation (review/hide/allow)
  - concise rationale

## UX Placement in Forum

1. Post detail page:
   - "Summarize this post"
   - "Suggest a reply"
   - "Related posts" panel

2. Create post page:
   - "Suggest title + tags"
   - "Rewrite for clarity"

3. Moderation dashboard:
   - "AI triage summary"
   - "Top risky posts today"

4. Assistant hub page:
   - multi-purpose chat with slash commands:
     - `/summarize {postId}`
     - `/draft {postId}`
     - `/related {query}`

## Suggested Technical Design

## Phase C1 (quick MVP on top of Option A)

- Keep UEBot sync-api as a separate service.
- Add a "Forum tools proxy" in Forum backend:
  - `POST /api/v1/assistant/tools/summarize-post`
  - `POST /api/v1/assistant/tools/draft-reply`
  - `POST /api/v1/assistant/tools/related-posts`
- UEBot calls this tools proxy instead of directly calling many Forum endpoints.

## Phase C2 (quality improvements)

- Add a retrieval layer (embeddings/index) for forum content.
- Add a policy layer for moderation recommendations.
- Add citation and hallucination guardrails.

## Guardrails

- Require citations for all fact-based outputs.
- Do not auto-post: return draft only, user submits manually.
- Keep moderation suggestions advisory (no auto-ban).
- Log prompts/results for audit (anonymize PII where needed).

## Success Metrics

- Reduced time-to-first-good-reply (%).
- Increased correct tag assignment rate.
- Reduced moderation review time.
- Increased related-posts CTR.
- Assistant user satisfaction above target.

## Combined Roadmap with Option A

1. Week 1:
   - Option A token exchange + `/assistant` route.
2. Week 2:
   - `summarize_post` + `related_posts`.
3. Week 3:
   - `draft_reply` + post detail UI actions.
4. Week 4:
   - `moderation_hint` (read-only) + metrics dashboard.

This roadmap lets you launch quickly while evolving UEBot into a strong, measurable support tool for Forum.
