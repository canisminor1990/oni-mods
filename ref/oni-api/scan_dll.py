"""Search ONI managed DLLs for type/method names. Writes nothing unless --out is set."""
import argparse
import sys
from pathlib import Path

DEFAULT_DLL = Path(
    r"E:\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll"
)
HERE = Path(__file__).resolve().parent


def collect_strings(data: bytes, needle: bytes, encodings: tuple[str, ...]) -> list[str]:
    hits: list[str] = []
    seen: set[str] = set()
    for enc in encodings:
        try:
            text = data.decode(enc, errors="ignore")
        except LookupError:
            continue
        start = 0
        key = needle.decode("ascii", errors="ignore")
        while True:
            i = text.find(key, start)
            if i < 0:
                break
            lo = max(0, i - 80)
            hi = min(len(text), i + len(key) + 120)
            snippet = " ".join(text[lo:hi].split())
            if snippet not in seen and len(snippet) > 8:
                seen.add(snippet)
                hits.append(snippet)
            start = i + len(key)
            if len(hits) >= 40:
                return hits
    return hits


def main() -> int:
    p = argparse.ArgumentParser(description="Scan Assembly-CSharp.dll for a name")
    p.add_argument("name", help="type or method name, e.g. BuildingFacades")
    p.add_argument("--dll", type=Path, default=DEFAULT_DLL)
    p.add_argument("--out", type=Path, help="write snippets under ref/oni-api/_scans/")
    args = p.parse_args()
    if not args.dll.is_file():
        print("missing DLL:", args.dll, file=sys.stderr)
        return 1
    data = args.dll.read_bytes()
    needle = args.name.encode("ascii", errors="ignore")
    if not needle:
        return 1
    hits = collect_strings(data, needle, ("utf-8", "utf-16le", "latin1"))
    if not hits:
        print("no hits for", args.name)
        return 1
    print(f"{len(hits)} hits for {args.name} in {args.dll.name}")
    for h in hits[:25]:
        line = h[:240].encode("utf-8", errors="replace").decode("utf-8")
        try:
            print("-", line)
        except UnicodeEncodeError:
            print("-", line.encode("ascii", errors="replace").decode("ascii"))
    if args.out:
        dest_dir = HERE / "_scans"
        dest_dir.mkdir(exist_ok=True)
        dest = args.out if args.out.suffix else dest_dir / (args.name + ".txt")
        dest.write_text("\n".join(hits), encoding="utf-8")
        print("wrote", dest)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
