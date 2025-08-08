using System.Numerics;
using Spectre.Console;

namespace PaperIO;

partial class Program {
	
	public enum Layer {
		Background = 0,
		Food = 1,
		Enemy = 2,
		Player = 3
	}

	public class Circle {
		public Vector2 center;
		public float radius;

		public int layer;
		public Color color;

		public Circle(Vector2 center, float radius, Layer layer, Color color) {
			this.center = center; 
			this.radius = radius;
			this.layer = (int)layer;
			this.color = color;
			
			world.AddCircle(this);
		}

		public void Destroy() => world.RemoveCircle(this);

		public void Translate(Vector2 vector) => center += vector;
		public void Translate(float x, float y) => center += new Vector2(x, y);
		
		public bool Intersects(Vector2 other) {
			float distance = Vector2.Distance(center, other);
			return distance < radius;
		}

		public bool Intersects(int x, int y) => Intersects(new Vector2(x, y));
		
		public bool Intersects(Circle other) {
			float distance = Vector2.Distance(center, other.center);
			return distance < radius + other.radius;
		}
	}

}