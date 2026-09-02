"""Parse ONI kanim build/anim bytes (BILD v9/10, ANIM v5)."""
from __future__ import annotations

import struct
from pathlib import Path


class Reader:
	def __init__(self, data: bytes):
		self.data = data
		self.i = 0

	def remaining(self) -> int:
		return len(self.data) - self.i

	def skip(self, n: int) -> None:
		self.i += n

	def chars(self, n: int) -> str:
		s = self.data[self.i : self.i + n].decode("ascii", errors="replace")
		self.i += n
		return s

	def i32(self) -> int:
		v = struct.unpack_from("<i", self.data, self.i)[0]
		self.i += 4
		return v

	def u32(self) -> int:
		v = struct.unpack_from("<I", self.data, self.i)[0]
		self.i += 4
		return v

	def f32(self) -> float:
		v = struct.unpack_from("<f", self.data, self.i)[0]
		self.i += 4
		return v

	def klei_string(self) -> str:
		n = self.i32()
		if n < 0 or n > self.remaining():
			raise ValueError(f"bad klei string len {n} at {self.i - 4}")
		s = self.data[self.i : self.i + n].decode("utf-8", errors="replace")
		self.i += n
		return s


def parse_hash_table(r: Reader) -> dict[int, str]:
	count = r.i32()
	table = {}
	for _ in range(count):
		h = r.i32()
		name = r.klei_string()
		table[h & 0xFFFFFFFF] = name
	return table


def parse_build(path: Path) -> None:
	r = Reader(path.read_bytes())
	header = r.chars(4)
	version = r.i32()
	symbol_count = r.i32()
	r.i32()
	name = r.klei_string()
	print(f"BUILD header={header} ver={version} symbols={symbol_count} name={name}")
	for i in range(symbol_count):
		h = r.i32()
		path_hash = None
		if version > 9:
			path_hash = r.i32()
		colour = r.i32()
		flags = r.i32()
		num_frames = r.i32()
		print(
			f"  symbol[{i}] hash={h} path={path_hash} colour={colour} flags={flags} frames={num_frames}"
		)
		for j in range(num_frames):
			src = r.i32()
			dur = r.i32()
			img = r.i32()
			cx = r.f32()
			cy = r.f32()
			w = r.f32()
			h = r.f32()
			u0 = r.f32()
			v0 = r.f32()
			u1 = r.f32()
			v1 = r.f32()
			uv_min_y = 1.0 - v0
			uv_max_y = 1.0 - v1
			print(
				f"    frame[{j}] src={src} dur={dur} img={img} bbox=({cx:.1f},{cy:.1f},{w:.1f}x{h:.1f})"
				f" uv=({u0:.4f},{v0:.4f})-({u1:.4f},{v1:.4f}) unityY=({uv_min_y:.4f}-{uv_max_y:.4f})"
			)
	table = parse_hash_table(r)
	print("  hashes:")
	for h, n in table.items():
		print(f"    {h} {n}")
	print(f"  leftover={r.remaining()}")


def parse_anim(path: Path) -> None:
	r = Reader(path.read_bytes())
	header = r.chars(4)
	version = r.u32()
	r.i32()
	r.i32()
	anim_count = r.i32()
	print(f"ANIM header={header} ver={version} anims={anim_count}")
	for i in range(anim_count):
		name = r.klei_string()
		root = r.i32()
		fps = r.f32()
		nframes = r.i32()
		print(f"  anim[{i}] {name!r} root={root} fps={fps} frames={nframes}")
		for j in range(nframes):
			x = r.f32()
			y = r.f32()
			w = r.f32()
			h = r.f32()
			ne = r.i32()
			print(f"    frame[{j}] origin=({x:.1f},{y:.1f}) size=({w:.1f}x{h:.1f}) elements={ne}")
			for k in range(ne):
				symbol = r.i32()
				frame = r.i32()
				folder = r.i32()
				r.i32()
				a = r.f32()
				b = r.f32()
				c = r.f32()
				d = r.f32()
				m00 = r.f32()
				m10 = r.f32()
				m01 = r.f32()
				m11 = r.f32()
				m02 = r.f32()
				m12 = r.f32()
				r.f32()
				print(
					f"      el[{k}] symbol={symbol} frame={frame} folder={folder}"
					f" alpha={a:.2f} color=({b:.2f},{c:.2f},{d:.2f})"
					f" m=[{m00:.3f} {m01:.3f} {m02:.1f}; {m10:.3f} {m11:.3f} {m12:.1f}]"
				)
	max_vis = r.i32()
	print(f"  maxVis={max_vis}")
	table = parse_hash_table(r)
	print("  hashes:")
	for h, n in table.items():
		print(f"    {h} {n}")
	print(f"  leftover={r.remaining()}")


def main() -> None:
	here = Path(__file__).resolve().parent / "painting_art_b"
	parse_build(here / "painting_art_b_build.bytes")
	print()
	parse_anim(here / "painting_art_b_anim.bytes")


if __name__ == "__main__":
	main()
