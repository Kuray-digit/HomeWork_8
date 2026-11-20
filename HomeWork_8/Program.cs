
using System;
using System.Collections.Generic;
using System.Linq;

namespace Program
{
    enum ProjectStatus
    {
        Project,
        Execution,
        Closed
    }

    enum TaskStatus
    {
        Assigned,
        InProgress,
        OnReview,
        Done
    }

    class Person
    {
        public string Name { get; }
        public string Role { get; }

        public Person(string name, string role)
        {
            Name = name;
            Role = role;
        }

        public override string ToString() => $"{Name} ({Role})";
    }

    class Report
    {
        public string Text { get; }
        public DateTime CompletedAt { get; }
        public Person Author { get; }

        public Report(string text, Person author)
        {
            Text = text;
            Author = author;
            CompletedAt = DateTime.Now;
        }

        public override string ToString() =>
            $"Отчёт от {CompletedAt:dd.MM.yyyy HH:mm} | исполнитель: {Author.Name} | {Text}";
    }

    class ProjectTask
    {
        public string Description { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public Person Initiator { get; }
        public Person Assignee { get; private set; }
        public TaskStatus Status { get; private set; }
        public Report Report { get; private set; }

        public ProjectTask(string description, DateTime start, DateTime end, Person initiator)
        {
            Description = description;
            StartDate = start;
            EndDate = end;
            Initiator = initiator;
            Assignee = null;
            Status = TaskStatus.Assigned;
        }

        public void Assign(Person person)
        {
            Assignee = person;
            Status = TaskStatus.Assigned;
        }

        public void TakeToWork(Person by)
        {
            if (Assignee != by)
                throw new InvalidOperationException("Взять в работу может только назначенный исполнитель.");
            if (Status != TaskStatus.Assigned)
                throw new InvalidOperationException("В работу можно взять только задачу в статусе «Назначена».");
            Status = TaskStatus.InProgress;
        }

        public void Delegate(Person to)
        {
            if (Assignee == null)
                throw new InvalidOperationException("Нельзя делегировать задачу без исполнителя.");
            if (Status != TaskStatus.Assigned && Status != TaskStatus.InProgress)
                throw new InvalidOperationException("Делегировать можно только назначенную или взятую в работу задачу.");
            Assignee = to;
            Status = TaskStatus.Assigned;
        }

        public void Reject(Person by)
        {
            if (Assignee != by)
                throw new InvalidOperationException("Отклонить задачу может только назначенный исполнитель.");
            Assignee = null;
            Status = TaskStatus.Assigned;
        }

        public void SubmitReport(string text, Person by)
        {
            if (Assignee != by)
                throw new InvalidOperationException("Отчёт может сдать только исполнитель задачи.");
            if (Status != TaskStatus.InProgress)
                throw new InvalidOperationException("Отчёт можно сдать только по задаче в работе.");
            Report = new Report(text, by);
            Status = TaskStatus.OnReview;
        }

        public void ApproveReport(Person by)
        {
            if (Initiator != by)
                throw new InvalidOperationException("Утвердить отчёт может только инициатор задачи.");
            if (Status != TaskStatus.OnReview)
                throw new InvalidOperationException("Утвердить можно только отчёт на проверке.");
            Status = TaskStatus.Done;
        }

        public void ReturnReport(Person by)
        {
            if (Initiator != by)
                throw new InvalidOperationException("Вернуть отчёт может только инициатор задачи.");
            if (Status != TaskStatus.OnReview)
                throw new InvalidOperationException("Вернуть можно только отчёт на проверке.");
            Status = TaskStatus.InProgress;
        }

        public override string ToString() =>
            $"[{Status}] {Description} | срок: {StartDate:dd.MM.yyyy}–{EndDate:dd.MM.yyyy} | " +
            $"исполнитель: {(Assignee?.Name ?? "—")} | отчёт: {(Report == null ? "нет" : "есть")}";
    }

