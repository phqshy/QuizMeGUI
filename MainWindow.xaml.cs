using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using QuizMe.Containers;
using QuizMe.Core;

namespace QuizMe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private QuizletData? data;
        private Random rnd = new Random();
        private MultipleChoice? multipleChoice;
        private Learn? learn;
        private WindowManager manager;

        private readonly string CACHE = Directory.GetCurrentDirectory() + "\\cache";
        private List<string> cacheFiles;
        int cacheIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
            initializeFiles();

            this.manager = new WindowManager(this);
        }

        private async void LoadQuizletURL_Click(object sender, RoutedEventArgs e)
        {
            data = new QuizletData(QuizletURL.Text);
            await data.init();
            currentURLBlock.Text = "Loaded: " + QuizletURL.Text;
            QuizletURL.Text = "";

            string fileName = data.fetchUrlDescription() + ".quiz";

            using FileStream createStream = File.Create(CACHE + "\\" + fileName);
            await JsonSerializer.SerializeAsync(createStream, data, new JsonSerializerOptions { WriteIndented = true });
            await createStream.DisposeAsync();
        }

        private void LearnButton_Click(object sender, RoutedEventArgs e)
        {
            if (data == null)
            {
                return;
            }

            this.learn = new LearnFactory()
                .setInputBox(LearnModeAnswer)
                .setTitleBlock(LearnModeQuestion)
                .setData(data.Data)
                .setAnswerBlock(LearnModeCorrectAnswer)
                .Build();

            learn.nextQuestion();

            manager.swapWindow(WindowStates.Learn);
        }

        private async void MCOption1_Click(object sender, RoutedEventArgs e)
        {
            onMultipleChoiceSelected(MCOption1);
        }

        private async void MCOption2_Click(object sender, RoutedEventArgs e)
        {
            onMultipleChoiceSelected(MCOption2);
        }
        private async void MCOption3_Click(object sender, RoutedEventArgs e)
        {
            onMultipleChoiceSelected(MCOption3);
        }
        private async void MCOption4_Click(object sender, RoutedEventArgs e)
        {
            onMultipleChoiceSelected(MCOption4);
        }

        private async void onMultipleChoiceSelected(Button input)
        {
            if (this.multipleChoice.isWaiting) return;
            this.multipleChoice.isWaiting = true;
            if (!multipleChoice.verifyAnswer(input.Content.ToString().Substring(3)))
            {
                input.Background = Brushes.Red;
                multipleChoice.markButtonAsCorrect();
            } else
            {
                input.Background= Brushes.LightGreen;
            }

            await Task.Delay(1000);

            this.multipleChoice.isWaiting = false;
            multipleChoice.nextQuestion();

        }

        private void MultipleChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (data == null) return;

            manager.swapWindow(WindowStates.MultipleChoice);

            multipleChoice = new MultipleChoice(MCQuestion, MCOption1, MCOption2, MCOption3, MCOption4, data.Data);
            multipleChoice.nextQuestion();
        }

        private void SwapTerms_Click(object sender, RoutedEventArgs e)
        {
            if (this.multipleChoice.isWaiting) return;
            multipleChoice.swapTermsAndDefinitions(data);
        }

        private void initializeFiles()
        {
            if (!Directory.Exists(CACHE)) Directory.CreateDirectory(CACHE);
        }

        private void loadCacheDirectory()
        {
            this.cacheFiles = Directory.EnumerateFiles(CACHE).Where(s => s.EndsWith(".quiz")).ToList();
        }

        private void CacheScreenButton_Click(object sender, RoutedEventArgs e)
        {
            manager.swapWindow(WindowStates.LoadCache);
            loadCacheDirectory();
            cacheIndex = 0;
            CacheCurrentSelection.Text = stripCacheText(cacheFiles.FirstOrDefault());
            Trace.WriteLine(cacheFiles.FirstOrDefault());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            manager.swapWindow(WindowStates.Home);
        }

        private void CacheLoadButton_Click(object sender, RoutedEventArgs e)
        {
            data = JsonSerializer.Deserialize<QuizletData>(File.ReadAllText(CACHE + "\\" + CacheCurrentSelection.Text));
            currentURLBlock.Text = "Loaded: " + data.url;
            manager.swapWindow(WindowStates.Home);
        }

        private void CycleCacheButton_Click(object sender, RoutedEventArgs e)
        {
            cacheIndex++;
            if (cacheIndex >= cacheFiles.Count) cacheIndex = 0;
            CacheCurrentSelection.Text = stripCacheText(cacheFiles[cacheIndex]);
        }

        private string stripCacheText(string element)
        {
            if (element == null) return "";
            Regex regex = new Regex("([a-zA-Z]+(-[a-zA-Z]+)+)\\.quiz");
            Match match = regex.Match(element);
            return match.Value;
        }

        private async void LearnSubmitButton_Click(object sender, RoutedEventArgs e)
        {
            float confidence = learn.verifyAnswer(LearnModeAnswer.Text);
            if (confidence >= .9f)
            {
                LearnModeQuestion.Text = "Correct!";
                await Task.Delay(2 * 1000);
                learn.nextQuestion();
                return;
            }

            if (confidence >= .8f)
            {
                LearnModeQuestion.Text = "I can't tell if you're right or not";
                learn.displayCorrectAnswer();
                await Task.Delay(2 * 1000);
                learn.nextQuestion();
                return;
            }

            LearnModeQuestion.Text = "Incorrect :(";
            learn.displayCorrectAnswer();
            await Task.Delay(2 * 1000);
            learn.nextQuestion();
            return;
        }
    }
}
