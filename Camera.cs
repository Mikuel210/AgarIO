using Spectre.Console;

namespace PaperIO;

partial class Program {

	public class Camera {

		public Vector dimensions;
		public Vector position;

		public Vector Center => new(
			position.x + dimensions.x / 2,
			position.y + dimensions.y / 2
		);
		
		public Color backgroundColor;

		public Camera(Vector position, Vector dimensions, Color backgroundColor) {
			this.position = position;
			this.dimensions = dimensions;
			
			this.backgroundColor = backgroundColor;
		}

		public void Translate(Vector vector) => position.Translate(vector);
		public void Translate(int x, int y) => position.Translate(x, y);

		public void MoveTo(Vector vector) => position.MoveTo(vector);
		public void MoveTo(int x, int y) => position.MoveTo(x, y);

		public void CenterAroundPoint(Vector vector) {
			MoveTo(new(
				vector.x - dimensions.x / 2,
				vector.y - dimensions.y / 2
			));
		}

		public (Vector bottomLeft, Vector topRight) GetBounds() {
			return (position, new(position.x + dimensions.x, position.y + dimensions.y));
		}

	}

}