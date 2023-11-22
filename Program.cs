namespace dacomp
{
  class Program
  {
    static void Main(string[] args)
    {
      bool showTree = false;
      while (true)
      {
        Console.Write("> ");
        var line = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(line))
          return;

        if (line == "#showTree")
        {
          showTree = !showTree;
          Console.WriteLine(showTree ? "Showing parse trees." : "Not showing parse trees");
          continue;
        }
        else if (line == "#cls")
        {
          Console.Clear();
          continue;
        }

        var syntaxTree = SyntaxTree.Parse(line);

        var color = Console.ForegroundColor;
        if (showTree)
        {
          Console.ForegroundColor = ConsoleColor.Gray;
          Print(syntaxTree.Root);
          Console.ForegroundColor = color;
        }
        if (syntaxTree.Diagnostics.Any())
        {
          Console.ForegroundColor = ConsoleColor.Red;
          foreach (var diagnostic in syntaxTree.Diagnostics)
            Console.WriteLine(diagnostic);
          Console.ForegroundColor = color;
        }
        else
        {
          var e = new Evaluator(syntaxTree.Root);
          var result = e.Evaluate();
          Console.WriteLine(result);
        }
      }
    }

    static void Print(SyntaxNode node, string indent = "", bool isLast = true)
    {
      var marker = isLast ? "└──" : "├──";

      Console.Write(indent);
      Console.Write(marker);
      Console.Write(node.Kind);

      if (node is SyntaxToken t && t.Value != null)
      {
        Console.Write(" ");
        Console.Write(t.Value);
      }
      Console.WriteLine();
      indent += isLast ? "   " : "│  ";

      var lastChild = node.GetChildren().LastOrDefault();

      foreach (var child in node.GetChildren())
        Print(child, indent, child == lastChild);
    }
  }

  enum SyntaxKind
  {
    EOFToken,
    NumberToken,
    WhiteSpaceToken,
    PlusToken,
    MinusToken,
    MultiplyToken,
    DivideToken,
    OpenParenthesisToken,
    CloseParenthesisToken,
    InvalidToken,
    NumberExpression,
    BinaryExpression,
    ParenthesizedExpression
  }

  class SyntaxToken : SyntaxNode
  {
    public SyntaxToken(SyntaxKind kind, int position, string text, object value)
    {
      Kind = kind;
      Position = position;
      Text = text;
      Value = value;
    }
    public override SyntaxKind Kind { get; }
    public int Position { get; }
    public string Text { get; }
    public object Value { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
      return Enumerable.Empty<SyntaxNode>();
    }

  }
  /// <summary>
  /// Tokenizes the input string. into different kind of syntax
  /// </summary>
  class Lexer
  {
    private readonly string _text;
    private int _position;
    private List<string> _diagnostics = new List<string>();

    public Lexer(string text)
    {
      _text = text;
    }

    public IEnumerable<string> Diagnostics => _diagnostics;

    private char Current
    {
      get
      {
        if (_position >= _text.Length)
          return '\0';

        return _text[_position];
      }
    }

    private void Next()
    {
      _position++;
    }

    /// <summary>
    /// reads characters one by one from the input text and identifies the kind of token
    /// </summary>
    /// <returns> Syntax Token of the following character </returns>
    public SyntaxToken NextToken()
    {
      if (_position >= _text.Length)
        return new SyntaxToken(SyntaxKind.EOFToken, _position, "\0", new());


      if (char.IsDigit(Current))
      {
        var start = _position;
        while (char.IsDigit(Current))
          Next();

        var length = _position - start;
        var text = _text.Substring(start, length);
        if (!int.TryParse(text, out var value))
        {
          _diagnostics.Add($"Text {_text} is not a valid int32");
        }

        return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
      }

      if (char.IsWhiteSpace(Current))
      {
        var start = _position;
        while (char.IsWhiteSpace(Current))
        {
          Next();
        }
        var length = _position - start;
        var text = _text.Substring(start, length);
        return new SyntaxToken(SyntaxKind.WhiteSpaceToken, start, text, new());
      }

      if (Current == '+')
        return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", new());
      if (Current == '-')
        return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-", new());
      if (Current == '*')
        return new SyntaxToken(SyntaxKind.MultiplyToken, _position++, "*", new());
      if (Current == '/')
        return new SyntaxToken(SyntaxKind.DivideToken, _position++, "/", new());
      if (Current == '(')
        return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(", new());
      if (Current == ')')
        return new SyntaxToken(SyntaxKind.CloseParenthesisToken, _position++, ")", new());

      _diagnostics.Add($"Invalid character input '{Current}'");
      return new SyntaxToken(SyntaxKind.InvalidToken, _position++, _text.Substring(_position - 1, 1), new());
    }
  }
  /// <summary>
  /// All nodes of syntax tree derive from this abstract class
  /// </summary>
  abstract class SyntaxNode
  {
    public abstract SyntaxKind Kind { get; }
    public abstract IEnumerable<SyntaxNode> GetChildren();
  }

  abstract class ExpressionSyntax : SyntaxNode
  {

  }

  /// <summary>
  ///  Represents nodes in the syntax tree with numeric values
  /// </summary>
  sealed class NumberExpressionSyntax : ExpressionSyntax
  {
    public NumberExpressionSyntax(SyntaxToken numberToken)
    {
      NumberToken = numberToken;
    }

    public SyntaxToken NumberToken { get; }
    public override SyntaxKind Kind => SyntaxKind.NumberExpression;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
      yield return NumberToken;
    }
  }

  /// <summary>
  /// Represents nodes in the syntax tree corresponding to arithmetic operation holding left and right expression operands along with the operator token.
  /// </summary>
  sealed class BinaryExpressionSyntax : ExpressionSyntax
  {
    public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
    {
      Left = left;
      OperatorToken = operatorToken;
      Right = right;
    }

    public ExpressionSyntax Left { get; }
    public SyntaxToken OperatorToken { get; }
    public ExpressionSyntax Right { get; }

    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
      yield return Left;
      yield return OperatorToken;
      yield return Right;
    }
  }

  sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
  {
    public ParenthesizedExpressionSyntax(SyntaxToken openParenthesisToken, ExpressionSyntax expression, SyntaxToken closeParenthesisToken)
    {
      OpenParenthesisToken = openParenthesisToken;
      Expression = expression;
      CloseParenthesisToken = closeParenthesisToken;
    }

    public SyntaxToken OpenParenthesisToken { get; }
    public ExpressionSyntax Expression { get; }
    public SyntaxToken CloseParenthesisToken { get; }

    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
      yield return OpenParenthesisToken;
      yield return Expression;
      yield return CloseParenthesisToken;
    }
  }

  sealed class SyntaxTree
  {
    public SyntaxTree(ExpressionSyntax root, SyntaxToken eoftoken, IEnumerable<string> diagnostic)
    {
      Root = root;
      EOFToken = eoftoken;
      Diagnostics = diagnostic.ToArray();
    }

    public ExpressionSyntax Root { get; }
    public SyntaxToken EOFToken { get; }
    public IReadOnlyList<string> Diagnostics { get; }


    public static SyntaxTree Parse(string text)
    {
      var parser = new Parser(text);
      return parser.Parse();
    }
  }

  // sealed class Numbe

  class Parser
  {
    private readonly SyntaxToken[] _tokens;
    private int _position;
    private List<string> _diagnostics = new List<string>();

    public Parser(string text)
    {
      var tokenList = new List<SyntaxToken>();
      var lexer = new Lexer(text);
      SyntaxToken token;

      while (true)
      {
        token = lexer.NextToken();
        if (token.Kind != SyntaxKind.WhiteSpaceToken && token.Kind != SyntaxKind.InvalidToken)
          tokenList.Add(token);

        if (token.Kind == SyntaxKind.EOFToken)
          break;
      }
      _tokens = tokenList.ToArray();
      _diagnostics.AddRange(lexer.Diagnostics);

    }

    public IEnumerable<string> Diagnostics => _diagnostics;

    private SyntaxToken Peek(int offset)
    {
      var index = _position + offset;
      if (index >= _tokens.Length)
        return _tokens[_tokens.Length - 1];

      return _tokens[index];
    }

    private SyntaxToken Current => Peek(0);

    private SyntaxToken NextToken()
    {
      var current = Current;
      _position++;
      return current;
    }

    private SyntaxToken Match(SyntaxKind kind)
    {
      if (Current.Kind == kind) return NextToken();
      _diagnostics.Add($"Error: Unexpected token, Expected: <{kind}> Found: <{Current.Kind}>");
      return new SyntaxToken(kind, Current.Position, "", new());
    }

    private ExpressionSyntax ParseExpression()
    {
      return ParseAddition();
    }

    public SyntaxTree Parse()
    {
      var expression = ParseAddition();
      var eofToken = Match(SyntaxKind.EOFToken);
      return new SyntaxTree(expression, eofToken, _diagnostics);
    }

    private ExpressionSyntax ParseFactor()
    {
      var left = ParsePrimaryExpression();
      while (Current.Kind == SyntaxKind.MultiplyToken || Current.Kind == SyntaxKind.DivideToken)
      {
        var operatorToken = NextToken();
        var right = ParsePrimaryExpression();
        left = new BinaryExpressionSyntax(left, operatorToken, right);
      }
      return left;
    }

    private ExpressionSyntax ParseAddition()
    {
      var left = ParseFactor();
      while (Current.Kind == SyntaxKind.PlusToken || Current.Kind == SyntaxKind.MinusToken)
      {
        var operatorToken = NextToken();
        var right = ParseFactor();
        left = new BinaryExpressionSyntax(left, operatorToken, right);
      }
      return left;
    }

    private ExpressionSyntax ParsePrimaryExpression()
    {
      if (Current.Kind == SyntaxKind.OpenParenthesisToken)
      {
        var left = NextToken();
        var expression = ParseExpression();
        var right = Match(SyntaxKind.CloseParenthesisToken);
        return new ParenthesizedExpressionSyntax(left, expression, right);
      }
      var numberToken = Match(SyntaxKind.NumberToken);
      return new NumberExpressionSyntax(numberToken);
    }
  }

  class Evaluator
  {
    private readonly ExpressionSyntax _root;

    public Evaluator(ExpressionSyntax root)
    {
      this._root = root;
    }

    public int Evaluate()
    {
      return EvaluateExpression(_root);
    }

    private int EvaluateExpression(ExpressionSyntax node)
    {
      if (node is NumberExpressionSyntax n)
        return (int)n.NumberToken.Value;
      if (node is BinaryExpressionSyntax b)
      {
        var left = EvaluateExpression(b.Left);
        var right = EvaluateExpression(b.Right);

        if (b.OperatorToken.Kind == SyntaxKind.PlusToken)
          return left + right;
        if (b.OperatorToken.Kind == SyntaxKind.MinusToken)
          return left - right;
        if (b.OperatorToken.Kind == SyntaxKind.MultiplyToken)
          return left * right;
        if (b.OperatorToken.Kind == SyntaxKind.DivideToken)
          return left / right;
      }

      if (node is ParenthesizedExpressionSyntax p)
        return EvaluateExpression(p.Expression);
      throw new Exception($"Unexpected node {node.Kind}");
    }
  }

}
