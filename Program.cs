using System.Diagnostics;
using System.Numerics;
using Spectre.Console;

namespace PaperIO;

partial class Program {

	private static readonly int _viewportWidth = Console.BufferWidth - 1;
	private static readonly int _viewportHeight = Console.BufferHeight * 2 - 2;
	
	public static Grid world = new(100, 100);
	
	public static Camera camera = new(
		new(0, 0),
		new(_viewportWidth, _viewportHeight),
		Color.Grey19
	);
	
	// Game loop
	
	static void Main() {
		Start();
		
		const int updatesPerSecond = 20;
		const double msPerUpdate = 1000.0 / updatesPerSecond;

		Stopwatch stopwatch = Stopwatch.StartNew();
		double previousTime = stopwatch.Elapsed.TotalMilliseconds;
		double lag = 0.0;

		while (true)
		{
			if (Console.KeyAvailable)
			{
				ConsoleKeyInfo key = Console.ReadKey(intercept: true); // true = don’t print key
				HandleInput(key);
			}
			
			double currentTime = stopwatch.Elapsed.TotalMilliseconds;
			double elapsed = currentTime - previousTime;
			
			previousTime = currentTime;
			lag += elapsed;

			// Update at a fixed rate
			while (lag >= msPerUpdate)
			{
				Update();
				lag -= msPerUpdate;
			}
			
			Console.SetCursorPosition(0, 0);
			Console.CursorVisible = false;

			Render();
			
			Thread.Sleep(1);
		}
	}

	
	// Game logic

	public static Player player = new(
		pixel: new(
			new(0, 0),
			new(4, 4),
			Layer.Player,
			Color.Blue
		),
		area: new(Color.SkyBlue1),
		trail: new(Color.SkyBlue2),
		direction: new(1, 0)
	);
	
	public static Player enemy = new(
		pixel: new(
			new(64, 0),
			new(4, 4),
			Layer.Player,
			Color.Red
		),
		area: new(Color.Red3),
		trail: new(Color.DarkRed),
		direction: new(1, 0)
	);

	static void Start() {
		camera.CenterAroundPoint(player.position);
	}

	static void Update() {
		player.UpdatePosition();
		enemy.UpdateAI();
		enemy.UpdatePosition();
		player.UpdateCollisionWithEnemy();
		enemy.UpdateCollisionWithPlayer();
		
		Vector distance = player.position - camera.Center;
		int maximumXDistance = _viewportWidth / 2 - 30;
		int maximumYDistance = _viewportHeight / 2 - 15;
		
		if (distance.x > maximumXDistance)
			camera.Translate(distance.x - maximumXDistance, 0);
		if (distance.y > maximumYDistance)
			camera.Translate(0, distance.y - maximumYDistance);
		if (distance.x < -maximumXDistance)
			camera.Translate(distance.x + maximumXDistance, 0);
		if (distance.y < -maximumYDistance)
			camera.Translate(0, distance.y + maximumYDistance);
	}

	static void HandleInput(ConsoleKeyInfo key) {
		if (key.Key == ConsoleKey.LeftArrow)
			player.SetDirection(-1, 0);
		else if (key.Key == ConsoleKey.RightArrow)
			player.SetDirection(1, 0);
		else if (key.Key == ConsoleKey.UpArrow)
			player.SetDirection(0, -1);
		else if (key.Key == ConsoleKey.DownArrow)
			player.SetDirection(0, 1);
	}

	static void Render() {
		world.Print(camera);
	}

}