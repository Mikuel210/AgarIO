namespace PaperIO;

partial class Program {

	public struct Vector {

		// x = 0 => Left
		// y = 0 => Top
		public int x;
		public int y;

		public Vector(int x, int y) {
			this.x = x;
			this.y = y;
		}

		public Vector Translate(Vector vector) {
			x += vector.x;
			y += vector.y;
			return this;
		}
		public Vector Translate(int x, int y) => Translate(new (x, y));
		
		// Operator overloads
		public static Vector operator +(Vector a, Vector b) => new(a.x + b.x, a.y + b.y);
		public static Vector operator -(Vector a, Vector b) => new(a.x - b.x, a.y - b.y);
		public static Vector operator *(Vector a, int b) => new(a.x * b, a.y * b);
		public static Vector operator /(Vector a, int b) => new(a.x / b, a.y / b);
		public static bool operator ==(Vector left, Vector right) => left.Equals(right);
		public static bool operator !=(Vector left, Vector right) => !left.Equals(right);
	}

}