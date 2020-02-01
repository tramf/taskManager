using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.IO;
using System.Globalization;

namespace TaskManager
{
    public class Task
    {
        public int id;
        public string title;
        public string description;
        public DateTime date;
        public Boolean complete;

        public void Print()
        {
            Console.WriteLine(("").PadRight(36, '-'));
            Console.WriteLine($" Task# {id} \n Date: {date}\n comlete: {complete}\n\t{title}\n {description}");
            Console.WriteLine(("").PadRight(36, '-'));
        }
    }
    public class TaskBase
    {
        private string Path = "TestDB.db";
        public List<Task> tasks;
        public TaskBase()
        {
            tasks = new List<Task>();
            if (!File.Exists(Path)) SQLiteConnection.CreateFile(Path);
            using (SQLiteConnection connection = new SQLiteConnection(String.Format("Data Source={0}", Path)))
            {
                string commandText = "CREATE TABLE IF NOT EXISTS [Tasks] ([id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, [title] TEXT, [description] TEXT, [date] TEXT, [complete] INTEGER)";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            using (SQLiteConnection connection = new SQLiteConnection(String.Format("Data Source={0}", Path)))
            {
                connection.Open();
                string commandText = "SELECT * FROM [Tasks] WHERE [title] NOT NULL";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                SQLiteDataReader sqlReader = command.ExecuteReader();
                while (sqlReader.Read())
                {
                    Task task = new Task();
                    task.id = Convert.ToInt32(sqlReader["id"]);
                    task.title = sqlReader["title"].ToString();
                    task.description = sqlReader["description"].ToString();
                    task.date = TakeDate(sqlReader["date"].ToString());
                    task.complete = (Convert.ToInt32(sqlReader["complete"]) == 1) ? true : false;
                    this.tasks.Add(task);
                }
                connection.Close();
            }
        }
        public static DateTime TakeDate (string dateValue)
        {
            string pattern = "dd.MM.YYYY";
            DateTime parsedDate;
            DateTime.TryParseExact(dateValue, pattern, null, DateTimeStyles.None, out parsedDate);
            return parsedDate;
        }
        public void Create(Task task)
        {
            tasks.Add(task);
            using (SQLiteConnection connection = new SQLiteConnection(String.Format("Data Source={0}", Path)))
            {
                string commandText = "INSERT INTO [Tasks] ([title],[description],[date],[complete]) VALUES (@title, @description, @date, @complete)";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                command.Parameters.AddWithValue("@title", task.title);
                command.Parameters.AddWithValue("@description", task.description);
                command.Parameters.AddWithValue("@date", task.date.ToString("dd.MM.YYYY"));
                command.Parameters.AddWithValue("@complete", 0);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            this.Update();
        }
        public void Delete(Task task)
        {
            tasks.Remove(task);
            using (SQLiteConnection connection = new SQLiteConnection(String.Format("Data Source={0}", Path)))
            {
                string commandText = "DELETE FROM [Tasks] WHERE [id]=@id";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                command.Parameters.AddWithValue("@id", task.id);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }
        public void Delete(int taskId)
        {
            Delete(this.GetTask(taskId));
        }
        public void Edit(Task task)
        {      
            using (SQLiteConnection connection = new SQLiteConnection(String.Format("Data Source={0}", Path)))
            {
                string commandText = "UPDATE [Tasks] SET [title] = @title, [description] = @description, [date] = @date, [complete] = @complete WHERE [id] = @id";
                SQLiteCommand Command = new SQLiteCommand(commandText, connection);
                Command.Parameters.AddWithValue("@title", "Task #2");
                Command.Parameters.AddWithValue("@description", task.description);
                Command.Parameters.AddWithValue("@date", task.date.ToString("dd.MM.YYYY"));
                Command.Parameters.AddWithValue("@complete", task.complete);
                Command.Parameters.AddWithValue("@id", task.id);
                connection.Open();
                Command.ExecuteNonQuery();
                connection.Close();
            }
        }
        public void Update()
        {
            tasks.Clear();
            using (SQLiteConnection connection = new SQLiteConnection(String.Format("Data Source={0}", Path)))
            {
                connection.Open();
                string commandText = "SELECT * FROM [Tasks] WHERE [title] NOT NULL";
                SQLiteCommand command = new SQLiteCommand(commandText, connection);
                SQLiteDataReader sqlReader = command.ExecuteReader();
                while (sqlReader.Read())
                {
                    Task task = new Task();
                    task.id = Convert.ToInt32(sqlReader["id"]);
                    task.title = sqlReader["title"].ToString();
                    task.description = sqlReader["description"].ToString();
                    task.date = TakeDate(sqlReader["date"].ToString());
                    task.complete = (Convert.ToInt32(sqlReader["complete"]) == 1) ? true : false;
                    this.tasks.Add(task);
                }
                connection.Close();
            }
        }
        public void Print()
        {
            this.Update();
            int i = 0;
            Console.WriteLine("\t\tВсе задания:\n");
            foreach (Task item in tasks)
            {
                item.Print();
                i++;
            }
            if (i == 0) Console.WriteLine("Список заданий пуст\n");
        }
        public void PrintActual()
        {
            int i = 0;
            Console.WriteLine("\t\tАктуальные задания:\n");
            foreach (Task item in tasks)
            {
                if ((item.complete == false) && (item.date >= DateTime.Today))
                {
                    item.Print();
                    i++;
                }
            }
            if (i == 0) Console.WriteLine("Нет актуальных заданий\n");
        }
        public void PrintToday()                          
        {
            Console.WriteLine("\t\tЗадания на сегодня:\n");
            int i=0;
            foreach (Task item in tasks)
            {
                if (item.date == DateTime.Today)
                { 
                    item.Print();
                    i++;
                }
            }
            if (i == 0) Console.WriteLine("Сегодня нет заданий\n");
        }
        public void DeleteAllCompleted()
        {
            var temp = tasks.Where(item => item.complete == true).Select(item => item).ToList();
            foreach (var i in temp) if (i.complete == true) Delete(i);
            Console.WriteLine("\tВсе завершённые задания удалены.\n");
        }
        public void DeleteAllOld()
        {   
            var temp = tasks.Where(item => item.date < DateTime.Today).Select(item => item).ToList();
            foreach (var i in temp) Delete(i);
            Console.WriteLine("\tВсе старые задания удалены.\n");
        }
        public Task GetTask(int taskId) 
        {
            Task task = tasks.Where(item => item.id == taskId).Select(item => item).FirstOrDefault();
            return task; 
        }
    }
    class Program
    {
        static void Main(string[] args)
        {   
            TaskBase database = new TaskBase();
            MenuUtils.MainMenu(database);
        }
    }
    public class MenuUtils
    {
        //int lang;
        public static void MainMenu(TaskBase db)
        {
            Console.WriteLine("\n\t\tВведите команду:");
            Console.WriteLine("\tS - Показать все задания");
            Console.WriteLine("\tT - Показать задания на сегодня");
            Console.WriteLine("\tA - Показать все актуальные задания\n");
            Console.WriteLine("\tC - Создать новое задание");
            Console.WriteLine("\tE - Редактировать задания\n");
            Console.WriteLine("\tD - Удалить все выполненные задания");
            Console.WriteLine("\tO - Удалить все просроченные задания\n");
            Console.WriteLine("\tX - Выход\n");
            MainCom(db);
        }
        public static void MainCom(TaskBase db)
        {
            string s = Console.ReadLine().ToUpper();
            s += "Q";
            char cmd = s[0];
            switch (cmd)
            {
                case 'S':
                    db.Print();
                    MainMenu(db);
                    break;
                case 'T':
                    db.PrintToday();
                    MainMenu(db);
                    break;
                case 'A':
                    db.PrintActual();
                    MainMenu(db);
                    break;
                case 'C':
                    CreateMenu(db);
                    MainMenu(db);
                    break;
                case 'E':
                    EditMenu(db);
                    MainMenu(db);
                    break;
                case 'D':
                    db.DeleteAllCompleted();
                    MainMenu(db);
                    break;
                case 'O':
                    db.DeleteAllOld();
                    MainMenu(db);
                    break;
                case 'X':
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("\tВведена неизвестная команда.\n");
                    MainMenu(db);
                    break;
            }
        }
        public static void CreateMenu(TaskBase db)
        {
            Task temp = new Task();
            Console.WriteLine("\t\tСоздание нового задания.");
            Console.WriteLine("\tВведите название задания");
            temp.title = Console.ReadLine();
            Console.WriteLine("\tВВедите дату в формате dd.MM.YYYY");
            temp.date = CreateDate();
            Console.WriteLine("\tВведите описание задания.");
            temp.description = Console.ReadLine();
            db.Create(temp);
            Console.WriteLine($"\n\tЗадание '{temp.title}' добавлено в список задач");
        }
        public static void EditMenu(TaskBase db)
        {
            bool correctId = false;
            Task temp;
            int tempId=0;
            while (!correctId)
            {
                Console.WriteLine("\tВведите номер существующего задания.");
                bool parseRes = int.TryParse(Console.ReadLine(), out tempId);
                while (!parseRes)
                {
                    Console.WriteLine("\tНекорректный формат.\nВведите номер редактируемого задания.");
                    parseRes = int.TryParse(Console.ReadLine(), out tempId);
                }
                if (db.GetTask(tempId) != null) correctId = true;
            }
            temp = db.GetTask(tempId);
            Console.Clear();
            Console.WriteLine($"\tРедактируемое задание: ");
            temp.Print();
            TaskEdit(temp);
            db.Edit(temp);
        }
        public static void TaskEdit(Task task)
        {
            bool exit = false;
            while (!exit) 
            {
                Console.WriteLine("\n\t\tВыберите объект редактирования:");
                Console.WriteLine("\tT - Изменить название");
                Console.WriteLine("\tD - Изменить дату");
                Console.WriteLine("\tE - Изменить описание");
                Console.WriteLine("\tP - Дополнить описание");
                Console.WriteLine("\tC - Пометить выполненным");
                Console.WriteLine("\tX - Назад\n");
                switch (Console.ReadLine().ToUpper())
                {
                    case "T":
                        Console.WriteLine("\tВведите новое название");
                        task.title = Console.ReadLine();
                        break;
                    case "D":
                        Console.WriteLine("\tВведите новую дату");
                        task.date = CreateDate();
                        break;
                    case "E":
                        Console.WriteLine("\tВведите новое описание");
                        task.description = Console.ReadLine();
                        break;
                    case "P":
                        Console.WriteLine("\tВведите дополнения к описанию");
                        string t = Console.ReadLine();
                        task.description = String.Concat(task.description, " ", t);
                        break;
                    case "C":
                        Console.WriteLine("\tВведите новую дату");
                        task.complete = true;
                        break;
                    case "X":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("\tВведена неизвестная команда. Повторите ввод\n");
                        break;
                }
                task.Print();
            }
        }
        public static DateTime CreateDate()
        {
            string tempDate = Console.ReadLine();
            string pattern = "dd.MM.yyyy";
            DateTime parsedDate;
            bool resultParse = DateTime.TryParseExact(tempDate, pattern, null, DateTimeStyles.None, out parsedDate);
            while (!resultParse)
            {
                Console.WriteLine("\tНекорректный формат даты. ВВедите дату в формате dd.MM.YYYY.");
                tempDate = Console.ReadLine();
                resultParse = DateTime.TryParseExact(tempDate, pattern, null, DateTimeStyles.None, out parsedDate);
            }
            return parsedDate;
        }

    }
}
