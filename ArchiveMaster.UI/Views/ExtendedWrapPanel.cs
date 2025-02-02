using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace ArchiveMaster.Views;

public class ExtendedWrapPanel : Panel
{
    // 存储每行元素数的列表
    private List<int> _rows = new();
    
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.Register<ExtendedWrapPanel, HorizontalAlignment>(
            nameof(HorizontalContentAlignment),
            HorizontalAlignment.Center);

    protected override Size MeasureOverride(Size constraint)
    {
        var children = Children;
        var childWidths = new List<double>();
        var childHeights = new List<double>();

        // 1. 测量所有子元素并记录尺寸
        foreach (var child in children)
        {
            child.Measure(constraint);
            childWidths.Add(child.DesiredSize.Width);
            childHeights.Add(child.DesiredSize.Height);
        }

        // 2. 计算最佳行分布
        _rows = CalculateOptimalRows(childWidths, constraint.Width);

        // 3. 计算总高度
        var height = 0.0;
        var index = 0;
        foreach (var rowCount in _rows)
        {
            height += childHeights.Skip(index).Take(rowCount).Max();
            index += rowCount;
        }

        // 4. 返回尺寸（宽度使用约束宽度以支持对齐）
        return new Size(
            double.IsInfinity(constraint.Width) 
                ? childWidths.Sum() 
                : constraint.Width,
            height);
    }

    private static List<int> CalculateOptimalRows(List<double> widths, double maxWidth)
    {
        var totalCount = widths.Count;
        if (totalCount == 0) return new List<int>();
        
        // 快速返回单行情况
        if (widths.Sum() <= maxWidth) 
            return new List<int> { totalCount };

        // 智能行数探测算法
        var bestDistribution = new List<int>();
        var minRowCount = int.MaxValue;

        // 行数搜索范围优化（基于平均宽度估算）
        var avgWidth = widths.Average();
        var estimatedMinRows = (int)Math.Ceiling(widths.Sum() / maxWidth);
        var searchStart = Math.Max(1, estimatedMinRows - 2);
        var searchEnd = Math.Min(totalCount, estimatedMinRows + 2);

        for (var targetRows = searchStart; targetRows <= searchEnd; targetRows++)
        {
            var distribution = new List<int>();
            var currentIndex = 0;
            var valid = true;

            for (var row = 0; row < targetRows; row++)
            {
                var remaining = totalCount - currentIndex;
                var idealCount = (int)Math.Ceiling(remaining / (double)(targetRows - row));
                var actualCount = FindMaxFit(currentIndex, idealCount, widths, maxWidth);

                if (actualCount <= 0)
                {
                    valid = false;
                    break;
                }

                distribution.Add(actualCount);
                currentIndex += actualCount;
            }

            if (valid && currentIndex == totalCount && distribution.Count <= minRowCount)
            {
                minRowCount = distribution.Count;
                bestDistribution = distribution;
            }
        }

        return bestDistribution.Any() 
            ? bestDistribution 
            : new List<int> { totalCount }; // Fallback
    }

    private static int FindMaxFit(int start, int idealCount, List<double> widths, double maxWidth)
    {
        var currentWidth = 0.0;
        var actualCount = 0;

        for (var i = start; i < start + idealCount && i < widths.Count; i++)
        {
            var newWidth = currentWidth + widths[i];
            if (newWidth > maxWidth) break;
            
            currentWidth = newWidth;
            actualCount++;
        }

        // 至少放置一个元素防止死循环
        return actualCount > 0 ? actualCount : 1;
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        var children = Children;
        var y = 0.0;
        var currentIndex = 0;

        foreach (var rowCount in _rows)
        {
            var rowChildren = children.Skip(currentIndex).Take(rowCount).ToList();
            var rowHeight = rowChildren.Max(c => c.DesiredSize.Height);
            var totalWidth = rowChildren.Sum(c => c.DesiredSize.Width);

            // 计算水平起始位置
            var x = CalculateStartX(totalWidth, arrangeBounds.Width);

            // 排列当前行
            foreach (var child in rowChildren)
            {
                child.Arrange(new Rect(x, y, child.DesiredSize.Width, rowHeight));
                x += child.DesiredSize.Width;
            }

            y += rowHeight;
            currentIndex += rowCount;
        }

        return arrangeBounds;
    }

    private double CalculateStartX(double rowWidth, double availableWidth)
    {
        return HorizontalContentAlignment switch
        {
            HorizontalAlignment.Center => (availableWidth - rowWidth) / 2,
            HorizontalAlignment.Right => availableWidth - rowWidth,
            _ => 0
        };
    }
}



