from pathlib import Path

files = list(Path('src').rglob('*.js')) + list(Path('src').rglob('*.jsx'))
print('Checked', len(files), 'files')
for f in files:
    data = f.read_bytes()
    try:
        text = data.decode('utf-8')
    except UnicodeDecodeError as e:
        print(f, 'NOT UTF-8:', e)
        continue
    if data.startswith(b'\xef\xbb\xbf'):
        print(f, 'has BOM')
