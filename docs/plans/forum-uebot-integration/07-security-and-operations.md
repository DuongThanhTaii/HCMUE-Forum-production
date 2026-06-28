# 07 — Bảo mật & vận hành

## 7.1 Bảo mật tích hợp

| Chủ đề | Hành động |
|--------|-----------|
| **Shared secret** | `X-Integration-Secret` chỉ trên server-to-server (Forum → sync-api); không đưa vào UEBot client |
| **Sync token** | Coi như secret trong UEBot local storage an toàn (keychain / credential manager) |
| **Forum JWT** | Hết hạn thì refresh/re-login; không log full token |
| **Rate limit** | Endpoint `exchange-token` dùng policy `integrations` — theo dõi abuse |

## 7.2 Authorization

- Không tin lời model “đã duyệt”; chỉ tin response Forum **200** sau `resolve` / `publish`.
- Permission `forum.reports.review` — kiểm tra định kỳ RBAC trên admin.

## 7.3 Audit

- Các command moderation/publish đã đi qua domain handlers — đảm bảo **user action logs** (nếu module bật) ghi **actorId**, **target**, **ip** khi cần điều tra.

## 7.4 Quan sát (observability)

- Metric: tỷ lệ `502` trên `exchange-token`, latency sync-api.
- Log: không ghi body chứa PII đầy đủ; mask email nếu debug.

## 7.5 Production checklist

- [ ] HTTPS end-to-end; secret trong vault.
- [ ] `SyncApiBaseUrl` trỏ DNS prod.
- [ ] CORS chỉ origin FE được phép.
- [ ] Load test nhẹ cho exchange + mod list.
