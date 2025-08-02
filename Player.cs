using System.Diagnostics;
using Spectre.Console;

namespace PaperIO;

partial class Program {
	
	public class Player {

		private Vector _startingPosition;

		public Pixel pixel;
		public Vector position;
		public Vector direction;

		public Area area;
		public Area trail;
		
		public Pixel currentTrailPixel;
		public bool drawingTrail = false;

		public bool dead = false;

		public Player(Pixel pixel, Area area, Area trail, Vector direction) {
			this.pixel = pixel;
			position = pixel.position;
			this._startingPosition = position;
			this.direction = direction;
			
			this.area = area;
			this.trail = trail;
			
			int areaDimensions = 32;
			int areaPosition = -areaDimensions / 2 - pixel.dimensions.x / 2;
			int areaPositionX = areaPosition + position.x;
			int areaPositionY = areaPosition + position.y;
		
			area.AddPixel(new(
				new(areaPositionX, areaPositionY),
				new(areaDimensions, areaDimensions),
				Layer.Area,
				area.color
			));
		}

		private void NewTrailPixel() {
			currentTrailPixel = new(
				position,
				pixel.dimensions,
				Layer.Trail,
				trail.color
			);
			
			trail.AddPixel(currentTrailPixel);
		}

		private List<Vector> GetStartingPositions(int y) {
			int minX = trail.pixels.Min(e => e.position.x);
			int maxX = trail.pixels.Max(e => e.position.x + e.dimensions.x);

			List<Vector> positions = new List<Vector>();
			bool intersectingTrail = false;
			
			for (int x = minX - 1; x <= maxX; x++) {
				if (!intersectingTrail) {
					intersectingTrail = trail.Intersects(x, y) || area.Intersects(x, y);
				}
				else {
					if (trail.Intersects(x, y) || area.Intersects(x, y)) continue;

					positions.Add(new(x, y));
					intersectingTrail = false;
				}
			}
			
			if (positions.Count != 0 && !(trail.Intersects(maxX, y) || area.Intersects(maxX, y)))
				positions.RemoveAt(positions.Count - 1);
			
			return positions;
		}
		
		private void FillAreaFromTrail() {
			int minY = trail.pixels.Min(e => e.position.y);
			int maxY = trail.pixels.Max(e => e.position.y + e.dimensions.y);
			
			List<Pixel> newPixels = new();

			for (int y = minY + pixel.dimensions.y; y <= maxY - pixel.dimensions.y; y++) {
				foreach (Vector position in GetStartingPositions(y)) {
					newPixels.Add(new(
						position,
						new(2, 2),
						Layer.Area, 
						area.color
					));
				}
			}

			foreach (Pixel pixel in newPixels) {
				while (!pixel.Intersects(area) && !pixel.Intersects(trail)) {
					pixel.dimensions.Translate(1, 0);
				}
			}
			
			List<Pixel> pixelsToRemove = new();

			for (int i = 0; i < newPixels.Count; i++) {
				Pixel pixel = newPixels[i];

				if (pixelsToRemove.Contains(pixel)) continue;

				int i2 = i + 1;

				while (i2 < newPixels.Count && 
					   newPixels[i2].position.x == pixel.position.x && 
					   newPixels[i2].dimensions.x == pixel.dimensions.x) {
					pixelsToRemove.Add(newPixels[i2]);
					pixel.dimensions.Translate(0, 1);

					i2++;
				}
			}
			
			foreach (Pixel pixel in pixelsToRemove)
				newPixels.Remove(pixel);
			
			area.pixels.AddRange(newPixels);

			foreach (Pixel pixel in trail.pixels) pixel.color = area.color;
			
			area.pixels.AddRange(trail.pixels);
			trail.pixels = new();
		}

		public void SetDirection(Vector direction) {
			if (this.direction == direction * -1) return;
			if (this.direction == direction) return;
			
			this.direction = direction;
			NewTrailPixel();
		} 
		public void SetDirection(int x, int y) => SetDirection(new(x, y));

		public void UpdatePosition() {
			if (dead) return;
			
			position.x += direction.x;
			position.y += direction.y;
			pixel.position = position;
			
			if (!drawingTrail) {
				if (!pixel.Intersects(area)) {
					drawingTrail = true;
					NewTrailPixel();
				}
			}
			else {
				if (direction == new Vector(-1, 0)) {
					currentTrailPixel.Translate(-1, 0);
					currentTrailPixel.dimensions.Translate(1, 0);
				} else if (direction == new Vector(1, 0)) {
					currentTrailPixel.dimensions.Translate(1, 0);
				} else if (direction == new Vector(0, 1)) {
					currentTrailPixel.dimensions.Translate(0, 1);
				} else if (direction == new Vector(0, -1)) {
					currentTrailPixel.Translate(0, -1);
					currentTrailPixel.dimensions.Translate(0, 1);
				}
				
				if (!pixel.Intersects(area)) return;
				
				drawingTrail = false;
				FillAreaFromTrail();
			}
		}

		public void UpdateCollisionWithPlayer() {
			if (dead) return;
			
			if (pixel.Intersects(player.trail))
				player.Die();
		}
		
		public void UpdateCollisionWithEnemy() {
			if (dead) return;
			
			if (pixel.Intersects(enemy.trail))
				enemy.Die();
		}

		public void Die() {
			pixel.Destroy();

			foreach (Pixel pixel in area.pixels)
				pixel.Destroy();
			
			foreach (Pixel pixel in trail.pixels)
				pixel.Destroy();

			dead = true;
		}

		private Random _random = new();
		private bool _returning = false;

		public void UpdateAI() {
			if (dead) return;
			
			Vector distanceVector = _startingPosition - position;
			Vector optimalDirection = new(0, 0);

			if (Math.Abs(distanceVector.x) > Math.Abs(distanceVector.y)) optimalDirection = new(distanceVector.x > 0 ? 1 : -1, 0);
			else optimalDirection = new(0, distanceVector.y > 0 ? 1 : -1);

			int minDistance = 30;

			if (!_returning && !pixel.Intersects(area) &&
				(Math.Abs(distanceVector.x) > minDistance || Math.Abs(distanceVector.y) > minDistance)) {
				_returning = true;
			}
			if (_returning && pixel.Intersects(area)) _returning = false;
			
			if (_random.Next(0, 5) != 0) return;
			
			Vector direction = new(0, 0);

			if (!_returning) {
				int directionIndex = _random.Next(0, 3);

				switch (directionIndex) {
					case 0: direction = new(1, 0); break;
					case 1: direction = new(0, 1); break;
					case 2: direction = new(-1, 0); break;
					case 3: direction = new(0, -1); break;
				}
			}
			else {
				if (_random.Next(0, 5) != 0) {
					direction = optimalDirection;	
				}
				else {
					int directionIndex = _random.Next(0, 3);

					switch (directionIndex) {
						case 0: direction = new(1, 0); break;
						case 1: direction = new(0, 1); break;
						case 2: direction = new(-1, 0); break;
						case 3: direction = new(0, -1); break;
					}
				}
			}
			
			SetDirection(direction);
		}

	}	

}