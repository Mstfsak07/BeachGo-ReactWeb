import os

# 1. Update DTO
path_dto = r'C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb\BeachRehberi.API\BeachRehberi.API\DTOs\BeachDtos.cs'
with open(path_dto, 'r', encoding='utf-8') as f:
    dto_content = f.read()

parts = dto_content.split('public class UpdateBeachDto')
if len(parts) == 2 and 'public string SocialContentSource' not in parts[1]:
    old_str = 'public string Instagram { get; set; } = string.Empty;'
    new_str = 'public string Instagram { get; set; } = string.Empty;\n    public string InstagramUsername { get; set; } = string.Empty;\n    public string SocialContentSource { get; set; } = "mock";'
    parts[1] = parts[1].replace(old_str, new_str)
    with open(path_dto, 'w', encoding='utf-8') as f:
        f.write(parts[0] + 'public class UpdateBeachDto' + parts[1])

# 2. Admin Controller Update
path_admin = r'C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb\BeachRehberi.API\BeachRehberi.API\Controllers\AdminController.cs'
with open(path_admin, 'r', encoding='utf-8') as f:
    adm_content = f.read()

if 'dto.SocialContentSource' not in adm_content:
    adm_content = adm_content.replace('beach.Instagram = dto.Instagram;', 'beach.Instagram = dto.Instagram;\n            beach.InstagramUsername = dto.InstagramUsername;\n            beach.SocialContentSource = dto.SocialContentSource;')
    adm_content = adm_content.replace('existing.Instagram = dto.Instagram;', 'existing.Instagram = dto.Instagram;\n                    existing.InstagramUsername = dto.InstagramUsername;\n                    existing.SocialContentSource = dto.SocialContentSource;')
    adm_content = adm_content.replace('Instagram = dto.Instagram,', 'Instagram = dto.Instagram,\n                        InstagramUsername = dto.InstagramUsername,\n                        SocialContentSource = dto.SocialContentSource,')
    adm_content = adm_content.replace('b.Rating', 'b.Rating,\n                    b.InstagramUsername,\n                    b.SocialContentSource')
    with open(path_admin, 'w', encoding='utf-8') as f:
        f.write(adm_content)

# 3. Business Controller Update
path_bus = r'C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb\BeachRehberi.API\BeachRehberi.API\Controllers\BusinessController.cs'
with open(path_bus, 'r', encoding='utf-8') as f:
    bus_content = f.read()

if 'dto.SocialContentSource' not in bus_content:
    bus_content = bus_content.replace('beach.Instagram = dto.Instagram;', 'beach.Instagram = dto.Instagram;\n            beach.InstagramUsername = dto.InstagramUsername;\n            beach.SocialContentSource = dto.SocialContentSource;')
    bus_content = bus_content.replace('b.Rating', 'b.Rating,\n                    b.InstagramUsername,\n                    b.SocialContentSource')
    with open(path_bus, 'w', encoding='utf-8') as f:
        f.write(bus_content)

print('BACKEND READY')
