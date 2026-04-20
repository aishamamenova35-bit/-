using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace ҚазақБике
{
    public partial class MainWindow : Window
    {
        // МОДЕЛЬДЕР 
        public class ArchiveFile
        {
            public List<CaseFile> files { get; set; } = new List<CaseFile>();
        }
        public class CaseFile
        {
     
            [JsonProperty("Аты/Имя")]
            public string FullName { get; set; }
            [JsonProperty("Іс/Дело")]
            public string Іс { get; set; }
            [JsonProperty("Дәлел/Доказательство")]
            public string Дәлел { get; set; }
            [JsonProperty("Талап арыз/Исковое заявление")]
            public string Талапарыз { get; set; }
            [JsonProperty("Құжаттар/Документы")]
            public string Құжаттар { get; set; }
            [JsonProperty("ИИН")]
            public string ИИН { get; set; }
            [JsonProperty("Телефон номер")]
            public string Тельномер { get; set; }
            [JsonProperty("Мекен жай/Адресс")]
            public string Адрессклиента {get; set;}
            [JsonProperty("Баж номері/Номер квитенции")]
            public string Баж { get; set; }

            [JsonProperty("Оқиға болған күн/Дата произошедшого события")]
            public DateTime Болғанкүн { get; set; }
            [JsonProperty("Сод болатын күн/Дата события Суда")]
            public DateTime Болатынкүн { get; set; }
            [JsonProperty("Сод мекен жайы/Адресс")]
            public string CourtAddres { get; set; }

        }

        public class Question
        {
            public string id { get; set; }
            public string type { get; set; }
            public TextContent text { get; set; }
            public string saveTo { get; set; }
            public string next { get; set; }
            public List<Answer> answers { get; set; }
        }

        public class TextContent
        {
            public string kz { get; set; }
            public string ru { get; set; }
        }

        public class Answer
        {
            public TextContent text { get; set; }
            public string saveTo { get; set; }
            public string next { get; set; }
            public string setLanguage { get; set; } // тіл таңдау үшін
        }
        //класс
        private List<Question> questions = new List<Question>();
        private int currentIndex = 0;
        private Dictionary<string, string> tempAnswers = new Dictionary<string, string>();
        private string currentLanguage = ""; // kz немесе ru

        //конструктор
        public MainWindow()
        {
            InitializeComponent();
            LoadQuestions();
            ShowQuestion(currentIndex);
        }
        //сұрақтар 
        private void LoadQuestions()
        {
            try
            {
                string path = "vopros.json"; // JSON файлы
                if (!File.Exists(path))
                {
                    MessageBox.Show("Файл vopros.json табылмады!");
                    Close();
                    return;
                }

                string json = File.ReadAllText(path);
                questions = JsonConvert.DeserializeObject<List<Question>>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки JSON: " + ex.Message);
            }
        }
        //сұрақты көрсету
        private void ShowQuestion(int index)
        {
           


            if (index < 0 || index >= questions.Count) return;

            Question q = questions[index];

            if (q.id == "by") // 2-ші сұрақта TextBox пен батырманы жасырамыз
            {
                Persontext.Visibility = Visibility.Collapsed;
                Dectiv.Visibility = Visibility.Collapsed;
            }
            else
            {
                Persontext.Visibility = q.type == "text" ? Visibility.Visible : Visibility.Collapsed;
                Dectiv.Visibility = q.type == "text" ? Visibility.Visible : Visibility.Collapsed;
            }
            // Текстті көрсету
            string displayText = q.text != null
                ? (string.IsNullOrEmpty(currentLanguage) ? q.text.kz : (currentLanguage == "ru" ? q.text.ru : q.text.kz))
                : "";


            // Placeholder ауыстыру
            foreach (var key in tempAnswers.Keys)
                displayText = displayText.Replace("{" + key + "}", tempAnswers[key]);

            Ainatext.Text = displayText;

            // Егер text сұрақ болса
            if (q.type == "text")
            {
                Stafftext.Children.Clear();
                Persontexts(q);
                Persontext.Text = tempAnswers.ContainsKey(q.saveTo) ? tempAnswers[q.saveTo] : "";
            }
            // Егер choice сұрақ болса
            else if (q.type == "choice")
            {
                Stafftext.Children.Clear();
                Persontexts(q);


                foreach (var ans in q.answers)
                {

                    Button b = new Button();
                    b.Content = string.IsNullOrEmpty(currentLanguage)

                    ? ans.text.kz // тіл әлі таңдалмаған болса қазақша көрсету
                    : (currentLanguage == "ru" ? ans.text.ru : ans.text.kz);

                    b.Style = (Style)FindResource("RoundButton"); // стильд қолданамыз

                    b.Margin = new Thickness(5);
                    b.FontSize = 12;

                    b.Click += (s, e) =>
                    {
                        // Тілді орнату
                        if (!string.IsNullOrEmpty(ans.setLanguage))
                            currentLanguage = ans.setLanguage;

                        // Жауапты сақтау
                        if (!string.IsNullOrEmpty(ans.saveTo))
                            tempAnswers[ans.saveTo] = currentLanguage == "ru" ? ans.text.ru : ans.text.kz;

                        // Келесі сұраққа өту
                        int nextIdx = questions.FindIndex(x => x.id == ans.next);
                        if (nextIdx >= 0)
                        {
                            currentIndex = nextIdx;
                            ShowQuestion(currentIndex);

                        }
                    };

                    Stafftext.Children.Add(b);

                }

                // TextBox бос қалдырылады



            }
            Persontext.Text = "";

        }
        private void Persontexts(Question q)
        {
            if (q.type == "choice")
            {
                Persontext.Visibility = Visibility.Collapsed;
            }
            else
            {
                Persontext.Visibility = Visibility.Visible;
            }
        }
        //жауап жазу
        private void Right_Click(object sender, RoutedEventArgs e)
        {
            Question q = questions[currentIndex];
            // TextBox-тен жауап сақтау
           /* if (q.type == "text" && !string.IsNullOrWhiteSpace(Persontext.Text))
            {
                tempAnswers[q.saveTo] = Persontext.Text;
            }*/


            if (q.type == "text")
            {
                if (string.IsNullOrWhiteSpace(Persontext.Text))
                {
                    MessageBox.Show("Пожалуйста, введите ответ!");
                    return;
                }
                tempAnswers[q.saveTo] = Persontext.Text;
            }

            if (!string.IsNullOrEmpty(q.next))
            {
                int nextIdx = questions.FindIndex(x => x.id == q.next);
                if (nextIdx >= 0)
                {
                    currentIndex = nextIdx;
                    ShowQuestion(currentIndex);
                }
            }
        }
        //белгісіз
        private void Left_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                ShowQuestion(currentIndex);
            }
        }
        private void Section_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string section = btn.Content.ToString();

            string message = "";
            foreach (var pair in tempAnswers)
                message += pair.Key + ": " + pair.Value + Environment.NewLine;

            MessageBox.Show("Ответы для секции " + section + ":\n" + message);
        }
        //архив сақтау
        private void Dectiv_Click(object sender, RoutedEventArgs e)
        {

            
            //ағымдағы сұрақ
            Question q = questions[currentIndex];
            // TextBox-тен жауап сақтау
            if (q.type == "text")
            {
                if (string.IsNullOrWhiteSpace(Persontext.Text))
                {
                    MessageBox.Show("Пожалуйста, введите ответ!");
                    return;
                }
                    tempAnswers[q.saveTo] = Persontext.Text;
                
            }


            SaveToArchive();

            //бкелесі сұрақ
            if (!string.IsNullOrEmpty(q.next))
            {
                int nextIdx = questions.FindIndex(x => x.id == q.next);
                if (nextIdx >= 0)
                {
                    currentIndex = nextIdx;
                    ShowQuestion(currentIndex);
                }
            }


            // MessageBox.Show("Іс сақталды!");
            //келесі сұрақ индексін табамыз

        }

        private void SaveToArchive()
        {
            string archivePath = "Archive.json";
            ArchiveFile archive;

            // Файлды оқу (өте маңызды!)
            if (File.Exists(archivePath))
            {
                string json = File.ReadAllText(archivePath);
                archive = JsonConvert.DeserializeObject<ArchiveFile>(json);

                if (archive == null)
                    archive = new ArchiveFile();
            }
            else
            {
                archive = new ArchiveFile();
            }

            // le newCase = new CaseFile
            CaseFile newCase = new CaseFile
            {
                FullName = tempAnswers.ContainsKey("FullName") ? tempAnswers["FullName"] : "",
                Іс = tempAnswers.ContainsKey("Іс") ? tempAnswers["Іс"] : "",
                Дәлел = tempAnswers.ContainsKey("Дәлел") ? tempAnswers["Дәлел"] : "",
                Талапарыз = tempAnswers.ContainsKey("Талапарыз") ? tempAnswers["Талапарыз"] : "",
                Құжаттар = tempAnswers.ContainsKey("Құжаттар") ? tempAnswers["Құжаттар"] : "",
                ИИН = tempAnswers.ContainsKey("ИИН") ? tempAnswers["ИИН"] : "",
                Тельномер = tempAnswers.ContainsKey("Тельномер") ? tempAnswers["Тельномер"] : "",
                Адрессклиента = tempAnswers.ContainsKey("Адрессклиента") ? tempAnswers["Адрессклиента"] : "",
                Баж = tempAnswers.ContainsKey("Баж") ? tempAnswers["Баж"] : ""
            };


            // Егер ИИН бос болса ноу
            if (string.IsNullOrWhiteSpace(newCase.Талапарыз))
            {
                //MessageBox.Show("ИИН ");
                return;
            }

            // Бар ма аа
            var existingCase = archive.files
                .FirstOrDefault(f => f.ИИН == newCase.ИИН);

            if (existingCase != null)
            {
                // жаңарту
                existingCase.FullName = newCase.FullName;
                existingCase.Іс = newCase.Іс;
                existingCase.Дәлел = newCase.Дәлел;
                existingCase.Талапарыз = newCase.Талапарыз;
                existingCase.Құжаттар = newCase.Құжаттар;
                existingCase.Тельномер = newCase.Тельномер;
                existingCase.Адрессклиента = newCase.Адрессклиента;
                existingCase.Баж = newCase.Баж;
            }
            else
            {
                // Егер жоқ болса қосу
                archive.files.Add(newCase);
            }

            //  Қайта жазу
            File.WriteAllText(archivePath,
                JsonConvert.SerializeObject(archive, Formatting.Indented));
        }

       
        

        //архив көрсету
        private void Arxiv_Click(object sender, RoutedEventArgs e)
        {
            string archivePath = "Archive.json";
            if (File.Exists(archivePath))
            {
                string json = File.ReadAllText(archivePath);
                ArchiveFile archive = JsonConvert.DeserializeObject<ArchiveFile>(json);
                MessageBox.Show("Барлық іс саны: " + archive.files.Count);
            }
            else
            {
                MessageBox.Show("Архив файл жоқ");
            }
        }
        private void ViewArchive_Click(object sender, RoutedEventArgs e)
        {
            string archivePath = "Archive.json";

            if (!File.Exists(archivePath))
            {
                MessageBox.Show("Архив файл табылмады!");
                return;
            }

            try
            {
                string json = File.ReadAllText(archivePath);
                ArchiveFile archive = JsonConvert.DeserializeObject<ArchiveFile>(json) ?? new ArchiveFile();

                if (archive.files.Count == 0)
                {
                    MessageBox.Show("Архив бос!");
                    return;
                }

                string message = "";
                int i = 1;
                foreach (var file in archive.files)
                {
                    message += $"Іс №{i}:\n";
                    message += $"Аты: {file.FullName}\n";
                    message += $"Іс: {file.Іс}\n";
                    message += $"Дәлел: {file.Дәлел}\n";
                    message += $"Талапарыз: {file.Талапарыз}\n";
                    message += $"Құжаттар: {file.Құжаттар}\n";
                    message += $"ИИН: {file.ИИН}\n";
                    message += $"Телефон: {file.Тельномер}\n";
                    message += $"Мекен жай: {file.Адрессклиента}\n";
                    message += $"Баж номері: {file.Баж}\n";
                    message += "--------------------------\n";
                    i++;
                }

                MessageBox.Show(message, "Архив");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Архив оқу кезінде қате: " + ex.Message);
            }
        }

    }
}
