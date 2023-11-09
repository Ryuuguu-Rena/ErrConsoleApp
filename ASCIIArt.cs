using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ErrConsoleApp
{
    internal class ASCIIArt //ширина, высота
    {
        private string _Art;
        private List<string> _NotSavedArt = new List<string>();
        private string _Title;
        private int _Height;
        private int _Width;
        public int Width
        {
            get { return _Width; }
        }
        public int Height
        {
            get { return _Height; }
        }
        public string Art
        {
            get { return _Art; }
        }
        public string Title
        {
            get { return _Title; }
            set 
            {
                File.Move("arts/" + _Title + ".txt", "arts/" + value + ".txt");
                _Title = value; 
            }
        }
        public ASCIIArt(StreamReader sr, string title) //читает арт
        {
            for (string s; (s = sr.ReadLine()) != null; _Height++)
            {
                _Art += s + "\n";
                _NotSavedArt.Add(s);
                if (s.Length > _Width)
                    _Width = s.Length;
            }
            sr.Close();
            _Title = title;
        }
        public ASCIIArt(string title) //создаёт арт
        {
            _Art = " ";
            _NotSavedArt.Add(" ");
            _Title = title;
            StreamWriter sw = File.CreateText("arts/" + _Title + ".txt");
            sw.Write(" ");
            sw.Close();
            _Width = _Height = 1;
        }
        public int[] CalculateWH()
        {
            _Height = _NotSavedArt.Count;
            _Width = 1;
            foreach (string s in _NotSavedArt)
            {
                if (s.Length > _Width)
                    _Width = s.Length;
            }
            return new int[] { _Width, _Height };
        }
        public void UpdateArt(string ch, int x, int y) //изменение символа в арте
        {
            string s;
            if (_NotSavedArt.Count > y) //в диапозоне
                s = _NotSavedArt[y];
            else //вне диапозона
            {
                while (_NotSavedArt.Count <= y)
                    _NotSavedArt.Add("");
                s = _NotSavedArt[y];
            }

            if (s.Length > x) //в диапозоне
                s = s.Remove(x, 1).Insert(x, ch);
            else //вне диапозона
            {
                string space = "";
                while (s.Length < x)
                {
                    space += " "; 
                    x--;
                }
                s = s + space + ch;
            }

            if (y == 0 && s.TrimEnd() == "") //оставить пробел на первой строке
                _NotSavedArt[y] = s.TrimEnd() + " ";
            else
                _NotSavedArt[y] = s.TrimEnd(); //убирает пробелы справа от текста

            for (int i = _NotSavedArt.Count - 1; _NotSavedArt[i].TrimEnd() == "" && i > 0; i--)
                _NotSavedArt.RemoveAt(i); //убирает пустые строки в списке
        }
        public void Save(int width, int height) //выход с сохранением арта
        {
            _Height = height;
            _Width = width;
            _Art = "";
            foreach (string s in _NotSavedArt)
                _Art += s + "\n";
            StreamWriter sw = File.CreateText("arts/" + _Title + ".txt");
            sw.Write(_Art);
            sw.Close();
        }
        public void NoSave() //выход без сохранения арта
        {
            _NotSavedArt = _Art.Split('\n').ToList();
        }
        public void Delete() //удаляет арт
        {
            File.Delete("arts/" + _Title + ".txt");
        }
    }
}
