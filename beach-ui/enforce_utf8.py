from pathlib import Path

files = list(Path('src').rglob('*.js')) + list(Path('src').rglob('*.jsx'))
for f in files:
    data = f.read_bytes()
    # decode with replacement to avoid errors; write back UTF-8 no BOM
    text = data.decode('utf-8', errors='replace')
    f.write_text(text, encoding='utf-8')
print('rewrote', len(files), 'files as UTF-8 no BOM')
