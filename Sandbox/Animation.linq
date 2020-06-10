<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\WPF\PresentationCore.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\WindowsBase.dll</Reference>
  <NuGetReference>PresentationFramework</NuGetReference>
  <NuGetReference>WindowsBase</NuGetReference>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Imaging</Namespace>
  <Namespace>System.Windows</Namespace>
  <Namespace>System.Windows.Controls</Namespace>
  <Namespace>System.Windows.Media.Imaging</Namespace>
</Query>

void Main()
{
	using (var wb = (Bitmap)Bitmap.FromFile(@"C:\Source\MapEditor\MapEngine\Content\water.jpg"))
	{
		var animation = new AnimationGenerator(wb);

		var water = new Animation(animation.Frames, animation.Width, animation.Height);
		var image = new System.Windows.Controls.Image
		{
			Width = 400,
			Height = 200,
			Source = water.Image
		};

		//var dc = new DumpContainer(image).Dump();

		//for (int i = 0; i < 200; i++)
		{
			//dc.Refresh();
			image.Dump();
			Thread.Sleep(50);
		}
	}
}

public class Animation
{
	private int _index;
	private List<byte[]> _frames;
	private WriteableBitmap _image;

	public int Width { get; }
	public int Height { get; }
	public WriteableBitmap Image
	{
		get
		{
			_index = (++_index) % Width;
			
			var buffer = _frames[_index];
			var area = new Int32Rect(0, 0, Width, Height);
			_image.WritePixels(area, buffer, Width * 4, 0);
			
			return _image;
		}
	}
	
	public Animation(List<byte[]> frames, int width, int height)
	{
		Width = width;
		Height = height;
		
		_frames = frames;
		_image = new WriteableBitmap(Width, Height, 96, 96, System.Windows.Media.PixelFormats.Bgr32, null);
	}
}

public class AnimationGenerator
{
	public int Width;
	public int Height;
	public List<byte[]> Frames = new List<byte[]>();

	public AnimationGenerator(Bitmap texture)
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
		for (int j = 1; j < Width + 1; j++)
		{
			var bitmap = new Bitmap(initial);
			using (var gfx = Graphics.FromImage(bitmap))
			{
				gfx.DrawImage(initial, 0, 0);
				gfx.DrawImage(overlay, j, 0);

				var i = Width - j;
				var a = overlay.Clone(new Rectangle(i, 0, j, Height), System.Drawing.Imaging.PixelFormat.DontCare);
				gfx.DrawImage(a, 0, 0);
			}
			
			var bytes = ImageToByte(bitmap);
			Frames.Add(bytes);
		}
	}

	public byte[] ImageToByte(System.Drawing.Bitmap img)
	{
		var buffer = new byte[Width * Height * 4];
		for (int y = 0; y < Height; y+=4)
		{
			for (int x = 0; x < Width; x+=4)
			{
				var i = x + (y * Width);
				var colour = img.GetPixel(x, y);
				buffer[i] = colour.B;
				buffer[i+1] = colour.G;
				buffer[i+2] = colour.R;
				buffer[i+3] = colour.A;
			}
		}
		return buffer;
	}
}