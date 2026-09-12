"""Build the dependency-free Java 8 agent; only maintainers/CI need a JDK."""
import base64
import io
import pathlib
import shutil
import subprocess
import sys
import tempfile
import zipfile

ROOT = pathlib.Path(__file__).resolve().parent

def compiler():
    javac = shutil.which('javac')
    return [javac] if javac else ['java', '-m', 'jdk.compiler/com.sun.tools.javac.Main']

def build(target):
    subprocess.run(compiler() + ['--release', '8', '-g:none', '-encoding', 'UTF-8', '-d', str(target)]
                   + [str(p) for p in (ROOT / 'src').rglob('*.java')], check=True)
    data = io.BytesIO()
    with zipfile.ZipFile(data, 'w', zipfile.ZIP_DEFLATED) as jar:
        files = {'META-INF/MANIFEST.MF': b'Manifest-Version: 1.0\r\nPremain-Class: nsocry.bridge.Agent\r\n\r\n'}
        files.update({p.relative_to(target).as_posix(): p.read_bytes() for p in target.rglob('*.class')})
        for name, content in sorted(files.items()):
            info = zipfile.ZipInfo(name, (2026, 1, 1, 0, 0, 0))
            info.compress_type = zipfile.ZIP_DEFLATED
            jar.writestr(info, content)
    return data.getvalue()

if __name__ == '__main__':
    with tempfile.TemporaryDirectory() as directory:
        content = build(pathlib.Path(directory))
    output = ROOT.parent / 'src/NSOCryPro/Resources/bridge.jar.b64'
    if '--check' in sys.argv:
        # ZIP compression varies by zlib version: compare the actual class/manifest bytes.
        with zipfile.ZipFile(io.BytesIO(content)) as new, zipfile.ZipFile(io.BytesIO(base64.b64decode(output.read_text()))) as old:
            assert set(new.namelist()) == set(old.namelist()), 'Agent files differ'
            for name in new.namelist():
                assert new.read(name) == old.read(name), 'Rebuild agent resource: ' + name
        print('Embedded agent matches source.')
    else:
        output.write_text(base64.b64encode(content).decode() + '\n', encoding='ascii')
        print('Built embedded agent.')
