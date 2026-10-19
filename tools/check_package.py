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
    manifest = json.loads((root / 'ExpandedHotbars/bin/Release/ExpandedHotbars.json').read_text(encoding='utf-8-sig'))
    subprocess.run([sys.executable, str(root / 'tools/package_release.py'), '--tag', 'v' + manifest['AssemblyVersion']], check=True)
    package = root / 'artifacts/ExpandedHotbars.zip'
    assert hashlib.sha256(package.read_bytes()).hexdigest() == (root / 'artifacts/SHA256SUMS.txt').read_text().split()[0]
    with zipfile.ZipFile(package) as archive:
        assert archive.testzip() is None
        assert {'ExpandedHotbars.dll', 'ExpandedHotbars.deps.json', 'ExpandedHotbars.json',
                'KamiToolKit.dll', 'images/icon.png', 'LICENSE.md', 'licenses/KamiToolKit.txt'} <= set(archive.namelist())
        assert struct.unpack('>II', archive.read('images/icon.png')[16:24]) == (512, 512)
        assert json.loads(archive.read('ExpandedHotbars.json'))['AssemblyVersion'] == manifest['AssemblyVersion']
    print('Package structure, manifest, icon, licenses and checksum passed.')
finally:
    feed.write_bytes(original)
