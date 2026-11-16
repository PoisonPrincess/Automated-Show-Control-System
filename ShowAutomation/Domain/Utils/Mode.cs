using System;
using System.ComponentModel;

namespace ShowAutomation.Domain.Utils
{
    /// <summary>
    /// Switches mode from play to edit using boolean values 
    /// </summary>
    public class Mode
    {
        //true is edit mode, false is show mode
        protected static Boolean mode = true;
        public static Boolean isEditMode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
                OnStaticPropertyChanged("isEditMode");
            }
        }

        private static void alignDevices ()
        {

        }

        private static String name ="";

        public static String projectName
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public static void setMode (Boolean mode)
        {
            Mode.isEditMode = mode;
        }

        public Mode ()
        {
           
        }

        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            var handler = StaticPropertyChanged;
            if (handler != null)
            {
                handler(null, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
