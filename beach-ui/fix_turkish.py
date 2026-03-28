from pathlib import Path
import re

root = Path('src')
fixes = [
    (re.compile('Giri�'), 'Giriş'),
    (re.compile('�ifre'), 'Şifre'),
    (re.compile('Ho�'), 'Hoş'),
]

changed = []
for file in list(root.rglob('*.js')) + list(root.rglob('*.jsx')):
    text = file.read_text(encoding='utf-8', errors='replace')
    orig = text
    for pat, rep in fixes:
        text = pat.sub(rep, text)
    if text != orig:
        file.write_text(text, encoding='utf-8')
        changed.append(file)

print('updated', len(changed), 'files')
for f in changed:
    print('  ', f)

issues = []
for file in list(root.rglob('*.js')) + list(root.rglob('*.jsx')):
    txt = file.read_text(encoding='utf-8', errors='replace')
    if 'Giri' in txt or '�ifre' in txt or 'Ho�' in txt:
        issues.append(file)
print('remaining bad tokens in', len(issues), 'files')
for f in issues:
    print('  ', f)
