using System;
using System.Collections.Generic;

namespace Program
{
    class Song
    {
        string name;
        string author;
        Song prev;

        public Song()
        {
            this.name = null;
            this.author = null;
            this.prev = null;
        }

        public Song(string name, string author)
        {
            this.name = name;
            this.author = author;
            this.prev = null;
        }

        public Song(string name, string author, Song prevSong)
        {
            this.name = name;
            this.author = author;
            this.prev = prevSong;
        }

        public void SetName(string name)
        {
            this.name = name;
        }
        public void SetAuthor(string author)
        {
            this.author = author;
        }
        public void SetPrev(Song prevSong)
        {
            this.prev = prevSong;
        }
        public string Title()
        {
            return $"{name} — {author}";
        }
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Song))
                return false;

            Song other = (Song)obj;

            return this.name == other.name && this.author == other.author;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, author);
        }
    }

    class HomeWork8
    {
        public static void Main()
        {
            List<Song> songs = new List<Song>();

            Song song1 = new Song();
            song1.SetName("Bohemian Rhapsody");
            song1.SetAuthor("Queen");
            songs.Add(song1);

            Song song2 = new Song();
            song2.SetName("Bohemian Rhapsody");
            song2.SetAuthor("The Beatles");
            song2.SetPrev(song1);
            songs.Add(song2);

            Song song3 = new Song();
            song3.SetName("Imagine");
            song3.SetAuthor("John Lennon");
            song3.SetPrev(song2);
            songs.Add(song3);

            Song song4 = new Song();
            song4.SetName("Hotel California");
            song4.SetAuthor("Eagles");
            song4.SetPrev(song3);
            songs.Add(song4);

            Console.WriteLine("Список песен:");
            foreach (Song song in songs)
            {
                Console.WriteLine(song.Title());
            }

            Console.WriteLine("\nСравнение первой и второй песни:");
            bool areEqual = songs[0].Equals(songs[1]);

            if (areEqual)
            {
                Console.WriteLine("Песни одинаковые (совпадают названия и авторы).");
            }
            else
            {
                Console.WriteLine("Песни разные.");
            }

            Console.WriteLine("\nПроверка конструкторов:");
            Song mySong = new Song();
            Console.WriteLine($"Создан пустой объект mySong: {mySong.Title()}");

            Song song5 = new Song("Yesterday", "The Beatles");
            Console.WriteLine($"Создана песня: {song5.Title()}");

            Song song6 = new Song("Let It Be", "The Beatles", song5);
            Console.WriteLine($"Создана песня: {song6.Title()}");
        }
    }
}