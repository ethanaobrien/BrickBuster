using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// the breakout game scene. everything on screen (bricks, paddle, ball, walls) is built in code
// in Start - no prefabs or image assets. arrow keys / A D move the paddle, Space launches the ball.
public class Game : MonoBehaviour
{
    public GameObject blockPrefab;

    // play area bounds and object sizes, in world units
    private const float Left = -8f;
    private const float Right = 8f;
    private const float Top = 4.65f;
    private const float Bottom = -4.9f;

    private const float PaddleY = -4.15f;
    private const float PaddleWidth = 2.15f;
    private const float PaddleHeight = 0.2f;

    private const float BallRadius = 0.15f;
    private const float BallSpeed = 7;

    private static readonly Color Background = Color.black;

    // one color per brick row, so the wall reads as horizontal stripes
    private static readonly Color[] RowColors =
    {
        Color.red,
        Color.orange,
        Color.yellow,
        Color.green,
        Color.blue,
        Color.purple,
        Color.gray,
        Color.pink,
        Color.darkMagenta,
        Color.yellowGreen,
        Color.cyan,
        Color.darkOrange,
        Color.deepPink,
        Color.red,
        Color.orange,
        Color.yellow,
        Color.green,
        Color.blue,
        Color.purple,
        Color.gray,
        Color.pink,
        Color.darkMagenta,
        Color.yellowGreen,
        Color.cyan,
        Color.darkOrange,
        Color.deepPink
    };

    private static readonly Color BorderColor = Color.saddleBrown;

    // every brick on screen. removing from this list is what "kills" a brick for the game
    private readonly List<Block> _blocks = new List<Block>();

    // a 1x1 white sprite generated at runtime; every shape is a scaled, tinted copy of it
    private Sprite _square;

    private GameObject _paddle;
    private GameObject _ball;

    // ball travel direction, always normalized. flipping a component bounces that axis
    private Vector2 _direction;

    private int _score;
    private int _lives;
    private bool _waitingToLaunch;

    // HUD style, created lazily on first draw (GUI code must run inside OnGUI)
    private GUIStyle _hudStyle;

    // called on startup. Sets up and begins the game
    private void Start()
    {
        var camera = Camera.main;
        // orthographic camera, sized to always fit the full play area at this window's aspect ratio
        camera.orthographic = true;
        camera.orthographicSize = Mathf.Max(5f, 8.45f / camera.aspect);
        camera.backgroundColor = Background;

        // make the shared 1x1 white pixel sprite
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        _square = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        _paddle = CreateRect(new Vector2(0, PaddleY), new Vector2(PaddleWidth, PaddleHeight), new Color(0.85f, 0.95f, 1f));
        _ball = CreateRect(Vector2.zero, Vector2.one * (BallRadius * 2f), Color.white);

        // border walls
        CreateRect(new Vector2(Left - 0.1f, 0), new Vector2(0.04f, 9.5f), BorderColor);
        CreateRect(new Vector2(Right + 0.1f, 0), new Vector2(0.04f, 9.5f), BorderColor);
        CreateRect(new Vector2(0, Top + 0.1f), new Vector2(16.25f, 0.04f), BorderColor);

        RestartGame();
    }

    // creates a colored rectangle from the shared white sprite
    private GameObject CreateRect(Vector2 position, Vector2 size, Color color)
    {
        var obj = new GameObject();
        // parent to this object so it's deleted with the rest of the game on scene unload
        obj.transform.SetParent(transform);
        obj.transform.position = position;
        obj.transform.localScale = new Vector3(size.x, size.y, 1f);
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = _square;
        renderer.color = color;
        return obj;
    }

    // fresh round: clear bricks, reset score/lives, rebuild the wall, put the ball on the paddle
    private void RestartGame()
    {
        foreach (var block in _blocks.Where(block => block != null)) Destroy(block.gameObject);
        _blocks.Clear();
        _score = 0;
        _lives = 3;
        _paddle.transform.position = new Vector3(0, PaddleY, 0);
        BuildBlocks();
        ResetBall();
    }