    class Project
    {
        public string Description { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public Person Initiator { get; }
        public Person TeamLead { get; }
        public List<ProjectTask> Tasks { get; } = new List<ProjectTask>();
        public ProjectStatus Status { get; private set; }

        public Project(string description, DateTime start, DateTime end, Person initiator, Person teamLead)
        {
            Description = description;
            StartDate = start;
            EndDate = end;
            Initiator = initiator;
            TeamLead = teamLead;
            Status = ProjectStatus.Project;
        }

        public void AddTask(ProjectTask task)
        {
            if (Status != ProjectStatus.Project)
                throw new InvalidOperationException("Задачи можно создавать только в статусе проекта «Проект».");
            Tasks.Add(task);
        }

        public void StartExecution()
        {
            if (Status != ProjectStatus.Project)
                throw new InvalidOperationException("Перевести в исполнение можно только проект в статусе «Проект».");
            Status = ProjectStatus.Execution;
        }

        public bool AllTasksDone =>
            Tasks.Count > 0 && Tasks.All(t => t.Status == TaskStatus.Done);

        public void TryClose()
        {
            if (Status != ProjectStatus.Execution)
                throw new InvalidOperationException("Закрыть можно только проект в исполнении.");
            if (!AllTasksDone)
                throw new InvalidOperationException("Нельзя закрыть проект: не все задачи выполнены.");
            Status = ProjectStatus.Closed;
        }

        public override string ToString() =>
            $"[{Status}] {Description} | срок: {StartDate:dd.MM.yyyy}–{EndDate:dd.MM.yyyy} | " +
            $"заказчик: {Initiator.Name} | тимлид: {TeamLead.Name} | задач: {Tasks.Count}";
    }

    class TaskManager
    {
        private readonly List<Person> people = new List<Person>();
        private readonly List<Project> projects = new List<Project>();
        private Person currentUser;

        public void Seed()
        {
            string[] names =
            {
                "Анна", "Борис", "Виктор", "Галина", "Дмитрий",
                "Елена", "Жанна", "Игорь", "Ксения", "Леонид"
            };
            string[] roles =
            {
                "Тимлид", "Разработчик", "Разработчик", "Тестировщик", "Аналитик",
                "Разработчик", "Дизайнер", "DevOps", "Тестировщик", "Разработчик"
            };

            for (int i = 0; i < names.Length; i++)
                people.Add(new Person(names[i], roles[i]));

            Person customer = new Person("Заказчик", "Инициатор");

            Person lead = people[0];
            Project project = new Project(
                "Внутренний Task Manager",
                DateTime.Today,
                DateTime.Today.AddMonths(1),
                customer,
                lead);
            projects.Add(project);

            for (int i = 1; i < people.Count; i++)
            {
                Person initiator = lead; 
                ProjectTask task = new ProjectTask(
                    $"Задача для {people[i].Name}: реализовать модуль #{i}",
                    DateTime.Today,
                    DateTime.Today.AddDays(7 * i),
                    initiator);
                project.AddTask(task);
                task.Assign(people[i]);
            }

            ProjectTask leadTask = new ProjectTask(
                "Задача тимлида: код-ревью всех модулей",
                DateTime.Today,
                DateTime.Today.AddDays(14),
                lead);
            project.AddTask(leadTask);
            leadTask.Assign(lead);

            project.StartExecution();
        }

