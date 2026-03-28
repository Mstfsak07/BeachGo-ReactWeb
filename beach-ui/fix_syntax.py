import os, re
from pathlib import Path

root = Path('.')

# node_modules klasörünü exclude et
excluded_dirs = {'node_modules', '.git', 'build', 'dist', '.next'}

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

# Sadece src klasöründe işlem yap
src_dir = Path('src')
if src_dir.exists():
    for file in list(src_dir.rglob('*.js')) + list(src_dir.rglob('*.jsx')):
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
