"""Report a chapter-set digest for a committee standard directory."""
import argparse
import hashlib
from pathlib import Path


def chapter_digest(standard: Path) -> str:
    """Hash sorted filename, NUL, lowercase file digest, and LF records."""
    records = []
    for path in sorted(standard.glob('*.md'), key=lambda entry: entry.name):
        if path.name == 'README.md':
            continue
        digest = hashlib.sha256(path.read_bytes()).hexdigest()
        records.append(f'{path.name}\0{digest}\n')
    return hashlib.sha256(''.join(records).encode('utf-8')).hexdigest()


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('standard', type=Path)
    print(chapter_digest(parser.parse_args().standard))
