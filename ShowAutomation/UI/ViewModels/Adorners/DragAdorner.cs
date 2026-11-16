using System.Windows;
using System.Windows.Documents;

namespace ShowAutomation.UI.ViewModels.Adorners
{
    class DragAdorner : Adorner
    {

        public DragAdorner(UIElement adornedElement) : base(adornedElement)
        { }

        protected double offsetLeft = 0;
        protected double offsetTop = 0;
        public double OffsetLeft
        {
            get { return this.offsetLeft; }
            set
            {
                this.offsetLeft = value;
                UpdateLocation();
            }
        }

        public double OffsetTop
        {
            get { return this.offsetTop; }
            set
            {
                this.offsetTop = value;
                UpdateLocation();
            }
        }


        public void SetOffsets(double left, double top)
        {
            this.offsetLeft = left;
            this.offsetTop = top;
            this.UpdateLocation();
        }

        private void UpdateLocation()
        {
            AdornerLayer adornerLayer = this.Parent as AdornerLayer;
            if (adornerLayer != null)
                adornerLayer.Update(this.AdornedElement);
        }
    }
}
