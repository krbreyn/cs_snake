using Spectre.Console;
using Spectre.Console.Rendering;

var game = new Game();
game.Run();

public class Player {
    public List<(int X, int Y)> Body {get; set;}
    public int LastDx {get; set;} = 1;
    public int LastDy {get; set;} = 0;

    public Player(int x, int y) {
        Body = new List<(int, int)> {
            (x, y),
            (x - 1, y),
            (x - 2, y)
        };
    }

    public (int x, int y) Head() {
        return Body[0];
    }

    public void Move(int dx, int dy) {
        var head = Body[0];
        Body.Insert(0, (head.X + dx, head.Y + dy));
        Body.RemoveAt(Body.Count - 1);
        LastDx = dx;
        LastDy = dy;
    }

    public void Grow(int dx, int dy) {
        var head = Body[0];
        Body.Insert(0, (head.X + dx, head.Y + dy));
        LastDx = dx;
        LastDy = dy;
    }
}


public class Game {
    public Player player;
    public int WindowHeight {get;}
    public int WindowWidth {get;}
    private bool running = true;

    public (int x, int y) apple {get; set;}

    public (int X, int Y) GetRandomEmptyPos() {
        var random = new Random();
        int x, y;

        do {
            x = random.Next(1, WindowWidth-1);
            y = random.Next(1, WindowHeight-1);
        } while (player.Body.Any(seg => seg.X == x && seg.Y == y));

        return (x, y);
    }

    public Game() {
        WindowHeight = Console.WindowHeight / 2;
        WindowWidth = Console.WindowWidth / 2;
        player = new Player((WindowWidth / 2) , WindowHeight / 2);
        apple = GetRandomEmptyPos();
    }

    public void Run() {
        Console.CursorVisible = false;
        AnsiConsole.Clear();

        // AnsiConsole.AlternateScreen(() => {
        //     while (running) {
        //         Render();
        //         var key = Console.ReadKey(intercept: true);
        //         Update(key);
        //     }
        // });
        AnsiConsole.Live(CreateDisplay())
            .Start(ctx => {
                while (running) {
                    ctx.UpdateTarget(CreateDisplay());

                    Update();

                    Thread.Sleep(100);
                }
            });

        // clean up
        Console.CursorVisible = true;
        // AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[green]Thanks for playing![/]");
    }

    private IRenderable CreateDisplay() {
        var canvas = new Canvas(WindowWidth, WindowHeight);

        var borderTiles = Enumerable.Range(0, WindowWidth)
            .SelectMany(x => new[] { (x, 0), (x, WindowHeight - 1) })
            .Concat(Enumerable.Range(0, WindowHeight)
                .SelectMany(y => new[] { (0, y), (WindowWidth - 1, y) }));

        foreach (var (x, y) in borderTiles) {
            canvas.SetPixel(x, y, Color.Grey);
        }

        foreach (var (x, y) in player.Body) {
            canvas.SetPixel(x, y, Color.White);
        }
        canvas.SetPixel(apple.x, apple.y, Color.Red);

        var layout = new Layout()
            .SplitRows(
                new Layout("title").Size(1),
                new Layout("game")
            );

        layout["title"].Update(new Markup("[green]Snake[/]").Centered());
        layout["game"].Update(Align.Center(canvas, VerticalAlignment.Middle));

        return layout;
    }

    private void Render() {
        var canvas = new Canvas(WindowWidth, WindowHeight);

        foreach (var (x, y) in player.Body.Skip(1)) {
            canvas.SetPixel(x, y, Color.White);
        }

        var head = player.Body[0];
        canvas.SetPixel(head.Item1, head.Item2, Color.Gold1);
        
        canvas.SetPixel(apple.x, apple.y, Color.Red);


        AnsiConsole.Write(new Markup("[green]Snake[/]").Centered());
        AnsiConsole.Write(canvas);
    }

    private void Update() {
        var dx = player.LastDx;
        var dy = player.LastDy;
        
        if (Console.KeyAvailable) {
            var key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Escape) {
                running = false;
                return;
            }

            (dx, dy) = key.Key switch {
                ConsoleKey.W or ConsoleKey.UpArrow => (0, -1),
                ConsoleKey.S or ConsoleKey.DownArrow => (0, 1),
                ConsoleKey.A or ConsoleKey.LeftArrow => (-1, 0),
                ConsoleKey.D or ConsoleKey.RightArrow => (1, 0),
                _ => (0, 0)
            };
        }


        if (dx == -player.LastDx && dy == -player.LastDy) {
            dx = player.LastDx;
            dy = player.LastDy;
        }

        var (px, py) = player.Head();

        int newX = px + dx;
        int newY = py + dy;

        var skipMove = false;

        if (newX == apple.x && newY == apple.y) {
            apple = GetRandomEmptyPos();
            player.Grow(dx, dy);
            skipMove = true;
        }

        foreach (var (x, y) in player.Body.Skip(1)) {
            if (newX == x && newY == y) {
                running = false;
                return;
            }
        }

        if (newX <= 0 || newX >= WindowWidth - 1 || newY <= 0 || newY >= WindowHeight - 1) {
            running = false;
            return;
        }

        if (skipMove) {
            return;
        }

        if ((px + dx >= 1 && px + dx < WindowWidth-1)
            && (py + dy >= 1 && py + dy < WindowHeight-1)) {
            player.Move(dx, dy);
            }
    }
}
