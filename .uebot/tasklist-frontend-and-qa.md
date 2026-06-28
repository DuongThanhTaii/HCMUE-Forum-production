# Frontend and QA Tasklist

## A. Forum Frontend - Assistant Integration

### A1. Routing and shell
- [ ] Add `/assistant` route
- [ ] Add protected access behavior (auth-required redirect/guard)
- [ ] Build assistant page layout

### A2. Iframe phase implementation
- [ ] Render UEBot web-app iframe
- [ ] Add allowed origin config
- [ ] Implement `postMessage` handshake protocol
- [ ] Send sync token on initial load
- [ ] Resend token on token refresh
- [ ] Send logout signal and clear state

### A3. Error and fallback UX
- [ ] Loading state for token exchange
- [ ] Error state with retry CTA
- [ ] Session expired state and recovery flow

---

## B. Forum Frontend - Co-Pilot Actions

### B1. Post detail page actions
- [ ] Add "Summarize post" action
- [ ] Add "Suggest reply" action
- [ ] Add "Related posts" action/panel

### B2. Draft UX
- [ ] Show draft preview modal/panel
- [ ] Add "Insert into editor" action
- [ ] Require explicit user confirmation before insertion

### B3. Create-post helper actions
- [ ] Add "Suggest title/tags"
- [ ] Add "Rewrite for clarity"
- [ ] Keep output editable before apply

---

## C. UEBot Web-App Completion

### C1. Auth hooks
- [ ] Implement missing methods in `useAuth`
- [ ] Validate guest session creation/restore
- [ ] Validate oauth completion path

### C2. Dialog and flow wiring
- [ ] Trigger auth-required dialog on login-required responses
- [ ] Ensure dialog actions complete end-to-end auth flow

---

## D. QA Test Matrix

### D1. Functional tests
- [ ] Login -> open assistant -> create thread/message
- [ ] Refresh browser -> conversation persistence validated
- [ ] Logout -> assistant session invalidated
- [ ] Relogin -> assistant session restored via new exchange token

### D2. Co-Pilot tool tests
- [ ] Summarize post returns expected structure
- [ ] Related posts returns relevant and non-empty results for known queries
- [ ] Draft reply returns editable output and no auto-post behavior

### D3. Negative-path tests
- [ ] Invalid/expired Forum token
- [ ] sync-api unavailable
- [ ] tool endpoint timeout
- [ ] malformed postId/query payload

### D4. Non-functional checks
- [ ] Accessibility sanity (keyboard + labels)
- [ ] Mobile/tablet responsiveness for assistant panel/page
- [ ] Localization compatibility with current i18n setup

---

## E. Release QA Sign-Off

- [ ] Critical user journey E2E passes in staging
- [ ] Regression check on Forum post flows passes
- [ ] No blocker bug in assistant/auth/tool flows
- [ ] Post-release monitoring dashboard validated
