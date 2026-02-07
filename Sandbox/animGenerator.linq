<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Imaging</Namespace>
  <Namespace>System.IO.Compression</Namespace>
  <Namespace>System.Runtime.InteropServices</Namespace>
  <Namespace>System.Text.Json</Namespace>
</Query>

void Main()
{
    using var wb = (Bitmap)Bitmap.FromFile(@"C:\src\MapEditor\Sandbox\waves3.png");
    var water = new Water(wb);

    water.SaveAnim(
        outputPath: @"C:\src\MapEditor\Sandbox\Waves.anim",
        clipName: "Waves",
        fps: 15,
        loop: true,
        maxSheetSize: 8192 // safe-ish. You can use 16384 if you want.
    );

    "Done".Dump();
}

public sealed class Water
{
    private readonly int _w;
    private readonly int _h;
    private readonly List<Bitmap> _frames = new();

    public Water(Bitmap texture)
    {
        _w = texture.Width;
        _h = texture.Height;

        using var initial = Force32bppArgb(texture);
        using var overlay = CreateAlphaOverlay(initial, alpha: 40);

        GenerateFrames(overlay, overlay);
    }

    public void SaveAnim(string outputPath, string clipName, int fps, bool loop, int maxSheetSize)
    {
        if (_frames.Count == 0)
            throw new InvalidOperationException("No frames were generated.");

        // Pack frames into one or more grid pages (sheet_0.png, sheet_1.png, ...)
        var pages = BuildPagedSheets(_frames, maxSheetSize);

        // Tiny manifest. No per-frame list.
        var manifestJson = BuildManifestJson(
            clipName: clipName,
            fps: fps,
            loop: loop,
            frameW: _w,
            frameH: _h,
            totalFrames: _frames.Count,
            pages: pages
        );

        WritePack(outputPath, pages, manifestJson);
    }

    // -----------------------------
    // Frame generation (your waves)
    // -----------------------------

