"""Package the fork and its standalone release feed without game dependencies."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import struct
import zipfile

parser = argparse.ArgumentParser()
parser.add_argument('--tag', required=True)
parser.add_argument('--changelog-file')
args = parser.parse_args()
if not re.fullmatch(r'v\d+\.\d+\.\d+\.\d+', args.tag):
    parser.error('Expected a four-part release tag such as v1.0.1.1')
root = Path(__file__).resolve().parents[1]
build = root / 'ExpandedHotbars/bin/Release'
output = root / 'artifacts'
output.mkdir(exist_ok=True)
manifest = json.loads((build / 'ExpandedHotbarsPlus.json').read_text(encoding='utf-8-sig'))
assert manifest['InternalName'] == 'ExpandedHotbarsPlus'
assert manifest['Name'] == 'ExpandedHotbars Plus'
assert manifest['DalamudApiLevel'] == 15
assert manifest['AssemblyVersion'] == args.tag[1:]
assert manifest['RepoUrl'] == 'https://github.com/kaldigo/ExpandedHotbars'
icon = build / 'images/icon.png'
assert struct.unpack('>II', icon.read_bytes()[16:24]) == (512, 512)
manifest['Changelog'] = 'Adds per-hotbar Close After Use for mouse clicks and keybinds, plus a matching Kaldigo plugin icon. Auto-close follows activation attempts, including cooldown failures. In-game testing remains pending.'
if args.changelog_file:
    manifest['Changelog'] = Path(args.changelog_file).read_text(encoding='utf-8').strip()
package = output / 'ExpandedHotbarsPlus.zip'
with zipfile.ZipFile(package, 'w', zipfile.ZIP_DEFLATED) as archive:
    for name in ['ExpandedHotbarsPlus.dll', 'ExpandedHotbarsPlus.deps.json', 'KamiToolKit.dll']:
        archive.write(build / name, name)
    archive.writestr('ExpandedHotbarsPlus.json', json.dumps(manifest, indent=2) + '\n')
    archive.write(root / 'LICENSE.md', 'LICENSE.md')
    archive.write(root / 'KamiToolKit/LICENSE', 'licenses/KamiToolKit.txt')
    for folder in ['Assets', 'images']:
        for asset in sorted((build / folder).rglob('*')):
            if asset.is_file():
                archive.write(asset, asset.relative_to(build).as_posix())
url = f'https://github.com/kaldigo/ExpandedHotbars/releases/download/{args.tag}/ExpandedHotbarsPlus.zip'
manifest.update(DownloadLinkInstall=url, DownloadLinkUpdate=url, DownloadLinkTesting=url)
feed = json.dumps([manifest], indent=2) + '\n'
(output / 'pluginmaster.json').write_text(feed, encoding='utf-8')
(root / 'pluginmaster.json').write_text(feed, encoding='utf-8')
(output / 'SHA256SUMS.txt').write_text(f'{hashlib.sha256(package.read_bytes()).hexdigest()}  {package.name}\n', encoding='utf-8')
print(f'Packaged {manifest["InternalName"]} {manifest["AssemblyVersion"]}')
