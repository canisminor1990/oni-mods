using KMod;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace ModListPreviews
{
	internal static class PreviewService
	{
		private const int MaxTextureSize = 256;
		private const int CacheDays = 14;
		private const int SteamBatchSize = 50;

		private static readonly string[] LocalNames =
		{
			"preview.png", "Preview.png", "PREVIEW.png",
			"preview.jpg", "Preview.jpg", "preview.jpeg",
			"thumbnail.png", "thumb.png"
		};

		private static readonly object Gate = new object();
		private static readonly Dictionary<string, CacheEntry> Memory = new Dictionary<string, CacheEntry>();
		private static readonly HashSet<ulong> SteamQueued = new HashSet<ulong>();
		private static readonly HashSet<ulong> SteamInFlight = new HashSet<ulong>();
		private static readonly HashSet<ulong> ForceHttp = new HashSet<ulong>();
		private static readonly HashSet<ulong> RetryIds = new HashSet<ulong>();
		private static readonly Dictionary<ulong, int> FailPass = new Dictionary<ulong, int>();

		private static string cacheDir;
		private static int httpBusy;
		private static int retryPass;
		private static bool ready;
		private static bool tlsReady;

		private sealed class CacheEntry
		{
			public Texture2D texture;
			public bool owned;
			public bool pending;
			public bool failed;
		}

		public static void EnsureReady()
		{
			if (ready)
				return;
			ready = true;

			try
			{
				cacheDir = Path.Combine(Util.RootFolder(), "mods", "config", "ModListPreviews");
				Directory.CreateDirectory(cacheDir);
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "cache folder failed: " + ex.Message);
				cacheDir = null;
			}

			LoadRetryFile();
		}

		public static void BeginRetryPass()
		{
			lock (Gate)
				retryPass++;
		}

		public static Texture2D Get(KMod.Mod mod)
		{
			EnsureReady();
			if (mod == null)
				return null;

			string key = KeyOf(mod);
			lock (Gate)
			{
				if (Memory.TryGetValue(key, out CacheEntry cached) && cached.texture != null)
					return cached.texture;
			}

			if (IsSteam(mod) && ulong.TryParse(mod.label.id, out ulong steamId))
			{
				if (IsForced(steamId))
				{
					MarkPending(key);
					if (!ShouldSkipDownload(steamId))
						QueueSteamDownload(steamId);
					return null;
				}

				Texture2D steam = LoadSteamLive(steamId);
				if (steam != null)
				{
					Store(key, steam, false, false);
					return steam;
				}

				Texture2D fromCache = LoadDiskCache(steamId);
				if (fromCache != null)
				{
					Store(key, fromCache, true, false);
					return fromCache;
				}

				Texture2D localSteam = LoadLocal(mod);
				if (localSteam != null)
				{
					Store(key, localSteam, true, false);
					return localSteam;
				}

				MarkPending(key);
				if (!ShouldSkipDownload(steamId))
					QueueSteamDownload(steamId);
				return null;
			}

			Texture2D local = LoadLocal(mod);
			if (local != null)
			{
				Store(key, local, true, false);
				return local;
			}

			return null;
		}

		public static int AbsorbLiveSteamPreviews()
		{
			List<ulong> pending;
			lock (Gate)
			{
				pending = new List<ulong>();
				foreach (KeyValuePair<string, CacheEntry> pair in Memory)
				{
					if (pair.Value == null || pair.Value.texture != null)
						continue;
					if (!TryParseSteamKey(pair.Key, out ulong id))
						continue;
					if (ForceHttp.Contains(id))
						continue;
					pending.Add(id);
				}
			}

			int absorbed = 0;
			for (int i = 0; i < pending.Count; i++)
			{
				ulong steamId = pending[i];
				Texture2D steam = LoadSteamLive(steamId);
				if (steam == null)
					continue;
				Store("Steam:" + steamId, steam, false, false);
				absorbed++;
			}
			return absorbed;
		}

		public static void RefreshAll()
		{
			EnsureReady();
			List<ulong> ids = new List<ulong>();
			try
			{
				List<KMod.Mod> mods = Global.Instance?.modManager?.mods;
				if (mods != null)
				{
					for (int i = 0; i < mods.Count; i++)
					{
						KMod.Mod mod = mods[i];
						if (!IsSteam(mod) || !ulong.TryParse(mod.label.id, out ulong id))
							continue;
						ids.Add(id);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "refresh collect failed: " + ex.Message);
			}

			lock (Gate)
			{
				foreach (CacheEntry entry in Memory.Values)
				{
					if (entry != null && entry.owned && entry.texture != null)
						UnityEngine.Object.Destroy(entry.texture);
				}
				Memory.Clear();
				SteamQueued.Clear();
				SteamInFlight.Clear();
				ForceHttp.Clear();
				RetryIds.Clear();
				FailPass.Clear();
				for (int i = 0; i < ids.Count; i++)
				{
					ForceHttp.Add(ids[i]);
					SteamQueued.Add(ids[i]);
					MarkPendingLocked("Steam:" + ids[i]);
				}
			}

			SaveRetryFile();

			ClearDiskCache();
			if (ids.Count > 0)
				StartHttpBatches(ids);
			Debug.Log(Mod.LogPrefix + "refreshing " + ids.Count + " workshop covers");
		}

		public static void RequestMissingSteam(IEnumerable<KMod.Mod> mods)
		{
			if (mods == null)
				return;
			List<ulong> ids = new List<ulong>();
			foreach (KMod.Mod mod in mods)
			{
				if (!IsSteam(mod) || !ulong.TryParse(mod.label.id, out ulong id))
					continue;
				string key = "Steam:" + id;
				lock (Gate)
				{
					if (Memory.TryGetValue(key, out CacheEntry entry) && entry.texture != null && !ForceHttp.Contains(id))
						continue;
					if (ShouldSkipDownloadLocked(id))
						continue;
					if (!SteamQueued.Add(id))
						continue;
				}
				ids.Add(id);
			}
			if (ids.Count > 0)
				StartHttpBatches(ids);
		}

		private static void QueueSteamDownload(ulong steamId)
		{
			lock (Gate)
			{
				if (!SteamQueued.Add(steamId))
					return;
			}
			StartHttpBatches(new List<ulong> { steamId });
		}

		private static Texture2D LoadLocal(KMod.Mod mod)
		{
			try
			{
				Texture2D fromApi = mod.GetPreviewImage();
				if (fromApi != null)
					return fromApi;
			}
			catch
			{
			}

			string[] roots =
			{
				mod.ContentPath,
				mod.label.install_path
			};
			for (int r = 0; r < roots.Length; r++)
			{
				string root = roots[r];
				if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
					continue;
				for (int i = 0; i < LocalNames.Length; i++)
				{
					string path = Path.Combine(root, LocalNames[i]);
					Texture2D tex = LoadFile(path);
					if (tex != null)
						return tex;
				}
			}
			return null;
		}

		private static Texture2D LoadSteamLive(ulong steamId)
		{
			try
			{
				SteamUGCService ugc = SteamUGCService.Instance;
				if (ugc == null)
					return null;
				SteamUGCService.Mod steamMod = ugc.FindMod(new PublishedFileId_t(steamId));
				if (steamMod != null && steamMod.previewImage != null)
					return steamMod.previewImage;
			}
			catch
			{
			}
			return null;
		}

		private static Texture2D LoadDiskCache(ulong steamId)
		{
			if (string.IsNullOrEmpty(cacheDir))
				return null;
			string path = Path.Combine(cacheDir, steamId + ".img");
			try
			{
				if (!File.Exists(path))
					return null;
				if (System.DateTime.UtcNow - File.GetLastWriteTimeUtc(path) > System.TimeSpan.FromDays(CacheDays))
					return null;
				byte[] bytes = File.ReadAllBytes(path);
				if (bytes == null || bytes.Length < 16)
					return null;
				return BytesToTexture("cache_" + steamId, bytes);
			}
			catch
			{
				return null;
			}
		}

		private static Texture2D LoadFile(string path)
		{
			try
			{
				if (!File.Exists(path))
					return null;
				return BytesToTexture(Path.GetFileNameWithoutExtension(path), File.ReadAllBytes(path));
			}
			catch
			{
				return null;
			}
		}

		private static Texture2D BytesToTexture(string name, byte[] bytes)
		{
			if (bytes == null || bytes.Length == 0)
				return null;
			Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false)
			{
				name = "MLP_" + name,
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};
			if (!tex.LoadImage(bytes))
			{
				UnityEngine.Object.Destroy(tex);
				return null;
			}
			tex.Apply(false, false);
			return Downscale(tex);
		}

		private static Texture2D Downscale(Texture2D source)
		{
			if (source == null)
				return null;
			int w = source.width;
			int h = source.height;
			int longest = Math.Max(w, h);
			if (longest <= MaxTextureSize)
				return source;

			float scale = (float)MaxTextureSize / longest;
			int nw = Math.Max(1, Mathf.RoundToInt(w * scale));
			int nh = Math.Max(1, Mathf.RoundToInt(h * scale));
			RenderTexture rt = RenderTexture.GetTemporary(nw, nh, 0, RenderTextureFormat.ARGB32);
			Graphics.Blit(source, rt);
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = rt;
			Texture2D copy = new Texture2D(nw, nh, TextureFormat.RGBA32, false)
			{
				name = source.name + "_s",
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};
			copy.ReadPixels(new Rect(0f, 0f, nw, nh), 0, 0);
			copy.Apply(false, false);
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(rt);
			UnityEngine.Object.Destroy(source);
			return copy;
		}

		private static void Store(string key, Texture2D texture, bool owned, bool pending)
		{
			bool forgetRetry = false;
			ulong steamId = 0;
			lock (Gate)
			{
				if (Memory.TryGetValue(key, out CacheEntry existing))
				{
					if (existing.owned && existing.texture != null && existing.texture != texture)
						UnityEngine.Object.Destroy(existing.texture);
					existing.texture = texture;
					existing.owned = owned;
					existing.pending = pending && texture == null;
					existing.failed = texture == null && !pending;
				}
				else
				{
					Memory[key] = new CacheEntry
					{
						texture = texture,
						owned = owned,
						pending = pending && texture == null,
						failed = texture == null && !pending
					};
				}

				if (texture != null && TryParseSteamKey(key, out steamId) && RetryIds.Remove(steamId))
				{
					FailPass.Remove(steamId);
					forgetRetry = true;
				}
			}
			if (forgetRetry)
				SaveRetryFile();
		}

		private static void MarkPending(string key)
		{
			lock (Gate)
				MarkPendingLocked(key);
		}

		private static void MarkPendingLocked(string key)
		{
			if (!Memory.TryGetValue(key, out CacheEntry entry))
				Memory[key] = new CacheEntry { pending = true };
			else if (entry.texture == null)
				entry.pending = true;
		}

		private static bool IsForced(ulong steamId)
		{
			lock (Gate)
				return ForceHttp.Contains(steamId);
		}

		private static bool ShouldSkipDownload(ulong steamId)
		{
			lock (Gate)
				return ShouldSkipDownloadLocked(steamId);
		}

		private static bool ShouldSkipDownloadLocked(ulong steamId)
		{
			if (ForceHttp.Contains(steamId))
				return false;
			return FailPass.TryGetValue(steamId, out int pass) && pass == retryPass;
		}

		private static void RememberRetry(ulong steamId)
		{
			lock (Gate)
			{
				RetryIds.Add(steamId);
				FailPass[steamId] = retryPass;
			}
			SaveRetryFile();
		}

		private static bool TryParseSteamKey(string key, out ulong steamId)
		{
			steamId = 0;
			const string prefix = "Steam:";
			if (string.IsNullOrEmpty(key) || key.Length <= prefix.Length || !key.StartsWith(prefix))
				return false;
			return ulong.TryParse(key.Substring(prefix.Length), out steamId);
		}

		private static void MarkFailed(ulong steamId)
		{
			Store("Steam:" + steamId, null, false, false);
		}

		private static void StartHttpBatches(List<ulong> ids)
		{
			if (ids == null || ids.Count == 0)
				return;
			if (Interlocked.CompareExchange(ref httpBusy, 1, 0) != 0)
			{
				lock (Gate)
				{
					for (int i = 0; i < ids.Count; i++)
						SteamQueued.Add(ids[i]);
				}
				return;
			}

			ThreadPool.QueueUserWorkItem(_ => HttpWorker());
		}

		private static void EnsureTls()
		{
			if (tlsReady)
				return;
			tlsReady = true;
			try
			{
				ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
			}
			catch
			{
			}
		}

		private static void HttpWorker()
		{
			EnsureTls();
			try
			{
				while (true)
				{
					List<ulong> batch = TakeBatch();
					if (batch.Count == 0)
						break;
					FetchBatch(batch);
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "Steam preview download failed: " + ex.Message);
			}
			finally
			{
				Interlocked.Exchange(ref httpBusy, 0);
				bool leftover = false;
				lock (Gate)
				{
					foreach (ulong id in SteamQueued)
					{
						if (!SteamInFlight.Contains(id) && !HasTextureLocked(id))
						{
							leftover = true;
							break;
						}
					}
				}
				if (leftover && Interlocked.CompareExchange(ref httpBusy, 1, 0) == 0)
					ThreadPool.QueueUserWorkItem(_ => HttpWorker());
			}
		}

		private static bool HasTextureLocked(ulong steamId)
		{
			return Memory.TryGetValue("Steam:" + steamId, out CacheEntry entry) && entry.texture != null;
		}

		private static List<ulong> TakeBatch()
		{
			List<ulong> batch = new List<ulong>(SteamBatchSize);
			lock (Gate)
			{
				foreach (ulong id in SteamQueued)
				{
					if (SteamInFlight.Contains(id) || (!ForceHttp.Contains(id) && HasTextureLocked(id)))
						continue;
					SteamInFlight.Add(id);
					batch.Add(id);
					if (batch.Count >= SteamBatchSize)
						break;
				}
			}
			return batch;
		}

		private static void FetchBatch(List<ulong> ids)
		{
			string body = "itemcount=" + ids.Count;
			for (int i = 0; i < ids.Count; i++)
				body += "&publishedfileids[" + i + "]=" + ids[i];

			string json;
			try
			{
				json = PostForm("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/", body);
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "Steam details request failed: " + ex.Message);
				for (int i = 0; i < ids.Count; i++)
					FinishSteam(ids[i], null);
				return;
			}

			Dictionary<ulong, string> urls = ParsePreviewUrls(json);
			for (int i = 0; i < ids.Count; i++)
			{
				ulong id = ids[i];
				if (!urls.TryGetValue(id, out string url) || string.IsNullOrEmpty(url))
				{
					FinishSteam(id, null);
					continue;
				}
				try
				{
					byte[] image = DownloadBytes(url);
					FinishSteam(id, image);
				}
				catch
				{
					FinishSteam(id, null);
				}
			}
		}

		private static Dictionary<ulong, string> ParsePreviewUrls(string json)
		{
			var result = new Dictionary<ulong, string>();
			if (string.IsNullOrEmpty(json))
				return result;

			MatchCollection matches = Regex.Matches(
				json,
				"\"publishedfileid\"\\s*:\\s*\"?(\\d+)\"?[\\s\\S]*?\"preview_url\"\\s*:\\s*\"([^\"]+)\"",
				RegexOptions.CultureInvariant);
			for (int i = 0; i < matches.Count; i++)
			{
				if (!ulong.TryParse(matches[i].Groups[1].Value, out ulong id))
					continue;
				string url = matches[i].Groups[2].Value.Replace("\\/", "/");
				if (!string.IsNullOrEmpty(url))
					result[id] = url;
			}
			return result;
		}

		private static string PostForm(string url, string body)
		{
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
			req.Method = "POST";
			req.ContentType = "application/x-www-form-urlencoded";
			req.UserAgent = "ModListPreviews/1.0";
			req.Timeout = 20000;
			byte[] payload = Encoding.ASCII.GetBytes(body);
			req.ContentLength = payload.Length;
			using (Stream stream = req.GetRequestStream())
				stream.Write(payload, 0, payload.Length);
			using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
			using (StreamReader reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
				return reader.ReadToEnd();
		}

		private static byte[] DownloadBytes(string url)
		{
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
			req.Method = "GET";
			req.UserAgent = "ModListPreviews/1.0";
			req.Timeout = 20000;
			using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
			using (Stream stream = resp.GetResponseStream())
			using (MemoryStream ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				return ms.ToArray();
			}
		}

		private static void FinishSteam(ulong steamId, byte[] bytes)
		{
			lock (Gate)
			{
				SteamInFlight.Remove(steamId);
				SteamQueued.Remove(steamId);
				ForceHttp.Remove(steamId);
			}

			if (bytes != null && bytes.Length > 16)
			{
				SaveDiskCache(steamId, bytes);
				byte[] copy = bytes;
				PreviewPump.Enqueue(() =>
				{
					Texture2D tex = BytesToTexture("steam_" + steamId, copy);
					if (tex != null)
						Store("Steam:" + steamId, tex, true, false);
					else
					{
						MarkFailed(steamId);
						RememberRetry(steamId);
					}
					PreviewListUI.RefreshOpenScreen();
				});
				return;
			}

			RememberRetry(steamId);
			PreviewPump.Enqueue(() =>
			{
				MarkFailed(steamId);
				PreviewListUI.RefreshOpenScreen();
			});
		}

		private static string RetryFilePath()
		{
			if (string.IsNullOrEmpty(cacheDir))
				return null;
			return Path.Combine(cacheDir, "retry.txt");
		}

		private static void LoadRetryFile()
		{
			string path = RetryFilePath();
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
				return;
			try
			{
				string[] lines = File.ReadAllLines(path);
				lock (Gate)
				{
					for (int i = 0; i < lines.Length; i++)
					{
						if (ulong.TryParse(lines[i].Trim(), out ulong id) && id != 0)
							RetryIds.Add(id);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "failed to read retry list: " + ex.Message);
			}
		}

		private static void SaveRetryFile()
		{
			string path = RetryFilePath();
			if (string.IsNullOrEmpty(path))
				return;
			try
			{
				List<string> lines;
				lock (Gate)
				{
					lines = new List<string>(RetryIds.Count);
					foreach (ulong id in RetryIds)
						lines.Add(id.ToString());
				}
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllLines(path, lines.ToArray());
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "failed to save retry list: " + ex.Message);
			}
		}

		private static void ClearDiskCache()
		{
			if (string.IsNullOrEmpty(cacheDir) || !Directory.Exists(cacheDir))
				return;
			try
			{
				string[] files = Directory.GetFiles(cacheDir, "*.img");
				for (int i = 0; i < files.Length; i++)
					File.Delete(files[i]);
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "failed to clear cover cache: " + ex.Message);
			}
		}

		private static void SaveDiskCache(ulong steamId, byte[] bytes)
		{
			if (string.IsNullOrEmpty(cacheDir) || bytes == null)
				return;
			try
			{
				File.WriteAllBytes(Path.Combine(cacheDir, steamId + ".img"), bytes);
			}
			catch
			{
			}
		}

		private static bool IsSteam(KMod.Mod mod)
		{
			return mod != null && mod.label.distribution_platform == Label.DistributionPlatform.Steam;
		}

		private static string KeyOf(KMod.Mod mod)
		{
			if (IsSteam(mod))
				return "Steam:" + mod.label.id;
			return mod.label.distribution_platform + ":" + mod.staticID;
		}
	}

	internal sealed class PreviewPump : MonoBehaviour
	{
		private const int LivePreviewInterval = 18;

		private static PreviewPump instance;
		private static readonly ConcurrentQueue<System.Action> Queue = new ConcurrentQueue<System.Action>();
		private int livePreviewTick;

		public static void Ensure(GameObject host)
		{
			if (host == null)
				return;
			if (instance != null)
				return;
			instance = host.GetComponent<PreviewPump>();
			if (instance == null)
				instance = host.AddComponent<PreviewPump>();
		}

		public static void Enqueue(System.Action action)
		{
			if (action == null)
				return;
			Queue.Enqueue(action);
		}

		public static void EnqueueAfterFrames(int frames, System.Action action)
		{
			if (action == null || instance == null)
				return;
			instance.StartCoroutine(RunAfter(frames, action));
		}

		private static IEnumerator RunAfter(int frames, System.Action action)
		{
			for (int i = 0; i < frames; i++)
				yield return null;
			try
			{
				action();
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "preview apply failed: " + ex.Message);
			}
		}

		private void Update()
		{
			int n = 0;
			while (n < 16 && Queue.TryDequeue(out System.Action action))
			{
				try
				{
					action();
				}
				catch (Exception ex)
				{
					Debug.LogWarning(Mod.LogPrefix + "preview apply failed: " + ex.Message);
				}
				n++;
			}

			if (!PreviewListUI.HasOpenScreen)
				return;
			livePreviewTick++;
			if (livePreviewTick < LivePreviewInterval)
				return;
			livePreviewTick = 0;
			if (PreviewService.AbsorbLiveSteamPreviews() > 0)
				PreviewListUI.RefreshOpenScreen();
		}

		private void OnDestroy()
		{
			if (instance == this)
				instance = null;
		}
	}
}