        public void Run()
        {
            Seed();
            ChooseUser();

            bool running = true;
            while (running)
            {
                Console.WriteLine($"\n=== Task Manager | вы: {currentUser} ===");
                Console.WriteLine("1. Список проектов");
                Console.WriteLine("2. Список моих задач");
                Console.WriteLine("3. Взять задачу в работу");
                Console.WriteLine("4. Делегировать задачу");
                Console.WriteLine("5. Отклонить задачу");
                Console.WriteLine("6. Сдать отчёт по задаче");
                Console.WriteLine("7. Утвердить отчёт");
                Console.WriteLine("8. Вернуть отчёт на доработку");
                Console.WriteLine("9. Сменить пользователя");
                Console.WriteLine("10. Попробовать закрыть проект");
                Console.WriteLine("0. Выход");
                Console.Write("Выбор: ");

                string choice = Console.ReadLine() ?? "";
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1": ShowProjects(); break;
                        case "2": ShowMyTasks(); break;
                        case "3": TakeToWork(); break;
                        case "4": DelegateTask(); break;
                        case "5": RejectTask(); break;
                        case "6": SubmitReport(); break;
                        case "7": ApproveReport(); break;
                        case "8": ReturnReport(); break;
                        case "9": ChooseUser(); break;
                        case "10": TryCloseProject(); break;
                        case "0": running = false; break;
                        default: Console.WriteLine("Неверный выбор."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
            Console.WriteLine("До свидания!");
        }

        private void ChooseUser()
        {
            Console.WriteLine("Выберите пользователя:");
            for (int i = 0; i < people.Count; i++)
                Console.WriteLine($"{i + 1}. {people[i]}");
            Console.Write("Номер: ");
            int idx = ReadInt();
            if (idx < 1 || idx > people.Count)
            {
                Console.WriteLine("Нет такого пользователя, выбран первый.");
                currentUser = people[0];
            }
            else currentUser = people[idx - 1];
            Console.WriteLine($"Текущий пользователь: {currentUser}");
        }

        private void ShowProjects()
        {
            if (projects.Count == 0) { Console.WriteLine("Проектов нет."); return; }
            for (int i = 0; i < projects.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {projects[i]}");
                foreach (ProjectTask t in projects[i].Tasks)
                    Console.WriteLine($"    - {t}");
            }
        }

        private void ShowMyTasks()
        {
            bool any = false;
            foreach (Project p in projects)
            {
                foreach (ProjectTask t in p.Tasks)
                {
                    if (t.Assignee == currentUser || t.Initiator == currentUser)
                    {
                        Console.WriteLine($"{p.Description} -> {t}");
                        any = true;
                    }
                }
            }
            if (!any) Console.WriteLine("У вас нет связанных задач.");
        }

        private ProjectTask PickTask(Func<ProjectTask, bool> predicate, string prompt)
        {
            var list = new List<ProjectTask>();
            foreach (Project p in projects)
                foreach (ProjectTask t in p.Tasks)
                    if (predicate(t)) list.Add(t);

            if (list.Count == 0) { Console.WriteLine("Подходящих задач нет."); return null; }

            Console.WriteLine(prompt);
            for (int i = 0; i < list.Count; i++)
                Console.WriteLine($"{i + 1}. {list[i]}");
            Console.Write("Номер: ");
            int idx = ReadInt();
            if (idx < 1 || idx > list.Count) { Console.WriteLine("Нет такого номера."); return null; }
            return list[idx - 1];
        }

        private void TakeToWork()
        {
            ProjectTask t = PickTask(
                x => x.Assignee == currentUser && x.Status == TaskStatus.Assigned,
                "Ваши назначенные задачи:");
            if (t == null) return;
            t.TakeToWork(currentUser);
            Console.WriteLine("Задача взята в работу.");
        }

        private void DelegateTask()
        {
            ProjectTask t = PickTask(
                x => x.Assignee == currentUser && (x.Status == TaskStatus.Assigned || x.Status == TaskStatus.InProgress),
                "Ваши задачи для делегирования:");
            if (t == null) return;

            Console.WriteLine("Кому делегировать?");
            for (int i = 0; i < people.Count; i++)
                Console.WriteLine($"{i + 1}. {people[i]}");
            Console.Write("Номер: ");
            int idx = ReadInt();
            if (idx < 1 || idx > people.Count) { Console.WriteLine("Нет такого человека."); return; }
            t.Delegate(people[idx - 1]);
            Console.WriteLine($"Делегировано на {people[idx - 1].Name}.");
        }

        private void RejectTask()
        {
            ProjectTask t = PickTask(
                x => x.Assignee == currentUser && x.Status == TaskStatus.Assigned,
                "Ваши назначенные задачи для отклонения:");
            if (t == null) return;
            t.Reject(currentUser);
            Console.WriteLine("Задача отклонена. Исполнитель снят.");
        }

        private void SubmitReport()
        {
            ProjectTask t = PickTask(
                x => x.Assignee == currentUser && x.Status == TaskStatus.InProgress,
                "Задачи в работе (для отчёта):");
            if (t == null) return;
            Console.Write("Текст отчёта: ");
            string text = Console.ReadLine() ?? "";
            t.SubmitReport(text, currentUser);
            Console.WriteLine("Отчёт отправлен на проверку инициатору.");
        }

        private void ApproveReport()
        {
            ProjectTask t = PickTask(
                x => x.Initiator == currentUser && x.Status == TaskStatus.OnReview,
                "Отчёты на проверке (ваши задачи как инициатора):");
            if (t == null) return;
            t.ApproveReport(currentUser);
            Console.WriteLine("Отчёт утверждён. Задача выполнена.");
        }

        private void ReturnReport()
        {
            ProjectTask t = PickTask(
                x => x.Initiator == currentUser && x.Status == TaskStatus.OnReview,
                "Отчёты на проверке:");
            if (t == null) return;
            t.ReturnReport(currentUser);
            Console.WriteLine("Отчёт возвращён на доработку.");
        }

        private void TryCloseProject()
        {
            if (projects.Count == 0) { Console.WriteLine("Проектов нет."); return; }
            Console.WriteLine("Выберите проект:");
            for (int i = 0; i < projects.Count; i++)
                Console.WriteLine($"{i + 1}. {projects[i]}");
            Console.Write("Номер: ");
            int idx = ReadInt();
            if (idx < 1 || idx > projects.Count) { Console.WriteLine("Нет такого проекта."); return; }
            projects[idx - 1].TryClose();
            Console.WriteLine("Проект закрыт.");
        }

        private static int ReadInt()
        {
            while (true)
            {
                string s = Console.ReadLine() ?? "";
                if (int.TryParse(s, out int r)) return r;
                Console.Write("Введите целое число: ");
            }
        }
    }

    class HomeWork8
    {
        static void Main()
        {
            new TaskManager().Run();
        }
    }
}