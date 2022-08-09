using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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


namespace QuizMe.Core
{
    internal class MultipleChoice
    {
        private readonly TextBlock questionText;
        private readonly Button answer1;
        private readonly Button answer2;
        private readonly Button answer3;
        private readonly Button answer4;
        private Dictionary<string, string> dataset;
        private readonly Random rnd;
        private Queue<string> randomTermsList;
        private string? currentAnswer;
        private Button? currentButton;
        public bool isSwapped { get; private set; }
        public bool isWaiting { get; set; }

        /**
         * Creates a new multiple choice game with the given buttons and text blocks.
         * @param 
         */
        public MultipleChoice(TextBlock questionText, Button answer1, Button answer2, Button answer3, Button answer4, Dictionary<string, string> dataset)
        {
            this.questionText = questionText;
            this.answer1 = answer1;
            this.answer2 = answer2;
            this.answer3 = answer3;
            this.answer4 = answer4;
            this.dataset = dataset;
            this.rnd = new Random();
            this.isSwapped = false;
            randomTermsList = shuffleData(dataset);
        }

        public Queue<string> shuffleData(Dictionary<string, string> set)
        {
            return new Queue<string>(set.Keys.OrderBy(x => rnd.Next()).ToList());
        }

        public void nextQuestion()
        {
            if (randomTermsList.Count == 0)
            {
                randomTermsList = shuffleData(dataset);
            }

            string term = randomTermsList.Dequeue();
            this.questionText.Text = term;

            int pos = rnd.Next(1, 5);
            currentAnswer = dataset[term];

            resetButton();

            switch (pos)
            {
                case 1:
                    this.currentButton = answer1;
                    this.answer1.Content = $"1. {currentAnswer}";
                    this.answer2.Content = $"2. {dataset.Values.ElementAt(rnd.Next(0, dataset.Values.Count))}";
                    this.answer3.Content = $"3. {dataset.Values.ElementAt(rnd.Next(0, dataset.Values.Count))}";
                    this.answer4.Content = $"4. {dataset.Values.ElementAt(rnd.Next(0, dataset.Values.Count))}";
                    break;
                case 2:
                    this.currentButton = answer2;
                    this.answer2.Content = $"2. {currentAnswer}";
                    this.answer1.Content = $"1. {dataset.Values.ElementAt(rnd.Next(0, dataset.Values.Count))}";
                    this.answer3.Content = $"3. {dataset.Values.ElementAt(rnd.Next(0, dataset.Values.Count))}";
                    this.answer4.Content = $"4. {dataset.Values.ElementAt(rnd.Next(0, dataset.Values.Count))}";
                    break;
                case 3:
                    this.currentButton = answer3;
                    this.answer3.Content = $"3. {currentAnswer}";
                    this.answer2.Content = $"2. {dataset.Values.ElementAt(rnd.Next(0, dataset.Values.Count))}";
                    this.answer1.Content = $"1. {dataset.Values.ElementAt(rnd.Next(0, dataset.Values.Count))}";
                    this.answer4.Content = $"4. {dataset.Values.ElementAt(rnd.Next(0, dataset.Values.Count))}";
                    break;
                case 4:
                    this.currentButton = answer4;
                    this.answer4.Content = $"4. {currentAnswer}";
                    this.answer2.Content = $"2. {dataset.Values.ElementAt(rnd.Next(0, dataset.Values.Count))}";
                    this.answer3.Content = $"3. {dataset.Values.ElementAt(rnd.Next(0, dataset.Values.Count))}";
                    this.answer1.Content = $"1. {dataset.Values.ElementAt(rnd.Next(0, dataset.Values.Count))}";
                    break;
            }
        }

        /**
         * Returns true if the answer is correct, and false otherwise.
         */
        public bool verifyAnswer(string input)
        {
            if (input.Equals(currentAnswer))
            {
                return true;
            }
            return false;
        }

        private void resetButton()
        {
            this.answer1.Background = Brushes.Transparent;
            this.answer2.Background = Brushes.Transparent;
            this.answer3.Background = Brushes.Transparent;
            this.answer4.Background = Brushes.Transparent;
        }

        public void markButtonAsCorrect()
        {
            this.currentButton.Background = Brushes.LightGreen;
        }

        public void swapTermsAndDefinitions(QuizletData container)
        {
            if (isSwapped)
            {
                this.dataset = container.Data;
                this.isSwapped = false;
                randomTermsList = shuffleData(dataset);
                nextQuestion();
                return;
            }

            this.dataset = container.swapTermsAndDefinitions();
            randomTermsList = shuffleData(dataset);
            this.isSwapped = true;
            nextQuestion();
        }
    }
}
