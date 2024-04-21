using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;
using LedCube.Core.UI.Messages;
using LedCube.PluginBase;
using LedCube.Sdf.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LedCube.Plugins.Animation.Snake3D;

public class Snake3DAnimation(IOptions<Snake3DConfiguration> options, ILogger<Snake3DAnimation> logger)
    : IFrameGenerator, IRecipient<KeyEventMessage>
{
    public new static FrameGeneratorInfo Info => new("Snake-3D", "3D Snake Game adaptation");

    private readonly Snake3DConfiguration _configuration = options.Value;
    
    private Point3D _size;
    private float _lastMove;
    private float _lastFrame;
    private GameState _gameState = GameState.WaitForStart;
    
    private int _gameSpeed = 1;
    private readonly List<SnakeFood> _food = [];
    private readonly Snake _snake = new();
    
    private Random _random = Random.Shared;
    
    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(20);

    public void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    public Task InitializeAsync(CancellationToken token)
    {
        WeakReferenceMessenger.Default.Register(this);
        return Task.CompletedTask;
    }

    public void Start(AnimationContext animationContext)
    {
        _size = animationContext.CubeData.Size;
        animationContext.CubeData.Clear();
        ResetGame();
    }

    private void ResetGame()
    {
        _gameState = GameState.WaitForStart;
        _snake.Reset(_random, _size, _configuration);
        _food.Clear();
        _gameSpeed = 1;
    }
    
    public void DrawFrame(FrameContext frameContext)
    {
        var elapsedTimeMs = (float) frameContext.ElapsedTimeUs / 1_000;
        var lastMoveDiff = elapsedTimeMs - _lastMove;
        var lastFrameDiff = elapsedTimeMs - _lastFrame;

        if (_gameState == GameState.Running)
        {
            if (lastMoveDiff > (1200.0f / _gameSpeed))
            {
                UpdateGameMove(elapsedTimeMs);
                _lastMove = elapsedTimeMs;
                logger.LogDebug("Game Move - Elapsed:{0}ms, State:{1}, GameSpeed:{2}, SnakeSize:{3}, Dir:{4}",
                    elapsedTimeMs, _gameState, _gameSpeed, _snake.Size, _snake.Direction);
            }
        }

        if (lastFrameDiff > 10)
        {
            RenderGameFrame(frameContext, elapsedTimeMs);
            _lastFrame = elapsedTimeMs;
        }
    }

    private void UpdateGameMove(float elapsedTimeMs)
    {
        var newHead = _snake.GetNewHead();

        var edgeCollision = false;
        if (_configuration.EdgeBehaviour == EdgeBehaviour.GameOver)
        {
            edgeCollision = (newHead >= _size || newHead < Point3D.Empty);
        }
        
        var bodyCollision = _snake.CheckCollision(newHead);
        if (edgeCollision || bodyCollision)
        {
            //Game Over!
            _gameState = GameState.GameOver;
            logger.LogDebug("GameOver!");
            return;
        }
        
        //Check Food collision and update body size
        var foodCollision = _food.FirstOrDefault(x => x.Position == newHead);
        if (foodCollision is not null)
        {
            _snake.GrowSnake();
            _food.Remove(foodCollision);
            
            //Increase GameSpeed
            _gameSpeed++;
        }
        
        //Move snake: Add Head and cut tail if necessary
        _snake.MoveSnake(newHead);
        
        //Spawn new Food if necessary
        while(_food.Count < _configuration.ActiveFoodCount)
        {
            _food.Add(new SnakeFood(GetFreePosition(), elapsedTimeMs));
        }
    }

    private Point3D GetFreePosition()
    {
        Point3D p;
        var n = 0;
        do
        {
            p = GetRandomPoint();
            n++;
        } while (!IsPositionFree(p) && n < 10);
        return p;
    }

    private Point3D GetRandomPoint()
    {
        return Point3D.CoordinateFromIndex(_size, _random.Next(_size.ElementProduct));
    }

    private bool IsPositionFree(Point3D p)
    {
        if (_snake.CheckCollision(p))
            return false;
        if (_food.Any(x => x.Position == p))
            return false;

        return true;
    }

    private void RenderGameFrame(FrameContext frameContext, float timeMs)
    {
        frameContext.Buffer.Clear();
        switch (_gameState)
        {
            case GameState.WaitForStart:
                break;
            case GameState.Prepare:
                var plane = (int) (timeMs / 200);
                if (plane < _size.Z)
                {
                    for (var i = 0; i < plane; i++)
                    {
                        frameContext.Buffer.SetPlane(plane, true);
                    }
                }
                else
                {
                    _gameState = GameState.Running;
                    logger.LogDebug("Game Started!");
                }
                break;
            case GameState.Running:
                foreach (var p in _snake.Body)
                {
                    frameContext.Buffer.SetLed(p, true);
                }

                if (!((timeMs / 200) % 3 > 1))
                    return;
                foreach (var p in _food)
                {
                    frameContext.Buffer.SetLed(p.Position, true);
                }
                break;
            case GameState.GameOver:
                if ((int) (timeMs / 800) % 2 == 0)
                {
                    frameContext.Buffer.SetCube(true);
                }
                break;
        }
    }

    private void StartGame()
    {
        ResetGame();
        _gameState = GameState.Prepare;
        logger.LogDebug("Prepare!");
    }
    
    public void Receive(KeyEventMessage message)
    {
        //logger.LogDebug("Key event: {keyEventMessage}", message);
        switch (_gameState)
        {
            case GameState.WaitForStart:
            case GameState.GameOver:
                switch (message.Key)
                {
                    case Key.Enter:
                        StartGame();
                        return;
                    default:
                        return;
                }        
            case GameState.Running:
                switch (message.Key)
                {
                    case Key.Up:
                        _snake.SetNewDirection(SnakeDirection.Front);
                        return;
                    case Key.Down:
                        _snake.SetNewDirection(SnakeDirection.Rear);
                        return;
                    case Key.Left:
                        _snake.SetNewDirection(SnakeDirection.Left);
                        return;
                    case Key.Right:
                        _snake.SetNewDirection(SnakeDirection.Right);
                        return;
                    case Key.LeftShift:
                    case Key.W:
                        _snake.SetNewDirection(SnakeDirection.Up);
                        return;
                    case Key.LeftCtrl:
                    case Key.S:
                        _snake.SetNewDirection(SnakeDirection.Down);
                        return;
                    default:
                        return;
                }        
            default:
                return;
        }
    }


}

