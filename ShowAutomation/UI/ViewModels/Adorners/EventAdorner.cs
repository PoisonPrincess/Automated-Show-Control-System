using ShowAutomation.Domain.Models.Events;
using ShowAutomation.UI.ViewModels.Adorners;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShowAutomation.UI.ViewModels
{
    class EventAdorner : DragAdorner
    {
        private Event listItem;
        private Canvas rect;
        public EventAdorner(Event item, UIElement adornedElement) : base(adornedElement)
        {
            rect = item.drawEvent();
            //rect.Opacity = 0.6;
            this.IsHitTestVisible = false;
            this.listItem = item;
            InvalidateVisual();
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(this.offsetLeft, this.offsetTop));
            return result;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            this.rect.Measure(constraint);
            return this.rect.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this.rect.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.rect;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }
    }
}
