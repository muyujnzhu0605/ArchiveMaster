namespace ArchiveMaster.Views;

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

//DeepSeek
public class ExtendedWrapPanel : Panel
{
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.Register<ExtendedWrapPanel, HorizontalAlignment>(
            nameof(HorizontalContentAlignment),
            HorizontalAlignment.Left,
            defaultBindingMode: Avalonia.Data.BindingMode.OneWay,
            enableDataValidation: false);

    protected override Size MeasureOverride(Size constraint)
    {
        var curLineSize = new Size();
        var panelSize = new Size();

        var children = this.Children;

        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];

            child.Measure(constraint);
            var sz = child.DesiredSize;

            if (curLineSize.Width + sz.Width > constraint.Width)
            {
                panelSize = new Size(
                    Math.Max(curLineSize.Width, panelSize.Width),
                    panelSize.Height + curLineSize.Height);
                
                curLineSize = sz;

                if (sz.Width > constraint.Width)
                {
                    panelSize = new Size(
                        Math.Max(sz.Width, panelSize.Width),
                        panelSize.Height + sz.Height);
                    curLineSize = new Size();
                }
            }
            else
            {
                curLineSize = new Size(
                    curLineSize.Width + sz.Width,
                    Math.Max(sz.Height, curLineSize.Height));
            }
        }

        panelSize = new Size(
            Math.Max(curLineSize.Width, panelSize.Width),
            panelSize.Height + curLineSize.Height);

        return panelSize;
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        int firstInLine = 0;
        var curLineSize = new Size();
        double accumulatedHeight = 0;
        var children = this.Children;

        for (int i = 0; i < children.Count; i++)
        {
            var sz = children[i].DesiredSize;

            if (curLineSize.Width + sz.Width > arrangeBounds.Width)
            {
                ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, i);
                accumulatedHeight += curLineSize.Height;
                curLineSize = sz;

                if (sz.Width > arrangeBounds.Width)
                {
                    ArrangeLine(accumulatedHeight, sz, arrangeBounds.Width, i, ++i);
                    accumulatedHeight += sz.Height;
                    curLineSize = new Size();
                }
                firstInLine = i;
            }
            else
            {
                curLineSize = new Size(
                    curLineSize.Width + sz.Width,
                    Math.Max(sz.Height, curLineSize.Height));
            }
        }

        if (firstInLine < children.Count)
            ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, children.Count);

        return arrangeBounds;
    }

    private void ArrangeLine(double y, Size lineSize, double boundsWidth, int start, int end)
    {
        double x = 0;
        if (this.HorizontalContentAlignment == HorizontalAlignment.Center)
        {
            x = (boundsWidth - lineSize.Width) / 2;
        }
        else if (this.HorizontalContentAlignment == HorizontalAlignment.Right)
        {
            x = boundsWidth - lineSize.Width;
        }

        var children = this.Children;
        for (int i = start; i < end; i++)
        {
            var child = children[i];
            child.Arrange(new Rect(x, y, child.DesiredSize.Width, lineSize.Height));
            x += child.DesiredSize.Width;
        }
    }
}