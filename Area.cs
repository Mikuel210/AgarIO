using Spectre.Console;

namespace PaperIO;

partial class Program {

	public class Area {
		
		public List<Pixel> pixels = new();
		public Color color;
		
		public Area(Color color) => this.color = color;

		public void AddPixel(Pixel pixel) => pixels.Add(pixel);
		public void RemovePixel(Pixel pixel) => pixels.Remove(pixel);

		public bool Intersects(Area other) {
			foreach (Pixel pixel in pixels) {
				foreach (Pixel otherPixel in other.pixels) {
					if (pixel.Intersects(otherPixel))
						return true;
				}
			}
			
			return false;
		}

		public bool Intersects(Pixel other) {
			foreach (Pixel pixel in pixels) {
				if (pixel.Intersects(other))
					return true;
			}
			
			return false;
		}
		
		public bool FullyIntersects(Pixel other) {
			foreach (Pixel pixel in pixels) {
				if (!pixel.Intersects(other))
					return false;
			}
			
			return true;
		}

		public bool Intersects(Vector position) {
			foreach (Pixel pixel in pixels) {
				if (pixel.Intersects(position))
					return true;
			}
			
			return false;
		}

		public bool Intersects(int x, int y) => Intersects(new Vector(x, y));

	}	

}