internal record SnakeFood(Point3D Position, float SpawnTime);

internal class Snake()
{
    public Point3D GameSize { get; private set; }
    public Snake3DConfiguration Configuration { get; private set; } = new();
    public List<Point3D> Body { get; } = [];
    public float Size { get; set; } = 2.0f;
    public SnakeDirection Direction { get; set; } = SnakeDirection.Right;

    public void Reset(Random random, Point3D gameSize, Snake3DConfiguration configuration)
    {
        GameSize = gameSize;
        Configuration = configuration;
        Body.Clear();
        GenerateStartPos(random);
        Size = 2.0f;
    }

    private void GenerateStartPos(Random random)
    {
        var p = random.RandomPoint(GameSize);
        Direction = (SnakeDirection) random.Next(6);
        var start = Direction switch
        {
            SnakeDirection.Up => new Point3D(p.X, p.Y, 0),
            SnakeDirection.Down => new Point3D(p.X, p.Y, GameSize.Z-1),
            SnakeDirection.Front => new Point3D(p.X, 0, p.Z),
            SnakeDirection.Rear => new Point3D(p.X, GameSize.Y-1, p.Z),
            SnakeDirection.Left => new Point3D(0, p.Y, p.Z),
            SnakeDirection.Right => new Point3D(GameSize.X-1, p.Y, p.Z),
            _ => throw new ArgumentOutOfRangeException()
        };
        Body.Add(start);
    }

    public static Point3D MoveHead(Point3D oldHead, SnakeDirection direction, Point3D gameSize, EdgeBehaviour edgeBehaviour)
    {
        var newHead = direction switch
        {
            SnakeDirection.Up => new Point3D(oldHead.X, oldHead.Y, oldHead.Z + 1),
            SnakeDirection.Down => new Point3D(oldHead.X, oldHead.Y, oldHead.Z - 1),
            SnakeDirection.Front => new Point3D(oldHead.X, oldHead.Y + 1, oldHead.Z),
            SnakeDirection.Rear => new Point3D(oldHead.X, oldHead.Y - 1, oldHead.Z),
            SnakeDirection.Left => new Point3D(oldHead.X + 1, oldHead.Y, oldHead.Z),
            SnakeDirection.Right => new Point3D(oldHead.X - 1, oldHead.Y, oldHead.Z),
            _ => throw new ArgumentOutOfRangeException()
        };
        return (edgeBehaviour == EdgeBehaviour.RollOver) ? newHead % gameSize : newHead;
    }
    
    public Point3D GetNewHead() => MoveHead(Body[^1], Direction, GameSize, Configuration.EdgeBehaviour);

    public bool CheckCollision(Point3D p) => Body.Any(x => x == p);

    public void MoveSnake(Point3D newHead)
    {
        //Move snake: Add Head and cut tail if necessary
        Body.Add(newHead);
        
        var tooLongCount = (int)(Body.Count - Size);
        if(tooLongCount >= 1)
        {
            Body.RemoveRange(0, tooLongCount);
        }
    }

    public void SetNewDirection(SnakeDirection direction)
    {
        Direction = direction;
    }
    
    public void GrowSnake() => Size += Configuration.FoodGrowthFactor;
}

internal enum GameState
{
    WaitForStart,
    Prepare,
    Running,
    GameOver
}

internal enum SnakeDirection
{
    Up,
    Down,
    Front,
    Rear,
    Left,
    Right
}