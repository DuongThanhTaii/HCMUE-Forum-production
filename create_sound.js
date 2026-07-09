const fs = require('fs');
const path = require('path');
// Base64 for a tiny beep WAV file
const base64 = 'UklGRiQAAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQAAAAA=';
const dir = path.join(__dirname, 'frontend/public/assets/sounds');
if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });
fs.writeFileSync(path.join(dir, 'message.mp3'), Buffer.from(base64, 'base64'));
console.log('Created message.mp3');
