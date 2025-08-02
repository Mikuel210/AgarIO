using Spectre.Console;

namespace PaperIO;

partial class Program {
	
	public enum Layer {

		Background = 0,
		Trail = 1,
		Area = 2,
		Player = 3,

	}

	public class Pixel {
		public Vector position;
		public Vector dimensions;

		public int layer;
		public Color color;

		public Pixel(Vector position, Vector dimensions, Layer layer, Color color) {
			this.position = position; 
			this.dimensions = dimensions;
			this.layer = (int)layer;
			this.color = color;
			
			world.AddPixel(this);
		}

		public void Destroy() {
			world.RemovePixel(this);
		}

		public void Translate(int x, int y) => position.Translate(x, y);
		
		public void MoveTo(int x, int y) => position.MoveTo(x, y);
		
		public bool Intersects(Vector other) {
			return other.x >= position.x && other.x < position.x + dimensions.x &&
				   other.y >= position.y && other.y < position.y + dimensions.y;
		}

		public bool Intersects(int x, int y) => Intersects(new Vector(x, y));

		public bool Intersects(Area other) => other.Intersects(this);
		public bool FullyIntersects(Area other) => other.FullyIntersects(this);
		
		public bool Intersects(Pixel other) {
			for (int y = other.position.y; y < other.position.y + other.dimensions.y; y++) {
				for (int x = other.position.x; x < other.position.x + other.dimensions.x; x++) {
					if (Intersects(x, y))
						return true;
				}	
			}

			return false;
		}
		
		// Equality implementation
		public bool Equals(Pixel? other)
		{
			if (other == null)
				return false;

			return position.Equals(other.position) &&
				   dimensions.Equals(other.dimensions) &&
				   layer == other.layer &&
				   color.Equals(other.color);
		}

		public override bool Equals(object? @object) => Equals(@object as Pixel ?? null);

		public override int GetHashCode()
		{
			// Combine hash codes of properties that define equality
			return HashCode.Combine(position, dimensions, layer, color);
		}

	}

}