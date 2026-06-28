# Chat Messenger parity — kế hoạch theo phase

Mục tiêu: UX chat gần Messenger, triển khai **A → E** (có thể song song D sau A).

**Design spec:** [docs/specs/2026-05-16-chat-messenger-parity-design.md](../../specs/2026-05-16-chat-messenger-parity-design.md)

## Thứ tự khuyến nghị

| # | Phase | Plan | Ước lượng | PR |
|---|--------|------|-----------|-----|
| 1 | **A** Scroll UX | [phase-a-scroll-ux.md](./phase-a-scroll-ux.md) | 2–3 ngày | `feat(chat): messenger scroll anchor` |
| 2 | **B** Tìm trong hội thoại | [phase-b-in-conversation-search.md](./phase-b-in-conversation-search.md) | 3–4 ngày | `feat(chat): conversation message search` |
| 3 | **C** Media & links | [phase-c-media-and-links.md](./phase-c-media-and-links.md) | 3–4 ngày | `feat(chat): shared media and links panel` |
| 4 | **D** Block / mute / report | [phase-d-block-mute-report.md](./phase-d-block-mute-report.md) | 4–5 ngày | `feat(chat): block mute report` |
| 5 | **E** Polish | [phase-e-messenger-polish.md](./phase-e-messenger-polish.md) | 4–6 ngày | `feat(chat): reactions read reply polish` |

**Tổng:** ~16–22 ngày dev (1 người), chưa kể QA cứng.

## Song song được

- **D** (backend Identity + Chat) có thể bắt đầu ngay sau **A** xong, trong khi làm **B**.
- **C** nên sau **B** (search API tái dùng filter media/links).

## Definition of Done chung mỗi phase

1. Code + i18n `vi`/`en` (`chat.json`)
2. Hoạt động trên **ChatPage** và **ChatDock** (nếu có thread)
3. Không regression: gửi tin, hub, outbox, gọi WebRTC
4. Checklist trong file plan được tick
5. Cập nhật `docs/api/chat-fe-api.md` nếu thêm endpoint

## Bắt đầu implement

Mở [phase-a-scroll-ux.md](./phase-a-scroll-ux.md) và làm tuần tự từ task 1.
