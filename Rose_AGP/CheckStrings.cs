using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Rose_AGP
{
    public static class CheckStrings
    {
        public static string Checker(string str, string strTitle)
        {
            string newstr = null;

            var regexItem = new Regex("[^a-zA-Z0-9_.]+");

            if (regexItem.IsMatch(str.ToString()))
            {
                newstr = Regex.Replace(str, "[^a-zA-Z0-9_.]+", "_");
            }

            if (newstr == null)
                return str;
            else
            {
                
                
                MessageBoxResult changename = MessageBox.Show("Name has forbidden characters. Replace with '_'?",
                    strTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (MessageBoxResult.Yes == changename)
                    return Regex.Replace(newstr, "[^a-zA-Z0-9_]+", "_");
                else
                    return "";

            }
        }

        public static string CheckFirstLetter(string str, string strTitle)
        {
            bool isLetter = !String.IsNullOrEmpty(str) && Char.IsLetter(str[0]);

            string letter = str[0].ToString();

            if (letter == "_")
            {
                isLetter = true;
            }

            if (!isLetter)
            {

                MessageBoxResult changename = MessageBox.Show("First letter of name must begin with a character. Replace with '_'?",
                    strTitle, MessageBoxButton.YesNo, MessageBoxImage.Question); 

                if (MessageBoxResult.Yes == changename)
                    return str.Replace(str[0], Convert.ToChar("_"));
                else
                    return "";

            }

            return str;

        }
    }
}
