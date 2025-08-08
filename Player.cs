using System.Diagnostics;
using System.Numerics;
using Spectre.Console;
using System.Timers;

namespace PaperIO;

partial class Program {
	
	public class Player
	{

		public Circle circle;

		public Vector2 Center
		{
			get => circle.center;
			set => circle.center = value;
		} 
		
		public Vector2 direction;
		public float speed;
		public bool dead;

		public Player(Circle circle, Vector2 direction, float speed = 0.3f) {
			this.circle = circle;
			this.direction = direction;
			this.speed = speed;
		}

		public void SetDirection(Vector2 direction) => this.direction = direction;
		public void SetDirection(int x, int y) => SetDirection(new(x, y));

		public bool IntersectsPlayer(Player player) => circle.Intersects(player.Center);

		public void Update()
		{
			if (dead) return;
			
			Center += new Vector2(direction.X, direction.Y) * speed * globalSpeedMultiplier;
			
			foreach (Circle food in world.food)
			{
				if (!food.Intersects(circle)) continue;
			
				food.Destroy();
				circle.radius += Math.Clamp(0.01f - circle.radius / 10_000f, 0.001f, 0.01f);
			}
			
			List<Player> otherPlayers = new List<Player>(world.enemies);
			otherPlayers.Add(player);
			otherPlayers.Remove(this);

			foreach (Player player in otherPlayers)
			{
				if (player.dead) continue;
				
				if (IntersectsPlayer(player))
				{
					if (circle.radius > player.circle.radius)
					{
						player.Die();
						circle.radius += player.circle.radius / 3f;
					}
					else
						Die();
				}
			}
		}

		public void Die()
		{
			dead = true;
			circle.Destroy();
			world.RemoveEnemy(this);

			if (this == player)
			{
				System.Timers.Timer timer = new(2000);

				timer.Elapsed += (_, _) =>
				{
					timer.Stop();
					
					AnsiConsole.Clear();
					
					gameState = GameState.GameOver;
					StartGameOver();
				};
				
				timer.Start();
			}
		}

		private Vector2 GetDirectionFromDistance(Vector2 distance)
		{
			if (Math.Abs(distance.X) > Math.Abs(distance.Y))
			{
				if (distance.X > 0)
					return new(1, 0);
				
				return new(-1, 0);
			}
			
			if (distance.Y > 0)
				return new(0, 1);

			return new(0, -1);
		}

		private Vector2 GetRandomDirection()
		{
			int directionNumber = _random.Next(1, 4);

			switch (directionNumber)
			{
				case 1: return new(0, 1);
				case 2: return new(0, -1);
				case 3: return new(1, 0);
				case 4: return new(-1, 0);
			}
			
			return new(0, 0);
		}

		public void UpdateAI()
		{
			List<Player> playersNearby = new List<Player>();
			
			if (!player.dead) playersNearby.Add(player);
			playersNearby.AddRange(world.enemies);
			playersNearby.Remove(this);

			playersNearby = playersNearby.Where(e => 
				Vector2.Distance(Center, e.Center) < circle.radius * 3 &&
				!player.dead
			).ToList();

			Vector2 direction = this.direction;

			if (playersNearby.Count == 0)
			{
				if (_random.Next(0, 30) != 0) return;
				direction = GetRandomDirection();
			}
			else
			{
				Player nearestPlayer = playersNearby.OrderBy(e => Vector2.Distance(Center, e.Center)).First();
				Vector2 distance = nearestPlayer.Center - Center;
				
				direction = GetDirectionFromDistance(distance);

				if (nearestPlayer.circle.radius > circle.radius)
					direction *= -1;
			}
			
			SetDirection(direction);
		}

	}	

}