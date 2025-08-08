using System.Diagnostics;
using System.Numerics;
using System.Media;
using Spectre.Console;

namespace PaperIO;

partial class Program {

	private static int _viewportWidth = Console.BufferWidth - 1;
	private static int _viewportHeight = Console.BufferHeight * 2 - 1;

	public static Grid world;
	
	public static Camera camera;

	private static int _animationFrames = 5;
	private static int _animationFrame = 0;
	
	// Game loop
	
	static void Main() {
		Start();
		
		const int updatesPerSecond = 60;
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
	
	private static Random _random = new();
	private static Vector2 _offset;
	private static Vector2 _movementOffset;
	private static Vector2 _targetMovementOffset;
	private static float _movementOffsetSmoothing = 0.1f; // Smaller = smoother

	private static float _targetZoom;
	private static float _targetZoomSmoothing = 0.1f;

	public static float globalSpeedMultiplier = 1f;

	public static Player player;
	
	private static List<Color> _animation = new()
	{
		Color.Cyan1,
		Color.Turquoise2,
		Color.DeepSkyBlue1,
		Color.DodgerBlue1,
		Color.Blue,
	};
	
	public static SoundPlayer musicPlayer = new();
	
	public enum GameState
	{
		StartMenu,
		HowToPlay,
		Playing,
		GameOver
	}
	
	public static GameState gameState = GameState.StartMenu;

	private static int _score;
	private static int _highScoreThisGame;
	private static int _highScoreAllTime;

	static void Start()
	{
		try
		{
			musicPlayer.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Soundtrack.wav";
			musicPlayer.PlayLooping();	
		} catch { }
		
		_highScoreAllTime = PlayerPrefs.Get();
		StartStartMenu();
	}

	static void StartStartMenu()
	{
		AnsiConsole.Clear();
		
		AnsiConsole.Write(new Panel(
			Align.Center(
				new FigletText("AGAR  I/O")
					.Color(Color.Blue)
			)
		));
		
		AnsiConsole.WriteLine('\n');
		
		AnsiConsole.WriteLine("Use fullscreen for a better experience");

		AnsiConsole.Markup(@"
Use the [yellow]number keys[/] to choose

[yellow]1.[/] Play
[yellow]2.[/] Exit

");
	}

	static void StartHowToPlay()
	{
		AnsiConsole.Clear();
		
		AnsiConsole.Write(new Panel(
			Align.Center(
				new FigletText("AGAR  I/O")
					.Color(Color.Blue)
			)
		));
		
		AnsiConsole.Markup(@"

[blue]How to play:[/]

- Use [blue]WASD[/] or [blue]arrow keys[/] to move your cell
- Eat [blue]agar[/] (yellow dots) to grow
- Press the same direction key twice to [blue]move faster[/]. Be careful though, as you'll lose size!
- You can eat smaller cells (red circles) to grow
- Larger cells (dark red circles) can eat you. Stay away!
");
		
		AnsiConsole.Markup(@"
Use the [yellow]number keys[/] to choose

[yellow]1.[/] Play
[yellow]2.[/] Back

");
	}

	private static int _previousHighScore;
	
	static void StartPlaying()
	{
		_previousHighScore = _highScoreAllTime;
		
		world = new();
		
		camera = new(
			new(0, 0),
			new(_viewportWidth, _viewportHeight),
			Color.Grey19,
			1
		);
		
		player = new(
			circle: new(
				new(0, 0),
				5,
				Layer.Player,
				Color.Blue
			),
			new(1, 0),
			0.35f
		);
		
		_offset = new();
		_movementOffset = new();
	    _targetMovementOffset = new();
	    _targetZoom = Math.Max(0.05f + player.circle.radius / 30f, 0.75f);
		camera.zoom = _targetZoom;
		
		AnsiConsole.Clear();
		
		UpdateCamera();
	}

	static void StartGameOver()
	{
		world = new();
		
		for (int i = 0; i < 10; i++) // uhhh if it works it works
		{
			AnsiConsole.Clear();
			AnsiConsole.Clear();
			AnsiConsole.Clear();
			AnsiConsole.Clear();

			AnsiConsole.Write(new Panel(
				Align.Center(
					new FigletText("GAME OVER")
						.Color(Color.Blue)
				)
			));

			AnsiConsole.Markup($@"

[blue]Game stats:[/]

- [blue]Score:[/] {_score}
- [blue]High score (this game):[/] {_highScoreThisGame} 
- [blue]High score (all time):[/] {_highScoreAllTime} {(_previousHighScore < _highScoreAllTime ? "[yellow]NEW BEST![/]" : "")}

");
		}
		AnsiConsole.Clear();
		AnsiConsole.Clear();
		AnsiConsole.Clear();
		AnsiConsole.Clear();

		AnsiConsole.Write(new Panel(
			Align.Center(
				new FigletText("GAME OVER")
					.Color(Color.Blue)
			)
		));

		AnsiConsole.Markup($@"

[blue]Game stats:[/]

- [blue]Score:[/] {_score}
- [blue]High score (this game):[/] {_highScoreThisGame} 
- [blue]High score (all time):[/] {_highScoreAllTime} {(_previousHighScore < _highScoreAllTime ? "[yellow]NEW BEST![/]" : "")}
");

		AnsiConsole.Markup(@"
Use the [yellow]number keys[/] to choose

[yellow]1.[/] Play again
[yellow]2.[/] Back
[yellow]3.[/] Exit

");
	}
	
	
	static void Update()
	{
		if (gameState != GameState.Playing) return;

		UpdateDigits();

		globalSpeedMultiplier = 1 + player.circle.radius / 250f;

		player.Update();
		world.UpdateEnemies();

		if (_random.Next(0, 150 + world.enemies.Count * 100) == 0)
			world.SpawnEnemy(camera);

		if (_random.Next(0, 15) == 0)
			world.SpawnFood(camera);

		UpdateCamera();

		if (_animationFrame == _animationFrames)
		{
			int index = _animation.IndexOf(player.circle.color);

			if (index == _animation.Count - 1) return;
	
			player.circle.color = _animation[index + 1];
			_animationFrame = 0;	
		}
		else
			_animationFrame++;
	}
	
	static void Render() {
		if (gameState == GameState.Playing)
			world.Print(camera);
	}
	
	static void HandleInput(ConsoleKeyInfo key)
	{
		if (gameState == GameState.Playing)
		{
			Vector2 previousDirection = player.direction;

			if (key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.A)
				player.SetDirection(-1, 0);
			else if (key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.D)
				player.SetDirection(1, 0);
			else if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.W)
				player.SetDirection(0, -1);
			else if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.S)
				player.SetDirection(0, 1);

			_targetMovementOffset = -player.direction * player.circle.radius / 10f;

			if (player.direction == previousDirection && player.circle.radius > 3f)
			{
				player.circle.center += player.direction * player.circle.radius / 3f;
				player.circle.radius /= 1.05f;
				player.circle.color = _animation[0];

				_offset = -player.direction * player.circle.radius / 3f;
				UpdateCamera();
			}
		} else if (gameState == GameState.StartMenu)
		{
			if (key.Key == ConsoleKey.D1)
			{
				gameState = GameState.HowToPlay;
				StartHowToPlay();
			} else if (key.Key == ConsoleKey.D2)
			{
				Environment.Exit(0);
			}
		} else if (gameState == GameState.HowToPlay)
		{
			if (key.Key == ConsoleKey.D1)
			{
				gameState = GameState.Playing;
				StartPlaying();
			} else if (key.Key == ConsoleKey.D2)
			{
				gameState = GameState.StartMenu;
				StartStartMenu();
			}
		} else if (gameState == GameState.GameOver)
		{
			if (key.Key == ConsoleKey.D1)
			{
				gameState = GameState.Playing;
				StartPlaying();
			} else if (key.Key == ConsoleKey.D2)
			{
				gameState = GameState.StartMenu;
				StartStartMenu();
			} else if (key.Key == ConsoleKey.D3)
				Environment.Exit(0);
		}
	}
	
	public static IEnumerable<int> GetDigits(int source)
	{
		while (source > 0)
		{
			var digit = source % 10;
			source /= 10;
			yield return digit;
		}
	}

	static void UpdateDigits()
	{
		int radius = (int)Math.Floor(player.circle.radius);
		_score = radius;
		_highScoreThisGame = int.Max(_highScoreThisGame, _score);
		
		int previousHighScore = _highScoreAllTime;
		_highScoreAllTime = int.Max(_highScoreAllTime, _score);
		
		if (_highScoreAllTime != previousHighScore)
			PlayerPrefs.Set(_highScoreAllTime);
		
		int digitCount = radius.ToString().Length;
		
		float spacing = 10;
		Vector2 startPosition = new(_viewportWidth / 2f - 7 + spacing * digitCount / 2f, 4);

		if (world.digits.Count != digitCount)
		{
			world.digits.Clear();

			for (int i = 0; i < digitCount; i++)
			{
				world.digits.Add(new(
					0,
					startPosition - new Vector2(i * spacing, 0)
				));
			}
		}
		else
		{
			for (int i = 0; i < digitCount; i++)
				world.digits[i].anchor = startPosition - new Vector2(i * spacing, 0);
		}

		List<int> digits = GetDigits(radius).ToList();

		for (int i = 0; i < world.digits.Count; i++)
			world.digits[i].digit = digits[i];
	}

	static void UpdateCamera()
	{
		camera.center = player.Center + _offset + _movementOffset;
		_targetZoom = Math.Max(0.05f + player.circle.radius / 30f, 0.75f);

		camera.zoom += (_targetZoom - camera.zoom) * _targetZoomSmoothing; 
		
		_movementOffset.X += (_targetMovementOffset.X - _movementOffset.X) * _movementOffsetSmoothing;
		_movementOffset.Y += (_targetMovementOffset.Y - _movementOffset.Y) * _movementOffsetSmoothing;

		_offset /= 1.05f;
		
		_viewportWidth = Console.BufferWidth - 1;
		_viewportHeight = Console.BufferHeight * 2 - 1;
		camera.dimensions = new(_viewportWidth, _viewportHeight);
	}

}