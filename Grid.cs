using System.Numerics;
using Spectre.Console;

namespace PaperIO;

partial class Program {
	public class Grid
	{
		public List<Circle> circles = new();
		public List<Circle> food = new();
		public List<Player> enemies = new();
		public List<Digit> digits = new();
		
		private List<Player> enemiesToRemove = new();

		public void AddCircle(Circle circle) => circles.Add(circle);
		public void RemoveCircle(Circle circle) => circles.Remove(circle);

		public List<Circle> GetCircles(Vector2 position) => circles.Where(e => e.Intersects(position)).ToList();
		public List<Circle> GetCircles(float x, float y) => GetCircles(new(x, y));

		public void Print(Camera camera)
		{
			if (gameState != GameState.Playing) return;
			
			var canvas = new Canvas(_viewportWidth, _viewportHeight);

			(Vector2 worldTopLeft, Vector2 worldBottomRight) = camera.GetWorldBounds();
			Color backgroundColor = camera.backgroundColor;
			float zoom = camera.zoom;
			
			float worldY = worldBottomRight.Y + zoom;
			
			for (int y = (int)camera.dimensions.Y; y >= 0; y--) {
				worldY -= zoom;
				float worldX = worldTopLeft.X - zoom;
				
				for (int x = 0; x < camera.dimensions.X; x++) {
					worldX += zoom;
					
					if (x >= _viewportWidth || y >= _viewportHeight) continue;
					
					List<Circle> circles = GetCircles(worldX, worldY);

					if (circles.Count == 0) {
						canvas.SetPixel(x, y, backgroundColor);

						continue;
					}
					
					Circle circle = circles
						.OrderByDescending(e => e.layer)
						.ThenByDescending(e => e.radius)
						.First();
					
					canvas.SetPixel(x, y, circle.color);
				}
			}

			foreach (Digit digit in digits)
			{
				List<Vector2> pixels = digit.GetPixels();
				
				foreach (Vector2 pixel in pixels)
					canvas.SetPixel((int)pixel.X, (int)pixel.Y, ConsoleColor.White);
			}
			
			AnsiConsole.Write(canvas);
		}

		public void RemoveEnemy(Player enemy) => enemiesToRemove.Add(enemy);

		public void UpdateEnemies()
		{
			foreach (Player enemy in enemiesToRemove)
				enemies.Remove(enemy);
			
			foreach (Player enemy in enemies)
			{
				if (Vector2.Distance(enemy.Center, player.Center) > Math.Max(player.circle.radius * 10f, 150f))
					enemy.Die();

				if (enemy.circle.radius > player.circle.radius)
					enemy.circle.color = Color.Red3_1;
				else
					enemy.circle.color = Color.Red1;
				
				enemy.UpdateAI();
				enemy.Update();
			}
			
			List<Circle> foodToRemove = new();
			
			foreach (Circle circle in food)
			{
				if (Vector2.Distance(circle.center, player.Center) < Math.Max(player.circle.radius * 8f, 150f)) continue;
				
				circle.Destroy();
				foodToRemove.Add(circle);
			}

			foreach (Circle circle in foodToRemove)
				food.Remove(circle);
		}

		private Vector2 GetPositionOutsideView(Camera camera, int margin)
		{
			(Vector2 topLeft, Vector2 bottomRight) = camera.GetWorldBounds();

			int xPosition = 0;
			int yPosition = 0;
			
			int random = _random.Next(0, 4);

			if (random == 0)
			{
				xPosition = (int)topLeft.X - margin;
				yPosition = _random.Next((int)topLeft.Y, (int)bottomRight.Y);
			} else if (random == 1)
			{
				xPosition = (int)bottomRight.X + margin;
				yPosition = _random.Next((int)topLeft.Y, (int)bottomRight.Y);	
			} else if (random == 2)
			{
				xPosition = _random.Next((int)topLeft.X, (int)bottomRight.X);
				yPosition = (int)topLeft.Y - margin;
			} else if (random == 3)
			{
				xPosition = _random.Next((int)topLeft.X, (int)bottomRight.X);
				yPosition = (int)bottomRight.Y + margin;
			}
			
			return new Vector2(xPosition, yPosition);
		}
		
		public void SpawnFood(Camera camera)
		{
			Vector2 position = GetPositionOutsideView(camera, 5);	
			float size = _random.Next(2, 4) + camera.zoom;
			
			Circle circle = new(
				position,
				size,
				Layer.Food,
				Color.Yellow1
			);
			
			foreach (Player enemy in world.enemies)
				if (circle.Intersects(enemy.circle))
				{
					circle.Destroy();
					return;
				}
			
			food.Add(circle);
		}

		public void SpawnEnemy(Camera camera)
		{
			float size = player.circle.radius * _random.Next(5, 15) / 10f;
			Vector2 position = GetPositionOutsideView(camera, (int)Math.Ceiling(size) + 5);

			foreach (Player other in world.enemies)
			{
				if (!other.circle.Intersects(position)) continue;
				
				SpawnEnemy(camera);
				return;
			}

			Player enemy = new(
				new(
					position,
					size,
					Layer.Enemy,
					Color.Red1
				),
				new(1, 0)
			);
			
			enemies.Add(enemy);
		}

	}

}