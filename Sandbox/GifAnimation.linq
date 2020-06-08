<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\WPF\PresentationCore.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\WindowsBase.dll</Reference>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Imaging</Namespace>
  <Namespace>System.Windows.Media.Imaging</Namespace>
  <Namespace>System.Windows</Namespace>
  <Namespace>System.Data.Objects</Namespace>
  <Namespace>System.Runtime.InteropServices</Namespace>
</Query>

void Main()
{
	using (var wb = (Bitmap)Bitmap.FromFile(@"C:\Source\MapEditor\MapEngine\Content\water.png"))
	{
		var water = new Water(wb);
//		var dc = new DumpContainer(water).Dump();
//
//		for (int i = 0; i < 200; i++)
//		{
//			dc.Refresh();
//			Thread.Sleep(50);
//		}
	}
}

public class Water
{
//	public Bitmap Bitmap
//	{
//		get
//		{
//			_frame = (++_frame) % Width;
//			//return _bitmapFrames[_frame];
//		}
//	}

	private int Width;
	private int Height;
	
	private int _frame;
//	private List<Bitmap> _bitmapFrames = new List<Bitmap>();
//	private List<byte[]> _imageFrames = new List<byte[]>();

	public Water(Bitmap texture)
	{
		Width = texture.Width;
		Height = texture.Height;
		
		var initial = texture;
		var overlay = new Bitmap(Width, Height);
		
		var alpha = 80;
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

	[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DeleteObject([In] IntPtr hObject);

	private void GenerateFrames(Bitmap initial, Bitmap overlay)
	{
		var images = new List<Bitmap>();
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
//			var bytes = ImageToByte(bitmap);
//			_imageFrames.Add(bytes);
//			_bitmapFrames.Add(bitmap);
			images.Add(bitmap);
		}
		CreateGif(@"C:\Source\MapEditor\Sandbox\test.gif", images);
	}
	
	private void CreateGif(string path, List<Bitmap> images)
	{
		var encoder = new GifBitmapEncoder();
		foreach (var img in images)
		{
			var bmp = img.GetHbitmap();
			var src = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
				bmp,
				IntPtr.Zero,
				Int32Rect.Empty,
				BitmapSizeOptions.FromEmptyOptions());
			encoder.Frames.Add(BitmapFrame.Create(src));
			DeleteObject(bmp); // recommended, handle memory leak
		}
		using (FileStream fs = new FileStream(path, FileMode.Create))
		{
			encoder.Save(fs);
		}
	}
}