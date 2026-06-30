import { useRef, useEffect } from 'react';
import { MessageBubble } from './MessageBubble';

interface Message {
  role: 'user' | 'assistant';
  content: string;
}

interface ChatMessagesProps {
  messages: Message[];
  isLoading: boolean;
}

export function ChatMessages({ messages, isLoading }: ChatMessagesProps) {
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages, isLoading]);

  return (
    <div className="flex-1 overflow-y-auto px-4 py-8 sm:px-8 space-y-6 scroll-smooth bg-gradient-to-b from-transparent to-[#F7F9FC]/30">
      {messages.map((msg, index) => (
        <MessageBubble 
          key={index} 
          message={msg} 
          isLast={index === messages.length - 1} 
          isLoading={isLoading} 
        />
      ))}
      <div ref={messagesEndRef} className="h-4" />
    </div>
  );
}
