using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuizMe.Core
{
    enum WindowStates
    {
        Home,
        MultipleChoice,
        Learn,
        LoadCache
    }

    internal class WindowManager
    {
        private MainWindow window;
        private List<Grid> screens = new List<Grid>();
        public WindowManager(MainWindow window)
        {
            this.window = window;
            this.screens.Add(window.MainScreen);
            this.screens.Add(window.MultipleChoiceScreen);
            this.screens.Add(window.LearnScreen);
            this.screens.Add(window.CacheScreen);
        }

        public void swapWindow(WindowStates state)
        {
            foreach (Grid g in screens)
            {
                g.Visibility = Visibility.Collapsed;
            }

            screens.ElementAt((int) state).Visibility = Visibility.Visible;

            if (!state.Equals(WindowStates.Home))
            {
                window.BackButton.Visibility = Visibility.Visible;
            } else
            {
                window.BackButton.Visibility = Visibility.Collapsed;
            }
        }
    }
}
