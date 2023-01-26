// See https://aka.ms/new-console-template for more information
using ConsoleApp2;

internal class Program
{

    static void Main(string[] args)
    {

        using (MyGameWindow game = new MyGameWindow(800, 600, "ConsoleOpenTK"))
        {

            game.Run();
        }
    }
}