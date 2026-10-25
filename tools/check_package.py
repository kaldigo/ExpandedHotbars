"""Build and validate a release package without publishing or changing the feed."""
import hashlib
import json
from pathlib import Path
import struct
import subprocess
import sys
import zipfile

root = Path(__file__).resolve().parents[1]
feed = root / 'pluginmaster.json'
original = feed.read_bytes()
try:
    manifest = json.loads((root / 'ExpandedHotbars/bin/Release/ExpandedHotbarsPlus.json').read_text(encoding='utf-8-sig'))
    assert manifest['InternalName'] == 'ExpandedHotbarsPlus'
    assert manifest['Name'] == 'ExpandedHotbars Plus'
    assert manifest['RepoUrl'] == 'https://github.com/kaldigo/ExpandedHotbars'
    subprocess.run([sys.executable, str(root / 'tools/package_release.py'), '--tag', 'v' + manifest['AssemblyVersion']], check=True)
    package = root / 'artifacts/ExpandedHotbarsPlus.zip'
    assert hashlib.sha256(package.read_bytes()).hexdigest() == (root / 'artifacts/SHA256SUMS.txt').read_text().split()[0]
    with zipfile.ZipFile(package) as archive:
        assert archive.testzip() is None
        assert {'ExpandedHotbarsPlus.dll', 'ExpandedHotbarsPlus.deps.json', 'ExpandedHotbarsPlus.json',
                'KamiToolKit.dll', 'images/icon.png', 'LICENSE.md', 'licenses/KamiToolKit.txt'} <= set(archive.namelist())
        assert struct.unpack('>II', archive.read('images/icon.png')[16:24]) == (512, 512)
        assert json.loads(archive.read('ExpandedHotbarsPlus.json'))['AssemblyVersion'] == manifest['AssemblyVersion']
        overrides = json.loads(archive.read('images/plugin-overrides/manifest.json'))
        fallback = overrides['Fallback']
        assert fallback['File'] == 'images/plugin-fallback.png'
        fallback_pixels = archive.read(fallback['File'])
        assert fallback_pixels.startswith(b'\x89PNG\r\n\x1a\n')
        fallback_width, fallback_height = struct.unpack('>II', fallback_pixels[16:24])
        assert fallback_width == fallback_height and 80 <= fallback_width <= 4096
        assert hashlib.sha256(fallback_pixels).hexdigest() == fallback['Sha256']
        names = set()
        for entry in overrides['Icons']:
            assert entry['InternalName'] not in names
            names.add(entry['InternalName'])
            assert entry['File'] == entry['InternalName'] + '.png'
            data = archive.read('images/plugin-overrides/' + entry['File'])
            assert data.startswith(b'\x89PNG\r\n\x1a\n')
            width, height = struct.unpack('>II', data[16:24])
            assert width == height and 80 <= width <= 4096
            assert hashlib.sha256(data).hexdigest() == entry['Sha256']
        assert {'Dalamud', 'Stylist'} <= names
        shipped = {name for name in archive.namelist() if name.startswith('images/plugin-overrides/') and name.endswith('.png')}
        assert len(shipped) == len(names), 'Every override must have a manifest/credit entry'
        assert 'images/plugin-overrides/CREDITS.md' in archive.namelist()
    print('Package structure, manifest, icon, licenses and checksum passed.')
finally:
    feed.write_bytes(original)
