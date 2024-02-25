<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Imaging</Namespace>
</Query>

void Main()
{
	var rng = new Random();
	var background = new Bitmap(600, 800);
	var dc = new DumpContainer(background).Dump();
	
	using (var p = (Bitmap) Bitmap.FromFile(@"C:\Source\MapEditor\Sandbox\ExplosionPalette.png"))
	using (var img = (Bitmap) Bitmap.FromFile(@"C:\Source\MapEditor\Sandbox\smoke1.png"))
	using (var bmp = Graphics.FromImage(background))
	{
		using (var si = new Bitmap(200, 200))
		using (var sg = Graphics.FromImage(si))
		for (int i = 0; i < Range; i++)
		{
			sg.DrawImage(img, 0, 0);
			var sprite = new Particle
			{
				Image = si,
				X = 50,
				Y = 25,
				ColourIndex = i
			};

			var colour = GetPalette(p, i);
			ChangeHue(sprite.Image, colour);

			bmp.DrawImage(sprite.Image, sprite.X, sprite.Y);
			dc.Content = background;
			dc.Refresh();
			
			Thread.Sleep(100);
		}
	}
}

private const int Range = 80;

private Color GetPalette(Bitmap palette, int index)
{
	var low = (int) Math.Floor(((float) index / Range) * palette.Height);
	var high = Math.Min(palette.Height - 1, low + 1);
	var lowColour = palette.GetPixel(0, low);
	var highColour = palette.GetPixel(0, high);
	
	var interpolation = (index % ((float) Range / palette.Height)) / 10;

	return ColorInterpolator.InterpolateBetween(lowColour, highColour, interpolation);
}

private void ChangeHue(Bitmap bmp, Color colour)
{
	for (int x = 0; x < bmp.Width; x++)
	{
		for (int y = 0; y < bmp.Height; y++)
		{
			var existing = bmp.GetPixel(x, y);

			var intensity = (float)existing.R / 256;
			if (intensity != 0)
			{
				var newColour = Color.FromArgb(
					(int)(colour.R * intensity),
					(int)(colour.G * intensity),
					(int)(colour.B * intensity));
				bmp.SetPixel(x, y, newColour);
			}
		}
	}
}

public class Particle
{
	public Bitmap Image {get; set;}
	public int X {get; set;}
	public int Y { get; set; }
	public int ColourIndex { get; set;}
	//public int FrameIndex { get; set;}
}

public static class ColorInterpolator
{
	delegate byte ComponentSelector(Color color);
	static ComponentSelector _redSelector = color => color.R;
	static ComponentSelector _greenSelector = color => color.G;
	static ComponentSelector _blueSelector = color => color.B;

	public static Color InterpolateBetween(
		Color endPoint1,
		Color endPoint2,
		double lambda)
	{
		if (lambda < 0 || lambda > 1)
		{
			throw new ArgumentOutOfRangeException("lambda");
		}
		Color color = Color.FromArgb(
			InterpolateComponent(endPoint1, endPoint2, lambda, _redSelector),
			InterpolateComponent(endPoint1, endPoint2, lambda, _greenSelector),
			InterpolateComponent(endPoint1, endPoint2, lambda, _blueSelector)
		);

		return color;
	}

	static byte InterpolateComponent(
		Color endPoint1,
		Color endPoint2,
		double lambda,
		ComponentSelector selector)
	{
		return (byte)(selector(endPoint1)
			+ (selector(endPoint2) - selector(endPoint1)) * lambda);
	}
}
