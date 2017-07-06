# Lex
A simple, customizable lexing library.

# What?
I was writing small interpreted programming languages for fun and knowledge, and I sort of got tired writing my lexer class over and over and over.

So I built Lex to be an extensible lexer that can be used for virtually any programming language through simple configuration.

# How to Use It
Somewhere in your program, you need to define a LexerConfiguration, or if you are okay with C#-style comments (// line comment, /* block comment */), then you can just create a List of TokenMapping objects.

    private List<TokenMapping> mappings = List<TokenMapping> {
        // when "print" is hit by the lexer, a token with TypeId "1" will be returned
        new TokenMapping("print", 1),
        // when a double-quote is hit, a token is returned containing the TypeId 2 and the value of the string
        new TokenMapping("\"", lexer => lexer.GetString(2))
        // when a letter is hit, a token is returned containing the TypeId 3 and the value of the identifier
        new TokenMapping(lexer => char.IsLetter(lexer.CurrentChar), lexer => lexer.GetIdentifier(3))
    };

The first parameter of a TokenMapping can be a string key or a predicate that takes a Lexer instance as an argument.
The second parameter can be either an integer (in which case, the resulting token will have a TypeId equal to this number),
or a function that takes a Lexer instance as an argument and returns a Token.

As you might see in the demo ConsoleLang, the Lexer has a few handy helpers for tokenizing numbers, identifiers, and strings.
These are very basic, but can be replaced with custom methods that handle more numeric types, or allow for string interpolation,
or whatever else you may want to do.

# Lex.Parsing

I'm going to rewrite this, probably. Don't worry about it, m'kay?

# Lex.Demo.ConsoleLang

A simple (very very simple) example of a programming language. You can create functions, do some basic math, and print stuff to the console.

    print "Hello, World";
    func int double(int n) n * 2;                   // You can create small functions that return the immediate expression,
    func float double(float n) { return n * 2; }    // or block functions containing a series of statements.
    func void say(string text) print text;          // You can also "alias" commands.
    cls                                             // You can clear the console!
    print "Twenty Five: " + (double(10) + 5);       // You can concat strings, and add numbers in real time.

# Lex.Demo.WordBuilder

A simple tool for generating words based on a simple set of rules.
