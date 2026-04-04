import os

b_path = r'C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb\BeachRehberi.API\BeachRehberi.API\Models\Beach.cs'
with open(b_path, 'r', encoding='utf-8') as f:
    b_content = f.read()

if 'InstagramUsername' not in b_content:
    b_content = b_content.replace('public string Instagram { get; set; } = string.Empty;', 'public string Instagram { get; set; } = string.Empty;\n    public string InstagramUsername { get; set; } = string.Empty;\n    public string SocialContentSource { get; set; } = "mock";')
    with open(b_path, 'w', encoding='utf-8') as f:
        f.write(b_content)

dto_path = r'C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb\BeachRehberi.API\BeachRehberi.API\DTOs\BeachDtos.cs'
with open(dto_path, 'r', encoding='utf-8') as f:
    dto_content = f.read()

if 'InstagramUsername' not in dto_content:
    dto_content = dto_content.replace('public string Instagram { get; set; } = string.Empty;', 'public string Instagram { get; set; } = string.Empty;\n    public string InstagramUsername { get; set; } = string.Empty;\n    public string SocialContentSource { get; set; } = "mock";')
    with open(dto_path, 'w', encoding='utf-8') as f:
        f.write(dto_content)

# We also need to update BeachRehberi.Application if there's GetBeachByIdQuery!
q_path = r'C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb\BeachRehberi.Application\Features\Beaches\Queries\GetBeachById\GetBeachByIdQuery.cs'
if os.path.exists(q_path):
    with open(q_path, 'r', encoding='utf-8') as f:
        q_content = f.read()
    if 'InstagramUsername' not in q_content:
        q_content = q_content.replace('string? Instagram,', 'string? Instagram,\n    string? InstagramUsername,\n    string? SocialContentSource,')
        q_content = q_content.replace('beach.Instagram,', 'beach.Instagram,\n            beach.InstagramUsername,\n            beach.SocialContentSource,')
        with open(q_path, 'w', encoding='utf-8') as f:
            f.write(q_content)

# And BeachRehberi.Domain\Entities\Beach.cs
dom_path = r'C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb\BeachRehberi.Domain\Entities\Beach.cs'
if os.path.exists(dom_path):
    with open(dom_path, 'r', encoding='utf-8') as f:
        dom_content = f.read()
    if 'InstagramUsername' not in dom_content:
        dom_content = dom_content.replace('public string? Instagram { get; private set; }', 'public string? Instagram { get; private set; }\n    public string? InstagramUsername { get; private set; }\n    public string SocialContentSource { get; private set; } = "mock";')
        with open(dom_path, 'w', encoding='utf-8') as f:
            f.write(dom_content)

print('SUCCESS')
