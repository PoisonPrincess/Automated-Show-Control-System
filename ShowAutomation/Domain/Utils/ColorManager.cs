using System;
using System.Windows.Media;

namespace ShowAutomation.Domain.Utils
{
    public class ColorManager
    {
        public static String TEXT_COLOR{
            get {
                if (Mode.isEditMode)
                {
                    return "Black";
                }
                else
                {
                    return "White";
                }
            }
        }

        public static String IS_PLAYING_COLOR
        {
            get
            {
                if (Mode.isEditMode)
                {
                    return "LightGreen";
                }
                else
                {
                    return "DarkGreen";
                }
            }
        }

        public static String LIST_ITEM_1 {
            get
            {
                if (Mode.isEditMode)
                {
                    return "#19f39611";
                }
                else
                {
                    return "#A2BABAD1";
                }
            }
        }
        public static String LIST_ITEM_2
        {
            get
            {
                if (Mode.isEditMode)
                {
                    return "#19000000";
                }
                else
                {
                    return "#A26C6C7F";
                }
            }
        }

        public static Color BACKGROUND
        {
            get
            {
                if (Mode.isEditMode)
                {
                    return Colors.White;
                }
                else
                {
                    return Color.FromRgb(0x30, 0x30, 0x30);
                }
            }
        }
        public static Color FOREGROUND
        {
            get
            {
                if (Mode.isEditMode)
                {
                    return Color.FromRgb(0xEE, 0xEE, 0xEE);
                }
                else
                {
                    return Colors.DimGray;
                }
            }
        }

        public static Color HIGHLIGHTMUTEDLANE
        {
            get
            {
                if (Mode.isEditMode)
                {
                    return Color.FromRgb(0xd3, 0xd3, 0xf6);
                }
                else
                {
                    return Color.FromRgb(0xa9, 0xa9, 0xff);
                }
            }
        }

        public static Color ACCENT
        {
            get
            {
                if (Mode.isEditMode)
                {
                    return Colors.LightGray;
                }
                else
                {
                    return Colors.DarkGray;
                }
            }
        }

        public static String BUTTON
        {
            get
            {
                if (Mode.isEditMode)
                {
                    return "LightGray";
                }
                else
                {
                    return "DimGray";
                }
            }
        }
        public static String BUTTON_BORDER
        {
            get
            {
                if (Mode.isEditMode)
                {
                    return "DarkGray";
                }
                else
                {
                    return "#ff303030";
                }
            }
        }

        public static String HIGHLIGHT
        {
            get
            {
                if (Mode.isEditMode)
                {
                    return "AliceBlue";
                }
                else
                {
                    return "#ff27374E";
                }
            }
        }
        public static String SKIP_COLOR
        {
            get
            {
                if (Mode.isEditMode)
                {
                    return "LightCoral";
                }
                else
                {
                    return "DarkRed";
                }
            }
        }
    }
}
