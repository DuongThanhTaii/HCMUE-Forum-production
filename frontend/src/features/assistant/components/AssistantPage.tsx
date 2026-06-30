import { useState } from 'react';
import { ChatLayout } from './ChatLayout';
import { ChatHeader } from './ChatHeader';
import { ChatMessages } from './ChatMessages';
import { ChatInput } from './ChatInput';
import { useTranslation } from 'react-i18next';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "https://anhfeee-hcmue-handbook-rag-api.hf.space";

interface Message {
  role: 'user' | 'assistant';
  content: string;
}

export function AssistantPage() {
  const { t } = useTranslation('assistant');
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const sendMessage = async (userMessage: string) => {
    if (!userMessage.trim() || isLoading) return;

    setMessages((prev) => [...prev, { role: 'user', content: userMessage }]);
    setIsLoading(true);

    // Initialize assistant message with empty content for streaming
    setMessages((prev) => [...prev, { role: 'assistant', content: '' }]);

    try {
      const response = await fetch(`${API_BASE_URL}/chat/stream`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          query: userMessage,
          cohort: 'K48-K49', // Default cohort
          chat_history: messages.filter(m => m.content).slice(-6).map(m => ({
            role: m.role,
            content: m.content
          }))
        }),
      });

      if (!response.body) throw new Error('No response body');
      
      const reader = response.body.getReader();
      const decoder = new TextDecoder('utf-8');
      let buffer = '';

      while (true) {
        const { value, done } = await reader.read();
        if (done) break;

        buffer += decoder.decode(value, { stream: true });
        const events = buffer.split('\n\n');
        buffer = events.pop() || '';

        for (const eventText of events) {
          const lines = eventText.split('\n');
          const eventTypeLine = lines.find((line) => line.startsWith('event: '));
          const dataTextLine = lines.find((line) => line.startsWith('data: '));
          
          const eventType = eventTypeLine?.replace('event: ', '');
          const dataText = dataTextLine?.replace('data: ', '');

          if (!eventType || !dataText) continue;

          try {
            const data = JSON.parse(dataText);
            
            if (eventType === 'token' && data.text) {
              setMessages((prev) => {
                const newMessages = [...prev];
                const lastIdx = newMessages.length - 1;
                newMessages[lastIdx] = {
                  ...newMessages[lastIdx],
                  content: newMessages[lastIdx].content + data.text
                };
                return newMessages;
              });
            } else if (eventType === 'error') {
               console.error("Chatbot error:", data);
            }
          } catch (err) {
            console.error("Error parsing SSE data", err);
          }
        }
      }
    } catch (error) {
      console.error('Failed to chat:', error);
      setMessages((prev) => {
        const newMessages = [...prev];
        const lastIdx = newMessages.length - 1;
        newMessages[lastIdx] = {
          ...newMessages[lastIdx],
          content: t('messages.error', 'Xin lỗi, đã có lỗi kết nối đến server. Vui lòng thử lại sau.')
        };
        return newMessages;
      });
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const userMsg = input.trim();
    setInput('');
    sendMessage(userMsg);
  };

  const handleSuggestClick = (query: string) => {
    sendMessage(query);
  };

  return (
    <ChatLayout>
      <ChatHeader />
      <ChatMessages 
        messages={messages} 
        isLoading={isLoading} 
        onSuggestClick={handleSuggestClick} 
      />
      <ChatInput 
        input={input} 
        setInput={setInput} 
        onSubmit={handleSubmit} 
        isLoading={isLoading} 
      />
    </ChatLayout>
  );
}
