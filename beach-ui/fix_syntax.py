import os, re
from pathlib import Path

root = Path('.')

patterns = [
    (re.compile(r'\\"'), '"'),
    (re.compile(r"\\\'"), "'"),
    (re.compile(r'Giri�'), 'Giriş'),
    (re.compile(r'�ifre'), 'Şifre'),
    (re.compile(r'Ho�'), 'Hoş'),
    (re.compile(r'<\s+([a-zA-Z][a-zA-Z0-9_-]*)\s*>'), r'<\1>'),
    (re.compile(r'<\s*/\s+([a-zA-Z][a-zA-Z0-9_-]*)\s*>'), r'</\1>'),
    (re.compile(r'"(\.\/[^"\\]*)\\"'), r'"\1"'),
    (re.compile(r'className=\\"'), 'className="'),
]
invalid = re.compile(r'[\x00-\x08\x0B\x0C\x0E-\x1F\uFEFF\u202A-\u202E]')

count = 0
issues = []
for file in list(root.rglob('*.js')) + list(root.rglob('*.jsx')):
    try:
        text = file.read_bytes().decode('utf-8', errors='replace')
    except Exception as e:
        issues.append((file, 'decode', str(e)))
        continue
    orig = text
    for pat, repl in patterns:
        text = pat.sub(repl, text)
    text = invalid.sub('', text)
    if text != orig:
        file.write_text(text, encoding='utf-8')
        count += 1

print(f'Processed files: {count}, issues: {len(issues)}')
for f,e,m in issues:
    print('ISSUE', f, e, m)
