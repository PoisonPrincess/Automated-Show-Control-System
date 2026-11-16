using ShowAutomation.UI.Views;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace ShowAutomation.Domain.Utils
{
    class HelpDatabaseManager
    {
        private static SQLiteConnection connection;                 //connection to database
        private static SQLiteCommand command = new SQLiteCommand(); //command to search database with
        private static string queryText = "Select * FROM Help";     //formatted user input to use as command to search with
        public static bool databaseIsOpen = false;                  //check if database is open so it doesn't accidentally get closed/opened when it is already closed/opened
        private static Dictionary<string, string> dict;             //local database of results of the user's search
        private static string[] keys;                               //used to store keys of search terms
        private static string[] categoryResults;                    //used to store categories and subcategories
        private static bool finalCat = false;                       //checks if the current category has any subcategories
        //Help Window
        private static bool helpWindowOpen = false;                 //used to determine if help window is open so only one can be open at once
        private static HelpGuideWindow helpWindow = new HelpGuideWindow();  //object of help window

        public static bool dbFound()
        {
            connectToDatabase();
            bool open = databaseIsOpen;
            closeConnectionToDatabase();
            return open;
        }

        public static void searchDatabase(string query)        //CALL THIS FROM OUTSIDE TO SEARCH
        {
            if (query == "")
            {
                searchCategories(query, 0);
            }
            else
            {
                queryText = "SELECT * FROM Help WHERE (Name LIKE \"%" + query + "%\") OR (Key1 LIKE \"%" + query + "%\") OR (Key2 LIKE \"%" + query + "%\") OR (Key3 LIKE \"%" + query + "%\") OR (Key4 LIKE \"%" + query + "%\") OR (Key5 LIKE \"%" + query + "%\") OR (Key6 LIKE \"%" + query + "%\") OR (Key7 LIKE \"%" + query + "%\") OR (Key8 LIKE \"%" + query + "%\") OR (Key9 LIKE \"%" + query + "%\") OR (Key10 LIKE \"%" + query + "%\") OR (Key11 LIKE \"%" + query + "%\") OR (Key12 LIKE \"%" + query + "%\") OR (Key13 LIKE \"%" + query + "%\") OR (Key14 LIKE \"%" + query + "%\") OR (Key15 LIKE \"%" + query + "%\") OR (Key16 LIKE \"%" + query + "%\")";
                
                ExecuteQuery();
                helpWindow.receiveData(dict, query);
            }
        }

        public static void searchCategories(string query, int catIndex)     //determines what categories to fetch
        {
            string[] categories = new string[4];
            categories[0] = "Category";
            categories[1] = "Subcategory";
            categories[2] = "SubSubcategory";
            categories[3] = "SubSubSubcategory";
            bool getCat;

            if((query == ""))       //reset finalCat for new query
            {
                finalCat = false;
            }

            if (finalCat && (catIndex < 4))     //gets names and content after going into a final category
            {
                queryText = "SELECT * FROM Help WHERE " + categories[catIndex] + " = \"" + query + "\"";
                ExecuteQuery();
                helpWindow.receiveData(dict, query);
                return;
            }else if((query == "") || (catIndex == 0))  //first set of categories
            {
                catIndex = 0;
                queryText = "SELECT DISTINCT " + categories[0] + " FROM Help";
            }else if(catIndex < 4)                      //second, third, and fourth levels of categories
            {
                queryText = "SELECT DISTINCT " + categories[catIndex] + " FROM Help WHERE " + categories[catIndex - 1] + " = \"" + query + "\"";
            }
            else                                        //if the catIndex is greater than 4, reruns this method
            {
                finalCat = true;
                searchCategories(query, 3);
                return;
            }
            getCat = getCategories(catIndex, query);    //gets categories
            if (getCat)                                 //prints categories if it's not the last level
            {
                helpWindow.printCategories(categoryResults, catIndex, query);
            }else
            {
                finalCat = false;
            }
                
        }

        private static void connectToDatabase()             //opens connection to database if one is not already open
        {
            if (!databaseIsOpen)
            {
                String installerFilePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\ASCS\\Help.db"; 
                String debugFilePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Help.db";
                String ConnectionString = ConnectionString = "Data Source=" +debugFilePath + ";Version=3;UseUTF16Encoding=True";

                if (File.Exists(installerFilePath))
                {
                    ConnectionString = "Data Source=" + installerFilePath + ";Version=3;UseUTF16Encoding=True";
                }

                try
                {
                    connection = new SQLiteConnection(ConnectionString);
                    connection.Open();
                    if (!helpWindow.getdbFound())
                    {
                        command = new SQLiteCommand(queryText, connection);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    Console.Write("Error"+ e);
                    return;
                }
                databaseIsOpen = true;
            }
        }

        private static void closeConnectionToDatabase()     //closes connection to database if connection is open
        {
            if (databaseIsOpen)
            {
                connection.Close();
                databaseIsOpen = false;
            }
        }

        public static string[] getKeys(string name)         //retrieves all keys for one name
        {
            queryText = "SELECT Key1, Key2, Key3, Key4, Key5, Key6, Key7, Key8, Key9, Key10, Key11, Key12, Key13, Key14, Key15, Key16 FROM Help WHERE Name = \"" + name + "\""; //formats name as sqlite statement
            connectToDatabase();
            if (databaseIsOpen)
            {
                command = new SQLiteCommand(queryText, connection);
                command.ExecuteNonQuery();
                SQLiteDataReader reader = command.ExecuteReader();
                keys = new string[16];
                if (reader.Read())
                {
                    for (int i = 0; i < 16; i++)
                    {
                        keys[i] = reader["Key" + (i + 1)].ToString();       //adds keys to array of strings
                    }
                }
                closeConnectionToDatabase();
            }
            return keys;                                                //returns keys to display in helpwindow
        }

        private static bool getCategories(int catIndex, string query)         //retrieves all categories
        {
            string[] categories = new string[4];
            categories[0] = "Category";
            categories[1] = "Subcategory";
            categories[2] = "SubSubcategory";
            categories[3] = "SubSubSubcategory";

            connectToDatabase();
            if (databaseIsOpen)
            {
                command = new SQLiteCommand(queryText, connection);
                command.ExecuteNonQuery();
                SQLiteDataReader reader = command.ExecuteReader();
                categoryResults = new string[105];
                int i = 0;
                while (reader.Read())
                {
                    categoryResults[i] = reader[categories[catIndex]].ToString();       //adds categories to array of strings
                    i++;
                }
                if ((i == 1) && (categoryResults[0] == ""))
                {
                    finalCat = true;
                    searchCategories(query, --catIndex);
                }
                closeConnectionToDatabase();
            }
            return !finalCat;
        }


        private static void ExecuteQuery()          //executes query and returns name and content of results
        {
            connectToDatabase();
            if (databaseIsOpen)
            {
                command = new SQLiteCommand(queryText, connection);
                command.ExecuteNonQuery();
                SQLiteDataReader reader = command.ExecuteReader();
                dict = new Dictionary<string, string>();
                while (reader.Read())
                {
                    dict.Add(reader["Name"].ToString(), reader["Content"].ToString());  //add results to dictionary database
                }
                closeConnectionToDatabase();
            }
        }

        public static void openHelpWindow()              //opens Help Window
        {
            if (!helpWindowOpen)
            {
                helpWindowOpen = true;
                helpWindow = new HelpGuideWindow();
                helpWindow.Closing += helpWindowClosed;
                helpWindow.Owner = MainWindow.main;
                helpWindow.Show();
            }
        }

        private static void helpWindowClosed(object sender, EventArgs e)
        {
            helpWindowOpen = false;
        }

    }
}
