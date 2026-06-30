import { motion } from 'framer-motion';
import { User, Bot } from 'lucide-react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { TypingIndicator } from './TypingIndicator';

interface Message {
  role: 'user' | 'assistant';
  content: string;
}

interface MessageBubbleProps {
  message: Message;
  isLast: boolean;
  isLoading: boolean;
}

export function MessageBubble({ message, isLast, isLoading }: MessageBubbleProps) {
  const isUser = message.role === 'user';
  const showTyping = !isUser && isLast && isLoading && !message.content;

  return (
    <motion.div
      initial={{ opacity: 0, y: 15 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3, ease: 'easeOut' }}
      className={`flex w-full ${isUser ? 'justify-end' : 'justify-start'}`}
    >
      <div className={`flex max-w-[85%] md:max-w-[75%] ${isUser ? 'flex-row-reverse' : 'flex-row'} gap-3`}>
        
        {/* Avatar */}
        <div className={`flex-shrink-0 flex items-center justify-center w-8 h-8 rounded-full mt-1 
          ${isUser ? 'bg-[#EDF4FF] text-[#1E5EFF]' : 'bg-gradient-to-tr from-[#1E5EFF] to-[#EDF4FF] text-white shadow-sm'}
        `}>
          {isUser ? <User size={16} strokeWidth={2.5} /> : <Bot size={18} strokeWidth={2} />}
        </div>

        {/* Bubble */}
        <div className={`px-5 py-3.5 text-[15px] leading-relaxed shadow-sm
          ${isUser
            ? 'bg-[#1E5EFF] text-white rounded-[20px] rounded-tr-[4px]'
            : 'bg-white text-[#111827] rounded-[20px] rounded-tl-[4px] border border-[#E5E7EB]'
          }
        `}>
          {showTyping ? (
            <TypingIndicator />
          ) : isUser ? (
            <div className="whitespace-pre-wrap">{message.content}</div>
          ) : (
            <div className="prose prose-sm md:prose-base prose-blue max-w-none 
              prose-p:leading-relaxed prose-pre:bg-[#F7F9FC] prose-pre:border prose-pre:border-[#E5E7EB] prose-pre:text-[#111827] 
              prose-code:text-[#1E5EFF] prose-code:bg-[#EDF4FF] prose-code:px-1.5 prose-code:py-0.5 prose-code:rounded-md
              prose-a:text-[#1E5EFF] hover:prose-a:text-blue-700
              prose-headings:text-[#111827] prose-strong:text-[#111827] prose-strong:font-semibold
              prose-li:marker:text-[#1E5EFF]">
              <ReactMarkdown remarkPlugins={[remarkGfm]}>
                {message.content}
              </ReactMarkdown>
            </div>
          )}
        </div>
      </div>
    </motion.div>
  );
}
