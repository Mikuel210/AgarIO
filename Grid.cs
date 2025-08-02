using Spectre.Console;

namespace PaperIO;

partial class Program {
	public class Grid {

		public int width;
		public int height;
		
		public List<Pixel> pixels = new();

		public Grid(int width, int height) {
			this.width = width;
			this.height = height;
		}

		public void AddPixel(Pixel pixel) => pixels.Add(pixel);
		public void RemovePixel(Pixel pixel) => pixels.Remove(pixel);

		public List<Pixel> GetPixels(Vector position) => pixels.Where(e => e.Intersects(position)).ToList();
		public List<Pixel> GetPixels(int x, int y) => GetPixels(new(x, y));

		public void Print(Vector topLeft, Vector bottomRight, Color backgroundColor) {
			var canvas = new Canvas(_viewportWidth, _viewportHeight);
			
			for (int y = bottomRight.y - topLeft.y; y >= 0; y--) {
				for (int x = 0; x < bottomRight.x - topLeft.x; x++) {
					if (x >= _viewportWidth || y >= _viewportHeight) continue;
					
					List<Pixel> pixels = GetPixels(x + topLeft.x, y + topLeft.y);

					if (pixels.Count == 0) {
						canvas.SetPixel(x, y, backgroundColor);

						continue;
					}
					
					Pixel pixel = pixels
						.OrderByDescending(p => p.layer)
						.First();
					
					canvas.SetPixel(x, y, pixel.color);
				}
			}
			
			AnsiConsole.Write(canvas);
		}

		public void Print(Camera camera) {
			(Vector bottomLeft, Vector topRight) = camera.GetBounds();
			Print(bottomLeft, topRight, camera.backgroundColor);
		}

	}

}