    // builds the wall based on the chosen difficulty - harder = bigger, denser wall
    private void BuildBlocks()
    {
        var difficulty = GameManager.Instance.GetDifficulty();
        int columns;
        int rows;
        switch (difficulty)
        {
            case "Easy":
                columns = 10;
                rows = 5;
                break;
            case "Medium":
                columns = 15;
                rows = 8;
                break;
            case "Hard":
                columns = 20;
                rows = 10;
                break;
            case "Expert":
                columns = 30;
                rows = 15;
                break;
            default:
                columns = 8;
                rows = 5;
                break;
        }

        // wall is a fixed 13.2 units wide; brick width shrinks as the column count grows
        var width = 13.2f / columns;
        const float height = 0.4f;
        const float gapX = 0.1f;
        const float gapY = 0.1f;
        var rowWidth = columns * width + (columns - 1) * gapX;

        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < columns; col++)
            {
                var x = -rowWidth / 2f + width / 2f + col * (width + gapX);
                var y = 3.55f - row * (height + gapY);

                var obj = Instantiate(blockPrefab, transform);
                obj.name = "Brick (" + (row + 1) + ", " + (col + 1) + ")";
                obj.transform.SetParent(transform);
                obj.transform.position = new Vector3(x, y, 0);

                var block = obj.GetComponent<Block>();
                block.Configure(new Vector2(width, height), _square, RowColors[row]);

                _blocks.Add(block);
            }
        }
    }

    // puts the ball back on the paddle, waiting for Space
    private void ResetBall()
    {
        _waitingToLaunch = true;
        _direction = Vector2.zero;
        _ball.transform.position = _paddle.transform.position + Vector3.up * 0.36f;
    }

    // moves the paddle to paddleX, clamped inside the walls
    private void UpdatePaddlePosition(float paddleX)
    {
        const float min = Left + PaddleWidth / 2;
        const float max = Right - PaddleWidth / 2;
        if (paddleX < min)
        {
            paddleX = min;
        }

        if (paddleX > max)
        {
            paddleX = max;
        }

        _paddle.transform.position = new Vector3(paddleX, PaddleY, 0);
    }

    // releases the ball, straight down at first
    private void LaunchBall()
    {
        _waitingToLaunch = false;
        _direction = new Vector2(0, -1);
    }

    // called once per frame, processed ball / paddle movement
    private void Update()
    {
        var keyboard = Keyboard.current;

        var paddleX = _paddle.transform.position.x;

        // +1 right, -1 left, 0 none. arrow keys and A D both work
        var direction = (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed ? 1 : 0)
                      - (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed ? 1 : 0);
        paddleX += direction * 11f * Time.deltaTime;

        UpdatePaddlePosition(paddleX);

        // while waiting to launch the ball just follows the paddle; Space launches it
        if (_waitingToLaunch)
        {
            _ball.transform.position = _paddle.transform.position + new Vector3(0, 0.25f, 0);
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                LaunchBall();
            }
            return;
        }

        // small substeps keep the ball from tunneling through a brick
        var remaining = Mathf.Min(Time.deltaTime, 0.05f);
        while (remaining > 0f && !_waitingToLaunch)
        {
            var step = Mathf.Min(remaining, 0.008f);
            remaining -= step;
            MoveBall(step);
        }
    }

    // ends the game and goes back to the title screen with a win/loss message
    private void FinishedGame(bool win)
    {
        GameManager.Instance.SetLastPlayMessage(win ? "Nice job! You won!!" : "Better luck next time :(");
        SceneManager.LoadScene("StartScene");
    }

    // advances the ball by `step` seconds, resolving walls, paddle, bottom and brick collisions
    private void MoveBall(float step)
    {
        Vector2 previous = _ball.transform.position;
        var velocity = _direction * BallSpeed;
        var next = previous + (velocity * step);

        const float leftBound = Left + BallRadius;
        const float rightBound = Right - BallRadius;
        const float topBound = Top - BallRadius;

        // bounce off the left/right walls
        if (next.x is < leftBound or > rightBound)
        {
            _direction.x = -_direction.x;
            next.x = Mathf.Clamp(next.x, Left + BallRadius, Right - BallRadius);
        }
        // bounce off the top wall
        if (next.y > topBound)
        {
            _direction.y = -_direction.y;
            next.y = Top - BallRadius;
        }

        // fell off the bottom: lose a life
        if (next.y < Bottom)
        {
            _lives--;
            if (_lives <= 0) FinishedGame(false);
            else ResetBall();
            return;
        }

        // hit the paddle
        if (_direction.y < 0 && previous.y >= PaddleY && next.y - BallRadius <= PaddleY &&
            Mathf.Abs(next.x - _paddle.transform.position.x) <= (PaddleWidth / 2) + BallRadius)
        {
            // where the ball landed on the paddle, -1 (far left) to 1 (far right)
            var hitFactor = (next.x - _paddle.transform.position.x) / (PaddleWidth / 2);
            hitFactor = Mathf.Clamp(hitFactor, -1f, 1f);

            // center hits go straight up, edge hits bounce at up to 75 degrees
            var maxBounceAngle = 75f * Mathf.Deg2Rad;
            var bounceAngle = hitFactor * maxBounceAngle;

            _direction = new Vector2(Mathf.Sin(bounceAngle), Mathf.Cos(bounceAngle)).normalized;

            // nudge clear of the paddle so it doesn't re-collide next step
            next.y = PaddleY + BallRadius + 0.01f;
        }

        // check against every brick. iterate from the end so removing one doesn't shift the rest
        for (var i = _blocks.Count - 1; i >= 0; i--)
        {
            var block = _blocks[i];
            Vector2 center = block.transform.position;
            var half = block.Size * 0.5f;

            // no overlap on either axis means no collision
            if (Mathf.Abs(next.x - center.x) > half.x + BallRadius ||
                Mathf.Abs(next.y - center.y) > half.y + BallRadius) continue;

            // bounce off whichever side has the smaller overlap
            var overlapX = half.x + BallRadius - Mathf.Abs(next.x - center.x);
            var overlapY = half.y + BallRadius - Mathf.Abs(next.y - center.y);
            if (overlapX < overlapY) _direction.x = -_direction.x;
            else _direction.y = -_direction.y;

            // undo this step's movement so the ball ends outside the brick
            next = previous;

            _score += 10;
            _blocks.RemoveAt(i);
            Destroy(block.gameObject);

            // all bricks cleared, win
            if (_blocks.Count == 0) FinishedGame(true);
            break;
        }
        _ball.transform.position = next;
    }

    // immediate-mode HUD: score top-left, lives top-right
    private void OnGUI()
    {
        if (_hudStyle == null)
        {
            _hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
            _hudStyle.normal.textColor = Color.white;
        }

        var w = Screen.width;

        GUI.Label(new Rect(50, 20, 280, 36), "SCORE: " + _score, _hudStyle);
        GUI.Label(new Rect(w - 200, 20, 130, 36), "LIVES: " + _lives, _hudStyle);
    }
}
