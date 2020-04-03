using System;

namespace PlayFabPowerTools.Utils
{

    public class ConsoleUtil
    {

        private static ConsoleColor errorColor = ConsoleColor.Red;
        private static ConsoleColor processColor = ConsoleColor.Yellow;
        private static ConsoleColor successColor = ConsoleColor.Green;
        private static ConsoleColor plainColor = ConsoleColor.White;

        private static int indents = 0;
        public static void Indent()
        {
            indents++;
        }
        public static void Outdent()
        {
            indents--;
        }
        private static string Indenter()
        {
            string prefix = "";
            for (int i = 0; i < indents; i++)
            {
                prefix += "    ";
            }
            return prefix;
        }

        public static void WriteLinePlain(string message)
        {
            WriteLine(message, plainColor);
        }
        public static void WriteLineProcess(string message)
        {
            WriteLine(message, processColor);
        }
        public static void WriteLineSuccess(string message)
        {
            WriteLine(message, successColor);
        }
        public static void WriteLineError(string errorMessage)
        {
            WriteLine(errorMessage, errorColor);
        }

        public static void WriteAtSamePlace(string something)
        {

            // Store the current position of the cursor
            var originalX = Console.CursorLeft;
            var originalY = Console.CursorTop;

            // Write the next frame (character) in the spinner animation
            Console.Write(something);

            // Restore cursor to original position
            Console.SetCursorPosition(originalX, originalY);
        }

        private static void WriteLine(string line, ConsoleColor color)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(Indenter() + line);
            Console.ForegroundColor = originalColor;
        }
    }

    public class ConsoleTaskWriter {

        private static ConsoleColor errorColor = ConsoleColor.Red;
        private static ConsoleColor processColor = ConsoleColor.Yellow;
        private static ConsoleColor successColor = ConsoleColor.Green;
        private static ConsoleColor plainColor = ConsoleColor.White;

        private bool init = false;

        private int originalX = Console.CursorLeft;
        private int originalY = Console.CursorTop;

        private int cursorLeftInfo = Console.CursorLeft;
        private int cursorTopInfo = Console.CursorTop;

        public ConsoleTaskWriter (string title) {
            originalX = Console.CursorLeft;
            originalY = Console.CursorTop;

            // Write the next frame (character) in the spinner animation
            Console.Write(title + " : ");

            // Store update position
            cursorLeftInfo = Console.CursorLeft;
            cursorTopInfo = Console.CursorTop;
        }

        public void WritePlain(string message) {
            WriteLine(message, plainColor);
        }
        public void LogProcess(string message) {
            WriteLine(message, processColor);
        }
        public void LogSuccess(string message) {
            WriteLine(message, successColor);
        }
        public void LogError(string errorMessage) {
            WriteLine(errorMessage, errorColor);
        }

        public void WriteAtSamePlace(string something) {

            // Store the current position of the cursor
            var originalX = Console.CursorLeft;
            var originalY = Console.CursorTop;

            // Write the next frame (character) in the spinner animation
            Console.Write(something);

            // Restore cursor to original position
            Console.SetCursorPosition(originalX, originalY);
        }

        public void ReportError(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(" - " + line);
        }

        private void WriteLine(string line, ConsoleColor color) {
            Console.SetCursorPosition(cursorLeftInfo, cursorTopInfo);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(cursorLeftInfo, cursorTopInfo);

            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(line);
            Console.ForegroundColor = originalColor;
        }
    }

}