    private void GenerateFrames(Bitmap initial, Bitmap overlay)
    {
        // Your original loop effectively made ~height frames.
        // Keep it, but you can change step if you want fewer frames.
        for (int j = _w; j > 0; j -= 2)
        {
            var frame = new Bitmap(_w, _h, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(frame))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;

				// ShiftWrapY(g, initial, Math.Max(1, j));
				//  ShiftWrapY(g, overlay, j * 2);
				ShiftWrapX(g, initial, Math.Max(1, j));
				ShiftWrapX(g, overlay, j * 2);
			}

            _frames.Add(frame);
        }
    }

    private void ShiftWrapY(Graphics g, Bitmap img, int shiftY)
    {
        if (img.Height <= 0) return;

        shiftY %= img.Height;
        if (shiftY < 0) shiftY += img.Height;
        if (shiftY == 0) shiftY = 1;

        // draw shifted down
        g.DrawImage(img, 0, shiftY);

        // wrap bottom slice to top
        int sliceH = shiftY;
        int bottomY = _h - shiftY;

        using var slice = img.Clone(new Rectangle(0, bottomY, _w, sliceH), PixelFormat.Format32bppArgb);
        g.DrawImage(slice, 0, 0);
    }

	private void ShiftWrapX(Graphics g, Bitmap img, int shiftX)
	{
		if (img.Width <= 0) return;

		shiftX %= img.Width;
		if (shiftX < 0) shiftX += img.Width;
		if (shiftX == 0) { g.DrawImage(img, 0, 0); return; }

		// Draw shifted LEFT
		g.DrawImage(img, -shiftX, 0);

		// Wrap the leftmost shiftX columns to the RIGHT
		using var slice = img.Clone(
			new Rectangle(0, 0, shiftX, img.Height),
			PixelFormat.Format32bppArgb);

		g.DrawImage(slice, img.Width - shiftX, 0);
	}

	// -----------------------------
    // FAST alpha overlay (LockBits)
    // -----------------------------

    private static Bitmap CreateAlphaOverlay(Bitmap src32bppArgb, byte alpha)
    {
        if (src32bppArgb.PixelFormat != PixelFormat.Format32bppArgb)
            throw new InvalidOperationException("Expected Format32bppArgb.");

        var dst = new Bitmap(src32bppArgb.Width, src32bppArgb.Height, PixelFormat.Format32bppArgb);

        var rect = new Rectangle(0, 0, src32bppArgb.Width, src32bppArgb.Height);

        var srcData = src32bppArgb.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var dstData = dst.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        try
        {
            int srcStride = srcData.Stride;
            int dstStride = dstData.Stride;
            int h = src32bppArgb.Height;
            int wBytes = src32bppArgb.Width * 4;

            var srcRow = new byte[wBytes];
            var dstRow = new byte[wBytes];

            for (int y = 0; y < h; y++)
            {
                IntPtr srcPtr = srcData.Scan0 + y * srcStride;
                IntPtr dstPtr = dstData.Scan0 + y * dstStride;

                Marshal.Copy(srcPtr, srcRow, 0, wBytes);

                // BGRA in 32bppArgb
                for (int x = 0; x < wBytes; x += 4)
                {
                    dstRow[x + 0] = srcRow[x + 0]; // B
                    dstRow[x + 1] = srcRow[x + 1]; // G
                    dstRow[x + 2] = srcRow[x + 2]; // R
                    dstRow[x + 3] = alpha;         // A (fixed)
                }

                Marshal.Copy(dstRow, 0, dstPtr, wBytes);
            }
        }
        finally
        {
            src32bppArgb.UnlockBits(srcData);
            dst.UnlockBits(dstData);
        }

        return dst;
    }

    private static Bitmap Force32bppArgb(Bitmap src)
    {
        if (src.PixelFormat == PixelFormat.Format32bppArgb)
            return (Bitmap)src.Clone();

        var clone = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(clone);
        g.DrawImage(src, 0, 0, src.Width, src.Height);
        return clone;
    }

    // -----------------------------
    // Sheet packing (paged grid)
    // -----------------------------

    public sealed class SheetPage
    {
        public string ImageName { get; init; } = "";
        public Bitmap Sheet { get; init; } = null!;
        public int Columns { get; init; }
        public int Rows { get; init; }
        public int Count { get; init; } // frames in this page
    }

    private static List<SheetPage> BuildPagedSheets(List<Bitmap> frames, int maxSheetSize)
    {
        if (frames.Count == 0) throw new InvalidOperationException("No frames.");

        int frameW = frames[0].Width;
        int frameH = frames[0].Height;

        for (int i = 1; i < frames.Count; i++)
            if (frames[i].Width != frameW || frames[i].Height != frameH)
                throw new InvalidOperationException("All frames must have the same size.");

        // How many columns/rows can fit within maxSheetSize?
        int maxCols = Math.Max(1, maxSheetSize / frameW);
        int maxRows = Math.Max(1, maxSheetSize / frameH);

        int framesPerPage = maxCols * maxRows;
        if (framesPerPage <= 0)
            throw new InvalidOperationException("Invalid maxSheetSize relative to frame size.");

        var pages = new List<SheetPage>();
        int pageIndex = 0;

        for (int start = 0; start < frames.Count; start += framesPerPage, pageIndex++)
        {
            int count = Math.Min(framesPerPage, frames.Count - start);

            int cols = Math.Min(maxCols, count);
            int rows = (int)Math.Ceiling(count / (double)cols);

            int sheetW = cols * frameW;
            int sheetH = rows * frameH;

            // Guard: GDI+ often hates > ~32767px dimensions, but our maxSheetSize should prevent that.
            if (sheetW > maxSheetSize || sheetH > maxSheetSize)
                throw new InvalidOperationException($"Sheet size exceeds maxSheetSize: {sheetW}x{sheetH}");

            var sheet = new Bitmap(sheetW, sheetH, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(sheet))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;

                for (int i = 0; i < count; i++)
                {
                    int globalIndex = start + i;
                    int col = i % cols;
                    int row = i / cols;
                    g.DrawImage(frames[globalIndex], col * frameW, row * frameH);
                }
            }

            pages.Add(new SheetPage
            {
                ImageName = $"sheet_{pageIndex}.png",
                Sheet = sheet,
                Columns = cols,
                Rows = rows,
                Count = count
            });
        }

        return pages;
    }

    // -----------------------------
    // Manifest + pack writing
    // -----------------------------

    private static string BuildManifestJson(
        string clipName,
        int fps,
        bool loop,
        int frameW,
        int frameH,
        int totalFrames,
        List<SheetPage> pages)
    {
        var manifest = new
        {
            version = 1,
            frameW,
            frameH,
            fps,
            loop,

            // optional, but nice for debugging
            totalFrames,

            // pages in order; global frame index maps by walking counts
            pages = pages.Select(p => new
            {
                image = p.ImageName,
                columns = p.Columns,
                rows = p.Rows,
                count = p.Count
            }).ToList(),

            // keep clip name without a huge schema
            clip = new { name = clipName, start = 0, count = totalFrames }
        };

        return JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
    }

    private static void WritePack(string outputPath, List<SheetPage> pages, string manifestJson)
    {
        if (File.Exists(outputPath))
            File.Delete(outputPath);

        using var fs = new FileStream(outputPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
        using var zip = new ZipArchive(fs, ZipArchiveMode.Create, leaveOpen: false);

        // Write all sheets
        foreach (var page in pages)
        {
            var entry = zip.CreateEntry(page.ImageName, CompressionLevel.NoCompression);

            // IMPORTANT: Save to MemoryStream first (GDI+ hates some non-seekable streams)
            using var entryStream = entry.Open();
            using var ms = new MemoryStream();
            page.Sheet.Save(ms, ImageFormat.Png);
            ms.Position = 0;
            ms.CopyTo(entryStream);
        }

        // anim.json (compresses extremely well)
        var jsonEntry = zip.CreateEntry("anim.json", CompressionLevel.Optimal);
        using (var entryStream = jsonEntry.Open())
        using (var writer = new StreamWriter(entryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
        {
            writer.Write(manifestJson);
        }

        // Clean up bitmaps we created for sheets
        foreach (var p in pages)
            p.Sheet.Dispose();
    }
}
