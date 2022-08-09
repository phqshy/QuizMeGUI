using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;

namespace QuizMe.Core
{
    public class Learn
    {
        private TextBlock titleBlock;
        private TextBlock answerBlock;
        private TextBox input;
        private Dictionary<string, string> data;
        private Queue<string> randomQuestions;
        private readonly Random rnd;
        private string currentAnswer;
        
        public Learn(TextBlock title, TextBlock answer, TextBox input, Dictionary<string, string> data)
        {
            this.titleBlock = title;
            this.answerBlock = answer;
            this.input = input;
            this.data = data;
            this.rnd = new Random();
            this.randomQuestions = shuffleData(data);
        }

        public void nextQuestion()
        {
            if (this.randomQuestions.Count == 0) this.randomQuestions = shuffleData(data);
            this.input.Text = "";
            this.answerBlock.Text = "";

            string question = randomQuestions.Dequeue();
            this.titleBlock.Text = question.Replace(" (dupe)", "");
            this.currentAnswer = convertSuperscript(data[question]).ToLower();
        }

        public float verifyAnswer(string s)
        {
            s = s.ToLower().Normalize(NormalizationForm.FormC);
            currentAnswer = currentAnswer.Normalize();
            Trace.WriteLine(s);
            Trace.WriteLine(currentAnswer);
            float confidence = 0;
            int offset = 0;
            try
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == currentAnswer[i] + offset)
                    {
                        confidence += (1f / s.Length);
                    }
                    else
                    {
                       offset += 1;
                    }
                }
            } catch { }
            Trace.WriteLine(confidence);
            return confidence;
        }

        public Queue<string> shuffleData(Dictionary<string, string> set)
        {
            return new Queue<string>(set.Keys.OrderBy(x => rnd.Next()).ToList());
        }

        public void displayCorrectAnswer()
        {
            this.answerBlock.Text = currentAnswer;
        }

        //https://stackoverflow.com/a/2675837
        public string convertSuperscript(string value)
        {
            string stringFormKd = value.Normalize(NormalizationForm.FormKD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char character in stringFormKd)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(character);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormKC);
        }
    }

    public class LearnFactory
    {
        private TextBlock titleBlock;
        private TextBox input;
        private Dictionary<string, string> data;
        private TextBlock correctBlock;
        
        public Learn Build()
        {
            return new Learn(titleBlock, correctBlock, input, data);
        }

        public LearnFactory setTitleBlock(TextBlock textBlock)
        {
            this.titleBlock = textBlock;
            return this;
        }

        public LearnFactory setInputBox(TextBox textBox)
        {
            this.input = textBox;
            return this;
        }

        public LearnFactory setData(Dictionary<string, string> data)
        {
            this.data = data;
            return this;
        }

        public LearnFactory setAnswerBlock(TextBlock data)
        {
            this.correctBlock = data;
            return this;
        }
    }
}
