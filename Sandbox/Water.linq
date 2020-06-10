<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\WPF\PresentationCore.dll</Reference>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Imaging</Namespace>
</Query>

void Main()
{
	using (var wb = (Bitmap)Bitmap.FromFile(@"C:\Source\MapEditor\MapEngine\Content\water.png"))
	{
		GC.Collect();
		GC.GetTotalMemory(true).Dump();
		var water = new Water(wb);
		var dc = new DumpContainer(water).Dump();

		for (int i = 0; i < 200; i++)
		{
			dc.Refresh();
			Thread.Sleep(50);
		}
		GC.Collect();
		GC.GetTotalMemory(true).Dump();
	}
}

public class Water
{
	public Bitmap Bitmap
	{
		get
		{
			_frame = (++_frame) % Width;
			return _bitmapFrames[_frame];
		}
	}

	private int Width;
	private int Height;
	
	private int _frame;
	private List<Bitmap> _bitmapFrames = new List<Bitmap>();
	private List<byte[]> _imageFrames = new List<byte[]>();

	public Water(Bitmap texture)
	{
		Width = texture.Width;
		Height = texture.Height;
		
		var initial = texture;
		var overlay = new Bitmap(Width, Height);
		
		var alpha = 64;
		SetTransperancy(initial, overlay, alpha);
		GenerateFrames(initial, overlay);
	}
	
	private void SetTransperancy(Bitmap initial, Bitmap overlay, int alpha)
	{
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				var colour = initial.GetPixel(x, y);
				var blend = System.Drawing.Color.FromArgb(alpha, colour.R, colour.G, colour.B);
				overlay.SetPixel(x, y, blend);
			}
		}
	}

	private void GenerateFrames(Bitmap initial, Bitmap overlay)
	{
		//		for (int j = 1; j < Width + 1; j++)
		//		{
		//			var bitmap = new Bitmap(initial);
		//			using (var gfx = Graphics.FromImage(bitmap))
		//			{
		//				gfx.DrawImage(initial, 0, 0);
		//				gfx.DrawImage(overlay, j, 0);
		//
		//				var i = Width - j;
		//				var a = overlay.Clone(new Rectangle(i, 0, j, Height), System.Drawing.Imaging.PixelFormat.DontCare);
		//				gfx.DrawImage(a, 0, 0);
		//			}
		//			
		//			var bytes = ImageToByte(bitmap);
		//			_imageFrames.Add(bytes);
		//			
		//			_bitmapFrames.Add(bitmap);
		//		}

		//		for (int j = 1; j < Height + 1; j++)
		//		{
		//			var bitmap = new Bitmap(initial);
		//			using (var gfx = Graphics.FromImage(bitmap))
		//			{
		//				gfx.DrawImage(initial, 0, 0);
		//				gfx.DrawImage(overlay, 0, j);
		//
		//				var i = Height - j;
		//				var a = overlay.Clone(new Rectangle(0, i, Width, j), System.Drawing.Imaging.PixelFormat.DontCare);
		//				gfx.DrawImage(a, 0, 0);
		//			}
		//
		//			var bytes = ImageToByte(bitmap);
		//			_imageFrames.Add(bytes);
		//
		//			_bitmapFrames.Add(bitmap);
		//		}

		for (int j = Height; j > 0; j--)
		{
			var bitmap = new Bitmap(initial);
			using (var gfx = Graphics.FromImage(bitmap))
			{
				gfx.DrawImage(initial, 0, 0);
				gfx.DrawImage(overlay, 0, j);

				var i = Height - j;
				var a = overlay.Clone(new Rectangle(0, i, Width, j), System.Drawing.Imaging.PixelFormat.DontCare);
				gfx.DrawImage(a, 0, 0);
			}

			var bytes = ImageToByte(bitmap);
			_imageFrames.Add(bytes);

			_bitmapFrames.Add(bitmap);
		}
	}

	public static byte[] ImageToByte(Image img)
	{
//		ImageConverter converter = new ImageConverter();
//		return (byte[])converter.ConvertTo(img, typeof(byte[]));

		using (var stream = new MemoryStream())
		{
			img.Save(stream, System.Drawing.Imaging.ImageFormat.Gif);
			return stream.ToArray();
		}
	}
}