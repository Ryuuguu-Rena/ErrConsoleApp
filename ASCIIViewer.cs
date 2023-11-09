using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ErrConsoleApp
{
    internal class ASCIIViewer
    {
        private int[] restrictedChars = { 8, 9, 13, 33, 34, 35, 36, 37, 38, 39, 40, 41, 45, 46,
            91, 93, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123};

        public ASCIIViewer()
        {
            if (!Directory.Exists("arts"))
                Directory.CreateDirectory("arts");
        }

        private void ChangeTextOnLine(string text, int top)
        {
            int curLeft = Console.CursorLeft;
            int curTop = Console.CursorTop;
            Console.SetCursorPosition(0, top);
            while (Console.WindowWidth > text.Length)
                text += " ";
            Console.Write(text);
            Console.SetCursorPosition(curLeft, curTop);
        }
        public void ChangeContent(string text)
        {
            for (int i = 3; i < Console.WindowHeight - 3; i++)
                ChangeTextOnLine(" ", i);
            Console.SetCursorPosition(0, 4);
            string[] s = text.Split('\n');
            for (int i = 0; i < s.Length && Console.CursorTop < Console.WindowHeight - 3; i++)
                Console.WriteLine("  " + s[i]);
        }
        public void DrawTable(int width, int height)
        {
            int curTop = Console.CursorTop;
            int curLeft = Console.CursorLeft;
            Console.SetCursorPosition(2, 3);
            ChangeTextOnLine("", 3);
            for (int i = 1, j = 1; i <= width; i++, j++)
            {
                if (j == 10)
                    j = 1;
                Console.Write(j);
            }
            for (int i = 4; i < Console.WindowHeight - 3; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write("  ");
            }
            for (int i = 1; i <= height && i < 10; i++)
            {
                Console.SetCursorPosition(1, i + 3);
                Console.Write(i);
            }
            for (int i = 10; i <= height; i++)
            {
                Console.SetCursorPosition(0, i + 3);
                Console.Write(i);
            }
            Console.CursorTop = curTop;
            Console.CursorLeft = curLeft;
        }
        private bool IsAllowedChar(ConsoleKey key)
        {
            foreach (int rChar in restrictedChars)
            {
                if ((ConsoleKey)rChar == key)
                    return false;
            }
            return true;
        }
        private string NeighbourFile(bool nextFile, string title)
        {
            string[] files = Directory.GetFiles("arts");
            int curFileId = -1;
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Substring(5, files[i].Length - 9) == title)
                {
                    curFileId = i;
                    break;
                }
            }
            
            if (curFileId == -1)
                ChangeContent("Ты зачем название файла трогаешь???");

            if (nextFile == true && curFileId < files.Length - 1) //следующий файл
                return files[curFileId + 1];
            else if (nextFile == true)
            {
                ChangeTextOnLine("Это последний арт", 0);
                return "";
            }
            else if (nextFile == false && curFileId > 0) //предыдущий файл
                return files[curFileId - 1];
            else
            {
                ChangeTextOnLine("Это первый арт", 0);
                return "";
            }

        }
        private string RenameArt()
        {
            ChangeTextOnLine("", 0);
            ChangeTextOnLine("Назовите арт; esc - выход", Console.WindowHeight - 2);
            Console.SetCursorPosition(0, 0);

            string title = "";
            while (true)
            {
                ConsoleKeyInfo pressedKey = Console.ReadKey(true);

                if (title.Length == 0)
                    ChangeTextOnLine(pressedKey.KeyChar.ToString(), 0);

                if (pressedKey.Key == ConsoleKey.Escape)
                    return "";
                else if (pressedKey.Key == ConsoleKey.Backspace && Console.CursorLeft > 0)
                {
                    Console.CursorLeft--;
                    Console.Write(" ");
                    Console.CursorLeft--;
                    title = title.Substring(0, title.Length - 1);
                }
                else if (pressedKey.Key == ConsoleKey.Enter)
                {
                    bool isCorrect = true;
                    foreach (char c in Path.GetInvalidPathChars())
                    {
                        if (title.Contains(c))
                        {
                            title = "";
                            ChangeTextOnLine("Недопустимые символы в названии, введите имя заново", 0);
                            isCorrect = false;
                        }
                    }
                    foreach (string file in Directory.GetFiles("arts"))
                    {
                        string artName = file.Substring(5, file.Length - 9);
                        if (title == artName)
                        {
                            title = "";
                            ChangeTextOnLine("Такой файл уже существует, введите имя заново", 0);
                            isCorrect = false;
                        }
                    }
                    if (title == "")
                    {
                        title = "";
                        ChangeTextOnLine("Поле пусто, введите имя заново", 0);
                        isCorrect = false;
                    }
                    if (isCorrect)
                        return title;
                }
                else if (IsAllowedChar(pressedKey.Key))
                {
                    title += pressedKey.KeyChar;
                    Console.Write(pressedKey.KeyChar);
                }
            }
        }
        public void CreateArt()
        {
            ChangeContent("");
            string title = RenameArt();
            if (title == "")
                ShowArtsList();
            else
            {
                ASCIIArt art = new ASCIIArt(title);
                EditArt(art);
            }
        }
        public void OpenArt(ASCIIArt art)
        {
            ChangeTextOnLine(art.Title, 0);
            ChangeTextOnLine("← - предыдущий арт; → - следующий арт; esc - выход; ctrl + e - редактировать; ctrl + n - новый арт", Console.WindowHeight - 2);
            ChangeContent(art.Art);
            Console.SetCursorPosition(0, 2);

            //ChangeContent("Мухахахахха не увидишь ты картинку!!");      //xxx

            while (true)
            {
                ConsoleKeyInfo pressedKey = Console.ReadKey(true);
                if (pressedKey.Key == ConsoleKey.Escape)
                    ShowArtsList();
                else if (pressedKey.Key == ConsoleKey.E &&
                    pressedKey.Modifiers == ConsoleModifiers.Control)
                    EditArt(art);
                else if (pressedKey.Key == ConsoleKey.N &&
                    pressedKey.Modifiers == ConsoleModifiers.Control)
                    CreateArt();
                else if (pressedKey.Key == ConsoleKey.RightArrow)
                {
                    string nextFile = NeighbourFile(true, art.Title);
                    if (nextFile != "")
                    {
                        StreamReader sr = File.OpenText(nextFile);
                        string artName = nextFile.Substring(5, nextFile.Length - 9);
                        ASCIIArt nextArt = new ASCIIArt(sr, artName);
                        OpenArt(nextArt);
                    }
                }
                else if (pressedKey.Key == ConsoleKey.LeftArrow)
                {
                    string prevFile = NeighbourFile(false, art.Title);
                    if (prevFile != "")
                    {
                        StreamReader sr = File.OpenText(prevFile);
                        string artName = prevFile.Substring(5, prevFile.Length - 9);
                        ASCIIArt nextArt = new ASCIIArt(sr, artName);
                        OpenArt(nextArt);
                    }
                }
            }
        }
        public void EditArt(ASCIIArt art)
        {
            ChangeTextOnLine(art.Title + " редактирование", 0);
            ChangeTextOnLine("ctrl + s - сохранить; F2 - переименовать; стрелки - перемещение; esc - выход", Console.WindowHeight - 2);
            Console.SetCursorPosition(2, 4);
            int width = art.Width, height = art.Height;
            DrawTable(width, height);
            while (true)
            {
                ConsoleKeyInfo pressedKey = Console.ReadKey(true);
                if (pressedKey.Key == ConsoleKey.Escape)
                {
                    art.NoSave();
                    OpenArt(art);
                }
                else if (pressedKey.Key == ConsoleKey.S &&
                    pressedKey.Modifiers == ConsoleModifiers.Control)
                {
                    art.Save(width, height);
                    OpenArt(art);
                }
                else if (pressedKey.Key == ConsoleKey.F2)
                {
                    string title = RenameArt();
                    if (title != "")
                        art.Title = title;
                    ChangeTextOnLine(art.Title + " редактирование", 0);
                    ChangeTextOnLine("ctrl + s - сохранить; F2 - переименовать; стрелки - перемещение; esc - выход", Console.WindowHeight - 2);
                }
                else if (pressedKey.Key == ConsoleKey.UpArrow && Console.CursorTop > 4)
                    Console.CursorTop--;
                else if (pressedKey.Key == ConsoleKey.DownArrow && Console.CursorTop < Console.WindowHeight - 4)
                    Console.CursorTop++;
                else if (pressedKey.Key == ConsoleKey.LeftArrow && Console.CursorLeft > 2)
                    Console.CursorLeft--;
                else if (pressedKey.Key == ConsoleKey.RightArrow && Console.CursorLeft < Console.WindowWidth - 2)
                    Console.CursorLeft++;
                else if (pressedKey.Key == ConsoleKey.Backspace)
                {
                    Console.Write(" ");
                    Console.CursorLeft--;
                    art.UpdateArt(" ", Console.CursorLeft - 2, Console.CursorTop - 4);
                    int[] WH = art.CalculateWH();
                    width = WH[0];
                    height = WH[1];
                    DrawTable(width, height);
                }
                else if (IsAllowedChar(pressedKey.Key))
                {
                    Console.Write(pressedKey.KeyChar);
                    Console.CursorLeft--;
                    art.UpdateArt(pressedKey.KeyChar.ToString(), Console.CursorLeft - 2, Console.CursorTop - 4);
                    if (Console.CursorLeft - 1 > width)
                    {
                        width = Console.CursorLeft - 1;
                        DrawTable(width, height);
                    }
                    if (Console.CursorTop - 3 > height)
                    {
                        height = Console.CursorTop - 3;
                        DrawTable(width, height);
                    }
                    if (pressedKey.Key == ConsoleKey.Spacebar)
                    {
                        int[] WH = art.CalculateWH();
                        width = WH[0];
                        height = WH[1];
                        DrawTable(width, height);
                    }
                }
            }
        }
        public void ShowArtsList()
        {   
            int artsCount = 0;
            Console.CursorSize = 100;
            Console.WindowHeight = 30;
            Console.WindowWidth = 120;
            Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
            
            string widthBorder = "";
            while (Console.WindowWidth > widthBorder.Length)
                widthBorder += "-";
            ChangeTextOnLine("Выбери ASCII-арт:", 0);
            ChangeTextOnLine(widthBorder, 1);
            

            if (Directory.GetFiles("arts").Length == 0)
            {
                ChangeTextOnLine("ctrl + c - выход", Console.WindowHeight - 2);
                ChangeContent("ctrl + n что бы создать новый ASCII-арт");
                Console.SetCursorPosition(0, 2);
            }
            else
            {
                ChangeTextOnLine("ctrl + n - новый файл; ↓ - вниз; ↑ - вверх; enter - выбор; ctrl + c - выход", Console.WindowHeight - 2);
                string artsList = "";
                foreach (string file in Directory.GetFiles("arts"))
                {
                    string artName = file.Substring(5, file.Length - 9);
                    artsList += artName + "\n";
                    artsCount++;
                }
                ChangeContent(artsList);
                Console.SetCursorPosition(2, 4);
            }

            string xxx = "";                                        //xxx
            for (int i = 0; i < 10; i++)                            //xxx
            {                                                       //xxx
                xxx += "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" +    //xxx
                    "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n";      //xxx
            }                                                       //xxx
            xxx += "\nЧто-то пошло не так... \n" +                  //xxx
                "удали все строки, помеченные xxx\n" +              //xxx
                "и раскомментируй строки, помеченные !!!";          //xxx
            ChangeContent(xxx);                                     //xxx

            int selectedId = 0;
            while (true)
            {
                ConsoleKeyInfo pressedKey = Console.ReadKey(true);
                if (pressedKey.Key == ConsoleKey.DownArrow && Console.CursorTop < 3 + artsCount)
                {
                    Console.CursorTop++;
                    selectedId++;
                }
                else if (pressedKey.Key == ConsoleKey.UpArrow && Console.CursorTop > 4)
                {
                    Console.CursorTop--;
                    selectedId--;
                }
                else if (pressedKey.Key == ConsoleKey.N
                    && pressedKey.Modifiers == ConsoleModifiers.Control)
                    CreateArt();
                //else if (pressedKey.Key == ConsoleKey.Enter)                      !!!
                //{                                                                 !!!
                //    string[] files = Directory.GetFiles("arts");                  !!!
                //    string selFile = files[selectedId];                           !!!
                //    StreamReader sr = File.OpenText(selFile);                     !!!
                //    string artName = selFile.Substring(5, selFile.Length - 9);    !!!
                //    ASCIIArt art = new ASCIIArt(sr, artName);                     !!!
                //    OpenArt(art);                                                 !!!
                //}                                                                 !!!
            }
        }
    }
}