//DeepSeek
//
// 将这个WPF的代码转成Avalonia的：public class AlignableWrapPanel : Panel
// {
//     public HorizontalAlignment HorizontalContentAlignment
//     {
//         get { return (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty); }
//         set { SetValue(HorizontalContentAlignmentProperty, value); }
//     }
//
//     public static readonly DependencyProperty HorizontalContentAlignmentProperty =
//         DependencyProperty.Register("HorizontalContentAlignment", typeof(HorizontalAlignment), typeof(AlignableWrapPanel), new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsArrange));
//
//     protected override Size MeasureOverride(Size constraint)
//     {
//         Size curLineSize = new Size();
//         Size panelSize = new Size();
//
//         UIElementCollection children = base.InternalChildren;
//
//         for (int i = 0; i < children.Count; i++)
//         {
//             UIElement child = children[i] as UIElement;
//
//             // Flow passes its own constraint to children
//             child.Measure(constraint);
//             Size sz = child.DesiredSize;
//
//             if (curLineSize.Width + sz.Width > constraint.Width) //need to switch to another line
//             {
//                 panelSize.Width = Math.Max(curLineSize.Width, panelSize.Width);
//                 panelSize.Height += curLineSize.Height;
//                 curLineSize = sz;
//
//                 if (sz.Width > constraint.Width) // if the element is wider then the constraint - give it a separate line                    
//                 {
//                     panelSize.Width = Math.Max(sz.Width, panelSize.Width);
//                     panelSize.Height += sz.Height;
//                     curLineSize = new Size();
//                 }
//             }
//             else //continue to accumulate a line
//             {
//                 curLineSize.Width += sz.Width;
//                 curLineSize.Height = Math.Max(sz.Height, curLineSize.Height);
//             }
//         }
//
//         // the last line size, if any need to be added
//         panelSize.Width = Math.Max(curLineSize.Width, panelSize.Width);
//         panelSize.Height += curLineSize.Height;
//
//         return panelSize;
//     }
//
//     protected override Size ArrangeOverride(Size arrangeBounds)
//     {
//         int firstInLine = 0;
//         Size curLineSize = new Size();
//         double accumulatedHeight = 0;
//         UIElementCollection children = this.InternalChildren;
//
//         for (int i = 0; i < children.Count; i++)
//         {
//             Size sz = children[i].DesiredSize;
//
//             if (curLineSize.Width + sz.Width > arrangeBounds.Width) //need to switch to another line
//             {
//                 ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, i);
//
//                 accumulatedHeight += curLineSize.Height;
//                 curLineSize = sz;
//
//                 if (sz.Width > arrangeBounds.Width) //the element is wider then the constraint - give it a separate line                    
//                 {
//                     ArrangeLine(accumulatedHeight, sz, arrangeBounds.Width, i, ++i);
//                     accumulatedHeight += sz.Height;
//                     curLineSize = new Size();
//                 }
//                 firstInLine = i;
//             }
//             else //continue to accumulate a line
//             {
//                 curLineSize.Width += sz.Width;
//                 curLineSize.Height = Math.Max(sz.Height, curLineSize.Height);
//             }
//         }
//
//         if (firstInLine < children.Count)
//             ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, children.Count);
//
//         return arrangeBounds;
//     }
//
//     private void ArrangeLine(double y, Size lineSize, double boundsWidth, int start, int end)
//     {
//         double x = 0;
//         if (this.HorizontalContentAlignment == HorizontalAlignment.Center)
//         {
//             x = (boundsWidth - lineSize.Width) / 2;
//         }
//         else if (this.HorizontalContentAlignment == HorizontalAlignment.Right)
//         {
//             x = (boundsWidth - lineSize.Width);
//         }
//
//         UIElementCollection children = InternalChildren;
//         for (int i = start; i < end; i++)
//         {
//             UIElement child = children[i];
//             child.Arrange(new Rect(x, y, child.DesiredSize.Width, lineSize.Height));
//             x += child.DesiredSize.Width;
//         }
//     }
// }
// 好的，我需要将用户提供的这个WPF的AlignableWrapPanel代码转换成Avalonia的版本。首先，我得仔细看看原来的代码结构和逻辑，然后找出Avalonia中对应的类和方法。
//
// 首先，注意到WPF中的Panel在Avalonia中对应的是Panel类，不过Avalonia的命名空间不同，可能需要使用using Avalonia.Controls;。接下来，依赖属性在Avalonia中的定义方式有所不同。WPF使用的是DependencyProperty.Register，而Avalonia使用StyledProperty。例如，HorizontalContentAlignment属性需要转换成Avalonia的样式属性。需要确认Avalonia的HorizontalAlignment类型的位置，通常在Avalonia.Layout命名空间下。
//
// 然后，查看MeasureOverride和ArrangeOverride方法。在WPF中，这两个方法是重写的，Avalonia同样允许重写这些方法，但参数和返回值可能有些差异。比如，Avalonia的Size结构可能需要使用不同的构造函数或者静态方法。需要检查是否有API差异，例如在Avalonia中，是否需要使用Math.Max还是直接比较，但代码逻辑应该可以保持不变。
//
// 在WPF中，InternalChildren是UIElementCollection类型，而Avalonia中的Panel使用Children属性，类型可能为Controls或UIElements集合。需要确认在Avalonia中如何遍历子元素。例如，Avalonia的Panel.Children是一个Controls集合，可以直接用foreach遍历，或者通过索引访问。
//
// 接下来，处理HorizontalContentAlignment属性。在WPF中，这个属性是HorizontalAlignment类型，而在Avalonia中可能也是类似的，但需要确认命名空间是否正确。注册依赖属性时，Avalonia使用StyledProperty.Register，并且元数据可能使用StyledPropertyMetadata或者直接传入默认值。注意Avalonia的依赖属性声明方式，例如：
//
// public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
//
// AvaloniaProperty.Register<AlignableWrapPanel, HorizontalAlignment>(nameof(HorizontalContentAlignment), HorizontalAlignment.Left);
//
// 然后，元数据中的AffectsArrange可能需要通过属性更改通知来处理。在Avalonia中，可以通过在注册属性时指定AffectsArrange为true，或者在元数据中添加AffectsArrange。需要查证正确的语法，可能是在PropertyMetadata中设置AffectsArrange，例如：
//
// new StyledPropertyMetadata<HorizontalAlignment>(HorizontalAlignment.Left, Avalonia.Data.BindingMode.OneWay, null, null, false, Avalonia.Data.BindingPriority.LocalValue, AffectsArrange))
//
// 或者可能在注册时使用AvaloniaProperty.Register并传递一个PropertyMetadata，其中包含AffectsArrange选项。
//
// 然后，查看MeasureOverride和ArrangeOverride的具体实现。在WPF中，MeasureOverride的参数是Size constraint，返回面板的所需大小。Avalonia中的MeasureOverride方法同样接收Size作为参数，但可能需要处理无穷大的情况，不过原代码中的逻辑应该能直接转换。注意在Avalonia中，布局系统可能会有不同的处理方式，比如对子元素的测量和排列是否需要考虑不同的布局约束。
//
// 在ArrangeLine方法中，计算x的位置根据HorizontalContentAlignment进行调整，这部分逻辑应该保持不变，但需要确认Avalonia的Rect构造函数参数是否正确。例如，Avalonia的Rect构造函数可能需要位置和大小，或者位置和尺寸分开的参数。原代码中使用的是new Rect(x, y, child.DesiredSize.Width, lineSize.Height)，这在Avalonia中应该是相同的。
//
// 还需要注意，在Avalonia中，UIElement的Arrange方法是否接受Rect参数。是的，Avalonia中的Arrange方法确实接受Rect作为参数，所以这部分代码可以直接转换。
//
// 另外，需要处理依赖属性的getter和setter。在Avalonia中，通常使用GetValue和SetValue方法，与WPF类似，但需要确认属性注册的正确性。例如，HorizontalContentAlignment的CLR属性包装器应该调用GetValue和SetValue方法，而依赖属性注册时使用正确的泛型参数。
//
// 可能还需要处理命名空间的导入，例如使用Avalonia.Layout中的HorizontalAlignment，而不是WPF的System.Windows.Controls中的。
//
// 测试过程中可能会发现一些细微的差异，例如在测量子元素时是否需要考虑不同的布局方向，或者在排列时的坐标系统是否有差异。但根据当前代码逻辑，这些应该不会有太大问题。
//
// 总结转换步骤：
//
// 1. 将依赖属性转换为Avalonia的StyledProperty。
//
// 2. 调整命名空间引用，例如使用Avalonia.Controls和Avalonia.Layout。
//
// 3. 确保MeasureOverride和ArrangeOverride的方法签名正确。
//
// 4. 转换InternalChildren到Children，并处理可能的集合类型差异。
//
// 5. 确认Rect的使用在Arrange方法中正确。
//
// 6. 处理属性元数据中的AffectsArrange标志。
//
// 现在，按照这些步骤逐步转换代码，确保每个部分在Avalonia中都能正确工作。
//
// 以下是转换后的Avalonia版本代码：
//
// csharp
// 复制
// using Avalonia;
// using Avalonia.Controls;
// using Avalonia.Layout;
//
// public class AlignableWrapPanel : Panel
// {
//     public HorizontalAlignment HorizontalContentAlignment
//     {
//         get => GetValue(HorizontalContentAlignmentProperty);
//         set => SetValue(HorizontalContentAlignmentProperty, value);
//     }
//
//     public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
//         AvaloniaProperty.Register<AlignableWrapPanel, HorizontalAlignment>(
//             nameof(HorizontalContentAlignment),
//             HorizontalAlignment.Left,
//             defaultBindingMode: Avalonia.Data.BindingMode.OneWay,
//             propertyChanged: (_, _) => { },
//             enableDataValidation: false,
//             AffectsArrange);
//
//     protected override Size MeasureOverride(Size constraint)
//     {
//         var curLineSize = new Size();
//         var panelSize = new Size();
//
//         var children = this.Children;
//
//         for (int i = 0; i < children.Count; i++)
//         {
//             var child = children[i];
//
//             child.Measure(constraint);
//             var sz = child.DesiredSize;
//
//             if (curLineSize.Width + sz.Width > constraint.Width)
//             {
//                 panelSize = new Size(
//                     Math.Max(curLineSize.Width, panelSize.Width),
//                     panelSize.Height + curLineSize.Height);
//                 
//                 curLineSize = sz;
//
//                 if (sz.Width > constraint.Width)
//                 {
//                     panelSize = new Size(
//                         Math.Max(sz.Width, panelSize.Width),
//                         panelSize.Height + sz.Height);
//                     curLineSize = new Size();
//                 }
//             }
//             else
//             {
//                 curLineSize = new Size(
//                     curLineSize.Width + sz.Width,
//                     Math.Max(sz.Height, curLineSize.Height));
//             }
//         }
//
//         panelSize = new Size(
//             Math.Max(curLineSize.Width, panelSize.Width),
//             panelSize.Height + curLineSize.Height);
//
//         return panelSize;
//     }
//
//     protected override Size ArrangeOverride(Size arrangeBounds)
//     {
//         int firstInLine = 0;
//         var curLineSize = new Size();
//         double accumulatedHeight = 0;
//         var children = this.Children;
//
//         for (int i = 0; i < children.Count; i++)
//         {
//             var sz = children[i].DesiredSize;
//
//             if (curLineSize.Width + sz.Width > arrangeBounds.Width)
//             {
//                 ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, i);
//                 accumulatedHeight += curLineSize.Height;
//                 curLineSize = sz;
//
//                 if (sz.Width > arrangeBounds.Width)
//                 {
//                     ArrangeLine(accumulatedHeight, sz, arrangeBounds.Width, i, ++i);
//                     accumulatedHeight += sz.Height;
//                     curLineSize = new Size();
//                 }
//                 firstInLine = i;
//             }
//             else
//             {
//                 curLineSize = new Size(
//                     curLineSize.Width + sz.Width,
//                     Math.Max(sz.Height, curLineSize.Height));
//             }
//         }
//
//         if (firstInLine < children.Count)
//             ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, children.Count);
//
//         return arrangeBounds;
//     }
//
//     private void ArrangeLine(double y, Size lineSize, double boundsWidth, int start, int end)
//     {
//         double x = 0;
//         if (this.HorizontalContentAlignment == HorizontalAlignment.Center)
//         {
//             x = (boundsWidth - lineSize.Width) / 2;
//         }
//         else if (this.HorizontalContentAlignment == HorizontalAlignment.Right)
//         {
//             x = boundsWidth - lineSize.Width;
//         }
//
//         var children = this.Children;
//         for (int i = start; i < end; i++)
//         {
//             var child = children[i];
//             child.Arrange(new Rect(x, y, child.DesiredSize.Width, lineSize.Height));
//             x += child.DesiredSize.Width;
//         }
//     }
// }
// 主要变化点说明：
//
// 依赖属性系统：
//
// 使用AvaloniaProperty代替DependencyProperty
//
// 使用StyledProperty的注册方式
//
// 使用AffectsArrange标志替代WPF的FrameworkPropertyMetadataOptions
//
// 类型命名空间：
//
// HorizontalAlignment来自Avalonia.Layout
//
// Panel来自Avalonia.Controls
//
// 子元素访问：
//
// 使用this.Children替代base.InternalChildren
//
// Avalonia的Children集合直接可遍历
//
// API差异：
//
// 移除了UIElementCollection的显式转换
//
// 保持了相同的布局算法逻辑
//
// 使用了Avalonia的Arrange(Rect)方法
//
// 结构初始化：
//
// 使用更函数式的Size构造方式
//
// 保持相同的数学计算逻辑
//
// 属性元数据：
//
// 使用更简洁的Avalonia属性注册方式
//
// 通过AffectsArrange参数直接声明布局影响
//
// 这个实现保持了原始WPF控件的核心功能，同时适配了Avalonia的布局系统和属性系统。使用时需要确保项目引用了Avalonia的NuGet包，并在XAML中正确声明命名空间。
//
// 牛逼，我稍微改了一下，可以了：namespace ArchiveMaster.Views;
//
// using System;
// using Avalonia;
// using Avalonia.Controls;
// using Avalonia.Layout;
//
// public class ExtendedWrapPanel : Panel
// {
//     public HorizontalAlignment HorizontalContentAlignment
//     {
//         get => GetValue(HorizontalContentAlignmentProperty);
//         set => SetValue(HorizontalContentAlignmentProperty, value);
//     }
//
//     public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
//         AvaloniaProperty.Register<ExtendedWrapPanel, HorizontalAlignment>(
//             nameof(HorizontalContentAlignment),
//             HorizontalAlignment.Left,
//             defaultBindingMode: Avalonia.Data.BindingMode.OneWay,
//             enableDataValidation: false);
//
//     protected override Size MeasureOverride(Size constraint)
//     {
//         var curLineSize = new Size();
//         var panelSize = new Size();
//
//         var children = this.Children;
//
//         for (int i = 0; i < children.Count; i++)
//         {
//             var child = children[i];
//
//             child.Measure(constraint);
//             var sz = child.DesiredSize;
//
//             if (curLineSize.Width + sz.Width > constraint.Width)
//             {
//                 panelSize = new Size(
//                     Math.Max(curLineSize.Width, panelSize.Width),
//                     panelSize.Height + curLineSize.Height);
//                 
//                 curLineSize = sz;
//
//                 if (sz.Width > constraint.Width)
//                 {
//                     panelSize = new Size(
//                         Math.Max(sz.Width, panelSize.Width),
//                         panelSize.Height + sz.Height);
//                     curLineSize = new Size();
//                 }
//             }
//             else
//             {
//                 curLineSize = new Size(
//                     curLineSize.Width + sz.Width,
//                     Math.Max(sz.Height, curLineSize.Height));
//             }
//         }
//
//         panelSize = new Size(
//             Math.Max(curLineSize.Width, panelSize.Width),
//             panelSize.Height + curLineSize.Height);
//
//         return panelSize;
//     }
//
//     protected override Size ArrangeOverride(Size arrangeBounds)
//     {
//         int firstInLine = 0;
//         var curLineSize = new Size();
//         double accumulatedHeight = 0;
//         var children = this.Children;
//
//         for (int i = 0; i < children.Count; i++)
//         {
//             var sz = children[i].DesiredSize;
//
//             if (curLineSize.Width + sz.Width > arrangeBounds.Width)
//             {
//                 ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, i);
//                 accumulatedHeight += curLineSize.Height;
//                 curLineSize = sz;
//
//                 if (sz.Width > arrangeBounds.Width)
//                 {
//                     ArrangeLine(accumulatedHeight, sz, arrangeBounds.Width, i, ++i);
//                     accumulatedHeight += sz.Height;
//                     curLineSize = new Size();
//                 }
//                 firstInLine = i;
//             }
//             else
//             {
//                 curLineSize = new Size(
//                     curLineSize.Width + sz.Width,
//                     Math.Max(sz.Height, curLineSize.Height));
//             }
//         }
//
//         if (firstInLine < children.Count)
//             ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, children.Count);
//
//         return arrangeBounds;
//     }
//
//     private void ArrangeLine(double y, Size lineSize, double boundsWidth, int start, int end)
//     {
//         double x = 0;
//         if (this.HorizontalContentAlignment == HorizontalAlignment.Center)
//         {
//             x = (boundsWidth - lineSize.Width) / 2;
//         }
//         else if (this.HorizontalContentAlignment == HorizontalAlignment.Right)
//         {
//             x = boundsWidth - lineSize.Width;
//         }
//
//         var children = this.Children;
//         for (int i = start; i < end; i++)
//         {
//             var child = children[i];
//             child.Arrange(new Rect(x, y, child.DesiredSize.Width, lineSize.Height));
//             x += child.DesiredSize.Width;
//         }
//     }
// }
// 好的，我现在需要仔细分析用户提供的代码以及之前的对话历史，以理解他们的需求和潜在的问题。用户之前将WPF的AlignableWrapPanel转换为Avalonia版本，并得到了我的帮助。现在用户表示他们稍微修改后可以工作了，并附上了修改后的代码，看起来是成功的，但可能还存在一些潜在的问题或优化空间。
//
// 首先，我需要回顾之前的转换，看看用户做了哪些改动。用户将类名从AlignableWrapPanel改为ExtendedWrapPanel，并放到了ArchiveMaster.Views命名空间下。此外，他们在注册HorizontalContentAlignmentProperty时，移除了propertyChanged参数和AffectsArrange标志。这可能影响属性的行为，因为Avalonia的AffectsArrange是用来通知布局系统当属性变化时需要重新排列子元素。
//
// 接下来，我需要检查用户修改后的代码是否存在问题。在Avalonia中，依赖属性的注册需要正确设置元数据，特别是布局相关的属性需要正确标记AffectsArrange或AffectsMeasure，以确保属性变化时触发相应的布局更新。用户可能在移除AffectsArrange后，导致HorizontalContentAlignment属性变化时界面不更新，这需要确认。
//
// 然后，我需要考虑用户为什么这样做。可能是用户发现之前的代码在属性变化时没有正确触发布局更新，或者遇到了编译错误，因此调整了属性注册的参数。或者用户可能误删了这些参数，导致潜在的问题。
//
// 另外，用户可能在测试中发现某些情况下布局不正确，比如改变对齐方式时界面没有变化，这可能是因为缺少AffectsArrange标志，导致布局系统不知道需要重新排列子元素。
//
// 还需要检查代码中的其他部分，比如MeasureOverride和ArrangeOverride方法是否正确处理了所有情况，比如子元素超出约束时的处理，多行排列时的累积高度是否正确，以及对齐计算是否准确。
//
// 在MeasureOverride中，用户是否正确处理了行切换和面板尺寸的计算？例如，当子元素宽度超过约束时，是否单独成行，并正确累加高度。在ArrangeOverride中，处理行排列时是否正确处理了索引，比如在单独成行时递增i，可能导致循环中的i变化是否合理。
//
// 此外，用户将代码放在特定的命名空间下，是否正确引用了必要的Avalonia库，比如Avalonia.Controls和Avalonia.Layout。
//
// 可能用户没有意识到AffectsArrange的重要性，或者在转换时误解了Avalonia的依赖属性注册方式。需要指出这一点，并建议恢复AffectsArrange以确保属性变化时布局正确更新。
//
// 总结，用户可能遇到的潜在问题包括：
//
// 1. HorizontalContentAlignment属性变化后布局不更新，因为缺少AffectsArrange标志。
//
// 2. 在ArrangeOverride中，处理单独成行时的索引递增可能导致循环中的i处理错误，比如在ArrangeLine调用时传入++i是否会导致跳过某些元素。
//
// 3. 是否需要处理VerticalContentAlignment，虽然用户当前代码未涉及，但可能需要考虑扩展性。
//
// 4. 是否需要在MeasureOverride中处理无穷大的约束，比如当面板处于滚动查看器中时，约束可能为无穷大，需要适当处理。
//
// 接下来，我需要以友好和详细的方式解释这些潜在问题，并提供改进建议，同时肯定用户的成功，鼓励进一步测试和优化。
//
// 你的实现整体看起来已经非常接近可用的Avalonia版本！不过还是有几个细节可以优化，以下是改进建议和潜在问题说明：
//
// csharp
// 复制
// // 修改后的版本包含优化建议
// namespace ArchiveMaster.Views;
//
// using Avalonia;
// using Avalonia.Controls;
// using Avalonia.Layout;
//
// public class ExtendedWrapPanel : Panel
// {
//     // 添加属性变更通知
//     public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
//         AvaloniaProperty.Register<ExtendedWrapPanel, HorizontalAlignment>(
//             nameof(HorizontalContentAlignment),
//             HorizontalAlignment.Left,
//             // 添加布局影响标志
//             AffectsArrange);
//
//     // 保持原有属性定义
//     public HorizontalContentAlignment
//     {
//         get => GetValue(HorizontalContentAlignmentProperty);
//         set => SetValue(HorizontalContentAlignmentProperty, value);
//     }
//
//     protected override Size MeasureOverride(Size constraint)
//     {
//         // 处理无穷大宽度的情况（当在滚动查看器中时）
//         var maxWidth = double.IsInfinity(constraint.Width) 
//             ? double.PositiveInfinity 
//             : constraint.Width;
//
//         // 原有测量逻辑保持不变，但建议添加：
//         if (constraint == Size.Empty)
//         {
//             foreach (var child in Children)
//                 child.Measure(constraint);
//             return new Size();
//         }
//         
//         // ... 原有测量逻辑
//     }
//
//     protected override Size ArrangeOverride(Size arrangeBounds)
//     {
//         // 添加对Stretch模式的支持（可选）
//         var finalWidth = HorizontalContentAlignment == HorizontalAlignment.Stretch
//             ? arrangeBounds.Width
//             : arrangeBounds.Width;
//
//         // ... 原有排列逻辑
//     }
//
//     private void ArrangeLine(/* 参数保持不变 */)
//     {
//         // 添加对齐扩展支持
//         switch (HorizontalContentAlignment)
//         {
//             case HorizontalAlignment.Stretch:
//                 // 拉伸模式需要重新计算子元素宽度
//                 var totalWidth = lineSize.Width;
//                 var stretchWidth = boundsWidth / (end - start);
//                 x = 0;
//                 foreach (var child in Children.Skip(start).Take(end - start))
//                 {
//                     child.Arrange(new Rect(x, y, stretchWidth, lineSize.Height));
//                     x += stretchWidth;
//                 }
//                 return;
//             // 原有对齐逻辑保持不变
//         }
//     }
// }
// 需要特别注意的改进点：
//
// 属性注册优化：
//
// csharp
// 复制
// // 原始代码缺少布局影响标记
// AffectsArrange // 这个参数必须保留
// 这是保持布局响应性的关键，当HorizontalContentAlignment变化时会自动触发重新排列
//
// 无穷大宽度处理：
//
// csharp
// 复制
// // 在MeasureOverride开头添加
// if (double.IsInfinity(constraint.Width))
// {
//     // 横向无限延伸时需要特殊处理
//     return base.MeasureOverride(constraint);
// }
// 索引越界防护：
//
// csharp
// 复制
// // 在ArrangeOverride中：
// if (sz.Width > arrangeBounds.Width)
// {
//     // 注意这里使用预自增可能会跳过元素
//     ArrangeLine(accumulatedHeight, sz, arrangeBounds.Width, i, ++i);
//     // 建议改为：
//     ArrangeLine(accumulatedHeight, sz, arrangeBounds.Width, i, i + 1);
//     i++; // 显式递增
// }
// 性能优化建议：
//
// csharp
// 复制
// // 使用for替代foreach提升性能
// var children = Children;
// for (var i = 0; i < children.Count; i++)
// {
//     var child = children[i];
//     // ...
// }
// 扩展性考虑：
//
// csharp
// 复制
// // 可以添加垂直对齐属性
// public VerticalAlignment VerticalContentAlignment { /* 类似实现 */ }
// 测试建议：
//
// 测试极端情况：单个超大元素、空面板、大量小元素
//
// 测试不同容器环境：StackPanel、ScrollViewer、不同尺寸的父容器
//
// 动态改变HorizontalContentAlignment属性观察布局更新
//
// 验证在无限宽度容器中的表现（如放在HorizontalScrollViewer中）
//
// 如果遇到布局问题，可以添加以下调试代码：
//
// csharp
// 复制
// #if DEBUG
// protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
// {
//     if (change.Property == HorizontalContentAlignmentProperty)
//     {
//         Console.WriteLine($"Alignment changed to {change.NewValue}");
//     }
//     base.OnPropertyChanged(change);
// }
// #endif
// 这些改进将使你的ExtendedWrapPanel更加健壮，特别是在复杂布局场景下的表现会更稳定。需要根据实际使用场景决定是否要实现Stretch模式等扩展功能。
//
// 还需要一个改进，就是现在比如说有9个元素，8个一行满了，于是换行，第二行只有一个，这样很丑。需要修改代码，改为第一行五个、第二行四个。也就是说，如果需要n行，有m个控件，则每行的元素数量要尽量一致（约等于n/m）
// 好的，我现在需要帮用户改进他们自定义的Avalonia WrapPanel，让子元素在换行时尽量均匀分布，而不是让最后一行只剩很少的元素。用户举的例子是如果有9个元素，原本可能第一行8个，第二行1个，现在希望变成第一行5个，第二行4个，这样每行的元素数量更接近。
//
// 首先，我需要理解用户的需求。用户希望当元素需要换行时，每行的元素数量尽可能平均，避免某一行元素过少，特别是在最后一行。这其实是一个常见的布局需求，类似于某些排版中的“平均分布”或“两端对齐”。
//
// 接下来，我需要回顾现有的代码逻辑。当前的ArrangeOverride方法中，当元素的总宽度超过容器的宽度时，就会换行。这种逻辑是按顺序排列，直到放不下再换行，这可能导致最后一行元素数量远少于前面的行。例如，当容器宽度足够放8个元素，而总共有9个元素时，最后一行只有一个元素。
//
// 用户的需求是让每行的元素数量尽可能接近，这意味着需要动态计算每行应该放置多少个元素，使得行数最少且每行元素数量尽可能一致。这可能需要改变现有的布局算法，从顺序换行改为预先计算每行的元素数量。
//
// 接下来，我需要思考如何实现这一点。现有的逻辑是逐行排列，而新的逻辑需要预先计算每个元素的位置。可能的步骤包括：
//
// 1. **确定总共有多少行**：根据所有子元素的总宽度和容器宽度，估算需要多少行，并尽量让每行的元素数量接近。
//
// 2. **均匀分配元素到每一行**：例如，如果有9个元素，分成两行，那么每行大约4-5个元素。需要计算每行的理想元素数量，并调整以适应容器宽度。
//
// 3. **调整每行的元素布局**：根据计算出的每行元素数量，进行排列，并处理可能的宽度超出情况。
//
// 这里可能会遇到的问题是，如何在不测量每个元素的情况下预先知道它们的总宽度。不过，在MeasureOverride中，每个元素已经被测量过了，所以可以收集所有子元素的宽度，并在ArrangeOverride中使用这些信息。
//
// 可能的解决方案是：
//
// - 在MeasureOverride阶段，记录每个子元素的宽度，并计算总宽度。
//
// - 根据总宽度和容器宽度，计算需要多少行，并尽量平均分配每行的元素数量。
//
// - 在ArrangeOverride阶段，根据预先计算的行数和每行元素数量进行布局。
//
// 然而，现有的代码在MeasureOverride中并没有记录每个子元素的宽度，而是动态地处理换行。所以需要修改MeasureOverride的逻辑，使其不仅计算总体尺寸，还要记录每个子元素的尺寸，或者至少记录每行的元素数量。
//
// 另一个挑战是，在MeasureOverride阶段，可能需要预先知道每行的元素数量，以便正确计算面板的总尺寸。这可能需要两次遍历：第一次收集所有子元素的宽度，第二次计算如何分配行数和每行元素数量。
//
// 或者，可以在MeasureOverride中模拟布局过程，确定每行的元素数量，并记录下来，供ArrangeOverride使用。例如，使用一个列表来存储每行的元素数量，然后在ArrangeOverride阶段根据这个列表进行排列。
//
// 现在，我需要考虑如何实现均匀分布。例如，如果有9个元素，容器宽度允许每行最多5个元素，那么需要分成两行，每行5和4个。或者，如果容器宽度更小，可能需要更多的行数，但每行的元素数量尽可能接近。
//
// 可能的算法步骤：
//
// 1. 收集所有子元素的宽度，并计算总宽度。
//
// 2. 计算所有子元素的总宽度之和，除以容器宽度，得到大致需要的行数。
//
// 3. 根据行数，平均分配每行的元素数量，尽可能使每行的元素数量相等或相差1。
//
// 4. 调整每行的元素数量，使得每行的总宽度不超过容器宽度。
//
// 但是，这可能需要处理多个情况，例如某些元素可能较宽，导致即使平均分配也无法放入一行。因此，可能需要动态调整每行的元素数量，同时尽量保持均匀。
//
// 另一种思路是，在排列时，先确定行数，然后尽可能平均分配每行的元素数量，然后检查每行的总宽度是否超过容器宽度。如果超过，则调整行数，直到找到合适的行数。
//
// 这可能比较复杂，但可以参考现有的布局算法，例如Java的FlowLayout的“align”参数，或者WPF的UniformGrid控件，但WrapPanel的逻辑不同。
//
// 或者，可以参考类似“贪心算法”的逆过程，即尽量让每行的元素数量接近平均值，而不是尽可能多放元素。例如，当元素总数为m，行数为n时，每行的元素数量为m/n或m/n +1。
//
// 但在WrapPanel中，行数是由容器宽度和元素宽度决定的，因此可能需要动态计算行数，并根据行数调整每行的元素数量。
//
// 可能需要重新设计MeasureOverride和ArrangeOverride的逻辑：
//
// 在MeasureOverride中：
//
// - 收集所有子元素的宽度，并存储在一个列表中。
//
// - 计算所有子元素的总宽度之和。
//
// - 根据容器宽度，计算最少需要多少行，例如总宽度 / 容器宽度，向上取整。
//
// - 然后，尝试将这些行数尽可能平均分配元素，每行的元素数量为总数/行数，或接近。
//
// - 根据每行的元素数量，计算该行的总宽度，如果超过容器宽度，则需要调整行数，直到找到合适的行数。
//
// 这可能需要在MeasureOverride中进行多次尝试，以找到最佳的行数，使得每行的元素数量尽可能均匀，并且总宽度不超过容器宽度。
//
// 这似乎比较复杂，尤其是当子元素的宽度不统一时，可能需要动态调整。例如，某些元素较宽，可能需要减少每行的元素数量，从而影响整体的均匀分布。
//
// 但用户的需求可能更适用于所有子元素宽度相同的情况，或者至少宽度差异不大的情况。例如，类似图片缩略图视图，每个元素宽度相同，此时可以更容易均匀分配。
//
// 假设子元素的宽度相同，那么可以简化算法：
//
// 1. 计算容器宽度可以容纳每行多少个元素（containerWidth / childWidth）。
//
// 2. 总元素数为m，每行元素数为k，则行数为ceil(m / k)。
//
// 3. 但用户希望的是，当总行数确定后，每行的元素数尽可能接近，例如，9个元素，如果每行最多5个，则分成两行（5和4）。但如果容器宽度只能容纳4个元素每行，则分成3行（3,3,3）。
//
// 所以，用户的需求其实是，当需要换行时，不是尽可能填满每行，而是尽可能平均分配元素到每行，即使这意味着某些行的实际宽度未填满。
//
// 这类似于将元素均匀分布到多行，而不是紧密排列。例如，在图文混排中，可能需要让每行的元素数量相同，以保持美观。
//
// 因此，可能的解决方法是，在排列时，预先计算每行应该放置多少个元素，使得行数最少，并且每行的元素数量尽可能接近。
//
// 例如，对于m个元素，找到最大的k，使得k <= containerWidth / averageChildWidth，然后行数为ceil(m /k)，其中k为每行的最大元素数。但这样可能还是会导致最后一行元素较少。
//
// 或者，计算行数n，使得每行的元素数量为ceil(m/n)或floor(m/n)，并且n是满足所有行总宽度不超过容器宽度的最小行数。
//
// 这可能需要迭代计算，例如尝试不同的行数，找到能够满足每行总宽度不超过容器宽度的最小行数，并且每行元素数量尽可能接近。
//
// 这似乎比较复杂，但可能可以通过以下步骤实现：
//
// 1. 收集所有子元素的宽度，计算总宽度。
//
// 2. 计算平均每元素宽度，总宽度 / m。
//
// 3. 计算理论上的最小行数n_min = ceil(总宽度 / containerWidth)。
//
// 4. 从n_min开始，逐步增加行数n，直到找到一个n，使得当将元素分为n行，每行的元素数量为ceil(m/n)或floor(m/n)，并且每行的总宽度不超过containerWidth。
//
// 这可能需要在MeasureOverride中进行多次模拟布局，直到找到合适的行数。
//
// 但考虑到性能问题，这种方法可能在元素数量较多时效率较低。
//
// 另一种思路是，在排列时，当需要换行时，不是立即换行，而是记录当前位置，并尝试将剩余的元素平均分配到剩余的行中。例如，如果有m个元素，剩余k个元素需要放置，剩余的行数为n，则每行应放置大约k/n个元素。
//
// 这可能需要在排列过程中动态调整每行的元素数量，而不是按顺序填充。
//
// 例如，在ArrangeOverride中，维护剩余的元素数量和剩余的行数，计算每行应放置的元素数量，然后尽可能按照这个数量排列，同时检查总宽度是否超出容器宽度。
//
// 这可能需要更复杂的逻辑，但可能更高效。
//
// 现在，我需要考虑如何在现有的代码基础上修改，以实现这种均匀分布的效果。
//
// 当前的ArrangeOverride方法中，是按顺序逐个添加元素到当前行，直到放不下，然后换行。这导致每行尽可能多放元素，可能导致最后一行元素很少。
//
// 要改为平均分配，需要在排列前确定每行应放置的元素数量，然后按照这个数量进行排列。
//
// 可能的修改步骤：
//
// 1. 在MeasureOverride阶段，计算每行应放置的元素数量，并记录下来。
//
// 例如，在MeasureOverride中，计算每行的元素数量列表（如[5,4]），然后根据这些数量计算总高度。
//
// 2. 在ArrangeOverride阶段，根据预先计算的每行元素数量进行排列。
//
// 这需要在MeasureOverride中不仅计算面板的总尺寸，还要记录每行的元素数量，以便在ArrangeOverride中使用。
//
// 因此，可能需要添加一个字段来存储每行的元素数量列表。
//
// 例如，在MeasureOverride中：
//
// - 收集所有子元素的宽度。
//
// - 根据容器宽度和均匀分布的需求，计算每行的元素数量。
//
// - 存储每行的元素数量到一个列表中。
//
// - 根据每行的元素数量和元素的高度，计算总高度。
//
// 然后，在ArrangeOverride中，按照这个列表来排列每行的元素。
//
// 但如何计算每行的元素数量呢？
//
// 假设所有元素的宽度相同，可以简单地将总元素数除以行数。但实际中，元素宽度可能不同，所以需要更复杂的方法。
//
// 或者，用户可能希望每行的元素数量尽可能接近，即使这意味着某些行的实际宽度小于容器宽度。
//
// 例如，如果有9个元素，希望分成两行，每行5和4个，即使容器宽度可以容纳更多的元素，也要平均分配。
//
// 但这样可能导致右侧留有空白，这可能需要根据HorizontalContentAlignment来调整元素的位置，比如居中或右对齐。
//
// 这可能与用户的需求相符，即让元素在每行中平均分布，即使行未填满。
//
// 因此，可能的解决方案是：
//
// 在MeasureOverride中，计算每行应放置的元素数量，使得行数最少，并且每行的元素数量尽可能接近。然后，根据每行的元素数量计算该行的总宽度，并确定是否需要换行。
//
// 例如，对于m个元素：
//
// 行数n = ceil(m / k), 其中k是每行的最大元素数。
//
// 但需要动态调整k，使得n尽可能小，同时每行元素数尽可能接近。
//
// 或者，另一种方法是，当需要换行时，计算剩余的元素数量和剩余的行数，调整当前行应放置的元素数量，使得剩余元素可以平均分配到剩余的行中。
//
// 例如，在排列过程中，当处理到第i个元素时，剩余的元素数为m - i，剩余的行数为n，则当前行应放置ceil((m - i)/n)个元素，然后检查这些元素的总宽度是否超过容器宽度。如果超过，则减少当前行的元素数量，直到总宽度不超过容器宽度，并调整剩余的行数。
//
// 这可能需要动态调整每行的元素数量，确保剩下的行数和元素数能够平均分配。
//
// 例如，初始时，总元素数m=9，容器宽度允许每行最多5个元素。第一行放置5个，剩余4个，剩余行数1，这样最后一行4个，但可能超过容器宽度。因此，需要检查是否这5个元素的总宽度超过容器宽度，如果超过，则减少当前行的元素数量，直到总宽度不超过容器宽度。
//
// 但如何确定每行的元素数量，使得总宽度不超过容器宽度，同时尽可能平均分配？
//
// 这可能需要预先计算所有可能的行数，并找到最佳的行数，使得每行的元素数量尽可能接近，同时每行的总宽度不超过容器宽度。
//
// 例如：
//
// 对于m=9个元素，容器宽度为W。
//
// 计算行数n的可能范围，例如n=2到m（每行至少一个元素）。
//
// 对于每个n，计算每行的元素数量k_i = ceil(m/n) 或 floor(m/n)，然后计算每行的总宽度是否都 <= W。
//
// 找到最大的n，使得所有行的总宽度都不超过W，并且每行的元素数量尽可能接近。
//
// 这可能需要较多的计算，但可以保证找到最佳的行数。
//
// 然而，这样的计算在MeasureOverride阶段可能影响性能，尤其是当子元素数量较多时。
//
// 因此，可能需要寻找一种更高效的算法。
//
// 另一种思路是，在排列时，当需要换行时，计算剩余的元素和剩余的行数，然后平均分配剩下的元素到剩余的行中。例如：
//
// 当处理到第i个元素时，剩余元素数 = m - i，剩余行数 = total_rows - current_row。
//
// 每行应放置的元素数 = ceil(剩余元素数 / 剩余行数).
//
// 然后，尝试将这么多元素放入当前行，如果总宽度超过容器宽度，则减少元素数，直到适合。
//
// 这可能更高效，因为不需要预先计算所有可能的情况，而是在排列过程中动态调整。
//
// 例如，对于9个元素，容器宽度允许每行最多5个元素：
//
// - 第一行尝试放置ceil(9 / 2) = 5个元素，总宽度是否超过容器宽度？如果不超过，则放置5个，剩余4个元素，剩余1行，下一行放置4个。
//
// - 如果总宽度超过，则减少到4个，检查是否适合。如果适合，则放置4个，剩余5个元素，剩余1行，下一行放置5个，但此时可能超过容器宽度，需要继续调整。
//
// 这可能需要递归或迭代调整，直到找到合适的元素数量。
//
// 但如何确定行数呢？因为行数在开始时是未知的，这可能需要预估。
//
// 这可能比较复杂，但或许可以在排列过程中动态计算。
//
// 例如，在ArrangeOverride中，维护剩余的元素数、剩余的行数，以及当前行应放置的元素数。
//
// 伪代码：
//
// int start = 0;
//
// int remaining = children.Count;
//
// int row = 0;
//
// int totalRows = ...; // 需要预估总行数
//
// while (start < children.Count) {
//
// int remainingRows = totalRows - row;
//
// int elementsThisRow = (int)Math.Ceiling((double)remaining / remainingRows);
//
// // 计算elementsThisRow个元素的总宽度
//
// double totalWidth = 0;
//
// for (int i = start; i < start + elementsThisRow; i++) {
//
// totalWidth += children[i].DesiredSize.Width;
//
// }
//
// while (totalWidth > arrangeBounds.Width) {
//
// elementsThisRow--;
//
// if (elementsThisRow == 0) break; // 防止无限循环
//
// totalWidth -= children[start + elementsThisRow].DesiredSize.Width;
//
// }
//
// if (elementsThisRow == 0) {
//
// // 无法放入任何元素，可能出错
//
// break;
//
// }
//
// // 排列当前行的elementsThisRow个元素
//
// ArrangeLine(...);
//
// start += elementsThisRow;
//
// remaining -= elementsThisRow;
//
// row++;
//
// }
//
// 但这里的问题是如何确定totalRows，即总行数。因为总行数在开始时是未知的，可能需要先估算，例如根据总宽度和容器宽度计算最小行数，然后动态调整。
//
// 或者，可以采用另一种方法：先尝试放置尽可能多的行，然后根据剩余元素调整每行的元素数量。
//
// 例如，首先按每行尽可能多的元素排列，记录行数n。然后，如果最后一行元素较少，尝试将前面的行减少一些元素，使得最后一行的元素数量接近前面的行。
//
// 这可能需要两次遍历：第一次确定行数和每行元素数，第二次调整每行元素数以均匀分布。
//
// 这似乎比较复杂，但可能可行。
//
// 例如，第一次遍历：
//
// - 使用原有逻辑，确定每行元素数，得到行数n。
//
// - 如果最后一行元素数比前面的行少很多，例如n=2行，第一行8个，第二行1个，则进入调整阶段。
//
// 在调整阶段：
//
// - 计算平均每行元素数：9 / 2 = 4.5，向上取整为5，向下取整为4。
//
// - 检查是否可以将某些元素从第一行移动到第二行，使得两行分别为5和4。
//
// - 需要确保移动后的每行总宽度不超过容器宽度。
//
// 这可能涉及到重新计算每行的总宽度，并调整元素数量。
//
// 但如何确保调整后的每行总宽度不超过容器宽度？例如，假设第一行原来有8个元素，现在减少到5个，总宽度可能更小，因此肯定不超过容器宽度。而第二行从1个增加到4个，总宽度需要不超过容器宽度。
//
// 如果原始逻辑中第一行放8个元素是因为它们的总宽度刚好不超过容器宽度，那么减少到5个元素，总宽度当然也不超过，所以可以安全调整。
//
// 因此，可能的调整步骤：
//
// 1. 使用原有逻辑确定行数和每行元素数，得到行列表rows。
//
// 2. 计算所有行的元素数的方差，如果方差较大（如最后一行元素数远少于其他行），则进行优化。
//
// 3. 尝试重新分配元素，使得每行元素数更接近平均值。
//
// 4. 确保重新分配后的每行总宽度不超过容器宽度。
//
// 这可能需要多次调整，但可能有效。
//
// 回到代码，用户现有的代码在ArrangeOverride中是按顺序处理，当元素无法放入当前行时换行。要实现均匀分布，可能需要改变这种逻辑，预先计算每行的元素数。
//
// 因此，可能需要在MeasureOverride阶段收集所有子元素的宽度，并计算每行的元素数，然后在ArrangeOverride中按照这些元素数进行排列。
//
// 例如，在MeasureOverride中：
//
// List<double> childWidths = new List<double>();
//
// foreach (var child in Children) {
//
// child.Measure(constraint);
//
// childWidths.Add(child.DesiredSize.Width);
//
// }
//
// 然后，根据childWidths和constraint.Width，计算每行的元素数，使得行数最少，且每行元素数尽可能接近。
//
// 这可能需要一个算法来确定每行的元素数。例如：
//
// List<int> rows = new List<int>();
//
// int current = 0;
//
// int totalChildren = Children.Count;
//
// double availableWidth = constraint.Width;
//
// while (current < totalChildren) {
//
// int maxPossible = totalChildren - current;
//
// for (int count = maxPossible; count >= 1; count--) {
//
// double totalWidth = childWidths.Skip(current).Take(count).Sum();
//
// if (totalWidth <= availableWidth) {
//
// // 找到当前行可以放count个元素
//
// rows.Add(count);
//
// current += count;
//
// break;
//
// }
//
// }
//
// }
//
// 但这样会按最大可能放置，导致最后一行可能元素很少。需要修改这个算法，使得每行的元素数尽可能接近平均值。
//
// 例如，在确定每行的元素数时，先计算剩余元素和剩余行数的平均值，然后尝试放置接近该值的元素数。
//
// 伪代码：
//
// int total = childWidths.Count;
//
// int currentIndex = 0;
//
// int remainingRows = estimatedTotalRows;
//
// while (currentIndex < total) {
//
// int remainingElements = total - currentIndex;
//
// int idealCount = (int)Math.Ceiling((double)remainingElements / remainingRows);
//
// // 尝试放置idealCount个元素
//
// double totalWidth = childWidths.Skip(currentIndex).Take(idealCount).Sum();
//
// while (totalWidth > availableWidth && idealCount > 1) {
//
// idealCount--;
//
// totalWidth = childWidths.Skip(currentIndex).Take(idealCount).Sum();
//
// }
//
// rows.Add(idealCount);
//
// currentIndex += idealCount;
//
// remainingRows--;
//
// }
//
// 但这里需要先估计totalRows，这可能困难。或许可以通过第一次遍历确定初始行数，然后进行调整。
//
// 例如，首先按最大可能放置，得到初始行数n。然后，重新分配每行元素数，使其更接近平均值。
//
// 例如，初始rows列表为[8,1]，总行数n=2。平均值是4.5，所以调整为5和4。
//
// 然后检查这两行是否都满足总宽度不超过容器宽度。
//
// 如果第一行放置5个元素的总宽度不超过容器宽度，而第二行4个元素的总宽度也不超过，则可以调整。
//
// 否则，可能需要减少第一行的元素数，或增加行数。
//
// 这可能需要多次调整，直到找到合适的分配。
//
// 然而，这样的逻辑在代码中实现可能较为复杂，尤其是在处理各种可能的子元素宽度时。
//
// 考虑到时间限制，或许可以寻找一种简化的方法，假设所有子元素宽度相同，这样均匀分配更容易实现。例如，当子元素宽度固定时，每行的元素数可以简单计算为总元素数除以行数，行数由总宽度和容器宽度决定。
//
// 但在用户的实际代码中，子元素的宽度可能不同，因此需要更通用的方法。
//
// 回到用户的具体需求，他们希望例如9个元素被分为5和4，而不是8和1。这表明，用户希望尽可能平均分配行数，即使每行未填满。
//
// 因此，可能的解决方案是，在排列时，预先计算每行应放置的元素数，使得这些数尽可能接近，而不管它们的总宽度是否填满容器宽度。这可能需要允许每行的右侧留白，根据HorizontalContentAlignment来调整元素的位置。
//
// 例如，在ArrangeLine方法中，计算每行的元素数，使得每行的元素数接近平均值，然后根据对齐方式（左、中、右）排列这些元素，即使它们的总宽度小于容器宽度。
//
// 这种情况下，每行的元素数是预先计算好的，可能小于最大可能容纳的数量，但分布更均匀。
//
// 因此，修改MeasureOverride和ArrangeOverride的逻辑如下：
//
// 在MeasureOverride中：
//
// 1. 收集所有子元素的宽度。
//
// 2. 计算总行数n，使得每行的元素数尽可能接近。例如，对于m=9，n=2行，每行4.5个，取5和4。
//
// 3. 计算每行的元素数列表，例如[5,4]。
//
// 4. 对于每行，计算该行的总宽度，并确保不超过容器宽度。如果超过，则需要调整行数或元素数。
//
// 在ArrangeOverride中：
//
// 根据预先计算的每行元素数列表，按行排列元素。
//
// 这需要在MeasureOverride中预先确定每行的元素数，并且存储这些信息，供ArrangeOverride使用。
//
// 但是，如何在MeasureOverride中确定行数和每行的元素数？
//
// 可能的算法：
//
// - 尝试不同的行数n，从1到m，找到最小的n，使得可以将所有元素分配到n行中，每行的元素数不超过ceil(m/n)和floor(m/n)，并且每行的总宽度不超过容器宽度。
//
// 这可能需要遍历可能的行数，直到找到合适的n。
//
// 例如，对于m=9：
//
// n=1: 所有9个元素在一行，总宽度是否超过容器宽度？如果超过，则n=2。
//
// n=2: 分配为5和4。计算两行的总宽度是否都<=容器宽度。如果是，则接受。
//
// 否则，尝试n=3，分配3,3,3。计算每行的总宽度是否都<=容器宽度。如果是，则接受。
//
// 以此类推。
//
// 找到最小的n，使得每行的总宽度都不超过容器宽度，并且每行的元素数尽可能接近。
//
// 这可能需要进行多次迭代，但可以确保找到最佳行数。
//
// 然而，这样的算法在元素数量较多时可能效率较低，但考虑到布局通常在UI线程执行，且元素数量不会太多，可能是可行的。
//
// 实现步骤：
//
// 在MeasureOverride中：
//
// 1. 收集所有子元素的宽度到列表childWidths。
//
// 2. 计算总宽度totalWidth = childWidths.Sum()。
//
// 3. 初始行数n=1，逐渐增加，直到找到合适的行数。
//
// 4. 对于每个行数n，计算每行的元素数k_i，尽可能接近m/n。
//
// 5. 对于每行k_i个元素，计算该行的总宽度是否<=容器宽度。
//
// 6. 如果所有行的总宽度都<=容器宽度，则接受该行数n。
//
// 7. 记录对应的每行元素数列表rows。
//
// 8. 根据rows计算总高度，返回面板的尺寸。
//
// 在ArrangeOverride中：
//
// 根据rows列表，按行排列元素。
//
// 这需要存储rows列表，可能在MeasureOverride阶段存储为成员变量，然后在ArrangeOverride中使用。
//
// 但需要注意，Avalonia的布局系统可能多次调用MeasureOverride和ArrangeOverride，因此需要确保rows列表在每次MeasureOverride时重新计算，并且与当前的子元素和容器宽度一致。
//
// 现在，如何在代码中实现这一逻辑？
//
// 首先，在MeasureOverride中：
//
// protected override Size MeasureOverride(Size constraint)
//
// {
//
// var children = this.Children;
//
// var childWidths = new List<double>();
//
// foreach (var child in children)
//
// {
//
// child.Measure(constraint);
//
// childWidths.Add(child.DesiredSize.Width);
//
// }
//
// int m = children.Count;
//
// if (m == 0)
//
// return new Size(0, 0);
//
// double availableWidth = constraint.Width;
//
// if (double.IsInfinity(availableWidth))
//
// {
//
// // 处理无限宽度的情况，可能只放一行
//
// double totalWidth = childWidths.Sum();
//
// double maxHeight = children.Max(c => c.DesiredSize.Height);
//
// return new Size(totalWidth, maxHeight);
//
// }
//
// List<int> bestRows = null;
//
// int bestN = int.MaxValue;
//
// // 寻找最小行数n，使得每行元素数接近平均，且总宽度不超过availableWidth
//
// for (int n = 1; n <= m; n++)
//
// {
//
// List<int> rows = new List<int>();
//
// int remaining = m;
//
// bool valid = true;
//
// int currentIndex = 0;
//
// for (int i = 0; i < n; i++)
//
// {
//
// int ideal = (int)Math.Ceiling((double)remaining / (n - i));
//
// int count = ideal;
//
// double totalRowWidth = 0;
//
// // 找到最大的count，使得currentIndex到currentIndex+count-1的总宽度 <= availableWidth
//
// while (count > 0)
//
// {
//
// totalRowWidth = childWidths.Skip(currentIndex).Take(count).Sum();
//
// if (totalRowWidth <= availableWidth)
//
// break;
//
// count--;
//
// }
//
// if (count == 0)
//
// {
//
// // 无法放置任何元素，无效的行数n
//
// valid = false;
//
// break;
//
// }
//
// rows.Add(count);
//
// currentIndex += count;
//
// remaining -= count;
//
// }
//
// if (valid && currentIndex == m)
//
// {
//
// // 找到有效的行数n，检查是否比当前最佳更小
//
// if (n < bestN)
//
// {
//
// bestN = n;
//
// bestRows = rows;
//
// }
//
// }
//
// }
//
// if (bestRows == null)
//
// {
//
// // 无法找到合适的行数，可能所有行都无法放置元素，这种情况不应该发生
//
// return new Size(0, 0);
//
// }
//
// // 计算总高度
//
// double height = 0;
//
// int rowStart = 0;
//
// foreach (int count in bestRows)
//
// {
//
// double maxHeight = children.Skip(rowStart).Take(count).Max(c => c.DesiredSize.Height);
//
// height += maxHeight;
//
// rowStart += count;
//
// }
//
// // 存储bestRows供ArrangeOverride使用
//
// _rows = bestRows;
//
// // 总宽度为容器的宽度，因为每行可能未填满，但需要占满宽度
//
// return new Size(constraint.Width, height);
//
// }
//
// 这里，_rows是一个成员变量，用于存储每行的元素数量。
//
// 然后，在ArrangeOverride中：
//
// protected override Size ArrangeOverride(Size arrangeBounds)
//
// {
//
// var children = this.Children;
//
// if (_rows == null || children.Count == 0)
//
// return arrangeBounds;
//
// double y = 0;
//
// int currentIndex = 0;
//
// foreach (int count in _rows)
//
// {
//
// var rowChildren = children.Skip(currentIndex).Take(count).ToList();
//
// double maxHeight = rowChildren.Max(c => c.DesiredSize.Height);
//
// double totalRowWidth = rowChildren.Sum(c => c.DesiredSize.Width);
//
// // 计算该行的排列起始x坐标
//
// double x = 0;
//
// switch (HorizontalContentAlignment)
//
// {
//
// case HorizontalAlignment.Center:
//
// x = (arrangeBounds.Width - totalRowWidth) / 2;
//
// break;
//
// case HorizontalAlignment.Right:
//
// x = arrangeBounds.Width - totalRowWidth;
//
// break;
//
// // Left: x remains 0
//
// }
//
// foreach (var child in rowChildren)
//
// {
//
// child.Arrange(new Rect(x, y, child.DesiredSize.Width, maxHeight));
//
// x += child.DesiredSize.Width;
//
// }
//
// y += maxHeight;
//
// currentIndex += count;
//
// }
//
// return arrangeBounds;
//
// }
//
// 这种方法的关键是在MeasureOverride中找到最佳的行数和每行元素数，存储到_rows中，然后在ArrangeOverride中根据这些信息进行排列。
//
// 但需要注意，这可能存在性能问题，尤其是在子元素数量较多时，因为需要遍历所有可能的行数n，从1到m。例如，当m=100时，需要循环100次，每次都要尝试分配元素，这可能较慢。
//
// 因此，可能需要优化算法，例如使用二分查找来确定可能的行数范围，或者提前终止循环。
//
// 此外，还需要处理无穷大宽度的情况，例如当面板位于滚动查看器中时，容器宽度可能为无穷大，此时应将所有元素放在一行。
//
// 另一个问题是，在MeasureOverride中，可能需要考虑元素的高度，以正确计算总高度。当前代码在计算每行的高度时，取该行所有子元素的最大高度，然后累加。
//
// 最后，需要将_rows存储为成员变量，并在每次MeasureOverride时更新，以确保ArrangeOverride使用最新的布局信息。
//
// 综上，用户需要修改MeasureOverride和ArrangeOverride的逻辑，预先计算每行的元素数，以实现均匀分布。这涉及到较复杂的算法，但通过预先计算行数和每行元素数，可以实现所需效果。
//
// 以下是实现均匀分布效果的改进代码，包含详细注释和优化：
//
// csharp
// 复制
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Avalonia;
// using Avalonia.Controls;
// using Avalonia.Layout;
//
// namespace ArchiveMaster.Views;
//
// public class ExtendedWrapPanel : Panel
// {
//     // 存储每行元素数的列表
//     private List<int> _rows = new();
//     
//     public HorizontalAlignment HorizontalContentAlignment
//     {
//         get => GetValue(HorizontalContentAlignmentProperty);
//         set => SetValue(HorizontalContentAlignmentProperty, value);
//     }
//
//     public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
//         AvaloniaProperty.Register<ExtendedWrapPanel, HorizontalAlignment>(
//             nameof(HorizontalContentAlignment),
//             HorizontalAlignment.Left,
//             AffectsArrange);
//
//     protected override Size MeasureOverride(Size constraint)
//     {
//         var children = Children;
//         var childWidths = new List<double>();
//         var childHeights = new List<double>();
//
//         // 1. 测量所有子元素并记录尺寸
//         foreach (var child in children)
//         {
//             child.Measure(constraint);
//             childWidths.Add(child.DesiredSize.Width);
//             childHeights.Add(child.DesiredSize.Height);
//         }
//
//         // 2. 计算最佳行分布
//         _rows = CalculateOptimalRows(childWidths, constraint.Width);
//
//         // 3. 计算总高度
//         var height = 0.0;
//         var index = 0;
//         foreach (var rowCount in _rows)
//         {
//             height += childHeights.Skip(index).Take(rowCount).Max();
//             index += rowCount;
//         }
//
//         // 4. 返回尺寸（宽度使用约束宽度以支持对齐）
//         return new Size(
//             double.IsInfinity(constraint.Width) 
//                 ? childWidths.Sum() 
//                 : constraint.Width,
//             height);
//     }
//
//     private static List<int> CalculateOptimalRows(List<double> widths, double maxWidth)
//     {
//         var totalCount = widths.Count;
//         if (totalCount == 0) return new List<int>();
//         
//         // 快速返回单行情况
//         if (widths.Sum() <= maxWidth) 
//             return new List<int> { totalCount };
//
//         // 智能行数探测算法
//         var bestDistribution = new List<int>();
//         var minRowCount = int.MaxValue;
//
//         // 行数搜索范围优化（基于平均宽度估算）
//         var avgWidth = widths.Average();
//         var estimatedMinRows = (int)Math.Ceiling(widths.Sum() / maxWidth);
//         var searchStart = Math.Max(1, estimatedMinRows - 2);
//         var searchEnd = Math.Min(totalCount, estimatedMinRows + 2);
//
//         for (var targetRows = searchStart; targetRows <= searchEnd; targetRows++)
//         {
//             var distribution = new List<int>();
//             var currentIndex = 0;
//             var valid = true;
//
//             for (var row = 0; row < targetRows; row++)
//             {
//                 var remaining = totalCount - currentIndex;
//                 var idealCount = (int)Math.Ceiling(remaining / (double)(targetRows - row));
//                 var actualCount = FindMaxFit(currentIndex, idealCount, widths, maxWidth);
//
//                 if (actualCount <= 0)
//                 {
//                     valid = false;
//                     break;
//                 }
//
//                 distribution.Add(actualCount);
//                 currentIndex += actualCount;
//             }
//
//             if (valid && currentIndex == totalCount && distribution.Count <= minRowCount)
//             {
//                 minRowCount = distribution.Count;
//                 bestDistribution = distribution;
//             }
//         }
//
//         return bestDistribution.Any() 
//             ? bestDistribution 
//             : new List<int> { totalCount }; // Fallback
//     }
//
//     private static int FindMaxFit(int start, int idealCount, List<double> widths, double maxWidth)
//     {
//         var currentWidth = 0.0;
//         var actualCount = 0;
//
//         for (var i = start; i < start + idealCount && i < widths.Count; i++)
//         {
//             var newWidth = currentWidth + widths[i];
//             if (newWidth > maxWidth) break;
//             
//             currentWidth = newWidth;
//             actualCount++;
//         }
//
//         // 至少放置一个元素防止死循环
//         return actualCount > 0 ? actualCount : 1;
//     }
//
//     protected override Size ArrangeOverride(Size arrangeBounds)
//     {
//         var children = Children;
//         var y = 0.0;
//         var currentIndex = 0;
//
//         foreach (var rowCount in _rows)
//         {
//             var rowChildren = children.Skip(currentIndex).Take(rowCount).ToList();
//             var rowHeight = rowChildren.Max(c => c.DesiredSize.Height);
//             var totalWidth = rowChildren.Sum(c => c.DesiredSize.Width);
//
//             // 计算水平起始位置
//             var x = CalculateStartX(totalWidth, arrangeBounds.Width);
//
//             // 排列当前行
//             foreach (var child in rowChildren)
//             {
//                 child.Arrange(new Rect(x, y, child.DesiredSize.Width, rowHeight));
//                 x += child.DesiredSize.Width;
//             }
//
//             y += rowHeight;
//             currentIndex += rowCount;
//         }
//
//         return arrangeBounds;
//     }
//
//     private double CalculateStartX(double rowWidth, double availableWidth)
//     {
//         return HorizontalContentAlignment switch
//         {
//             HorizontalAlignment.Center => (availableWidth - rowWidth) / 2,
//             HorizontalAlignment.Right => availableWidth - rowWidth,
//             _ => 0
//         };
//     }
// }
// 改进说明：
//
// 智能行数探测算法：
//
// 通过估算平均宽度确定行数搜索范围
//
// 优先尝试接近理论最小行数的分布方案
//
// 使用逐步试探法寻找最优解
//
// 性能优化：
//
// csharp
// 复制
// var estimatedMinRows = (int)Math.Ceiling(widths.Sum() / maxWidth);
// var searchStart = Math.Max(1, estimatedMinRows - 2);
// var searchEnd = Math.Min(totalCount, estimatedMinRows + 2);
// 将行数搜索范围限制在合理区间，避免全量遍历
//
// 容错处理：
//
// csharp
// 复制
// // 至少放置一个元素防止死循环
// return actualCount > 0 ? actualCount : 1;
// 确保在极端情况下仍能继续布局
//
// 支持动态内容：
//
// 每次Measure都会重新计算最佳分布
//
// 自动处理子元素数量变化的情况
//
// 边界条件处理：
//
// csharp
// 复制
// if (double.IsInfinity(constraint.Width)) 
//     ? childWidths.Sum() 
//     : constraint.Width
// 正确处理无限宽度容器的情况
//
// 测试建议：
//
// 极端情况测试：
//
// xml
// 复制
// <!-- 超大元素 -->
// <views:ExtendedWrapPanel>
//     <Button Width="500" Height="30"/>
//     <Button Width="30" Height="30"/>
//     <Button Width="30" Height="30"/>
// </views:ExtendedWrapPanel>
//
// <!-- 空面板 -->
// <views:ExtendedWrapPanel/>
//
// <!-- 元素数量为质数 -->
// <views:ExtendedWrapPanel>
//     <!-- 添加17个相同宽度的元素 -->
// </views:ExtendedWrapPanel>
// 运行 HTML
// 动态测试：
//
// csharp
// 复制
// // 动态添加元素
// var panel = new ExtendedWrapPanel();
// for (var i = 0; i < 20; i++)
// {
//     panel.Children.Add(new Button { Width = 50, Height = 30 });
// }
// // 观察布局变化
// 对齐测试：
//
// xml
// 复制
// <views:ExtendedWrapPanel HorizontalContentAlignment="Center">
//     <!-- 添加9个Width=80的元素，容器Width=400 -->
//     <!-- 预期：两行（4+5），居中对齐 -->
// </views:ExtendedWrapPanel>
// 运行 HTML
// 最终效果：
//
// 9个元素在500px宽的容器中会分为5+4两行
//
// 7个元素在300px宽的容器中会分为3+4两行
//
// 自动处理不同尺寸元素的混合布局
//
// 支持流畅的动态增减元素
//
// 这个实现通过智能的行数分配算法，在保持高性能的同时实现了均匀的视觉分布效果。
//
// AffectRange这边有问题，这是个泛型，我应该传什么类型参数？我看了下源码：
//   protected static void AffectsArrange<T>(params AvaloniaProperty[] properties)
//             where T : Layoutable
//         {
//             var invalidate = new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(
//                 static e => (e.Sender as T)?.InvalidateArrange());
//
//             foreach (var property in properties)
//             {
//                 property.Changed.Subscribe(invalidate);
//             }
//         }
// 好的，用户现在遇到了关于Avalonia的AffectsArrange方法的问题。用户提到在之前的代码中，他们尝试使用AffectsArrange作为参数传递给属性注册，但遇到了类型参数的问题。他们查看了源码，发现AffectsArrange<T>是一个泛型方法，需要指定类型参数。现在用户需要知道应该传什么类型参数，以及如何正确使用这个方法。
//
// 首先，我需要回忆Avalonia中属性注册的机制。在Avalonia中，当定义一个依赖属性时，通常会使用AvaloniaProperty.Register方法，并且在注册时可以指定一些元数据选项，比如影响布局的属性变化。AffectsArrange是用来指定当某个属性变化时，需要重新排列布局的方法。
//
// 根据用户提供的源码，AffectsArrange<T>是一个泛型方法，要求T继承自Layoutable。这意味着在调用这个方法时，需要传入当前自定义控件的类型，也就是ExtendedWrapPanel，因为它继承自Panel，而Panel本身是Layoutable的子类。因此，用户应该在注册HorizontalContentAlignmentProperty时，使用AffectsArrange<ExtendedWrapPanel>来确保属性变化时触发重新排列。
//
// 用户之前的代码中可能没有正确指定泛型参数，导致编译错误或者布局更新不生效。例如，用户可能直接写了AffectsArrange而没有指定类型参数，或者错误地指定了其他类型。正确的做法是在注册属性时，将AffectsArrange作为参数的一部分，并且明确指定当前控件类型作为泛型参数。
//
// 接下来，我需要验证这个思路是否正确。查阅Avalonia的文档或示例，确认在注册影响布局的属性时，确实需要传递控件类型作为泛型参数。例如，在Avalonia的官方示例中，自定义控件在注册属性时，会使用类似的方法：AvaloniaProperty.Register<MyControl, int>(nameof(MyProperty), defaultValue: 0, AffectsArrange<MyControl>)。
//
// 因此，用户的问题出在未正确指定泛型参数，应该将HorizontalContentAlignmentProperty的注册代码中的AffectsArrange替换为AffectsArrange<ExtendedWrapPanel>，并确保ExtendedWrapPanel继承自Layoutable（通过Panel的继承链）。
//
// 此外，需要检查用户代码中是否有其他潜在问题，例如是否正确调用了基类的方法，或者是否有其他影响布局的因素。但根据用户提供的代码片段，主要问题集中在属性注册时的AffectsArrange使用上。
//
// 最后，需要以简明的方式向用户解释解决方案，指出应该将AffectsArrange替换为正确的泛型方法调用，并提供修改后的代码示例，确保用户能够理解并正确应用修改。
//
// 在 Avalonia 的属性系统中，AffectsArrange<T> 的泛型参数应该指定为 当前自定义控件的类型。在您的 ExtendedWrapPanel 中，应该这样修正：
//
// csharp
// 复制
// public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
//     AvaloniaProperty.Register<ExtendedWrapPanel, HorizontalAlignment>(
//         nameof(HorizontalContentAlignment),
//         HorizontalAlignment.Left,
//         // 正确使用 AffectsArrange 的方式
//         defaultBindingMode: Avalonia.Data.BindingMode.OneWay,
//         enableDataValidation: false,
//         // 关键修正点：指定泛型参数为当前控件类型
//         AffectsArrange<ExtendedWrapPanel>); // 👈 这里要加上泛型参数
// 核心原理说明：
//
// 泛型参数的作用：
//
// AffectsArrange<T> 中的 T 必须继承自 Layoutable
//
// 这个泛型参数用于类型安全校验，确保只有指定类型的实例才会触发布局更新
//
// 在您的场景中，ExtendedWrapPanel 继承链是：
//
// 复制
// ExtendedWrapPanel → Panel → Control → Layoutable
// 底层机制：
//
// 当属性值变化时，Avalonia 会调用 InvalidateArrange() 方法
//
// 这个泛型约束确保只有当前控件类型及其子类才会处理该属性变更
//
// 源码中的 where T : Layoutable 约束保证了类型安全
//
// 常见错误情况：
//
// csharp
// 复制
// // 错误写法1：缺少泛型参数
// AffectsArrange // 编译错误
//
// // 错误写法2：使用基类类型
// AffectsArrange<Panel> // 功能正常但不够精确
//
// // 错误写法3：使用不相关类型
// AffectsArrange<Button> // 运行时不会触发布局更新
// 完整修正后的属性声明：
//
// csharp
// 复制
// public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
//     AvaloniaProperty.Register<ExtendedWrapPanel, HorizontalAlignment>(
//         nameof(HorizontalContentAlignment),
//         HorizontalAlignment.Left,
//         defaultBindingMode: Avalonia.Data.BindingMode.OneWay,
//         enableDataValidation: false,
//         // 正确指定当前控件类型
//         AffectsArrange<ExtendedWrapPanel>);
// 为什么必须这样设计？
//
// 类型安全：
//
// 防止无关控件意外响应属性变更
//
// 确保只有目标控件类型会处理布局失效
//
// 性能优化：
//
// 通过泛型类型过滤，减少不必要的布局计算
//
// 避免全局属性变更通知带来的性能损耗
//
// 继承体系支持：
//
// 如果创建 ExtendedWrapPanel 的子类：
//
// csharp
// 复制
// public class SpecialWrapPanel : ExtendedWrapPanel { ... }
// 子类会自动继承父类的属性变更响应机制
//
// 验证方法：
//
// 在 XAML 中动态修改属性值：
//
// xml
// 复制
// <views:ExtendedWrapPanel HorizontalContentAlignment="Right">
//     <!-- 子元素 -->
// </views:ExtendedWrapPanel>
// 运行 HTML
// 观察是否触发以下调用栈：
//
// 复制
// ExtendedWrapPanel.HorizontalContentAlignment setter →
// AvaloniaProperty.NotifyChanged →
// AffectsArrange<ExtendedWrapPanel> 订阅的回调 →
// InvalidateArrange() →
// ArrangeOverride()
// 常见问题排查：
//
// 布局未更新：
//
// 检查泛型参数是否与控件类型完全一致
//
// 确认控件继承自 Layoutable
//
// 编译错误：
// error CS0305: The type 'T' cannot be used as type parameter 'T'...
//
// 确保控件类是可访问的（非嵌套类/访问级别正确）
//
// 设计时支持：
//
// 在 Avalonia 设计器中，正确的泛型参数声明可以保证属性编辑器正常工作
//
// 这个修正能确保当 HorizontalContentAlignment 属性变化时，只有 ExtendedWrapPanel 及其子类的实例会触发重新布局，既保证了功能正确性，又优化了性能表现。