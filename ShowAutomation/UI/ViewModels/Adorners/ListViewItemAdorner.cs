using ShowAutomation.UI.ViewModels.Adorners;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ShowAutomation.UI.ViewModels
{
    class ListViewItemAdorner : DragAdorner
    {
        private ListViewItem listItem;
        private Rectangle rect;
        public ListViewItemAdorner(ListViewItem item, UIElement adornedElement) : base(adornedElement)
        {
            rect = new Rectangle();
            rect.Width = item.RenderSize.Width;
            rect.Height = item.RenderSize.Height;
            rect.Fill = new VisualBrush(item);
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
