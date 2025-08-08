using System.Numerics;
using Spectre.Console;

namespace PaperIO;

partial class Program {

	public class Camera {

		public Vector2 dimensions;
		public Vector2 center;
		public Color backgroundColor;
		public float zoom;

		public Camera(Vector2 center, Vector2 dimensions, Color backgroundColor, float zoom) {
			this.center = center;
			this.dimensions = dimensions;
			this.backgroundColor = backgroundColor;
			this.zoom = zoom;
		}

		public void Translate(Vector2 vector) => center += vector;
		public void Translate(float x, float y) => center += new Vector2(x, y);
		
		public (Vector2 topLeft, Vector2 bottomRight) GetBounds()
		{
			Vector2 halfDimensions = dimensions / 2;
			return (center - halfDimensions, center + halfDimensions);
		}
		
		public (Vector2 topLeft, Vector2 bottomRight) GetWorldBounds()
		{
			Vector2 halfDimensions = dimensions * zoom / 2;
			return (center - halfDimensions, center + halfDimensions);
		}

	}

}