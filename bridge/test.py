import base64
import os
import pathlib
import secrets
import socket
import struct
import subprocess
import tempfile
import time
from build import ROOT, compiler

with tempfile.TemporaryDirectory() as directory:
    target = pathlib.Path(directory)
    subprocess.run(compiler() + ['--release', '8', '-encoding', 'UTF-8', '-d', str(target)]
                   + [str(p) for p in (ROOT / 'src').rglob('*.java')]
                   + [str(p) for p in (ROOT / 'tests').glob('*.java')], check=True)
    path = target / 'aY.class'
    data = bytearray(path.read_bytes())
    # Rename just CONSTANT_Utf8 "b" to "a", keeping both distinct field descriptors.
    pos, index = 10, 1
    while index < int.from_bytes(data[8:10], 'big'):
        tag = data[pos]; pos += 1
        if tag == 1:
            length = int.from_bytes(data[pos:pos+2], 'big'); pos += 2
            if data[pos:pos+length] == b'b': data[pos] = ord('a')
            pos += length
        elif tag in (7, 8, 16, 19, 20): pos += 2
        elif tag in (3, 4, 9, 10, 11, 12, 17, 18): pos += 4
        elif tag in (5, 6): pos += 8; index += 1
        elif tag == 15: pos += 3
        else: raise AssertionError(tag)
        index += 1
    path.write_bytes(data)
    subprocess.run(['java', '-cp', str(target), 'nsocry.bridge.AdapterTest'], check=True)
    agent = target / 'agent.jar'
    agent.write_bytes(base64.b64decode((ROOT.parent / 'src/NSOCryPro/Resources/bridge.jar.b64').read_text()))
    with socket.socket() as listener:
        listener.bind(('127.0.0.1', 0)); listener.listen(); listener.settimeout(10)
        token = secrets.token_hex(32)
        env = dict(os.environ, NSOCRY_BRIDGE_PORT=str(listener.getsockname()[1]), NSOCRY_BRIDGE_TOKEN=token, NSOCRY_BRIDGE_SUPPORTED='0')
        process = subprocess.Popen(['java', '-javaagent:' + str(agent), '-cp', str(target), 'AgentHost'], env=env)
        try:
            connection, _ = listener.accept()
            with connection:
                connection.settimeout(5)
                file = connection.makefile('rwb', buffering=0)
                assert file.readline().decode().strip() == 'HELLO\t1\t' + token
                file.write(b'OK\nPOLL\n')
                assert file.readline() == b'STATE\tUNSUPPORTED\t\t\n'
                file.write(b'INVALID\n')
                assert file.readline() == b''
                assert process.poll() is None, 'Bridge failure terminated game host'
            print('Agent handshake, polling, unsupported version and disconnect tests passed.')
        finally:
            process.terminate(); process.wait(timeout=10)
