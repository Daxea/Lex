using static System.Console;

namespace Lex.Demo.WordGenerator
{
    /*
     * The Lex Word Generator Demo is a demonstration of lexing or tokenization to build a word generator.
     * The core idea of lexing is parsing text input into tokens that can then be understood by a Parser.
     * In the case of a word generator, the Parser will use the tokens to select appropriate sounds
     * to construct a new word.
     */

    class Program
    {
        static void Main(string[] args)
        {
            var random = new System.Random();

            Print("Key: ");
            var input = ReadLine();
            Print("Repeat n Times: n = ");
            var n = int.Parse(ReadLine());
            ForegroundColor = System.ConsoleColor.Green;
            while (n-- > 0)
                WriteLine(WordParser.Parse(input, random));
            ReadKey();
        }

        static void Print(string text)
        {
            ForegroundColor = System.ConsoleColor.Yellow;
            Write(text);
            ForegroundColor = System.ConsoleColor.White;
        }
    }
}