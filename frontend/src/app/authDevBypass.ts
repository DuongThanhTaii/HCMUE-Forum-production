/** Phải trùng logic với `RequireAuth` / `RequireRole`: khi bật, vẫn cần `ChatProvider` ở `MainLayout`. */
export const AUTH_BYPASS_IN_DEV =
  import.meta.env.DEV && import.meta.env.VITE_DEV_BYPASS_AUTH !== 'false'
