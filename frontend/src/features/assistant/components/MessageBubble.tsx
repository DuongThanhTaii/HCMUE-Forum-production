import { motion } from 'framer-motion';
import { User } from 'lucide-react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';

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
  const showTypingIndicator = isLast && !isUser && isLoading && !message.content;

  return (
    <motion.div 
      initial={{ opacity: 0, y: 15 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.4, ease: 'easeOut' }}
      className={`flex items-start gap-3 sm:gap-4 w-full ${isUser ? 'flex-row-reverse' : 'flex-row'}`}
    >
      <div 
        className={`flex-shrink-0 w-8 h-8 sm:w-10 sm:h-10 rounded-full flex items-center justify-center
          ${isUser ? 'bg-[#EFF6FF] text-[#1E5EFF]' : 'bg-transparent border border-[#E5E7EB] shadow-sm'}
        `}
      >
        {isUser ? (
          <User size={18} strokeWidth={2.5} />
        ) : (
          <img src="/logochatbot.png" alt="Bot Avatar" className="w-6 h-6 object-contain" />
        )}
      </div>
      
      <div 
        className={`relative max-w-[85%] sm:max-w-[75%] px-5 py-3.5 
          ${isUser 
            ? 'bg-[#1E5EFF] text-white rounded-2xl rounded-tr-sm shadow-sm' 
            : 'bg-white text-[#111827] border border-[#E5E7EB] rounded-2xl rounded-tl-sm shadow-[0_2px_4px_rgba(0,0,0,0.02)]'
          }
        `}
      >
        {showTypingIndicator ? (
          <div className="flex gap-1.5 h-6 items-center px-1">
            <motion.div animate={{ y: [0, -5, 0] }} transition={{ duration: 0.6, repeat: Infinity, delay: 0 }} className="w-1.5 h-1.5 bg-[#9CA3AF] rounded-full" />
            <motion.div animate={{ y: [0, -5, 0] }} transition={{ duration: 0.6, repeat: Infinity, delay: 0.2 }} className="w-1.5 h-1.5 bg-[#9CA3AF] rounded-full" />
            <motion.div animate={{ y: [0, -5, 0] }} transition={{ duration: 0.6, repeat: Infinity, delay: 0.4 }} className="w-1.5 h-1.5 bg-[#9CA3AF] rounded-full" />
          </div>
        ) : (
          <div className={`prose prose-sm sm:prose-base max-w-none ${isUser ? 'prose-invert text-white' : 'prose-slate text-[#111827]'}
            prose-p:leading-relaxed prose-pre:bg-[#F3F4F6] prose-pre:text-[#111827] prose-headings:font-semibold prose-a:text-[#1E5EFF] prose-a:no-underline hover:prose-a:underline
            [&>*:first-child]:mt-0 [&>*:last-child]:mb-0
          `}>
            <ReactMarkdown remarkPlugins={[remarkGfm]}>
              {message.content}
            </ReactMarkdown>
          </div>
        )}
      </div>
    </motion.div>
  );
}
