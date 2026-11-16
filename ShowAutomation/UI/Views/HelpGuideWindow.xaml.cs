using ShowAutomation.Domain.Utils;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace ShowAutomation.UI.Views
{
    /// <summary>
    /// Interaction logic for HelpGuideWindow.xaml
    /// </summary>
    public partial class HelpGuideWindow : Window
    {
        private Dictionary<string, string> dict;    //name and contents of results of search
        private string query = "";                  //the exact item the user searched for
        private int catIndex;
        private bool dbFound = false;
        private TextBox searchBox;                  //the search box at the top of the window (initialized in inidializeHelpWindow())

        public HelpGuideWindow()
        {
            InitializeComponent();
            dbFound = HelpDatabaseManager.dbFound();
            if(!dbFound)
            {
                formatHeaderAndQuery("ERROR: ", "Help Database Not Found");
            }
        }

        public bool getdbFound()
        {
            return dbFound;
        }

        private void initializeHelpWindow()
        {
            if (dbFound)
            {
                RoutedCommand enterKey = new RoutedCommand();
                enterKey.InputGestures.Add(new KeyGesture(Key.Enter)); //enter = search
                CommandBindings.Add(new CommandBinding(enterKey, newSearch));

                searchBox = new TextBox();              //adds and formats search box
                Grid.SetRow(searchBox, 0);
                Grid.SetColumn(searchBox, 0);
                searchPanel.Children.Add(searchBox);

                Button Search = new Button();
                Search.Content = "Search";
                Search.Click += newSearch;
                Grid.SetRow(Search, 0);
                Grid.SetColumn(Search, 1);
                searchPanel.Children.Add(Search);
            }
        }

        public void receiveData(Dictionary<string, string> dict, string query)  //creates, formats, and displays main menu for search term
        {
            if (dbFound)
            {
                this.dict = dict;       //updates dictionary with new data
                this.query = query;     //updates query with new query
                mainPanel.Children.Clear(); //clears main panel to allow new items to be added
                mainPanel.ColumnDefinitions.Clear();
                initializeHelpWindow();     //opens window


                formatHeaderAndQuery("Results From: ", query);

                ScrollViewer scroll = new ScrollViewer();   //creates and displays results of search as buttons
                RowDefinition sp = new RowDefinition();
                RowDefinition sp1 = new RowDefinition();
                RowDefinition sep = new RowDefinition();
                RowDefinition sp2 = new RowDefinition();
                GridLengthConverter gridLengthConverter = new GridLengthConverter();
                sp.Height = sp1.Height = GridLength.Auto;
                sep.Height = new GridLength(10);
                sp2.Height = (GridLength)gridLengthConverter.ConvertFrom("*");
                Grid stackPanels = new Grid();
                stackPanels.RowDefinitions.Add(sp);
                stackPanels.RowDefinitions.Add(sp1);
                stackPanels.RowDefinitions.Add(sep);
                stackPanels.RowDefinitions.Add(sp2);
                StackPanel stackPanel = new StackPanel();
                StackPanel stackPanel1 = new StackPanel();
                StackPanel stackPanel2 = new StackPanel();

                foreach (string key in dict.Keys)       //creating buttons
                {
                    Button articleButton = new Button();
                    articleButton.Content = key;
                    articleButton.Click += openArticle;
                    articleButton.DataContext = key;
                    if (key == query)                //puts more relevant results first
                    {
                        stackPanel.Children.Add(articleButton);
                    }
                    else if (key.Contains(query))
                    {
                        stackPanel1.Children.Add(articleButton);
                    }
                    else
                    {
                        stackPanel2.Children.Add(articleButton);
                    }
                }
                Separator separator = new Separator();
                Grid.SetRow(stackPanel, 0);
                stackPanels.Children.Add(stackPanel);
                Grid.SetRow(stackPanel1, 1);
                stackPanels.Children.Add(stackPanel1);
                Grid.SetRow(separator, 2);
                stackPanels.Children.Add(separator);
                Grid.SetRow(stackPanel2, 3);
                stackPanels.Children.Add(stackPanel2);
                scroll.Content = stackPanels;
                Grid.SetRow(scroll, 2);
                mainPanel.Children.Add(scroll);
            }
        }

        public void printCategories(string[] categoriesToPrint, int catIndex, string query)
        {
            if (dbFound)
            {
                this.catIndex = catIndex;
                string[] categories = new string[4];
                categories[0] = "Category";
                categories[1] = "Subcategory";
                categories[2] = "SubSubcategory";
                categories[3] = "SubSubSubcategory";

                mainPanel.Children.Clear();     //Clears main panel to put new items in

                string header = "Results from Category: ";
                string queryTitle = query;

                if (query == "")
                {
                    header = "Categories: ";
                    queryTitle = "All";
                }

                formatHeaderAndQuery(header, queryTitle);

                initializeHelpWindow();
                StackPanel stackPanel = new StackPanel();
                ScrollViewer scroll = new ScrollViewer();

                foreach (string cat in categoriesToPrint)
                {
                    if ((cat != "") && (cat != null))
                    {
                        Button catButton = new Button();
                        catButton.Content = cat;
                        catButton.Click += openCat;
                        catButton.DataContext = cat;
                        stackPanel.Children.Add(catButton);
                    }
                }
                scroll.Content = stackPanel;
                Grid.SetRow(scroll, 2);
                mainPanel.Children.Add(scroll);
            }
        }

        private void openArticle(object sender, RoutedEventArgs e)  //creates, formats, and displays the article the user clicked on
        {
            if (dbFound)
            {
                mainPanel.Children.Clear();     //Clears main panel to put new items in
                Button backBtn = new Button();  //create, format, and add back button
                backBtn.Content = "Back";
                backBtn.Click += backToMenu;
                Grid.SetRow(backBtn, 0);
                mainPanel.Children.Add(backBtn);

                //create, format, and add Title
                TextBlock articleName = new TextBlock();
                string key = (sender as Button).DataContext.ToString();
                articleName.Text = key;
                articleName.TextWrapping = TextWrapping.Wrap;
                articleName.FontWeight = FontWeights.Bold;
                articleName.FontSize = 15;

                TextBlock articleInformation = new TextBlock();         //create and add content
                articleInformation.TextWrapping = TextWrapping.Wrap;
                articleInformation.Text = dict[key];
                Grid.SetRow(articleName, 1);
                mainPanel.Children.Add(articleName);

                RowDefinition ai = new RowDefinition();                 //create, format, and add related search keys
                RowDefinition sp = new RowDefinition();
                RowDefinition rl = new RowDefinition();
                RowDefinition btns = new RowDefinition();
                GridLengthConverter gridLengthConverter = new GridLengthConverter();
                ai.Height = btns.Height = (GridLength)gridLengthConverter.ConvertFrom("*");
                sp.Height = new GridLength(10);
                rl.Height = GridLength.Auto;
                Grid fullContent = new Grid();
                fullContent.RowDefinitions.Add(ai);
                fullContent.RowDefinitions.Add(sp);
                fullContent.RowDefinitions.Add(rl);
                fullContent.RowDefinitions.Add(btns);
                Grid.SetRow(articleInformation, 0);
                fullContent.Children.Add(articleInformation);
                Separator s = new Separator();
                Grid.SetRow(s, 1);
                fullContent.Children.Add(s);
                TextBlock related = new TextBlock();
                related.Text = "Related Topics: ";
                Grid.SetRow(related, 2);
                fullContent.Children.Add(related);

                StackPanel stackPanel = new StackPanel();
                string[] articleKeys = HelpDatabaseManager.getKeys(key);        //retrieve keys for current topic
                foreach (string keyword in articleKeys)                         //create buttons
                {
                    if (keyword != "")
                    {
                        Button articleButton = new Button();
                        articleButton.Content = keyword;
                        articleButton.Click += openNewArticle;
                        articleButton.DataContext = keyword;
                        stackPanel.Children.Add(articleButton);
                    }
                }
                Grid.SetRow(stackPanel, 3);
                fullContent.Children.Add(stackPanel);

                ScrollViewer scroll = new ScrollViewer();
                scroll.Content = (fullContent);
                Grid.SetRow(scroll, 2);
                mainPanel.Children.Add(scroll);
            }
        }

        private void formatHeaderAndQuery(string header, string query)
        {
            TextBlock head = new TextBlock();
            head.Inlines.Add(new Bold(new Run(header)));
            head.Inlines.Add("\"" + query + "\"");
            head.FontSize = 15;
            head.TextWrapping = TextWrapping.Wrap;
            Grid.SetColumn(head, 0);
            mainPanel.Children.Add(head);
        }

        private void openCat(object sender, RoutedEventArgs e)      //opens next level of categories
        {
            HelpDatabaseManager.searchCategories((sender as Button).DataContext.ToString(), ++catIndex);
        }

        private void newSearch(object sender, RoutedEventArgs e)    //refreshes database for a new search
        {
            HelpDatabaseManager.searchDatabase(searchBox.Text);
        }

        private void openNewArticle(object sender, RoutedEventArgs e)   //searches database after clicking on key
        {
            HelpDatabaseManager.searchDatabase((sender as Button).DataContext.ToString());
        }

        private void backToMenu(object sender, RoutedEventArgs e)       //returns user to menu
        {
            receiveData(dict, query);
        }
    }
}
