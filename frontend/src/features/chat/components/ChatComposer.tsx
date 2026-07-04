import { useCallback, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { Mic, Paperclip, Send } from "lucide-react";
import {
  useSendMessageMutation,
  useSendMessageWithAttachmentsMutation,
  useUploadChatFileMutation,
} from "../api/chat.api";
import { useChatContext } from "../context/ChatContext";
import { useTypingComposer } from "../hooks/useTypingComposer";
import { useVoiceRecorder } from "../hooks/useVoiceRecorder";
import { enqueueOutbox, openOutboxDb } from "../lib/outboxDb";
import { getRtkQueryErrorMessage } from "../lib/rtkErrorMessage";
import { drainChatOutbox } from "../lib/processOutbox";
import type { ChatThreadRef, ReplyTarget } from "../types/chat.types";
import { useAppSelector } from "@shared/hooks/useAppSelector";
import { ReplyPreviewBar } from "./ReplyPreviewBar";
import { useChatOutboxBanner } from "../hooks/useChatOutboxBanner";

function fileExtForAudioMime(mime: string): string {
  if (mime.includes("mp4") || mime.includes("m4a") || mime.includes("aac"))
    return "m4a";
  if (mime.includes("ogg")) return "ogg";
  return "webm";
}

function formatUploadError(
  t: (k: string) => string,
  err: unknown,
  hasToken: boolean,
): string {
  const msg = getRtkQueryErrorMessage(err);
  if (msg) return msg;
  if (!hasToken) return t("chat.uploadNeedLogin");
  return t("chat.uploadError");
}

export function ChatComposer({
  threadRef,
  onUserSentMessage,
  disabled = false,
  replyTarget = null,
  onClearReply,
}: {
  threadRef: ChatThreadRef
  onUserSentMessage?: () => void
  disabled?: boolean
  replyTarget?: ReplyTarget | null
  onClearReply?: () => void
}) {
  const { t } = useTranslation();
  const accessToken = useAppSelector((s) => s.auth.accessToken);
  const { sendTyping, sendChannelMessage } = useChatContext();
  const [text, setText] = useState("");
  const [sendMessage] = useSendMessageMutation();
  const [uploadFile] = useUploadChatFileMutation();
  const [sendAttachments] = useSendMessageWithAttachmentsMutation();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const voice = useVoiceRecorder();

  const conversationId =
    threadRef.kind === "conversation" ? threadRef.conversationId : null;
  const hasToken = Boolean(accessToken?.trim());
  const { state: outboxState, retry: retryOutbox, dismissFailed } =
    useChatOutboxBanner(conversationId);

  const { onComposerChange, flushStop } = useTypingComposer({
    enabled: threadRef.kind === "conversation",
    conversationId,
    sendTyping,
  });

  const handleChange = (v: string) => {
    setText(v);
    onComposerChange(v);
  };

  const notifySent = useCallback(() => {
    onUserSentMessage?.();
  }, [onUserSentMessage]);

  const sendWithOutboxFallback = useCallback(
    async (content: string) => {
      if (threadRef.kind !== "conversation") return;
      const replyToMessageId = replyTarget?.messageId ?? null;
      try {
        await sendMessage({
          conversationId: threadRef.conversationId,
          content,
          replyToMessageId,
        }).unwrap();
        await drainChatOutbox();
        notifySent();
      } catch {
        const db = await openOutboxDb();
        const id = crypto.randomUUID();
        const enq = await enqueueOutbox(db, {
          id,
          conversationId: threadRef.conversationId,
          body: {
            type: "text",
            content,
            replyToMessageId: replyTarget?.messageId,
          },
          attempts: 0,
          createdAt: Date.now(),
        });
        if (enq === "full") {
          window.alert(t("chat.outbox.full"));
        }
      }
    },
    [notifySent, replyTarget?.messageId, sendMessage, t, threadRef],
  );

  const submitText = async () => {
    if (disabled) return;
    const trimmed = text.trim();
    if (!trimmed) return;
    flushStop();
    setText("");
    if (threadRef.kind === "channel") {
      await sendChannelMessage(threadRef.channelId, trimmed);
      notifySent();
      return;
    }
    if (!navigator.onLine) {
      const db = await openOutboxDb();
      const id = crypto.randomUUID();
      const enq = await enqueueOutbox(db, {
        id,
        conversationId: threadRef.conversationId,
        body: {
          type: "text",
          content: trimmed,
          replyToMessageId: replyTarget?.messageId,
        },
        attempts: 0,
        createdAt: Date.now(),
      });
      if (enq === "full") {
        window.alert(t("chat.outbox.full"));
      } else {
        notifySent();
      }
      return;
    }
    await sendWithOutboxFallback(trimmed);
  };

  const onPickFile = async (files: FileList | null) => {
    if (!files?.length || threadRef.kind !== "conversation") return;
    if (!hasToken) {
      window.alert(t("chat.uploadNeedLogin"));
      if (fileInputRef.current) fileInputRef.current.value = "";
      return;
    }
    const file = files[0];
    flushStop();
    const fd = new FormData();
    fd.append("file", file, file.name);
    try {
      const up = await uploadFile(fd).unwrap();
      await sendAttachments({
        conversationId: threadRef.conversationId,
        content: text.trim() || null,
        replyToMessageId: replyTarget?.messageId ?? null,
        attachments: [
          {
            fileName: up.fileName,
            fileUrl: up.fileUrl,
            fileSize: up.fileSize,
            mimeType: up.contentType,
            thumbnailUrl: null,
          },
        ],
      }).unwrap();
      setText("");
      notifySent();
    } catch (e) {
      window.alert(formatUploadError(t, e, hasToken));
    }
    if (fileInputRef.current) fileInputRef.current.value = "";
  };

  const sendVoiceIfReady = async () => {
    const recorded = voice.blob;
    if (!recorded || threadRef.kind !== "conversation") return;
    if (!hasToken) {
      window.alert(t("chat.uploadNeedLogin"));
      return;
    }
    const mime = recorded.type || voice.lastMime || "audio/webm";
    const ext = fileExtForAudioMime(mime);
    const file = new File([recorded], `voice-${Date.now()}.${ext}`, {
      type: mime,
    });
    voice.reset();
    const fd = new FormData();
    fd.append("file", file, file.name);
    try {
      const up = await uploadFile(fd).unwrap();
      await sendAttachments({
        conversationId: threadRef.conversationId,
        content: null,
        attachments: [
          {
            fileName: up.fileName,
            fileUrl: up.fileUrl,
            fileSize: up.fileSize,
            mimeType: up.contentType,
            thumbnailUrl: null,
          },
        ],
      }).unwrap();
      notifySent();
    } catch (e) {
      window.alert(formatUploadError(t, e, hasToken));
    }
  };

  return (
    <div className="space-y-2 pb-2 px-2">
      {outboxState.kind === "pending" && (
        <p className="text-xs text-slate-500">{t("chat.delivery.sending")}</p>
      )}
      {outboxState.kind === "failed" && (
        <div className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-amber-200 bg-amber-50 px-2 py-1.5 text-xs text-amber-900">
          <span>{t("chat.delivery.failed")}</span>
          <span className="flex gap-2">
            <button
              type="button"
              className="font-medium text-indigo-700 hover:underline"
              onClick={() => void retryOutbox()}
            >
              {t("chat.delivery.retry")}
            </button>
            <button
              type="button"
              className="text-slate-600 hover:underline"
              onClick={() => void dismissFailed()}
            >
              {t("chat.delivery.dismiss")}
            </button>
          </span>
        </div>
      )}
      {replyTarget && onClearReply ? (
        <ReplyPreviewBar target={replyTarget} onClear={onClearReply} />
      ) : null}
      {voice.error && (
        <p className="rounded-lg border border-red-200 bg-red-50 px-2 py-1.5 text-xs text-red-800">
          {voice.error === "VOICE_EMPTY" ? t("chat.voice.empty") : voice.error}
        </p>
      )}

      {voice.state === "recording" && (
        <div className="flex items-center justify-between rounded-lg bg-slate-100 px-3 py-2 text-sm">
          <span>
            {t("chat.voice.recording")} · {voice.seconds}s
          </span>
          <div className="flex gap-2">
            <button
              type="button"
              className="rounded-md border border-slate-300 px-2 py-1 text-xs"
              onClick={() => {
                voice.cancelRecording();
              }}
            >
              {t("chat.voice.discard")}
            </button>
            <button
              type="button"
              className="rounded-md bg-indigo-600 px-2 py-1 text-xs text-white"
              onClick={() => {
                voice.finishRecording();
              }}
            >
              {t("chat.voice.stop")}
            </button>
          </div>
        </div>
      )}

      {voice.state === "stopped" &&
        voice.blob &&
        threadRef.kind === "conversation" && (
          <div className="flex items-center justify-between rounded-lg border border-slate-200 px-3 py-2 text-sm">
            <span>{t("chat.voice.preview")}</span>
            <div className="flex gap-2">
              <button
                type="button"
                className="rounded-md border border-slate-300 px-2 py-1 text-xs"
                onClick={() => voice.reset()}
              >
                {t("chat.voice.discard")}
              </button>
              <button
                type="button"
                className="rounded-md bg-indigo-600 px-2 py-1 text-xs text-white"
                onClick={() => void sendVoiceIfReady()}
              >
                {t("chat.voice.send")}
              </button>
            </div>
          </div>
        )}

      {voice.state === "idle" && (
        <div className="flex min-w-0 items-end gap-1.5 sm:gap-2">
          {threadRef.kind === "conversation" && (
            <div className="flex shrink-0 items-center gap-1 pb-1">
              <button
                type="button"
                className="flex h-8 w-8 items-center justify-center rounded-full text-slate-500 transition-colors hover:bg-slate-100 hover:text-indigo-600 active:bg-slate-200"
                onClick={() => fileInputRef.current?.click()}
                aria-label={t("chat.attachFile")}
              >
                <Paperclip className="h-[20px] w-[20px]" strokeWidth={2} />
              </button>
              <button
                type="button"
                className="flex h-8 w-8 items-center justify-center rounded-full text-slate-500 transition-colors hover:bg-slate-100 hover:text-indigo-600 active:bg-slate-200 disabled:opacity-40 disabled:hover:bg-transparent disabled:hover:text-slate-500"
                onClick={() => void voice.start()}
                disabled={voice.state !== "idle"}
                aria-label={t("chat.voice.start")}
              >
                <Mic className="h-[20px] w-[20px]" strokeWidth={2} />
              </button>
            </div>
          )}
          
          <div className="min-h-[36px] min-w-0 flex-1 rounded-[20px] bg-slate-100 transition-colors focus-within:bg-slate-200/50 sm:min-h-[36px]">
            {threadRef.kind === "conversation" && (
              <input
                ref={fileInputRef}
                type="file"
                className="hidden"
                onChange={(e) => void onPickFile(e.target.files)}
              />
            )}
            <textarea
              value={text}
              onChange={(e) => handleChange(e.target.value)}
              onBlur={() => flushStop()}
              onKeyDown={(e) => {
                if (e.key === "Enter" && !e.shiftKey) {
                  e.preventDefault();
                  void submitText();
                }
              }}
              placeholder={disabled ? t("chat.safety.composerDisabled") : t("chat.typeMessage")}
              rows={1}
              disabled={disabled}
              className="max-h-28 min-h-[36px] w-full resize-none border-0 bg-transparent px-3 py-2 text-[0.9375rem] leading-snug text-slate-900 outline-none ring-0 placeholder:text-slate-500 focus:ring-0 disabled:opacity-50"
            />
          </div>

          <div className="flex shrink-0 items-center pb-1">
            <button
              type="button"
              onClick={() => void submitText()}
              disabled={disabled || (!text.trim() && outboxState.kind !== "pending")}
              className="flex h-8 w-8 items-center justify-center rounded-full text-indigo-600 transition-colors hover:bg-slate-100 disabled:opacity-50 disabled:hover:bg-transparent"
              aria-label={t("chat.send")}
            >
              <Send className="h-[22px] w-[22px]" strokeWidth={2.5} />
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
