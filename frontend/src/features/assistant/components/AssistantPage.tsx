import { useState, useRef, useEffect } from 'react'
import { Send, User, Bot, Loader2 } from 'lucide-react'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "https://anhfeee-hcmue-handbook-rag-api.hf.space"

interface Message {
  role: 'user' | 'assistant'
  content: string
}

export function AssistantPage() {
  const [messages, setMessages] = useState<Message[]>([
    { role: 'assistant', content: 'Xin chào! Tôi là trợ lý ảo của diễn đàn. Bạn muốn hỏi gì về thông tin học vụ?' }
  ])
  const [input, setInput] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const messagesEndRef = useRef<HTMLDivElement>(null)

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }

  useEffect(() => {
    scrollToBottom()
  }, [messages])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!input.trim() || isLoading) return

    const userMessage = input.trim()
    setInput('')
    setMessages((prev) => [...prev, { role: 'user', content: userMessage }])
    setIsLoading(true)

    // Initialize assistant message with empty content for streaming
    setMessages((prev) => [...prev, { role: 'assistant', content: '' }])

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
      })

      if (!response.body) throw new Error('No response body')
      
      const reader = response.body.getReader()
      const decoder = new TextDecoder('utf-8')
      let buffer = ''

      while (true) {
        const { value, done } = await reader.read()
        if (done) break

        buffer += decoder.decode(value, { stream: true })
        const events = buffer.split('\n\n')
        buffer = events.pop() || ''

        for (const eventText of events) {
          const lines = eventText.split('\n')
          const eventTypeLine = lines.find((line) => line.startsWith('event: '))
          const dataTextLine = lines.find((line) => line.startsWith('data: '))
          
          const eventType = eventTypeLine?.replace('event: ', '')
          const dataText = dataTextLine?.replace('data: ', '')

          if (!eventType || !dataText) continue

          try {
            const data = JSON.parse(dataText)
            
            if (eventType === 'token' && data.text) {
              setMessages((prev) => {
                const newMessages = [...prev]
                const lastIdx = newMessages.length - 1
                newMessages[lastIdx] = {
                  ...newMessages[lastIdx],
                  content: newMessages[lastIdx].content + data.text
                }
                return newMessages
              })
            } else if (eventType === 'error') {
               console.error("Chatbot error:", data)
            }
          } catch (err) {
            console.error("Error parsing SSE data", err)
          }
        }
      }
    } catch (error) {
      console.error('Failed to chat:', error)
      setMessages((prev) => {
        const newMessages = [...prev]
        const lastIdx = newMessages.length - 1
        newMessages[lastIdx] = {
          ...newMessages[lastIdx],
          content: 'Xin lỗi, đã có lỗi kết nối đến server. Vui lòng thử lại sau.'
        }
        return newMessages
      })
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <section className="mx-auto w-full max-w-5xl px-4 py-8">
      <div className="flex flex-col h-[calc(100vh-140px)] min-h-[500px] rounded-2xl border border-border/60 bg-background/80 backdrop-blur-xl shadow-[0_8px_30px_rgb(0,0,0,0.12)] overflow-hidden ring-1 ring-black/5 dark:ring-white/10 transition-all">
        
        {/* Header */}
        <div className="flex items-center px-6 py-4 border-b bg-gradient-to-r from-primary/10 via-primary/5 to-transparent">
          <div className="flex items-center justify-center w-12 h-12 rounded-full bg-primary/20 text-primary mr-4 shadow-inner">
            <Bot size={28} />
          </div>
          <div>
            <h2 className="text-xl font-bold tracking-tight bg-gradient-to-br from-foreground to-foreground/70 bg-clip-text text-transparent">Trợ lý Ảo HCMUE</h2>
            <p className="text-sm text-muted-foreground font-medium">Hỏi đáp thông tin học vụ nhanh chóng</p>
          </div>
        </div>

        {/* Messages area */}
        <div className="flex-1 overflow-y-auto p-6 space-y-6 scroll-smooth bg-gradient-to-b from-transparent to-muted/10">
          {messages.map((msg, index) => (
            <div key={index} className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}>
              <div className={`flex max-w-[85%] ${msg.role === 'user' ? 'flex-row-reverse' : 'flex-row'}`}>
                
                {/* Avatar */}
                <div className={`flex-shrink-0 flex items-center justify-center w-10 h-10 rounded-full mt-1 ${
                  msg.role === 'user' 
                    ? 'bg-blue-600 text-white ml-3 shadow-md' 
                    : 'bg-primary text-primary-foreground mr-3 shadow-md'
                }`}>
                  {msg.role === 'user' ? <User size={20} /> : <Bot size={20} />}
                </div>

                {/* Message Bubble */}
                <div className={`px-6 py-4 rounded-2xl whitespace-pre-wrap leading-relaxed text-sm shadow-sm ${
                  msg.role === 'user'
                    ? 'bg-blue-600 text-white rounded-tr-sm'
                    : 'bg-card border border-border/50 rounded-tl-sm text-card-foreground'
                }`}>
                  {msg.content || (msg.role === 'assistant' && isLoading && index === messages.length - 1 ? (
                    <span className="flex items-center space-x-1.5 text-muted-foreground h-5">
                      <span className="w-2 h-2 rounded-full bg-current animate-bounce" style={{ animationDelay: '0ms' }} />
                      <span className="w-2 h-2 rounded-full bg-current animate-bounce" style={{ animationDelay: '150ms' }} />
                      <span className="w-2 h-2 rounded-full bg-current animate-bounce" style={{ animationDelay: '300ms' }} />
                    </span>
                  ) : '')}
                </div>
              </div>
            </div>
          ))}
          <div ref={messagesEndRef} />
        </div>

        {/* Input area */}
        <div className="p-5 bg-background/95 backdrop-blur border-t border-border/50">
          <form onSubmit={handleSubmit} className="relative flex items-center max-w-4xl mx-auto">
            <input
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder="Nhập câu hỏi của bạn..."
              className="w-full pl-6 pr-16 py-4 rounded-full border border-border/80 bg-muted/30 hover:bg-muted/50 focus:bg-background focus:ring-2 focus:ring-primary/30 focus:border-primary transition-all outline-none shadow-sm text-sm"
              disabled={isLoading}
            />
            <button
              type="submit"
              disabled={!input.trim() || isLoading}
              className="absolute right-2 flex items-center justify-center w-11 h-11 rounded-full bg-primary text-primary-foreground hover:bg-primary/90 transition-all active:scale-95 disabled:opacity-50 disabled:active:scale-100 shadow-md disabled:shadow-none"
            >
              {isLoading ? <Loader2 size={20} className="animate-spin" /> : <Send size={20} className="ml-0.5" />}
            </button>
          </form>
          <div className="text-center mt-3">
            <span className="text-[11px] text-muted-foreground/80 uppercase tracking-widest font-semibold">Được cung cấp bởi HCMUE Handbook RAG</span>
          </div>
        </div>
      </div>
    </section>
  )
}
