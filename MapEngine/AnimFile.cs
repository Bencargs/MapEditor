using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace MapEngine;

public sealed class AnimFile
{
    public AnimManifest Manifest { get; }
    public WriteableBitmap[] Frames { get; }

    private AnimFile(AnimManifest manifest, WriteableBitmap[] frames)
    {
        Manifest = manifest;
        Frames = frames;
    }

    public static AnimFile Load(string filename)
    {
        using var fs = File.OpenRead(filename);
        using var zip = new ZipArchive(fs, ZipArchiveMode.Read, leaveOpen: false);

        var manifest = ReadManifest(zip);

        var pages = manifest.Pages.Select(p => LoadBitmapSource(zip, p.Image)).ToArray();

        // Pre-slice frames into WriteableBitmaps (simple + matches your old GIF approach)
        var frames = new WriteableBitmap[manifest.TotalFrames];
        for (int i = 0; i < manifest.TotalFrames; i++)
        {
            var (pageIndex, localIndex) = LocateFrame(manifest, i);

            var page = manifest.Pages[pageIndex];
            var sheet = pages[pageIndex];

            var col = localIndex % page.Columns;
            var row = localIndex / page.Columns;

            var x = col * manifest.FrameW;
            var y = row * manifest.FrameH;

            var crop = new CroppedBitmap(sheet, new Int32Rect(x, y, manifest.FrameW, manifest.FrameH));

            // If you were using the Scale(0.99) workaround for GIF tearing/bug, keep it at callsite.
            var wb = new WriteableBitmap(crop);
            frames[i] = wb;
        }

        return new AnimFile(manifest, frames);
    }

    public int FrameRateMs => (int)Math.Round(1000.0 / Math.Max(1, Manifest.Fps));

    private static AnimManifest ReadManifest(ZipArchive zip)
    {
        var entry = zip.GetEntry("anim.json")
                    ?? throw new InvalidOperationException("anim.json not found in .anim");

        using var s = entry.Open();
        using var reader = new StreamReader(s);
        var json = reader.ReadToEnd();

        var manifest = JsonConvert.DeserializeObject<AnimManifest>(json)
                       ?? throw new InvalidOperationException("Failed to parse anim.json.");

        if (manifest.Version != 1)
            throw new InvalidOperationException($"Unsupported anim.json version: {manifest.Version}");

        if (manifest.Pages is null || manifest.Pages.Length == 0)
            throw new InvalidOperationException("anim.json has no pages.");

        if (manifest.TotalFrames <= 0)
            manifest.TotalFrames = manifest.Pages.Sum(p => p.Count);

        if (manifest.FrameW <= 0 || manifest.FrameH <= 0)
            throw new InvalidOperationException("anim.json frameW/frameH must be > 0.");

        return manifest;
    }

    private static BitmapSource LoadBitmapSource(ZipArchive zip, string entryName)
    {
        var entry = zip.GetEntry(entryName)
                    ?? throw new InvalidOperationException($"Missing image in .anim: {entryName}");

        using var s = entry.Open();
        using var ms = new MemoryStream();
        s.CopyTo(ms);
        ms.Position = 0;

        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.StreamSource = ms;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    private static (int pageIndex, int localIndex) LocateFrame(AnimManifest manifest, int globalFrameIndex)
    {
        var idx = globalFrameIndex;
        for (int p = 0; p < manifest.Pages.Length; p++)
        {
            var count = manifest.Pages[p].Count;
            if (idx < count) return (p, idx);
            idx -= count;
        }

        // fallback: last frame
        return (manifest.Pages.Length - 1, Math.Max(0, manifest.Pages.Last().Count - 1));
    }

    public sealed class AnimManifest
    {
        [JsonProperty("version")] public int Version { get; set; }

        [JsonProperty("frameW")] public int FrameW { get; set; }

        [JsonProperty("frameH")] public int FrameH { get; set; }

        [JsonProperty("fps")] public int Fps { get; set; }

        [JsonProperty("loop")] public bool Loop { get; set; }

        [JsonProperty("totalFrames")] public int TotalFrames { get; set; }

        [JsonProperty("pages")] public Page[] Pages { get; set; } = Array.Empty<Page>();

        public sealed class Page
        {
            [JsonProperty("image")] public string Image { get; set; } = "";

            [JsonProperty("columns")] public int Columns { get; set; }

            [JsonProperty("rows")] public int Rows { get; set; }

            [JsonProperty("count")] public int Count { get; set; }
        }
    }
}