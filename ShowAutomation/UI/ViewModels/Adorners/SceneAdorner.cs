using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.Domain.Utils;
using ShowAutomation.UI.ViewModels.Adorners;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShowAutomation.UI.ViewModels
{
    class SceneAdorner : DragAdorner
    {
        private Scene listItem;
        private Grid rect = new Grid();
        public SceneAdorner(Scene item, UIElement adornedElement) : base(adornedElement)
        {
            item.drawLanes(rect, null, null, MainWindow.main.outputs.DisplayList);
            rect.Background = new SolidColorBrush(ColorManager.BACKGROUND);
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
