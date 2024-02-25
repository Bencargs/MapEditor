<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

void Main()
{
	using (var heightmap = (Bitmap)Bitmap.FromFile(@"C:\Source\MapEditor\Sandbox\BridgeHeightmap.png"))
	using (var texture = (Bitmap)Bitmap.FromFile(@"C:\Source\MapEditor\Sandbox\BridgeTexture.png"))
	using (var output = new Bitmap(texture.Width, texture.Height))
	{
		//var sunDirection = new Vector2(-0.05f, 0.05f); // Example sun direction
		//var sunDirection = new Vector2(-1, 1); // Example sun direction
		var sunDirection = GetSunDirectionVector(10);
		//var sunDirection = new Vector2(-1f, 1f); // Example sun direction
		float shadowIntensity = 0.5f;
		float shadowLengthMultiplier = 0.05f;

		for (int x = 0; x < heightmap.Width; x++)
		{
			for (int y = 0; y < heightmap.Height; y++)
			{
				if (IsInShadow(x, y, heightmap, sunDirection, shadowLengthMultiplier))
				{
					// Darken the texture pixel if it's in shadow
					Color originalColor = texture.GetPixel(x, y);
					Color shadowColor = DarkenColor(originalColor, shadowIntensity); // Example shadow intensity
					output.SetPixel(x, y, shadowColor);
				}
				else
				{
					// No shadow, copy the pixel directly
					output.SetPixel(x, y, texture.GetPixel(x, y));
				}
			}
		}
		output.Dump();
	}
}

public Vector2 GetSunDirectionVector(int hour)
{
	// Define key times of the day.
	float morningStart = 6f;  // 6 AM
	float midday = 12f;       // 12 PM
	float eveningEnd = 18f;   // 6 PM

	// Initialize the sun direction vector.
	Vector2 sunDirection = new Vector2(-1, 0);

	if (hour >= morningStart && hour < midday)
	{
		// Interpolate from (-1, 1) at 6 AM to (0, 0) at midday.
		float progress = (hour - morningStart) / (midday - morningStart);
		sunDirection = new Vector2(-1, 5 - (5 * progress));
	}
	else if (hour > midday && hour <= eveningEnd)
	{
		// Interpolate from (0, 0) at midday to (1, -1) at 6 PM.
		float progress = (hour - midday) / (eveningEnd - midday);
		sunDirection = new Vector2(-1, (5 * -progress));
	}
	// From 6 PM to 6 AM, sunDirection remains at (0, 0), as initialized.

	return sunDirection;
}


bool IsInShadow(int x, int y, Bitmap heightmap, Vector2 sunDirection, float shadowLengthMultiplier)
{
	float currentHeight = GetHeightAt(x, y, heightmap);

	// Calculate the initial step increments based on the sun direction.
	float stepX = sunDirection.X;
	float stepY = sunDirection.Y;

	// Determine how far we should check for shadows based on the multiplier.
	float maxDistance = Math.Max(heightmap.Width, heightmap.Height) * shadowLengthMultiplier;
	// Use the vector's magnitude to influence the checking distance for shadows.
	//var a = sunDirection.Length();
	//float magnitude = (float)Math.Sqrt(stepX * stepX + stepY * stepY);
	//float maxDistance = Math.Max(heightmap.Width, heightmap.Height) * sunDirection.Length(); // Directly use magnitude as the distance factor.


	float distanceChecked = 0;

	while (distanceChecked < maxDistance)
	{
		x += (int)stepX;
		y += (int)stepY;

		// Break if outside heightmap boundaries.
		if (x < 0 || x >= heightmap.Width || y < 0 || y >= heightmap.Height)
		{
			break;
		}

		float heightAtCurrentStep = GetHeightAt(x, y, heightmap);

		if (heightAtCurrentStep > currentHeight)
		{
			return true; // The point is in shadow.
		}

		// Increment the distance checked based on the step increments.
		//distanceChecked += (float) Math.Sqrt(stepX * stepX + stepY * stepY);
		distanceChecked += 1;
	}

	return false; // No higher terrain was found, the point is not in shadow.
}

float GetHeightAt(int x, int y, Bitmap heightmap)
{
	// Assuming heightmap is grayscale, where pixel brightness indicates height.
	Color pixelColor = heightmap.GetPixel(x, y);
	// Convert to grayscale by averaging the RGB components; this is a simplification.
	return (pixelColor.R + pixelColor.G + pixelColor.B) / 3.0f;
}

Color DarkenColor(Color color, double darknessFactor)
{
	int r = (int)(color.R * darknessFactor);
	int g = (int)(color.G * darknessFactor);
	int b = (int)(color.B * darknessFactor);
	return Color.FromArgb(r, g, b);
}