const fs = require('fs');
const path = require('path');

function walkDir(dir, callback) {
  fs.readdirSync(dir).forEach(f => {
    let dirPath = path.join(dir, f);
    let isDirectory = fs.statSync(dirPath).isDirectory();
    isDirectory ? walkDir(dirPath, callback) : callback(path.join(dir, f));
  });
}

const map = {
  'bg-white': 'bg-surface',
  'bg-slate-50': 'bg-background',
  'bg-slate-100': 'bg-background',
  'text-slate-900': 'text-foreground',
  'text-slate-800': 'text-foreground',
  'text-slate-600': 'text-muted',
  'text-slate-500': 'text-muted',
  'border-slate-200': 'border-border',
  'border-slate-100': 'border-border',
  'border-slate-300': 'border-border-strong',
  'bg-indigo-600': 'bg-primary',
  'text-indigo-600': 'text-primary',
  'hover:bg-indigo-500': 'hover:bg-primary-hover',
  'bg-indigo-50': 'bg-primary/10',
  'text-indigo-900': 'text-primary',
};

const regex = new RegExp(Object.keys(map).join('|'), 'g');

walkDir('frontend/src/features/chat/components', (filePath) => {
  if (filePath.endsWith('.tsx') || filePath.endsWith('.ts')) {
    let content = fs.readFileSync(filePath, 'utf8');
    let original = content;
    content = content.replace(regex, matched => map[matched]);
    
    // Fix text-white inside primary
    content = content.replace(/bg-primary([^\"']*?)text-white/g, 'bg-primary$1text-primary-foreground');
    content = content.replace(/text-white([^\"']*?)bg-primary/g, 'text-primary-foreground$1bg-primary');
    
    if (content !== original) {
      fs.writeFileSync(filePath, content);
      console.log('Updated: ' + filePath);
    }
  }
});
